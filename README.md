# Chudley

<p align="center">
  <img src="assets/readme/chudley-banner.svg" alt="Chudley banner" width="100%">
</p>

<p align="center"><strong>🇺🇸 Freedom. 🇺🇸</strong></p>
<p align="center"><strong>🎆 Fireworks. 🎆</strong></p>
<p align="center"><strong>🤖 Codex. 🤖</strong></p>
<p align="center"><strong>🦅 Chudley. 🦅</strong></p>

## What Started It All

[Watch the original Chudley clip](WearForbidden_video_1.mp4). The clip responsible for this entire situation. Creator: [@WearForbidden](https://x.com/WearForbidden).

## Get Chudley

Chudley is a local desktop pet for **Windows 11 x64**. He lives in a transparent, always-on-top window, animates from the verified Codex v2 art, reacts to clicks, and can be dragged around the desktop. There is no account, network access, telemetry, or background service.

Download **Chudley-win-x64-portable.zip** from the [latest GitHub release](https://github.com/saxton-network/chudley/releases/latest), extract the entire ZIP to a folder you control, and run `Chudley.Desktop.exe`. The package is portable and self-contained; no .NET runtime, SDK, Python, Git, Visual Studio, or administrator access is required. There is no installer yet. Until a release asset is published, the [Actions build artifacts](https://github.com/saxton-network/chudley/actions/workflows/ci.yml) provide review builds; GitHub may require sign-in to download those artifacts.

To remove Chudley, quit him from the tray or right-click menu and delete the extracted folder. Optional saved settings live at `%LOCALAPPDATA%\Chudley\settings.json`; delete that file to forget his position, scale, and pause state.

### Controls

| Action | Result |
| --- | --- |
| Left click | Wave reaction |
| Left drag | Move Chudley without triggering a click; his verified mirrored directional animation follows the drag |
| Right click or tray icon | Open controls |
| Pause / Resume | Stop or continue animation |
| Scale | Select 1×, 2×, 3×, or 4× nearest-neighbor rendering |
| Reset Position | Return to a visible default location |
| Quit | Exit cleanly |

Only one instance runs at a time. A saved position outside the connected screens recovers to a visible location. While idle, Chudley plays a restrained loop and occasionally performs one of the supplied waiting, jump, review, running, or failed sequences. Dragging can prompt a constitutionally protected directional adjustment. The standalone app does not yet receive live Codex task events; the event boundary is reserved for later integration.

## For developers

Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) on Windows 11 x64, then clone this repository. From its root:

```powershell
dotnet restore src/Chudley.Desktop/Chudley.Desktop.csproj
./scripts/validate-assets.ps1
dotnet build src/Chudley.Desktop/Chudley.Desktop.csproj --configuration Release --warnaserror
dotnet run --project tests/Chudley.Core.Tests/Chudley.Core.Tests.csproj --configuration Release
dotnet run --project src/Chudley.Desktop/Chudley.Desktop.csproj --configuration Release
```

To produce the end-user package in one command:

```powershell
./scripts/publish-windows.ps1
```

The script restores, validates assets, builds, tests, publishes a self-contained `win-x64` application, checks its runtime files, and creates `artifacts/Chudley-win-x64-portable.zip`. GitHub Actions runs the same script for pushes and pull requests. Publishing a future versioned GitHub Release triggers the release workflow to attach this ZIP; this repository does not automatically create a release.

`src/Chudley.Core` contains the deterministic animation engine, atlas coordinates, settings, and monitor positioning. `src/Chudley.Desktop` contains the WPF window and tray shell. `assets/runtime` contains the verified v2 `pet.json`, lossless WebP sheet for Codex compatibility, and pixel-identical PNG sheet for WPF rendering. The 8×11 atlas uses 192×208 cells. `scripts/validate-assets.ps1` checks the approved hashes, metadata, and dimensions so missing or altered artwork fails clearly. The character reference remains under `assets/reference`.

The portable package is generated output and is not committed. There is currently no installer or clean-machine launch automation. A human should check the ZIP on a fresh Windows 11 x64 user profile before release. Source code has no outbound network dependency.

## License and disclaimer

No open-source license has been selected yet. Until one is added, normal copyright restrictions apply.

This is a parody/experimental software project. It is not affiliated with or endorsed by OpenAI, Anthropic, any political campaign, or the brands depicted in the artwork.
