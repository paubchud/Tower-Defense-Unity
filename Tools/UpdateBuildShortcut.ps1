param(
    [Parameter(Mandatory = $true)][string]$ShortcutPath,
    [Parameter(Mandatory = $true)][string]$ExecutablePath,
    [Parameter(Mandatory = $true)][string]$Version
)

$ErrorActionPreference = 'Stop'
$shortcutBuildRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\Builds')) + [System.IO.Path]::DirectorySeparatorChar
$ShortcutPath = [System.IO.Path]::GetFullPath($ShortcutPath)
$ExecutablePath = [System.IO.Path]::GetFullPath($ExecutablePath)
if ($Version -notmatch '\A(0|[1-9][0-9]*)(\.(0|[1-9][0-9]*)){1,2}\z') { throw 'Invalid build version.' }
if (-not $ShortcutPath.StartsWith($shortcutBuildRoot, [System.StringComparison]::OrdinalIgnoreCase) -or
    [System.IO.Path]::GetExtension($ShortcutPath) -ne '.lnk' -or
    -not $ExecutablePath.StartsWith($shortcutBuildRoot, [System.StringComparison]::OrdinalIgnoreCase) -or
    -not (Test-Path -LiteralPath $ExecutablePath -PathType Leaf)) { throw 'The shortcut helper only accepts build artifacts inside this repository.' }

$shortcutShell = New-Object -ComObject WScript.Shell
$buildShortcut = $null
try {
    $buildShortcut = $shortcutShell.CreateShortcut($ShortcutPath)
    $buildShortcut.TargetPath = $ExecutablePath
    $buildShortcut.WorkingDirectory = [System.IO.Path]::GetDirectoryName($ExecutablePath)
    $buildShortcut.Arguments = ''
    $buildShortcut.Description = 'Tower Defense v' + $Version + ' - latest successful Windows build'
    $buildShortcut.IconLocation = $ExecutablePath + ',0'
    $buildShortcut.Save()
} finally {
    if ($null -ne $buildShortcut) { [System.Runtime.InteropServices.Marshal]::FinalReleaseComObject($buildShortcut) | Out-Null }
    [System.Runtime.InteropServices.Marshal]::FinalReleaseComObject($shortcutShell) | Out-Null
}
