# 🇺🇸 Chudley has entered the homes of ordinary Americans

Chudley no longer requires citizens to understand ZIP extraction as a prerequisite for liberty.

This first installable release introduces a conventional per-user Windows installer while retaining the portable edition for constitutional traditionalists.

## Downloads

- `Chudley-win-x64-installer.exe` — recommended. Run the wizard, then launch Chudley from the finish page or Start Menu.
- `Chudley-win-x64-portable.zip` — extract the complete ZIP and run `Chudley.Desktop.exe`.

Both artifacts are self-contained Windows x64 distributions built from the same validated publish payload. Windows 11 x64 is required. Neither requires the .NET runtime, SDK, Python, Git, Visual Studio, or administrator privileges.

## Installation and removal

The installer uses Inno Setup 6 and installs per user under `%LOCALAPPDATA%\Programs\Chudley`. It creates a Start Menu shortcut and a standard Windows uninstall entry. Uninstalling removes the installed program and shortcut while preserving `%LOCALAPPDATA%\Chudley\settings.json`.

## What Chudley does

Chudley is a transparent, always-on-top desktop pet with nearest-neighbor 1×–4× scaling, idle animation, click reactions, drag repositioning, pause/resume, tray and context controls, monitor-coordinate recovery, and persistent position and scale settings. The standalone release does not yet receive live Codex task events.

## License and notices

Project-authored material is governed by the PolyForm Strict License 1.0.0. Third-party character, video, and reference material is separately described in `THIRD_PARTY_NOTICES.md` and is not sublicensed by this release.
