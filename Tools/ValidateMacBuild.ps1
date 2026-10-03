param([string]$ManifestPath = (Join-Path $PSScriptRoot '../Builds/latest-build-macOS.json'))

# Static package validation only. Windows cannot execute a macOS player or verify Gatekeeper.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Add-Type -AssemblyName System.IO.Compression.FileSystem
$macRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../Builds')) + [IO.Path]::DirectorySeparatorChar
$macBuild = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
if ($macBuild.platform -ne 'macOS' -or $macBuild.architecture -ne 'x86_64+arm64') { throw 'Expected a Universal macOS build manifest.' }
if ($macBuild.version -notmatch '\A(0|[1-9][0-9]*)(\.(0|[1-9][0-9]*)){1,2}\z') { throw 'Invalid Mac version.' }

function Resolve-MacFile([string]$relative) {
    $path = [IO.Path]::GetFullPath((Join-Path $macRoot $relative))
    if (-not $path.StartsWith($macRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Mac artifact is outside Builds.' }
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing Mac file: $relative" }
    $ancestor = $path
    while ($ancestor.Length -ge $macRoot.TrimEnd('\').Length) {
        if ((Get-Item -LiteralPath $ancestor).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Mac artifact must not traverse filesystem links.' }
        $ancestor = [IO.Path]::GetDirectoryName($ancestor)
    }
    return $path
}

function Read-MacUInt32BE($reader) {
    $bytes = $reader.ReadBytes(4)
    if ($bytes.Length -ne 4) { throw 'Truncated Mach-O header.' }
    return [uint32]([int64]$bytes[0] * 16777216 + [int64]$bytes[1] * 65536 + [int64]$bytes[2] * 256 + $bytes[3])
}

function Assert-UniversalMacBinary([string]$path) {
    $reader = [IO.BinaryReader]::new([IO.File]::OpenRead($path))
    try {
        $magic = Read-MacUInt32BE $reader
        if ($magic -ne 3405691582 -and $magic -ne 3405691583) { throw "Not a universal Mach-O binary: $path" }
        $count = Read-MacUInt32BE $reader
        if ($count -lt 2 -or $count -gt 32) { throw 'Invalid Mach-O architecture table.' }
        $cpus = @()
        for ($index = 0; $index -lt $count; $index++) {
            $cpus += Read-MacUInt32BE $reader
            $null = $reader.BaseStream.Seek($(if ($magic -eq 3405691582) { 16 } else { 28 }), [IO.SeekOrigin]::Current)
        }
        if ($cpus -notcontains 0x01000007 -or $cpus -notcontains 0x0100000C) { throw 'Intel and Apple Silicon must both be present.' }
    } finally { $reader.Dispose() }
}

$macArchive = Resolve-MacFile $macBuild.archive
$macExe = Resolve-MacFile $macBuild.executable
$parts = $macBuild.executable.Replace('\', '/').Split('/')
if ($parts.Length -ne 5 -or $parts[0] -notmatch ('\ATowerDefense-' + [regex]::Escape($macBuild.version) + '-macOS(-build[1-9][0-9]*)?\z') -or
    $parts[1] -ne 'TowerDefense.app' -or $parts[2] -ne 'Contents' -or $parts[3] -ne 'MacOS') { throw 'Unexpected Mac bundle executable path.' }
$macDirectory = Join-Path $macRoot $parts[0]
if ($macBuild.archive -ne ($parts[0] + '.zip')) { throw 'Mac ZIP and build directory do not match.' }
$plistPath = Resolve-MacFile ($parts[0] + '/TowerDefense.app/Contents/Info.plist')
$plist = [Xml.XmlDocument]::new()
$plist.XmlResolver = $null
$plist.Load($plistPath)
if ($plist.SelectSingleNode('/plist/dict/key[text()="CFBundleExecutable"]/following-sibling::string[1]').InnerText -ne $parts[4] -or
    $plist.SelectSingleNode('/plist/dict/key[text()="CFBundleShortVersionString"]/following-sibling::string[1]').InnerText -ne $macBuild.version) { throw 'Mac bundle name/version does not match its manifest.' }
$macNative = @($macExe)
$nativeNames = @('UnityPlayer.dylib', 'libsteam_api.dylib')
if ($macBuild.PSObject.Properties.Name -contains 'eosGuestConfigured' -and $macBuild.eosGuestConfigured) { $nativeNames += 'libEOSSDK-Mac-Shipping.dylib' }
foreach ($name in $nativeNames) {
    $matches = @(Get-ChildItem -LiteralPath (Join-Path $macDirectory 'TowerDefense.app') -Recurse -File -Filter $name)
    if ($matches.Count -ne 1) { throw "Expected exactly one $name in the Mac bundle." }
    $macNative += Resolve-MacFile ($parts[0] + '/' + $matches[0].FullName.Substring($macDirectory.Length + 1))
}
foreach ($binary in $macNative) { Assert-UniversalMacBinary $binary }

$macZip = [IO.Compression.ZipFile]::OpenRead($macArchive)
try {
    foreach ($required in @('build-info.json', 'MAC_TESTING.md', 'PRIVATE-STEAM-TEST.txt', 'Start-Private-Steam-Test.command')) {
        if (-not $macZip.GetEntry($required)) { throw "Missing packaged $required" }
    }
    $infoReader = [IO.StreamReader]::new($macZip.GetEntry('build-info.json').Open())
    try { $packaged = $infoReader.ReadToEnd() | ConvertFrom-Json } finally { $infoReader.Dispose() }
    foreach ($field in @('version', 'platform', 'architecture', 'executable', 'archive', 'builtAtUtc', 'onlineProvider', 'steamAppId', 'privateSteamPlaytestAvailable')) {
        if ($packaged.$field -ne $macBuild.$field) { throw "Packaged Mac metadata mismatch: $field" }
    }
    foreach ($entry in $macZip.Entries) {
        if ($entry.FullName -match '(^|/)\.\.(/|$)|^/|\\|(^|/)steam_appid\.txt$|_BackUpThisFolder_ButDontShipItWithYourGame|(^|/)\.git(/|$)|(^|/)\.env') { throw 'Unsafe or excluded file in Mac ZIP.' }
        $local = Resolve-MacFile ($parts[0] + '/' + $entry.FullName)
        $sha = [Security.Cryptography.SHA256]::Create()
        $stream = $entry.Open()
        try { $zipHash = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-', '') } finally { $stream.Dispose(); $sha.Dispose() }
        if ($zipHash -ne (Get-FileHash -LiteralPath $local -Algorithm SHA256).Hash) { throw "ZIP differs from built Mac file: $($entry.FullName)" }
        if ($macNative -contains $local -or $entry.FullName.EndsWith('.command')) {
            $mode = ([int64]$entry.ExternalAttributes -shr 16) -band 65535
            if ($mode -ne 0x81ED) { throw 'Missing Mac executable permission in ZIP.' }
        }
    }
} finally { $macZip.Dispose() }

# Verify Unix creator metadata, which is necessary for macOS extractors to honor the modes.
$zipReader = [IO.BinaryReader]::new([IO.File]::OpenRead($macArchive))
try {
    $zipReader.BaseStream.Position = $zipReader.BaseStream.Length - 22
    if ($zipReader.ReadUInt32() -ne 0x06054B50) { throw 'Invalid Mac ZIP end record.' }
    $zipReader.BaseStream.Position = $zipReader.BaseStream.Length - 12
    $count = $zipReader.ReadUInt16()
    $centralSize = $zipReader.ReadUInt32()
    $offset = $zipReader.ReadUInt32()
    for ($index = 0; $index -lt $count; $index++) {
        $zipReader.BaseStream.Position = $offset
        if ($zipReader.ReadUInt32() -ne 0x02014B50) { throw 'Invalid Mac ZIP central directory.' }
        $null = $zipReader.ReadByte()
        if ($zipReader.ReadByte() -ne 3) { throw 'Mac ZIP must use a Unix creator OS.' }
        $zipReader.BaseStream.Position = $offset + 28
        $offset += 46 + $zipReader.ReadUInt16() + $zipReader.ReadUInt16() + $zipReader.ReadUInt16()
    }
} finally { $zipReader.Dispose() }
Write-Output "TD_MAC_PACKAGE_PASS v$($macBuild.version): Universal player and required native libraries (including EOS when configured), bundle metadata, every ZIP file hash and executable modes verified. Real Mac launch/security/input/Steam/EOS peer testing remains pending."
