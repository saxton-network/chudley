# 🗽 Chudley has secured his rightful place in Codex

This release corrects the installation target. Chudley now installs **inside the Codex desktop app's Pets menu**, not as a separate Windows desktop application.

## Downloads

- `Chudley-Codex-pet-installer.exe` — recommended for Windows 11 x64. Run the per-user wizard, restart Codex if it was open, and select Chudley in the Pets menu.
- `Chudley-Codex-pet.zip` — manual import. Extract `chudley-v2` into your Codex `pets` folder.

The installer writes only the verified `pet.json` and `spritesheet.webp` to `%USERPROFILE%\.codex\pets\chudley-v2`, or `%CODEX_HOME%\pets\chudley-v2` when that environment variable is set. License, notices, and uninstall metadata live separately. No administrator privileges, .NET runtime, SDK, Python, Git, or Visual Studio are required.

## Install and remove

Open or restart Codex, then select Chudley from the Pets menu. To remove the installed pet, uninstall **Chudley for Codex** in Windows Settings and restart Codex. Uninstall leaves other pets and Codex settings alone.

The old v0.1.0 download installed a separate WPF pet. This release replaces that distribution with the Codex pet package. It does not install or launch `Chudley.Desktop.exe`.

## What Chudley does

The verified Codex v2 atlas provides idle, working, waiting, review, failed, and directional drag animations. Codex owns pet interaction and animation playback. The package does not include a network client or OpenAI API credentials.

## License and notices

Project-authored material is governed by the PolyForm Strict License 1.0.0. Third-party character, video, and reference material is separately described in `THIRD_PARTY_NOTICES.md` and is not sublicensed by this release. The original video is not included in the installer or ZIP.
