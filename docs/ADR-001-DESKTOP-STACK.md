# ADR-001: Windows desktop stack

Status: Accepted (MVP)

Target: Windows 11 x64.

WPF on .NET 8 is the smallest fitting choice here. Its native window supports transparent, frameless, always-on-top display and mouse input without a browser runtime. WPF's `BitmapScalingMode.NearestNeighbor`, fixed 128-pixel frame source, and integer window scales preserve pixel edges. Windows Forms integration supplies a tray icon. A separate .NET core library keeps animation and settings testable without opening a window. Publish as a self-contained `win-x64` application.

Avalonia would support other platforms, but cross-platform runtime and window behavior add surface we do not need for this Windows-only MVP. Tauri brings a WebView, frontend build, and platform bridge for a small animated sprite. Electron has a substantially larger idle footprint and packaging surface.

The runtime reads individual PNG frames and a manifest. Replacing art requires keeping frame names and dimensions or editing declarative animation definitions, not changing window code. No network integration is in this version.
