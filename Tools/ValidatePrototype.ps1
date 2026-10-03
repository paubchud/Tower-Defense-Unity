param([switch]$Capture, [switch]$Combat, [switch]$Relay, [string]$ExecutablePath)

$ErrorActionPreference = 'Stop'
if ($Relay) { throw 'This branch uses Steam, not Unity Relay. Use the v0.1 checkout for legacy Relay tests. Steam peer tests need two accounts/devices; see STEAM_SETUP.md.' }
$prototypeRepo = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ($ExecutablePath) {
    $prototypeExe = [System.IO.Path]::GetFullPath($ExecutablePath)
} else {
    $manifestPath = Join-Path $prototypeRepo 'Builds\latest-build.json'
    if (-not (Test-Path -LiteralPath $manifestPath)) { throw 'Create a versioned Windows build first; see README.md.' }
    $latest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    $buildRoot = [System.IO.Path]::GetFullPath((Join-Path $prototypeRepo 'Builds')) + [System.IO.Path]::DirectorySeparatorChar
    $prototypeExe = [System.IO.Path]::GetFullPath((Join-Path $buildRoot $latest.executable))
    if (-not $prototypeExe.StartsWith($buildRoot, [System.StringComparison]::OrdinalIgnoreCase)) { throw 'The latest-build executable is outside Builds.' }
}
if (-not (Test-Path -LiteralPath $prototypeExe)) { throw 'Build the Windows development player first; see README.md.' }
$runDirectory = Join-Path $prototypeRepo ('Builds\Validation\Smoke-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null
$ownedProcesses = @{}

function Start-SmokePlayer([string]$role, [string]$flags) {
    $log = Join-Path $runDirectory ($role + '.log')
    $arguments = '-screen-fullscreen 0 -screen-width 1440 -screen-height 900 {0} -logFile "{1}"' -f $flags, $log
    if ($Combat -and $role -ne 'third') { $arguments += ' -td-combat' }
    if ($Capture -and $role -ne 'third') { $arguments += ' -td-captures "' + $runDirectory + '"' }
    $process = Start-Process -FilePath $prototypeExe -ArgumentList $arguments -WindowStyle Hidden -PassThru
    $null = $process.Handle
    $ownedProcesses[$role] = $process
}

try {
    Start-SmokePlayer 'host' '-td-smoke-host'
    Start-Sleep -Milliseconds 1500
    Start-SmokePlayer 'client' '-td-smoke-client'
    $deadline = (Get-Date).AddSeconds(30)
    $hostLog = Join-Path $runDirectory 'host.log'
    do {
        Start-Sleep -Milliseconds 100
        $started = (Test-Path -LiteralPath $hostLog) -and (Select-String -LiteralPath $hostLog -Pattern '^TD_MATCH_STARTED' -Quiet)
        if ($ownedProcesses['host'].HasExited) { throw 'Host exited before the first match started.' }
    } while (-not $started -and (Get-Date) -lt $deadline)
    if (-not $started) { throw 'Timed out waiting for the two-player match.' }
    # Launch during the FIRST active match, not between leave and rejoin.
    Start-SmokePlayer 'third' '-td-smoke-client -td-expect-reject'
    $deadline = (Get-Date).AddSeconds(120)
    do {
        Start-Sleep -Milliseconds 250
        $running = @($ownedProcesses.Values | Where-Object { -not $_.HasExited })
    } while ($running.Count -gt 0 -and (Get-Date) -lt $deadline)
    if ($running.Count -gt 0) { throw 'Smoke processes exceeded their timeout.' }
    foreach ($role in @('host', 'client', 'third')) {
        $process = $ownedProcesses[$role]
        $process.WaitForExit()
        if ($process.ExitCode -ne 0) { throw "$role exited with code $($process.ExitCode). See $runDirectory." }
        $log = Get-Content -LiteralPath (Join-Path $runDirectory ($role + '.log')) -Raw
        $markers = if ($role -eq 'third') { @('TD_REJECTION_PASS') } else {
            @('TD_SMOKE_PASS', 'TD_CONTROLS_PASS', 'TD_RESET_PASS', 'TD_REJOIN_PASS')
        }
        if ($role -eq 'host') { $markers += 'TD_DISCONNECT_PASS' }
        if ($Combat -and $role -ne 'third') { $markers += @('TD_COMBAT_PASS', 'TD_GHOST_PASS', 'TD_RESULT_PASS') }
        foreach ($marker in $markers) {
            if (-not $log.Contains($marker)) { throw "$role is missing $marker. See $runDirectory." }
        }
        if ($log.Contains('TD_SMOKE_FAIL') -or $log.Contains('TD_REJECTION_FAIL')) { throw "$role reported a failed check." }
        Write-Output "$role PASS"
    }
    Write-Output "Validation output: $runDirectory"
}
finally {
    # Only terminate processes this script started; never touch an editor or a user's game instance.
    foreach ($process in $ownedProcesses.Values) {
        if (-not $process.HasExited) { Stop-Process -Id $process.Id -Force }
        $process.Dispose()
    }
}
