param(
    [string] $OutputDirectory = 'artifacts',
    [switch] $BuildInstaller,
    [string] $InnoCompiler
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$out = if ([IO.Path]::IsPathRooted($OutputDirectory)) { $OutputDirectory } else { Join-Path $root $OutputDirectory }
$payload = Join-Path $out ('codex-pet-payload-' + [Guid]::NewGuid().ToString('N'))
$petPayload = Join-Path $payload 'chudley-v2'
$archive = Join-Path $out 'Chudley-Codex-pet.zip'
$installer = Join-Path $out 'Chudley-Codex-pet-installer.exe'

& "$PSScriptRoot/validate-assets.ps1"
if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) { throw 'Asset validation failed' }
New-Item -ItemType Directory -Path $petPayload -Force | Out-Null
foreach ($name in @('pet.json', 'spritesheet.webp')) {
    Copy-Item -LiteralPath (Join-Path $root "assets/runtime/$name") -Destination (Join-Path $petPayload $name)
}
foreach ($name in @('LICENSE', 'THIRD_PARTY_NOTICES.md')) {
    Copy-Item -LiteralPath (Join-Path $root $name) -Destination (Join-Path $payload $name)
}

$expectedFiles = @('chudley-v2/pet.json', 'chudley-v2/spritesheet.webp', 'LICENSE', 'THIRD_PARTY_NOTICES.md')
$actualFiles = @(Get-ChildItem -LiteralPath $payload -File -Recurse | ForEach-Object {
    [IO.Path]::GetRelativePath($payload, $_.FullName).Replace('\', '/')
} | Sort-Object)
if (@(Compare-Object ($expectedFiles | Sort-Object) $actualFiles).Count -ne 0) {
    throw "Unexpected Codex pet payload: $($actualFiles -join ', ')"
}
foreach ($name in @('pet.json', 'spritesheet.webp')) {
    $sourceHash = (Get-FileHash -LiteralPath (Join-Path $root "assets/runtime/$name") -Algorithm SHA256).Hash
    $stagedHash = (Get-FileHash -LiteralPath (Join-Path $petPayload $name) -Algorithm SHA256).Hash
    if ($sourceHash -ne $stagedHash) { throw "Staged pet file differs: $name" }
}

Compress-Archive -Path (Join-Path $payload '*') -DestinationPath $archive -CompressionLevel Optimal -Force
if (!(Test-Path -LiteralPath $archive -PathType Leaf)) { throw 'Codex pet ZIP was not created' }
$manifest = Get-ChildItem -LiteralPath $payload -File -Recurse | ForEach-Object {
    [pscustomobject]@{
        path = [IO.Path]::GetRelativePath($payload, $_.FullName).Replace('\', '/')
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
    & $InnoCompiler "/DPayloadDir=$payload" "/O$out" (Join-Path $root 'installer/chudley.iss')
    if ($LASTEXITCODE -ne 0) { throw 'Codex pet installer compilation failed' }
    if (!(Test-Path -LiteralPath $installer -PathType Leaf)) { throw 'Codex pet installer artifact is missing' }
    Write-Host "Codex pet installer: $installer"
}
Write-Host "Codex pet ZIP: $archive"
Write-Host "Validated payload: $payload"
