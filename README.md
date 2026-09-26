# Chudley

<p align="center">
  <img src="assets/readme/chudley-banner.svg" alt="Chudley banner" width="100%">
</p>

<p align="center"><strong>🇺🇸 Freedom. 🇺🇸</strong></p>
<p align="center"><strong>🎆 Fireworks. 🎆</strong></p>
<p align="center"><strong>🤖 Codex. 🤖</strong></p>
<p align="center"><strong>🦅 Chudley. 🦅</strong></p>

## What Started It All

<p align="center">
  <strong>The clip responsible for this entire situation.</strong><br>
  Original viral clip by <a href="https://x.com/WearForbidden">@WearForbidden</a>
</p>

<!-- GitHub-hosted video attachment URL will be inserted here before merge. -->

**Status:** Windows MVP candidate. Chudley is a local desktop pet for Windows 11 x64. It displays the supplied pixel art in a transparent, always-on-top window. It idles, reacts to clicks, and occasionally plays snack, drink, wave, movement, and personality keyframe sequences. Drag him to a new position. The application makes no API calls and has no telemetry or background service.

## Run from source

Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and Python 3.12 or later. From the repository root on Windows:

```powershell
python tools/validate_assets.py
dotnet run --project tests/Chudley.Core.Tests/Chudley.Core.Tests.csproj --configuration Release
dotnet run --project src/Chudley.Desktop/Chudley.Desktop.csproj --configuration Release
```

The app requires no administrator privileges. If a required frame is missing, startup reports the asset error instead of substituting another pose.

## Controls

| Action | Result |
| --- | --- |
| Left click | Short attention/click reaction |
| Left drag | Reposition without click reaction |
| Right click or tray icon | Open controls |
| Pause / Resume | Stop or continue animation |
| Scale | Choose crisp 1×, 2×, 3×, or 4× pixels |
| Reset Position | Move to a visible default location |
| Quit | Exit cleanly |

Only one instance can run. The chosen scale and last valid position are stored in the current user's local application data, outside the installation directory. A saved position that is no longer visible on any monitor is reset on startup.

## Package for Windows

From the repository root:

```powershell
./scripts/publish-windows.ps1
```

This validates the assets, runs the core tests, and publishes a self-contained `win-x64` folder under `artifacts/chudley-win-x64/`. Copy the complete folder to a Windows 11 x64 machine and launch `Chudley.Desktop.exe`; no separate .NET runtime installation is needed. The folder includes `assets/sprites/candidate/manifest.json` and all 24 frame PNGs. There is no installer or automatic release in this MVP.

For individual checks:

```powershell
dotnet build src/Chudley.Desktop/Chudley.Desktop.csproj --configuration Release --warnaserror
dotnet publish src/Chudley.Desktop/Chudley.Desktop.csproj --configuration Release --runtime win-x64 --self-contained true --output artifacts/chudley-win-x64
```

GitHub Actions builds, tests, validates assets, and checks the published frame bundle on a Windows runner.

## Artwork and extension

The [original character reference](assets/reference/original-character.png) is separate from the [runtime candidate](assets/sprites/candidate/). The candidate contains a 512×768 atlas and 24 individual 128×128 RGBA keyframes. `manifest.json` provides the frame names, grid coordinates, and source provenance. The first 20 frames are `approved_master`; the repaired last row is `repaired_bottom_strip`. Source generations and the supplied atlas builder are retained under `assets/source/generated/` and `tools/` for provenance. Imported PNG bytes are unchanged.

The runtime uses individual frames. Animation sequences and timing are defined in the core library; new artwork with the same frame names can be swapped without editing window code. A small event boundary is reserved for future Codex state signals, with no network or process monitoring attached.

These are keyframes rather than finished inbetween animation sets. Face, hands, scooter, and prop continuity need later artist review. The bottom row was repaired after the earlier master clipped the wheels. Movement is deliberately conservative; directional sprite sets would support more convincing desktop travel. See [art pipeline](docs/ART-PIPELINE.md) and [desktop stack decision](docs/ADR-001-DESKTOP-STACK.md).

## License and disclaimer

No open-source license has been selected yet. Until one is added, normal copyright restrictions apply.

This is a parody/experimental software project. It is not affiliated with or endorsed by OpenAI, Anthropic, any political campaign, or the brands depicted in the artwork.
