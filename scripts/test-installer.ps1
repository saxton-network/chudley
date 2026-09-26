param([string] $ArtifactsDirectory = 'artifacts')

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$out = if ([IO.Path]::IsPathRooted($ArtifactsDirectory)) { $ArtifactsDirectory } else { Join-Path $root $ArtifactsDirectory }
$installer = Join-Path $out 'Chudley-Codex-pet-installer.exe'
$portable = Join-Path $out 'Chudley-Codex-pet.zip'
$manifestPath = Join-Path $out 'payload-sha256.json'
$codexHome = if ($env:CODEX_HOME) { $env:CODEX_HOME } else { Join-Path $env:USERPROFILE '.codex' }
$petDir = Join-Path $codexHome 'pets/chudley-v2'
$appDir = Join-Path $env:LOCALAPPDATA 'Programs/Chudley Codex Pet'
$uninstallRoot = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall'

foreach ($file in @($installer, $portable, $manifestPath)) {
    if (!(Test-Path -LiteralPath $file -PathType Leaf)) { throw "Missing test input: $file" }
}
if (Test-Path -LiteralPath $petDir) { throw "Existing Codex pet must be preserved: $petDir" }
if (Test-Path -LiteralPath $appDir) { throw "Existing installer metadata must be preserved: $appDir" }
$existing = @(Get-ItemProperty "$uninstallRoot\*" -ErrorAction SilentlyContinue | Where-Object { $_.DisplayName -eq 'Chudley for Codex' })
if ($existing.Count -gt 0) { throw 'An existing Chudley for Codex uninstall registration must be preserved' }

$manifest = @(Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json)
$expectedPaths = @('chudley-v2/pet.json', 'chudley-v2/spritesheet.webp', 'LICENSE', 'THIRD_PARTY_NOTICES.md')
if (@(Compare-Object ($manifest.path | Sort-Object) ($expectedPaths | Sort-Object)).Count -ne 0) {
    throw 'Payload manifest does not list exactly the Codex pet and attribution files'
}
Add-Type -AssemblyName System.IO.Compression
$zip = [IO.Compression.ZipFile]::OpenRead($portable)
try {
    $zipFiles = @($zip.Entries | Where-Object { $_.Name })
    if ($zipFiles.Count -ne $manifest.Count) { throw 'ZIP and payload manifest have different file counts' }
    foreach ($entry in $manifest) {
        $zipEntry = $zip.GetEntry($entry.path)
        if (!$zipEntry) { throw "Codex pet ZIP missing $($entry.path)" }
        $stream = $zipEntry.Open()
        try {
            $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
            if ($hash -ne $entry.sha256) { throw "Codex pet ZIP differs: $($entry.path)" }
        } finally { $stream.Dispose() }
    }
} finally { $zip.Dispose() }
Write-Host 'PASS ZIP contains only the verified pet files, license, and notices'

$installed = $false
try {
    $setup = Start-Process -FilePath $installer -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/SP-') -PassThru -Wait -WindowStyle Hidden
    if ($setup.ExitCode -ne 0) { throw "Installer exited with $($setup.ExitCode)" }
    $installed = $true
    $registration = @(Get-ItemProperty "$uninstallRoot\*" -ErrorAction SilentlyContinue | Where-Object { $_.DisplayName -eq 'Chudley for Codex' })
    if ($registration.Count -ne 1) { throw 'Expected one per-user Chudley for Codex uninstall registration' }
    foreach ($entry in $manifest) {
        $destination = if ($entry.path.StartsWith('chudley-v2/')) {
            Join-Path $petDir ([IO.Path]::GetFileName($entry.path))
        } else {
            Join-Path $appDir $entry.path
        }
        if (!(Test-Path -LiteralPath $destination -PathType Leaf)) { throw "Installed file missing: $($entry.path)" }
        if ((Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash -ne $entry.sha256) {
            throw "Installed file differs: $($entry.path)"
        }
    }
    $pet = Get-Content -LiteralPath (Join-Path $petDir 'pet.json') -Raw | ConvertFrom-Json
    if ($pet.id -ne 'chudley-v2' -or $pet.spriteVersionNumber -ne 2 -or $pet.spritesheetPath -ne 'spritesheet.webp') {
        throw 'Installed Codex pet metadata is invalid'
    }
    if (Get-ChildItem -LiteralPath $petDir -File | Where-Object Name -notin @('pet.json', 'spritesheet.webp')) {
        throw 'Codex pet directory contains unexpected files'
    }
    if (Test-Path -LiteralPath (Join-Path $petDir 'Chudley.Desktop.exe')) { throw 'Standalone WPF application was installed unexpectedly' }
    if (Test-Path -LiteralPath (Join-Path $petDir 'WearForbidden_video_1.mp4')) { throw 'Origin video was installed unexpectedly' }
    Write-Host "PASS Codex pet installed at $petDir with verified v2 metadata and sprite hashes"
} finally {
    if ($installed -and (Test-Path -LiteralPath (Join-Path $appDir 'unins000.exe') -PathType Leaf)) {
        $uninstall = Start-Process -FilePath (Join-Path $appDir 'unins000.exe') -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART') -PassThru -Wait -WindowStyle Hidden
        if ($uninstall.ExitCode -ne 0) { throw "Uninstaller exited with $($uninstall.ExitCode)" }
    }
}
if (Test-Path -LiteralPath (Join-Path $petDir 'pet.json')) { throw 'Uninstall left Codex pet metadata' }
if (Test-Path -LiteralPath (Join-Path $petDir 'spritesheet.webp')) { throw 'Uninstall left Codex pet artwork' }
if (Test-Path -LiteralPath $appDir) { throw 'Uninstall left installer metadata' }
$remaining = @(Get-ItemProperty "$uninstallRoot\*" -ErrorAction SilentlyContinue | Where-Object { $_.DisplayName -eq 'Chudley for Codex' })
if ($remaining.Count -ne 0) { throw 'Uninstall registration remains' }
Write-Host 'PASS uninstall removed only installed Chudley pet files and its metadata'
