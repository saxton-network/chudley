# ADR-001: Windows desktop stack

Status: Accepted (working v2 pet migration)

Target: Windows 11 x64.

WPF on .NET 8 remains the smallest fitting choice here. Its native window supports transparent, frameless, always-on-top display and mouse input without a browser runtime. WPF's `BitmapScalingMode.NearestNeighbor`, 192×208 atlas cells, and integer window scales preserve pixel edges. Windows Forms integration supplies a tray icon. A separate .NET core library keeps animation and settings testable without opening a window. Publish as a self-contained `win-x64` portable ZIP.

Avalonia would support other platforms, but cross-platform runtime and window behavior add surface we do not need for this Windows-only v2 desktop pet. Tauri brings a WebView, frontend build, and platform bridge for a small animated sprite. Electron has a substantially larger idle footprint and packaging surface.

The runtime crops frames from a pixel-identical PNG copy of the verified Codex v2 WebP atlas. Both sheets and `pet.json` are retained under `assets/runtime`; approved SHA-256 hashes are checked during packaging. Replacing art requires retaining the atlas grid dimensions or updating the declarative animation catalog. No network integration is in this version.
