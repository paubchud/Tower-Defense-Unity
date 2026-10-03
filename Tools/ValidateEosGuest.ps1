param([string]$ExecutablePath, [switch]$Capture)
$ErrorActionPreference = 'Stop'
$repository = Split-Path $PSScriptRoot -Parent
if (-not $ExecutablePath) {
    $manifest = Get-Content -LiteralPath (Join-Path $repository 'Builds/latest-build.json') -Raw | ConvertFrom-Json
    if (-not $manifest.eosGuestConfigured) { throw 'Build a configured EOS Windows player first.' }
    $ExecutablePath = Join-Path $repository ('Builds/' + $manifest.executable)
}
$ExecutablePath = [IO.Path]::GetFullPath($ExecutablePath)
if (-not (Test-Path -LiteralPath $ExecutablePath -PathType Leaf)) { throw 'Missing game executable.' }
$runDirectory = Join-Path $repository ('Builds/Validation/EOS-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null
$log = Join-Path $runDirectory 'guest.log'
$arguments = '-td-eos-check -screen-fullscreen 0 -screen-width 1440 -screen-height 900 -logFile "' + $log + '"'
if ($Capture) { $arguments += ' -td-eos-captures "' + $runDirectory + '"' }
$ownedProcess = Start-Process -FilePath $ExecutablePath -ArgumentList $arguments -WindowStyle Hidden -PassThru
try {
    $deadline = (Get-Date).AddSeconds(180)
    while (-not $ownedProcess.HasExited -and (Get-Date) -lt $deadline) { Start-Sleep -Milliseconds 250 }
    if (-not $ownedProcess.HasExited) { throw "EOS diagnostic timed out. See $runDirectory" }
    $ownedProcess.WaitForExit()
    $failed = @(Select-String -LiteralPath $log -Pattern '^TD_EOS_SMOKE_FAIL.*')
    if ($failed.Count -gt 0) { throw $failed[0].Line }
    if ($ownedProcess.ExitCode -ne 0 -or -not (Select-String -LiteralPath $log -Pattern '^TD_EOS_SMOKE_PASS' -Quiet)) { throw "EOS diagnostic did not pass. See $runDirectory" }
    Write-Output 'TD_EOS_VALIDATION_PASS anonymous login, cancellation/late cleanup, host, leave and fresh rehost; NO second-peer/reachability proof.'
    Write-Output "Validation output: $runDirectory"
} finally {
    if (-not $ownedProcess.HasExited) { Stop-Process -Id $ownedProcess.Id -Force }
    $ownedProcess.Dispose()
}
