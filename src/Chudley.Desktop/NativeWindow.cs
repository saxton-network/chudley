using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Chudley.Desktop;

internal static class NativeWindow
{
    private const uint SwpNoZOrder = 0x0004;
    private const uint SwpNoActivate = 0x0010;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Point
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int width, int height, uint flags);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out Point point);

    internal static Rect GetRect(IntPtr hwnd)
    {
        if (!GetWindowRect(hwnd, out Rect rect))
            throw new InvalidOperationException("Could not read the Chudley window position.");
        return rect;
    }

    internal static Point CursorPosition()
    {
        if (!GetCursorPos(out Point point))
            throw new InvalidOperationException("Could not read the cursor position.");
        return point;
    }

    internal static void Place(IntPtr hwnd, int x, int y, int size)
    {
        if (!SetWindowPos(hwnd, IntPtr.Zero, x, y, size, size, SwpNoZOrder | SwpNoActivate))
            throw new InvalidOperationException("Could not place the Chudley window.");
    }

    internal static Screen[] Screens => Screen.AllScreens;
}
