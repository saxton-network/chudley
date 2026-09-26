param(
    [string] $OutputDirectory = 'artifacts/chudley-win-x64'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$out = Join-Path $root $OutputDirectory

Push-Location $root
try {
    python tools/validate_assets.py
    if ($LASTEXITCODE -ne 0) { throw 'Asset validation failed' }
    dotnet run --project tests/Chudley.Core.Tests/Chudley.Core.Tests.csproj --configuration Release
    if ($LASTEXITCODE -ne 0) { throw 'Core tests failed' }
    dotnet publish src/Chudley.Desktop/Chudley.Desktop.csproj --configuration Release --runtime win-x64 --self-contained true --output $out
    if ($LASTEXITCODE -ne 0) { throw 'Publish failed' }
    if (!(Test-Path (Join-Path $out 'assets/sprites/candidate/manifest.json'))) { throw 'Published manifest is missing' }
    $frames = @(Get-ChildItem (Join-Path $out 'assets/sprites/candidate/frames') -Filter *.png)
    if ($frames.Count -ne 24) { throw "Expected 24 published frames; found $($frames.Count)" }
    Write-Host "Published to $out"
} finally {
    Pop-Location
}
