param()
$ErrorActionPreference = 'Stop'
$repository = Split-Path $PSScriptRoot -Parent
$dependencyDirectory = Join-Path $repository '.local'
$package = Join-Path $dependencyDirectory 'com.playeveryware.eos-6.2.0.tgz'
$expectedHash = 'aafe5a1cc278f2f65e0373777706d0a1028eea49b9548ef4a7520cc50f647c4b'
New-Item -ItemType Directory -Path $dependencyDirectory -Force | Out-Null
if (Test-Path -LiteralPath $package) {
    if ((Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash.ToLowerInvariant() -ne $expectedHash) {
        throw 'Existing EOS archive does not match the pinned SDK. It was not replaced.'
    }
}
$temporaryPackage = Join-Path $dependencyDirectory ('eos-download-' + [guid]::NewGuid().ToString('N') + '.tmp')
if (-not (Test-Path -LiteralPath $package)) { try {
    Invoke-WebRequest -UseBasicParsing -Uri 'https://github.com/EOS-Contrib/eos_plugin_for_unity/releases/download/v6.2.0/com.playeveryware.eos-6.2.0.tgz' -OutFile $temporaryPackage
    if ((Get-FileHash -LiteralPath $temporaryPackage -Algorithm SHA256).Hash.ToLowerInvariant() -ne $expectedHash) { throw 'EOS SDK download checksum failed.' }
    Move-Item -LiteralPath $temporaryPackage -Destination $package
    Write-Output 'TD_EOS_SDK_READY version=6.2.0 hash=verified'
} finally {
    if (Test-Path -LiteralPath $temporaryPackage) { Remove-Item -LiteralPath $temporaryPackage }
} }
# Extract only the official C# SDK and native binaries into a minimal UPM package.
# Full-plugin sample build tooling/overlay/integrated-Steam are deliberately excluded.
$extraction = Join-Path $dependencyDirectory 'eos-sdk-6.2.0'
if (-not (Test-Path -LiteralPath (Join-Path $extraction 'package/package.json'))) {
    New-Item -ItemType Directory -Path $extraction -Force | Out-Null
    & tar -xf $package -C $extraction
    if ($LASTEXITCODE -ne 0) { throw 'EOS SDK extraction failed.' }
}
$sdkRoot = Join-Path $dependencyDirectory 'td-eos-sdk-6.2.0'
foreach ($directory in @($dependencyDirectory, $extraction, $sdkRoot)) {
    if ((Test-Path -LiteralPath $directory) -and ((Get-Item -LiteralPath $directory).Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw 'EOS installation must not follow filesystem links.' }
}
if (-not (Test-Path -LiteralPath (Join-Path $sdkRoot 'Runtime/EOS_SDK/com.Epic.OnlineServices.asmdef'))) {
    New-Item -ItemType Directory -Path (Join-Path $sdkRoot 'Runtime/Plugins') -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $extraction 'package/Runtime/EOS_SDK') -Destination (Join-Path $sdkRoot 'Runtime') -Recurse
    foreach ($native in @('Windows/x64/EOSSDK-Win64-Shipping.dll', 'macOS/libEOSSDK-Mac-Shipping.dylib')) {
        Copy-Item -LiteralPath (Join-Path $extraction ('package/Runtime/' + $native)) -Destination (Join-Path $sdkRoot 'Runtime/Plugins')
        Copy-Item -LiteralPath (Join-Path $extraction ('package/Runtime/' + $native + '.meta')) -Destination (Join-Path $sdkRoot 'Runtime/Plugins')
    }
    Copy-Item -LiteralPath (Join-Path $extraction 'package/LICENSE.md') -Destination $sdkRoot
    Copy-Item -LiteralPath (Join-Path $extraction 'package/Runtime/macOS/license.txt') -Destination (Join-Path $sdkRoot 'EOS-NATIVE-LICENSE.txt')
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'EosSdkCompatibility/package.json') -Destination $sdkRoot
Write-Output 'TD_EOS_SDK_READY plugin-source=6.2.0 hash=verified SDK-only no-sample-build-tools'
