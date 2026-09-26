param([string] $OutputDirectory = 'artifacts')

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$out = if ([IO.Path]::IsPathRooted($OutputDirectory)) { $OutputDirectory } else { Join-Path $root $OutputDirectory }
$stamp = [Guid]::NewGuid().ToString('N')
$publish = Join-Path $out "staging-$stamp"
$archive = Join-Path $out 'Chudley-win-x64-portable.zip'
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
    foreach ($relative in @('Chudley.Desktop.exe', 'hostfxr.dll', 'hostpolicy.dll', 'assets/runtime/pet.json', 'assets/runtime/spritesheet.png', 'assets/runtime/spritesheet.webp')) {
        if (!(Test-Path -LiteralPath (Join-Path $publish $relative) -PathType Leaf)) { throw "Published file missing: $relative" }
    }
    & "$PSScriptRoot/validate-assets.ps1" -AssetsDirectory (Join-Path $publish 'assets/runtime')
    if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) { throw 'Published asset validation failed' }
    Compress-Archive -Path (Join-Path $publish '*') -DestinationPath $archive -CompressionLevel Optimal -Force
    if (!(Test-Path -LiteralPath $archive -PathType Leaf)) { throw 'Portable archive was not created' }
    Write-Host "Portable package: $archive"
    Write-Host "Published folder: $publish"
} finally {
    Pop-Location
}
