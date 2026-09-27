
# Chudley

<p align="center">
  <img width="1280" height="640" alt="banner" src="https://github.com/user-attachments/assets/fb2dc3a1-3972-4467-b65a-b100068ddad4" />
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

<p align="center"><img width="192" height="208" alt="chudley-all-frames" src="https://github.com/user-attachments/assets/ad5197e1-c3e8-4cd8-8b87-eddf70ed2f57" /><em>One meme. One scooter. One 'Murica🇺🇸.</em><img width="192" height="208" alt="chudley-all-frames" src="https://github.com/user-attachments/assets/ad5197e1-c3e8-4cd8-8b87-eddf70ed2f57" /></p>

## 🎆 Origin Lore:

**Flock camera footage definitely wasn't used to blackmail me into making this**

<div align="center">

https://github.com/user-attachments/assets/cc6b0d87-e352-412e-8c52-0c7b32c0d288

</div>

▶️ **[Open the repository copy](WearForbidden_video_1.mp4)**

Orginal video posted by **[@WearForbidden](https://x.com/WearForbidden)**. The broader character and mobility-scooter meme lineage predates this repository; attribution trail and third-party rights live in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

## 🦅 What Is Chudley?

Chudley is a custom pet **inside the Codex desktop app** on Windows 11 x64. The installer places his verified v2 sprite package in Codex's Pets directory. Select him in Codex's pet menu; the installer does not launch a separate desktop application. **May or may not make Codex great again**

Chudley does **not** solve a problem. Because he has nothing to hide. Do you?

### ⭐ Current capabilities

- 🎨 Verified 8×11 Codex v2 sprite atlas with idle, working, waiting, review, failed, and directional drag animation
- 🖱️ Codex's built-in pet interactions and controls
- 🛴 A mirrored directional animation for the constitutionally protected directional adjustment
- 📦 Two runtime files: `pet.json` and `spritesheet.webp`
- 🚫 No separate Chudley process, .NET runtime, service, account, liberalism, or telemetry

Codex controls the pet's animation and behavior. This package contains no OpenAI API key, authentication, or network client. It does however contain FREEDOM™️

## 🇺🇸 Acquire Chudley

### 🇺🇸 Recommended: Installer

1. Open the [latest GitHub release](https://github.com/saxton-network/chudley/releases/latest).
2. Download **`Chudley-Codex-pet-installer.exe`**.
3. Run the per-user installer. No administrator privileges or development tools are required.
4. Restart Codex if it was open, then select **Chudley** in the Codex Pets menu.

The installer writes the pet package to `%USERPROFILE%\.codex\pets\chudley-v2` (or `%CODEX_HOME%\pets\chudley-v2` when `CODEX_HOME` is set). License, notices, and uninstall metadata live separately under `%LOCALAPPDATA%\Programs\Chudley Codex Pet`. Windows Settings lists **Chudley for Codex** for uninstall.

### 🗽 Manual import ZIP

1. Download **`Chudley-Codex-pet.zip`** from the same release.
2. Extract its `chudley-v2` folder under `%USERPROFILE%\.codex\pets` (or `%CODEX_HOME%\pets`).
3. Restart Codex if needed, then select **Chudley** in its Pets menu.

Both formats contain the same verified pet metadata and artwork. The ZIP also includes the license and third-party notices. Neither requires the .NET runtime, SDK, Python, Git, or Visual Studio.

### 🧹 Removal

For the installer, uninstall **Chudley for Codex** from Windows Settings. For the manual ZIP, remove only the `chudley-v2` folder you extracted. Restart Codex to refresh its pet menu.

Other Codex pets and Codex settings are outside Chudley's installer scope....for now

## 🎮 Rules of Engagement

| Action | Result |
| --- | --- |
| Select Chudley in Codex's Pets menu | Use him as the active Codex desktop pet |
| Click or drag | Use Codex's built-in pet interaction |
| Change or remove the pet | Use the Codex Pets menu |

While being dragged, Chudley may perform a **constitutionally protected directional adjustment**.

## 🛠️ Department of Chudley Engineering

Packaging requires **Windows 11 x64**, PowerShell, and **Inno Setup 6**.

From the repository root:

```powershell
./scripts/validate-assets.ps1
./scripts/build-installer.ps1
./scripts/test-installer.ps1
```

`build-installer.ps1` stages only the verified Codex pet payload plus licensing/notices, produces the manual ZIP, and optionally compiles the Inno Setup installer. It finds `ISCC.exe` in a normal Inno Setup 6 installation or accepts `-InnoCompiler`.

Generated output is written under `artifacts/` and is intentionally not committed.

`test-installer.ps1` verifies ZIP parity, performs a real silent per-user install into a clean Codex pet path, checks installed hashes and metadata, and proves uninstall removes only Chudley's files and registration. It refuses to overwrite an existing Chudley pet.

## 🧪 Federal Quality Assurance

GitHub Actions validates the approved pet assets, builds the installer and manual ZIP, then exercises install/uninstall on a clean Windows runner.

A separate release workflow listens for a **published GitHub Release whose tag starts with `v`**, rebuilds and tests from that tag, then attaches `Chudley-Codex-pet-installer.exe` and `Chudley-Codex-pet.zip`.

The workflow does **not** create or publish a release on its own.

## 🗂️ Strategic Asset Deployment Map

| Path | Constitutional responsibility |
| --- | --- |
| `assets/runtime/` | Verified Codex v2 metadata and sprite assets |
| `assets/reference/` | Character/reference material; see third-party notices |
| `assets/readme/` | Public-facing README artwork |
| `installer/chudley.iss` | Per-user Inno Setup installer definition |
| `scripts/validate-assets.ps1` | Verifies approved hashes, metadata, and atlas dimensions |
| `scripts/build-installer.ps1` | Stages and packages the Codex pet |
| `scripts/test-installer.ps1` | Tests ZIP parity and installer lifecycle |
| `.github/workflows/` | CI and release-attachment automation |
| `THIRD_PARTY_NOTICES.md` | Character, video, meme-lineage, and rights attribution |

The verified v2 atlas uses **8×11 cells at 192×208 pixels each**. Codex consumes `spritesheet.webp` and `pet.json`. The lossless `spritesheet.png` is retained as the canonical validation/provenance counterpart and is not included in the distributed pet package.

## 📜 License: Freedom, With Extremely Specific Terms

Project-authored Chudley software is offered under the **[PolyForm Strict License 1.0.0](LICENSE)**.

In plain English: the license permits covered **noncommercial use**, but it does **not** grant permission to redistribute the software or create changes/new works based on it. Commercial use is not a permitted purpose under this license.

That makes this project **source-available, not open source**.

The official license text in [LICENSE](LICENSE) controls. This summary is only a convenience and does not replace the license.

Third-party media, character/reference material, and other works the project does not own are **not sublicensed** merely because they appear in this repository. See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) for the attribution and scope boundary.

## 🧾 Credit Where Credit Is Due

Chudley exists because America is Great Again

The repository preserves credit for:

- the documented older MAGA Wojak meme lineage, whose true original creator is uncertain but likely a true patriot 
- **[@WearForbidden](https://x.com/WearForbidden)** for the viral AI video that directly inspired this project;
- **[@normposter](https://x.com/normposter)** for an early documented mobility-scooter adaptation in the meme's September 2026 spread.

Details and source references are in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

## 🏛️ Mandatory Government Disclaimer

Chudley is a very serious mascot of sorts 

It is not affiliated with communism, socialism, democrats, or ANTIFA

He is however affiliated with red, white, and blue real Americans and making this country Great Again
