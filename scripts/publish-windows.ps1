param(
    [string] $OutputDirectory = 'artifacts',
    [switch] $BuildInstaller,
    [string] $InnoCompiler
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$out = if ([IO.Path]::IsPathRooted($OutputDirectory)) { $OutputDirectory } else { Join-Path $root $OutputDirectory }
$stamp = [Guid]::NewGuid().ToString('N')
$publish = Join-Path $out "staging-$stamp"
$archive = Join-Path $out 'Chudley-win-x64-portable.zip'
$installer = Join-Path $out 'Chudley-win-x64-installer.exe'
New-Item -ItemType Directory -Force $out | Out-Null
Push-Location $root
try {
    & "$PSScriptRoot/validate-assets.ps1"
    if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) { throw 'Asset validation failed' }
    dotnet restore src/Chudley.Desktop/Chudley.Desktop.csproj
    if ($LASTEXITCODE -ne 0) { throw 'Dependency restore failed' }
    dotnet build src/Chudley.Desktop/Chudley.Desktop.csproj --configuration Release --no-restore --warnaserror
    if ($LASTEXITCODE -ne 0) { throw 'Desktop build failed' }
    dotnet run --project tests/Chudley.Core.Tests/Chudley.Core.Tests.csproj --configuration Release
    if ($LASTEXITCODE -ne 0) { throw 'Core tests failed' }
    dotnet publish src/Chudley.Desktop/Chudley.Desktop.csproj --configuration Release --runtime win-x64 --self-contained true --output $publish
    if ($LASTEXITCODE -ne 0) { throw 'Self-contained publish failed' }
    Copy-Item -LiteralPath (Join-Path $root 'LICENSE') -Destination (Join-Path $publish 'LICENSE')
    Copy-Item -LiteralPath (Join-Path $root 'THIRD_PARTY_NOTICES.md') -Destination (Join-Path $publish 'THIRD_PARTY_NOTICES.md')
    foreach ($relative in @('Chudley.Desktop.exe', 'hostfxr.dll', 'hostpolicy.dll', 'assets/runtime/pet.json', 'assets/runtime/spritesheet.png', 'assets/runtime/spritesheet.webp', 'LICENSE', 'THIRD_PARTY_NOTICES.md')) {
        if (!(Test-Path -LiteralPath (Join-Path $publish $relative) -PathType Leaf)) { throw "Published file missing: $relative" }
    }
    if (Test-Path -LiteralPath (Join-Path $publish 'WearForbidden_video_1.mp4')) { throw 'Origin video must not be in the runtime payload' }
    & "$PSScriptRoot/validate-assets.ps1" -AssetsDirectory (Join-Path $publish 'assets/runtime')
    if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) { throw 'Published asset validation failed' }
    Compress-Archive -Path (Join-Path $publish '*') -DestinationPath $archive -CompressionLevel Optimal -Force
    if (!(Test-Path -LiteralPath $archive -PathType Leaf)) { throw 'Portable archive was not created' }
    $manifest = Get-ChildItem -LiteralPath $publish -File -Recurse | ForEach-Object {
        [pscustomobject]@{
            path = [IO.Path]::GetRelativePath($publish, $_.FullName).Replace('\', '/')
            sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
        }
    }
    $manifest | Sort-Object path | ConvertTo-Json -Depth 3 | Set-Content -LiteralPath (Join-Path $out 'payload-sha256.json') -Encoding utf8
    if ($BuildInstaller) {
        if (!$InnoCompiler) {
            $candidates = @(
                (Join-Path $root '.tools/inno/ISCC.exe'),
                'C:\Program Files (x86)\Inno Setup 6\ISCC.exe',
                'C:\Program Files\Inno Setup 6\ISCC.exe'
            )
            $InnoCompiler = $candidates | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
        }
        if (!$InnoCompiler -or !(Test-Path -LiteralPath $InnoCompiler -PathType Leaf)) {
            throw 'Inno Setup 6 compiler (ISCC.exe) is required. Pass -InnoCompiler or install Inno Setup 6.'
        }
        & $InnoCompiler "/DPayloadDir=$publish" "/O$out" (Join-Path $root 'installer/chudley.iss')
        if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed' }
        if (!(Test-Path -LiteralPath $installer -PathType Leaf)) { throw 'Installer artifact is missing' }
        Write-Host "Installer package: $installer"
    }
    Write-Host "Portable package: $archive"
    Write-Host "Published folder: $publish"
} finally {
    Pop-Location
}
