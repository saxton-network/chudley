# Chudley

<p align="center">
  <img src="assets/readme/chudley-banner.svg" alt="Chudley banner" width="100%">
</p>

<p align="center"><strong>🇺🇸 Freedom. 🇺🇸</strong></p>
<p align="center"><strong>🎆 Fireworks. 🎆</strong></p>
<p align="center"><strong>🤖 Codex. 🤖</strong></p>
<p align="center"><strong>🦅 Chudley. 🦅</strong></p>

<p align="center">
  <img alt="Windows 11 x64" src="https://img.shields.io/badge/Windows-11%20x64-0078D4?logo=windows11&logoColor=white">
  <img alt=".NET 8" src="https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet&logoColor=white">
  <img alt="License: PolyForm Strict 1.0.0" src="https://img.shields.io/badge/license-PolyForm%20Strict%201.0.0-goldenrod">
</p>

<p align="center"><em>One meme. One scooter. An unreasonable amount of CI.</em></p>

## 🎆 What Started It All

**One 38-second video. One catastrophically disproportionate software response.**

▶️ **[Watch the original viral clip](WearForbidden_video_1.mp4)**

The viral AI clip that kicked this whole thing off was posted by **[@WearForbidden](https://x.com/WearForbidden)**. The broader character and mobility-scooter meme lineage predates this repository; the attribution trail and third-party rights notes live in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

## 🦅 What Is Chudley?

Chudley is a local desktop Codex pet for **Windows 11 x64**. He occupies a transparent, always-on-top window, animates from the verified Codex v2 art, reacts to clicks, remembers where you abandoned him, and can be dragged around the desktop like a tiny federally funded nuisance.

Chudley does **not** solve a business problem. Chudley is what happens when a 38-second meme receives a .NET build pipeline and nobody intervenes.

### ⭐ Current capabilities

- 🪟 Transparent, frameless, always-on-top Windows pet
- 🎨 Crisp nearest-neighbor pixel rendering at 1×, 2×, 3×, or 4×
- 🖱️ Click reactions and drag-to-reposition behavior
- 🛴 Verified mirrored directional animation during movement
- 💤 Idle loops plus waiting, jump, review, running, and failed sequences
- 💾 Persistent position, scale, and pause state
- 🖥️ Recovery when a saved position is no longer visible on connected monitors
- 🚫 No account
- 📡 No network access
- 👁️ No telemetry
- 🛑 No background service
- 🧍 One Chudley at a time, because the republic has limits

The standalone app does **not yet** receive live Codex task events. The event boundary exists for later integration; no process monitoring or network integration is attached to it today.

## 🇺🇸 Acquire Chudley

### 🇺🇸 Recommended: Installer

1. Open the [latest GitHub release](https://github.com/saxton-network/chudley/releases/latest).
2. Download **`Chudley-win-x64-installer.exe`**.
3. Run it and complete the wizard. Chudley installs per user under `%LOCALAPPDATA%\Programs\Chudley`, so administrator privileges are not required.
4. Leave **Launch Chudley** selected or start **Chudley** later from the Start Menu.

The installer includes the self-contained application, runtime files, verified sprites, license, and third-party notices. It creates a normal Windows uninstall entry. Uninstalling removes the installed program and shortcut but preserves `%LOCALAPPDATA%\Chudley\settings.json`.

### 🗽 Portable edition

1. Download **`Chudley-win-x64-portable.zip`** from the same release.
2. Extract the entire ZIP to a folder you control.
3. Run **`Chudley.Desktop.exe`**.

Both distributions use the same validated Windows x64 application payload. No separate .NET runtime, SDK, Python installation, Git client, Visual Studio installation, or administrator access is required. Until the first release asset is published, review builds are available from the [Windows CI workflow](https://github.com/saxton-network/chudley/actions/workflows/ci.yml); GitHub may require sign-in to download Actions artifacts.

### 🧹 Removal

Quit Chudley from the tray/right-click menu, then delete the extracted folder.

Optional saved settings live at:

`%LOCALAPPDATA%\Chudley\settings.json`

Delete that file if you also want the government to forget his position, scale, and pause state.

## 🎮 Rules of Engagement

| Action | Result |
| --- | --- |
| Left click | Wave reaction |
| Left drag | Move Chudley without triggering a click reaction |
| Right click or tray icon | Open controls |
| Pause / Resume | Stop or continue animation |
| Scale | Select crisp 1×, 2×, 3×, or 4× rendering |
| Reset Position | Return Chudley to a visible default location |
| Quit | Peaceful transfer of power back to Windows |

While being dragged, Chudley's mirrored movement animation follows the drag direction. Chudley does not turn left. He may, under exceptional constitutional authority, perform a **constitutionally protected directional adjustment**.

## 🛠️ Department of Chudley Engineering

For development, use **Windows 11 x64** with the **.NET 8 SDK** installed.

Clone the repository, then run from the repository root:

```powershell
dotnet restore src/Chudley.Desktop/Chudley.Desktop.csproj
./scripts/validate-assets.ps1
dotnet build src/Chudley.Desktop/Chudley.Desktop.csproj --configuration Release --warnaserror
dotnet run --project tests/Chudley.Core.Tests/Chudley.Core.Tests.csproj --configuration Release
dotnet run --project src/Chudley.Desktop/Chudley.Desktop.csproj --configuration Release
```

### 📦 Manufacture domestic Chudley

Produce the complete end-user package with one command:

```powershell
./scripts/publish-windows.ps1
```

That script:

1. validates the approved runtime artwork;
2. restores dependencies;
3. builds with warnings treated as errors;
4. runs the core test program;
5. publishes a self-contained `win-x64` application;
6. verifies required runtime files and assets;
7. re-validates the published artwork;
8. creates `artifacts/Chudley-win-x64-portable.zip`;
9. when run through `./scripts/build-installer.ps1`, compiles `artifacts/Chudley-win-x64-installer.exe` from that same publish directory.

Generated release output is intentionally **not committed** to source control.

A human should still smoke-test the ZIP from a fresh Windows 11 x64 user profile before publishing a release, because CI is powerful but has not yet achieved citizenship.

## 🧪 Federal Quality Assurance

GitHub Actions runs the same packaging path for pushes to the release-development branches and for pull requests.

A separate release workflow listens for a **published GitHub Release whose tag starts with `v`**, rebuilds both artifacts from that tag, and attaches `Chudley-win-x64-installer.exe` and `Chudley-win-x64-portable.zip` to the existing release. Inno Setup 6 is the installer technology: its stable script format gives Chudley a per-user install, Start Menu shortcut, and standard uninstall entry without adding a runtime framework. `./scripts/build-installer.ps1` finds the compiler in `.tools/inno/ISCC.exe` or a normal Inno Setup 6 installation.

The workflow does **not** create or publish a release on its own.

## 🗂️ Strategic Asset Deployment Map

| Path | Constitutional responsibility |
| --- | --- |
| `src/Chudley.Core/` | Deterministic animation engine, settings, positioning, animation catalog |
| `src/Chudley.Desktop/` | WPF window, tray shell, sprite loading, desktop behavior |
| `assets/runtime/` | Verified v2 runtime atlas and `pet.json` metadata |
| `assets/reference/` | Character/reference material; see third-party notices |
| `assets/readme/` | Public-facing README artwork |
| `scripts/validate-assets.ps1` | Verifies approved artwork hashes, dimensions, and metadata |
| `scripts/publish-windows.ps1` | Builds, tests, publishes, validates, and packages the Windows ZIP |
| `scripts/build-installer.ps1` | Builds the same payload plus the Inno Setup installer |
| `scripts/test-installer.ps1` | Installs, launches, relaunches, compares, and uninstalls a temporary test install |
| `.github/workflows/` | CI and release-attachment automation |
| `THIRD_PARTY_NOTICES.md` | Character, video, meme-lineage, and rights attribution |

The verified v2 runtime atlas uses **8×11 cells at 192×208 pixels each**. `assets/runtime/spritesheet.png` is the pixel-identical PNG used by WPF; `spritesheet.webp` and `pet.json` are retained alongside it. Packaging fails if the approved hashes or required dimensions no longer match.

## 📜 License: Freedom, With Extremely Specific Terms

Project-authored Chudley software is offered under the **[PolyForm Strict License 1.0.0](LICENSE)**.

In plain English: the license permits covered **noncommercial use**, but it does **not** grant permission to redistribute the software or create changes/new works based on it. Commercial use is not a permitted purpose under this license.

That makes this project **source-available, not open source**.

The official license text in [LICENSE](LICENSE) controls. This summary is only a convenience and does not replace the license.

Third-party media, character/reference material, and other works the project does not own are **not sublicensed** merely because they appear in this repository. See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) for the attribution and scope boundary.

## 🧾 Credit Where Credit Is Due

Chudley exists because internet culture has no adult supervision.

The repository preserves credit for:

- the documented older MAGA Wojak meme lineage, whose true original creator is uncertain;
- **[@WearForbidden](https://x.com/WearForbidden)** for the viral AI video that directly inspired this project;
- **[@normposter](https://x.com/normposter)** for an early documented mobility-scooter adaptation in the meme's September 2026 spread.

Details and source references are in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

## 🏛️ Mandatory Government Disclaimer

Chudley is a **parody/experimental software project**.

It is not affiliated with, sponsored by, or endorsed by OpenAI, Anthropic, any political campaign, political party, candidate, or any brand depicted or referenced in the artwork or documentation.

The absurd patriotic presentation is part of the project's parody branding. The actual build pipeline, unfortunately, is completely serious.
