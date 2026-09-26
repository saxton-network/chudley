param([string] $AssetsDirectory = 'assets/runtime')

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$assets = if ([IO.Path]::IsPathRooted($AssetsDirectory)) { $AssetsDirectory } else { Join-Path $root $AssetsDirectory }
$png = Join-Path $assets 'spritesheet.png'
$webp = Join-Path $assets 'spritesheet.webp'
$metadata = Join-Path $assets 'pet.json'
foreach ($file in @($png, $webp, $metadata)) {
    if (!(Test-Path -LiteralPath $file -PathType Leaf)) { throw "Required asset missing: $file" }
}
$pet = Get-Content -LiteralPath $metadata -Raw | ConvertFrom-Json
if ($pet.id -ne 'chudley-v2' -or $pet.spriteVersionNumber -ne 2 -or $pet.spritesheetPath -ne 'spritesheet.webp') {
    throw 'Invalid Codex pet metadata'
}
$expected = @{
    'spritesheet.png' = 'EFF5A9E1CC19A844E22D540D77417D60566B88F4888A1F90ADDE690D906CC5A2'
    'spritesheet.webp' = 'BDDBE9D941A6E98174C5EA74CB694E4CFEA6654593C194ED310285822D884C5E'
}
foreach ($name in $expected.Keys) {
    $actual = (Get-FileHash -LiteralPath (Join-Path $assets $name) -Algorithm SHA256).Hash
    if ($actual -ne $expected[$name]) { throw "$name does not match the verified working asset" }
}
$bytes = [IO.File]::ReadAllBytes($png)
if ($bytes.Length -lt 24 -or [Text.Encoding]::ASCII.GetString($bytes, 1, 3) -ne 'PNG') { throw 'Invalid PNG header' }
[array]::Reverse($bytes, 16, 4)
[array]::Reverse($bytes, 20, 4)
$width = [BitConverter]::ToInt32($bytes, 16)
$height = [BitConverter]::ToInt32($bytes, 20)
if ($width -ne 1536 -or $height -ne 2288) { throw "Unexpected atlas dimensions: ${width}x${height}" }
Write-Host "Verified Chudley v2 assets: 8x11 cells, 192x208 each, ${width}x${height} atlas, approved PNG/WebP hashes"
