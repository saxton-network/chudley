param(
    [string] $ArtifactsDirectory = 'artifacts',
    [switch] $RequireVisibleWindow
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$out = if ([IO.Path]::IsPathRooted($ArtifactsDirectory)) { $ArtifactsDirectory } else { Join-Path $root $ArtifactsDirectory }
$installer = Join-Path $out 'Chudley-win-x64-installer.exe'
$portable = Join-Path $out 'Chudley-win-x64-portable.zip'
$manifestPath = Join-Path $out 'payload-sha256.json'
$installDir = Join-Path $env:LOCALAPPDATA 'Programs/Chudley'
$shortcutPath = Join-Path $env:APPDATA 'Microsoft/Windows/Start Menu/Programs/Chudley.lnk'
$settingsPath = Join-Path $env:LOCALAPPDATA 'Chudley/settings.json'
$uninstallRoot = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall'

foreach ($file in @($installer, $portable, $manifestPath)) {
    if (!(Test-Path -LiteralPath $file -PathType Leaf)) { throw "Missing test input: $file" }
}
if (Test-Path -LiteralPath $installDir) { throw "Existing Chudley install must be handled separately: $installDir" }
if (Test-Path -LiteralPath $shortcutPath) { throw "Existing Chudley shortcut must be preserved: $shortcutPath" }
if (Get-Process -Name 'Chudley.Desktop' -ErrorAction SilentlyContinue) { throw 'A Chudley desktop process is already running' }
$existing = @(Get-ItemProperty "$uninstallRoot\*" -ErrorAction SilentlyContinue | Where-Object { $_.DisplayName -eq 'Chudley' })
if ($existing.Count -gt 0) { throw 'An existing Chudley uninstall registration must be preserved' }

Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class ChudleyWindowApi {
    public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);
    [DllImport("user32.dll")] public static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint processId);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);
}
'@

function Get-ChudleyWindow([int] $ProcessId) {
    $script:FoundChudleyWindow = [IntPtr]::Zero
    $callback = [ChudleyWindowApi+EnumWindowsProc] {
        param([IntPtr] $hwnd, [IntPtr] $unused)
        [uint32] $owner = 0
        [void][ChudleyWindowApi]::GetWindowThreadProcessId($hwnd, [ref] $owner)
        if ($owner -eq $ProcessId -and [ChudleyWindowApi]::IsWindowVisible($hwnd)) {
            $script:FoundChudleyWindow = $hwnd
            return $false
        }
        return $true
    }
    [void][ChudleyWindowApi]::EnumWindows($callback, [IntPtr]::Zero)
    return $script:FoundChudleyWindow
}

function Stop-TestPet($process) {
    if (!$process) { return $false }
    $process.Refresh()
    if ($process.HasExited) { return $true }
    $hwnd = Get-ChudleyWindow $process.Id
    if ($hwnd -ne [IntPtr]::Zero) {
        [void][ChudleyWindowApi]::PostMessage($hwnd, 0x0010, [IntPtr]::Zero, [IntPtr]::Zero)
        if ($process.WaitForExit(8000)) { return $true }
    }
    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    return $false
}

$manifest = @(Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json)
$settingsExisted = Test-Path -LiteralPath $settingsPath -PathType Leaf
$installed = $false
$first = $null
$second = $null
try {
    $setup = Start-Process -FilePath $installer -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/SP-') -PassThru -Wait -WindowStyle Hidden
    if ($setup.ExitCode -ne 0) { throw "Installer exited with $($setup.ExitCode)" }
    $installed = $true
    $registration = @(Get-ItemProperty "$uninstallRoot\*" -ErrorAction SilentlyContinue | Where-Object { $_.DisplayName -eq 'Chudley' })
    if ($registration.Count -ne 1) { throw 'Expected one per-user uninstall registration' }
    $exe = Join-Path $installDir 'Chudley.Desktop.exe'
    foreach ($required in @('Chudley.Desktop.exe', 'hostfxr.dll', 'hostpolicy.dll', 'assets/runtime/pet.json', 'assets/runtime/spritesheet.png', 'assets/runtime/spritesheet.webp', 'LICENSE', 'THIRD_PARTY_NOTICES.md')) {
        if (!(Test-Path -LiteralPath (Join-Path $installDir $required) -PathType Leaf)) { throw "Installed file missing: $required" }
    }
    if (Test-Path -LiteralPath (Join-Path $installDir 'WearForbidden_video_1.mp4')) { throw 'Origin video was installed unexpectedly' }
    & "$PSScriptRoot/validate-assets.ps1" -AssetsDirectory (Join-Path $installDir 'assets/runtime')
    foreach ($entry in $manifest) {
        $file = Join-Path $installDir $entry.path
        if (!(Test-Path -LiteralPath $file -PathType Leaf)) { throw "Installed payload file missing: $($entry.path)" }
        if ((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne $entry.sha256) { throw "Installed payload differs: $($entry.path)" }
    }
    if (!(Test-Path -LiteralPath $shortcutPath -PathType Leaf)) { throw 'Start Menu shortcut is missing' }
    $shortcut = (New-Object -ComObject WScript.Shell).CreateShortcut($shortcutPath)
    if ($shortcut.TargetPath -ne $exe) { throw "Start Menu shortcut targets $($shortcut.TargetPath)" }

    Add-Type -AssemblyName System.IO.Compression
    $zip = [IO.Compression.ZipFile]::OpenRead($portable)
    try {
        $zipFiles = @($zip.Entries | Where-Object { $_.Name })
        if ($zipFiles.Count -ne $manifest.Count) { throw 'Portable ZIP and installed payload have different file counts' }
        foreach ($entry in $manifest) {
            $zipEntry = $zip.GetEntry($entry.path)
            if (!$zipEntry) { throw "Portable ZIP missing $($entry.path)" }
            $stream = $zipEntry.Open()
            try {
                $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
                if ($hash -ne $entry.sha256) { throw "Portable ZIP differs: $($entry.path)" }
            } finally { $stream.Dispose() }
        }
    } finally { $zip.Dispose() }
    Write-Host "PASS installed and portable payload parity ($($manifest.Count) files)"

    $first = Start-Process -FilePath $exe -WorkingDirectory $env:TEMP -PassThru
    Start-Sleep -Seconds 5
    $first.Refresh()
    if ($first.HasExited) { throw "Installed pet exited immediately: $($first.ExitCode)" }
    $window = Get-ChudleyWindow $first.Id
    if ($RequireVisibleWindow -and $window -eq [IntPtr]::Zero) { throw 'Installed pet did not create a visible window' }
    Write-Host "PASS installed pet startup; visible window=$($window -ne [IntPtr]::Zero)"

    $duplicate = Start-Process -FilePath $exe -WorkingDirectory $env:TEMP -PassThru
    if (!$duplicate.WaitForExit(5000)) {
        Stop-Process -Id $duplicate.Id -Force
        throw 'Second Chudley instance did not exit'
    }
    $first.Refresh()
    if ($first.HasExited) { throw 'Primary Chudley instance exited during single-instance test' }
    Write-Host 'PASS single-instance behavior'

    $firstClosedNormally = Stop-TestPet $first
    $first = $null
    if ($RequireVisibleWindow -and !$firstClosedNormally) { throw 'Could not close installed pet through its window' }
    Write-Host "First quit through window=$firstClosedNormally"
    $second = Start-Process -FilePath $shortcutPath -PassThru
    if (!$second) { throw 'Start Menu shortcut did not return the launched Chudley process' }
    Start-Sleep -Seconds 4
    $second.Refresh()
    if ($second.HasExited) { throw 'Installed pet failed to relaunch' }
    $secondClosedNormally = Stop-TestPet $second
    $second = $null
    if ($RequireVisibleWindow -and !$secondClosedNormally) { throw 'Could not close relaunched pet through its window' }
    Write-Host "PASS installed pet relaunch; second quit through window=$secondClosedNormally"
} finally {
    [void](Stop-TestPet $first)
    [void](Stop-TestPet $second)
    if ($installed -and (Test-Path -LiteralPath (Join-Path $installDir 'unins000.exe') -PathType Leaf)) {
        $uninstaller = Join-Path $installDir 'unins000.exe'
        $uninstall = Start-Process -FilePath $uninstaller -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART') -PassThru -Wait -WindowStyle Hidden
        if ($uninstall.ExitCode -ne 0) { throw "Uninstaller exited with $($uninstall.ExitCode)" }
    }
}
if (Test-Path -LiteralPath $installDir) { throw 'Uninstall left the installed program directory' }
if (Test-Path -LiteralPath $shortcutPath) { throw 'Uninstall left the Start Menu shortcut' }
$remaining = @(Get-ItemProperty "$uninstallRoot\*" -ErrorAction SilentlyContinue | Where-Object { $_.DisplayName -eq 'Chudley' })
if ($remaining.Count -ne 0) { throw 'Uninstall registration remains' }
if ($settingsExisted -and !(Test-Path -LiteralPath $settingsPath -PathType Leaf)) { throw 'Uninstall deleted pre-existing settings' }
Write-Host 'PASS uninstall removed program files, shortcut, and registration while preserving user settings'
