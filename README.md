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
  <img alt="Codex pet v2" src="https://img.shields.io/badge/Codex-pet%20v2-512BD4">
  <img alt="License: PolyForm Strict 1.0.0" src="https://img.shields.io/badge/license-PolyForm%20Strict%201.0.0-goldenrod">
</p>

<p align="center"><em>One meme. One scooter. An unreasonable amount of CI.</em></p>

## 🎆 What Started It All

**One 38-second video. One catastrophically disproportionate software response.**

▶️ **[Watch the original viral clip](WearForbidden_video_1.mp4)**

The viral AI clip that kicked this whole thing off was posted by **[@WearForbidden](https://x.com/WearForbidden)**. The broader character and mobility-scooter meme lineage predates this repository; the attribution trail and third-party rights notes live in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

## 🦅 What Is Chudley?

Chudley is a custom pet **inside the Codex desktop app** on Windows 11 x64. The installer places his verified v2 sprite package in Codex's Pets directory. Select him in Codex's pet menu; the installer does not launch a separate desktop application.

Chudley does **not** solve a business problem. Chudley is what happens when a 38-second meme receives an 8×11 sprite atlas and nobody intervenes.

### ⭐ Current capabilities

- 🎨 Verified 8×11 Codex v2 sprite atlas, with idle, working, waiting, review, failed, and directional drag animation
- 🖱️ Codex's built-in pet interactions and controls
- 🛴 A mirrored directional animation for the constitutionally protected directional adjustment
- 📦 Two runtime files: `pet.json` and `spritesheet.webp`
- 🚫 No separate Chudley process, .NET runtime, service, account, or telemetry

Codex controls the pet's animation and behavior. This package contains no OpenAI API key, authentication, or network client.

## 🇺🇸 Acquire Chudley

### 🇺🇸 Recommended: Installer

1. Open the [latest GitHub release](https://github.com/saxton-network/chudley/releases/latest).
2. Download **`Chudley-Codex-pet-installer.exe`**.
3. Run the per-user installer. No administrator privileges or development tools are required.
4. Restart Codex if it was open, then select **Chudley** in the Codex Pets menu.

The installer writes the pet package to `%USERPROFILE%\.codex\pets\chudley-v2` (or `%CODEX_HOME%\pets\chudley-v2` when `CODEX_HOME` is set). License, notices, and uninstall metadata live separately under `%LOCALAPPDATA%\Programs\Chudley Codex Pet`. Windows Settings lists **Chudley for Codex** for uninstall. The installer contains no standalone `Chudley.Desktop.exe`.

### 🗽 Manual import ZIP

1. Download **`Chudley-Codex-pet.zip`** from the same release.
2. Extract its `chudley-v2` folder under `%USERPROFILE%\.codex\pets` (or `%CODEX_HOME%\pets`).
3. Restart Codex if needed, then select **Chudley** in its Pets menu.

Both formats contain the same verified pet metadata and artwork. The ZIP also includes the license and third-party notices. Neither requires the .NET runtime, SDK, Python, Git, or Visual Studio.

### 🧹 Removal

For the installer, uninstall **Chudley for Codex** from Windows Settings. For the manual ZIP, remove only the `chudley-v2` folder you extracted. Restart Codex to refresh its pet menu.

Other Codex pets and Codex settings are outside Chudley's installer scope.

## 🎮 Rules of Engagement

| Action | Result |
| --- | --- |
| Select Chudley in Codex's Pets menu | Use him as the active Codex desktop pet |
| Click or drag | Use Codex's built-in pet interaction |
| Change or remove the pet | Use the Codex Pets menu |

While being dragged, Chudley may perform a **constitutionally protected directional adjustment**.

## 🛠️ Department of Chudley Engineering

Packaging the Codex pet requires Windows 11 x64 and Inno Setup 6. The .NET 8 SDK is needed only if you also work on the retained standalone WPF source.

Clone the repository, then run from the repository root:

```powershell
./scripts/validate-assets.ps1
./scripts/build-installer.ps1
```

The resulting `artifacts/Chudley-Codex-pet-installer.exe` and `artifacts/Chudley-Codex-pet.zip` contain the same approved Codex pet. The build script finds `ISCC.exe` in a normal Inno Setup 6 installation or accepts `-InnoCompiler`.

The standalone WPF source remains in `src/` for development but is **not** included in either Codex pet distribution. To build and test that source separately:

```powershell
dotnet restore src/Chudley.Desktop/Chudley.Desktop.csproj
dotnet build src/Chudley.Desktop/Chudley.Desktop.csproj --configuration Release --warnaserror
dotnet run --project tests/Chudley.Core.Tests/Chudley.Core.Tests.csproj --configuration Release
dotnet run --project src/Chudley.Desktop/Chudley.Desktop.csproj --configuration Release
```

### 📦 Manufacture domestic Chudley

Produce the Codex pet installer and manual ZIP with one command:

```powershell
./scripts/build-installer.ps1
```

That script:

1. validates the approved runtime artwork;
2. stages only `pet.json`, `spritesheet.webp`, the license, and notices;
3. verifies the staged pet files match the approved source hashes;
4. creates a manual-import ZIP and an Inno Setup installer from that payload.

Generated release output is intentionally **not committed** to source control.

`./scripts/test-installer.ps1` runs the installer lifecycle against a clean Codex pet path. It refuses to overwrite an existing Chudley pet during testing. To test locally without touching an installed pet, set `CODEX_HOME` to an isolated temporary directory for that process. The ordinary installer uses the user's actual Codex home.

## 🧪 Federal Quality Assurance

GitHub Actions validates the Codex artwork, builds the retained .NET source, runs core tests, packages the Codex pet, and tests install/uninstall on a clean Windows runner.

A separate release workflow listens for a **published GitHub Release whose tag starts with `v`**, rebuilds and tests from that tag, then attaches `Chudley-Codex-pet-installer.exe` and `Chudley-Codex-pet.zip` to the release. Inno Setup 6 provides the per-user uninstall entry without adding a runtime framework.

The workflow does **not** create or publish a release on its own.

## 🗂️ Strategic Asset Deployment Map

| Path | Constitutional responsibility |
| --- | --- |
| `assets/runtime/` | Verified Codex v2 `pet.json` and spritesheet; PNG retained for source/provenance |
| `assets/reference/` | Character/reference material; see third-party notices |
| `assets/readme/` | Public-facing README artwork |
| `scripts/validate-assets.ps1` | Verifies approved artwork hashes, dimensions, and metadata |
| `scripts/build-installer.ps1` | Packages the validated Codex pet as an installer and manual ZIP |
| `scripts/test-installer.ps1` | Tests the Codex pet path, package parity, metadata, and uninstall |
| `src/` and `tests/` | Retained standalone WPF source and its separate tests; not distributed as the Codex pet |
| `.github/workflows/` | CI and release-attachment automation |
| `THIRD_PARTY_NOTICES.md` | Character, video, meme-lineage, and rights attribution |

The verified v2 runtime atlas uses **8×11 cells at 192×208 pixels each**. The Codex pet uses `spritesheet.webp` and `pet.json`; `spritesheet.png` remains as the pixel-identical source for the retained WPF renderer. Packaging fails if approved hashes or required dimensions no longer match.

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
