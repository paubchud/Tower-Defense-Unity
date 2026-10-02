param([switch]$DryRun)

# Explicit release command: local builds never upload themselves or modify main.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$releaseRepo = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$releaseBuildRoot = [System.IO.Path]::GetFullPath((Join-Path $releaseRepo 'Builds')) + [System.IO.Path]::DirectorySeparatorChar
$manifestPath = Join-Path $releaseBuildRoot 'latest-build.json'
if (-not (Test-Path -LiteralPath $manifestPath)) { throw 'Create a versioned Windows development build first.' }
$build = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($build.version -notmatch '\A(0|[1-9][0-9]*)(\.(0|[1-9][0-9]*)){1,2}\z') { throw 'Invalid build version.' }

function Resolve-ReleaseArtifact([string]$relative) {
    $resolved = [System.IO.Path]::GetFullPath((Join-Path $releaseBuildRoot $relative))
    if (-not $resolved.StartsWith($releaseBuildRoot, [System.StringComparison]::OrdinalIgnoreCase)) { throw 'Release artifact is outside Builds.' }
    if (-not (Test-Path -LiteralPath $resolved -PathType Leaf)) { throw "Missing release artifact: $relative" }
    return $resolved
}

$archive = Resolve-ReleaseArtifact $build.archive
$executable = Resolve-ReleaseArtifact $build.executable
$notesPath = Join-Path $releaseRepo ('Releases\' + $build.version + '.md')
if (-not (Test-Path -LiteralPath $notesPath)) { throw "Add release notes at Releases/$($build.version).md first." }
$archiveSize = (Get-Item -LiteralPath $archive).Length
$archiveHash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
$tag = 'v' + $build.version

function Read-ReleaseGit([string[]]$arguments) {
    $result = & git -C $releaseRepo @arguments 2>$null
    if ($LASTEXITCODE -ne 0) { throw "Git check failed: $($arguments[0])." }
    return ($result -join "`n").Trim()
}

$remote = Read-ReleaseGit -arguments @('remote', 'get-url', 'origin')
if ($remote -notmatch '\A(?:https://github\.com/|git@github\.com:)(?<owner>[A-Za-z0-9_-]+)/(?<repo>[A-Za-z0-9_.-]+)/?\z') {
    throw 'Origin must be a github.com repository, with no embedded credentials.'
}
$repositoryName = $Matches.owner + '/' + ($Matches.repo -replace '\.git$', '')
$commit = Read-ReleaseGit -arguments @('rev-parse', 'HEAD')
Write-Output "Release candidate: $repositoryName / $tag / $commit"
Write-Output "Archive: $archive ($archiveSize bytes, SHA256 $archiveHash)"
if ($DryRun) { Write-Output 'Dry run: no authentication, upload, or GitHub changes.'; return }
if (Read-ReleaseGit -arguments @('status', '--porcelain=v1')) { throw 'Commit the tested changes before publishing. This command does not commit, push, merge, or overwrite releases.' }

# Always regress the exact executable that will be uploaded, using LAN (no service quotas).
& (Join-Path $PSScriptRoot 'ValidatePrototype.ps1') -ExecutablePath $executable

$oldInteractive = $env:GCM_INTERACTIVE
$oldTerminalPrompt = $env:GIT_TERMINAL_PROMPT
$headers = @{}
try {
    # Use the installed Git credential helper. Never print or save its credentials.
    $env:GCM_INTERACTIVE = 'never'
    $env:GIT_TERMINAL_PROMPT = '0'
    $credentialRequest = "protocol=https`nhost=github.com`npath=$repositoryName.git`n`n"
    $credentialLines = $credentialRequest | & git -C $releaseRepo credential fill 2>$null
    if ($LASTEXITCODE -ne 0) { throw 'Git has no usable GitHub sign-in. Sign in with Git Credential Manager, then retry.' }
    $passwordLine = @($credentialLines | Where-Object { $_.StartsWith('password=') })
    if ($passwordLine.Count -ne 1) { throw 'The Git credential helper did not return a GitHub credential.' }
    $token = $passwordLine[0].Substring('password='.Length)
    $headers = @{
        Authorization = 'Bearer ' + $token
        Accept = 'application/vnd.github+json'
        'X-GitHub-Api-Version' = '2026-03-10'
        'User-Agent' = 'TowerDefense-ReleaseTool'
    }
    $credentialLines = $null
    $passwordLine = $null
    $token = $null

    function Invoke-ReleaseApi([string]$method, [string]$url, $body = $null, [switch]$AllowNotFound, [string]$upload = '') {
        try {
            if ($upload) { return Invoke-RestMethod -Method $method -Uri $url -Headers $headers -ContentType 'application/zip' -InFile $upload }
            if ($null -ne $body) {
                $json = [System.Text.Encoding]::UTF8.GetBytes(($body | ConvertTo-Json -Depth 10))
                return Invoke-RestMethod -Method $method -Uri $url -Headers $headers -ContentType 'application/json; charset=utf-8' -Body $json
            }
            return Invoke-RestMethod -Method $method -Uri $url -Headers $headers
        } catch {
            $status = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { 0 }
            if ($AllowNotFound -and $status -eq 404) { return $null }
            throw "GitHub request failed (HTTP $status). Check connection and repository Contents permissions. Any incomplete new release remains a draft."
        }
    }

    $api = 'https://api.github.com/repos/' + $repositoryName
    $repository = Invoke-ReleaseApi 'GET' $api
    if (-not $repository.permissions.push) { throw 'The signed-in account cannot publish in this repository.' }
    # Ensures the release references uploaded source, not an unrelated default-branch revision.
    $null = Invoke-ReleaseApi 'GET' ($api + '/commits/' + $commit)
    $release = Invoke-ReleaseApi 'GET' ($api + '/releases/tags/' + $tag) -AllowNotFound
    if ($release -and (-not $release.draft -or $release.target_commitish -ne $commit)) {
        throw "$tag already exists. Bump the game version and rebuild; published releases are never overwritten."
    }
    if (-not $release) {
        $release = Invoke-ReleaseApi 'POST' ($api + '/releases') @{
            tag_name = $tag
            target_commitish = $commit
            name = 'Tower Defense ' + $build.version
            body = Get-Content -LiteralPath $notesPath -Raw
            draft = $true
            prerelease = $false
            make_latest = 'false'
        }
    }
    $asset = @($release.assets | Where-Object { $_.name -eq $build.archive })
    if ($asset.Count -eq 0) {
        $uploadUrl = ($release.upload_url -replace '\{\?name,label\}$', '') + '?name=' + [System.Uri]::EscapeDataString($build.archive)
        if (-not $uploadUrl.StartsWith('https://uploads.github.com/repos/' + $repositoryName + '/')) { throw 'Unexpected GitHub upload destination.' }
        $asset = @(Invoke-ReleaseApi 'POST' $uploadUrl -upload $archive)
    }
    if ($asset.Count -ne 1 -or $asset[0].state -ne 'uploaded' -or $asset[0].size -ne $archiveSize -or $asset[0].digest -ne ('sha256:' + $archiveHash)) {
        throw 'GitHub asset verification failed. The draft is not published; no existing assets were deleted or replaced.'
    }
    $release = Invoke-ReleaseApi 'PATCH' ($api + '/releases/' + $release.id) @{ draft = $false; prerelease = $false; make_latest = 'true' }
    Write-Output "GitHub release: $($release.html_url)"
    Write-Output "Download: $($asset[0].browser_download_url)"
} finally {
    $headers.Clear()
    $credentialLines = $null
    $passwordLine = $null
    $token = $null
    $env:GCM_INTERACTIVE = $oldInteractive
    $env:GIT_TERMINAL_PROMPT = $oldTerminalPrompt
}
