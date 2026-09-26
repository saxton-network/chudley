using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Chudley.Core;
using Forms = System.Windows.Forms;

namespace Chudley.Desktop;

public partial class PetWindow : Window
{
    private const int FramePixels = 128;
    private const int DragThresholdPixels = 4;

    private readonly PetSettingsStore _store;
    private readonly SpriteCatalog _sprites;
    private readonly PetAnimationEngine _engine;
    private readonly DispatcherTimer _timer;
    private readonly Forms.NotifyIcon _tray;
    private PetSettings _settings;
    private IntPtr _hwnd;
    private NativeWindow.Point _pressCursor;
    private NativeWindow.Rect _pressWindow;
    private bool _pressed;
    private bool _dragged;
    private string? _shownFrame;

    public PetWindow(PetSettingsStore store, PetSettings settings)
    {
        InitializeComponent();
        _store = store;
        _settings = settings;
        _sprites = new SpriteCatalog(Path.Combine(AppContext.BaseDirectory, "assets", "sprites", "candidate"), AnimationCatalog.AllFrameNames);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        _engine = new PetAnimationEngine(now, behaviorFrequencyMinutes: settings.BehaviorFrequencyMinutes);
        if (settings.Paused)
            _engine.Pause(now);

        _timer = new DispatcherTimer(DispatcherPriority.Background);
        _timer.Tick += OnTick;
        _tray = CreateTray();
        ContextMenu = CreatePetMenu();
        RenderFrame();
        SourceInitialized += OnSourceInitialized;
        MouseLeftButtonDown += OnLeftDown;
        MouseMove += OnPointerMove;
        MouseLeftButtonUp += OnLeftUp;
        LostMouseCapture += OnLostCapture;
        Closed += OnClosed;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        _hwnd = new WindowInteropHelper(this).Handle;
        ApplyScaleAndPosition(reset: false);
        ScheduleNext();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        _timer.Stop();
        _engine.Tick(DateTimeOffset.UtcNow);
        RenderFrame();
        ScheduleNext();
    }

    private void ScheduleNext()
    {
        _timer.Stop();
        if (_engine.IsPaused || _engine.NextDueAt == DateTimeOffset.MaxValue)
            return;

        TimeSpan delay = _engine.NextDueAt - DateTimeOffset.UtcNow;
        // One active UI timer, asleep until the next state transition.
        _timer.Interval = delay > TimeSpan.Zero ? delay : TimeSpan.FromMilliseconds(1);
        _timer.Start();
    }

    private void RenderFrame()
    {
        string name = _engine.CurrentFrameName;
        if (name == _shownFrame)
            return;
        Sprite.Source = _sprites.Get(name);
        _shownFrame = name;
    }

    private void OnLeftDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 1 || _pressed)
            return;
        _pressed = true;
        _dragged = false;
        _pressCursor = NativeWindow.CursorPosition();
        _pressWindow = NativeWindow.GetRect(_hwnd);
        Mouse.Capture(this);
        e.Handled = true;
    }

    private void OnPointerMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!_pressed || e.LeftButton != MouseButtonState.Pressed)
            return;
        NativeWindow.Point cursor = NativeWindow.CursorPosition();
        int dx = cursor.X - _pressCursor.X;
        int dy = cursor.Y - _pressCursor.Y;
        if (!_dragged && Math.Abs(dx) < DragThresholdPixels && Math.Abs(dy) < DragThresholdPixels)
            return;
        if (!_dragged)
        {
            _dragged = true;
            _engine.DragStarted(DateTimeOffset.UtcNow);
            RenderFrame();
            ScheduleNext();
        }
        NativeWindow.Place(_hwnd, _pressWindow.Left + dx, _pressWindow.Top + dy, FramePixels * _settings.Scale);
        e.Handled = true;
    }

    private void OnLeftUp(object sender, MouseButtonEventArgs e)
    {
        if (!_pressed)
            return;
        bool dragged = _dragged;
        _pressed = false;
        _dragged = false;
        Mouse.Capture(null);
        if (dragged)
        {
            _engine.DragEnded(DateTimeOffset.UtcNow);
            SaveCurrentPosition();
        }
        else
        {
            _engine.Click(DateTimeOffset.UtcNow);
        }
        RenderFrame();
        ScheduleNext();
        e.Handled = true;
    }

    private void OnLostCapture(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!_pressed)
            return;
        bool dragged = _dragged;
        _pressed = false;
        _dragged = false;
        if (dragged)
        {
            _engine.DragEnded(DateTimeOffset.UtcNow);
            SaveCurrentPosition();
            RenderFrame();
            ScheduleNext();
        }
    }

    private void ApplyScaleAndPosition(bool reset)
    {
        int size = FramePixels * _settings.Scale;
        DpiScale dpi = VisualTreeHelper.GetDpi(this);
        Width = size / dpi.DpiScaleX;
        Height = size / dpi.DpiScaleY;
        Sprite.Width = Width;
        Sprite.Height = Height;

        if (reset)
            _settings = _settings with { Left = null, Top = null };
        PointD point = Positioning.Resolve(_settings, size, CurrentScreens());
        NativeWindow.Place(_hwnd, (int)Math.Round(point.X), (int)Math.Round(point.Y), size);
        SaveCurrentPosition();
    }

    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);
        if (_hwnd != IntPtr.Zero)
            ApplyScaleAndPosition(reset: false);
    }

    private void SetScale(int scale)
    {
        if (scale is < 1 or > 4 || scale == _settings.Scale)
            return;
        _settings = _settings with { Scale = scale };
        ApplyScaleAndPosition(reset: false);
    }

    private void TogglePause()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (_engine.IsPaused)
            _engine.Resume(now);
        else
            _engine.Pause(now);
        _settings = _settings with { Paused = _engine.IsPaused };
        _store.Save(_settings);
        RenderFrame();
        ScheduleNext();
    }

    private void SaveCurrentPosition()
    {
        if (_hwnd == IntPtr.Zero)
            return;
        NativeWindow.Rect rect = NativeWindow.GetRect(_hwnd);
        int size = FramePixels * _settings.Scale;
        PetSettings requested = _settings with { Left = rect.Left, Top = rect.Top };
        PointD valid = Positioning.Resolve(requested, size, CurrentScreens());
        int x = (int)Math.Round(valid.X);
        int y = (int)Math.Round(valid.Y);
        if (x != rect.Left || y != rect.Top)
            NativeWindow.Place(_hwnd, x, y, size);
        _settings = requested with { Left = x, Top = y };
        _store.Save(_settings);
    }

    private static ScreenBounds[] CurrentScreens() => NativeWindow.Screens
        .Select(s => new ScreenBounds(s.WorkingArea.Left, s.WorkingArea.Top, s.WorkingArea.Width, s.WorkingArea.Height))
        .ToArray();

    private ContextMenu CreatePetMenu()
    {
        var menu = new ContextMenu();
        var pause = new MenuItem();
        pause.Click += (_, _) => TogglePause();
        menu.Items.Add(pause);
        var scale = new MenuItem { Header = "Scale" };
        for (int value = 1; value <= 4; value++)
        {
            int selected = value;
            var item = new MenuItem { Header = $"{value}×" };
            item.Click += (_, _) => SetScale(selected);
            scale.Items.Add(item);
        }
        menu.Items.Add(scale);
        var reset = new MenuItem { Header = "Reset Position" };
        reset.Click += (_, _) => ApplyScaleAndPosition(reset: true);
        menu.Items.Add(reset);
        menu.Items.Add(new Separator());
        var quit = new MenuItem { Header = "Quit" };
        quit.Click += (_, _) => Close();
        menu.Items.Add(quit);
        menu.Opened += (_, _) =>
        {
            pause.Header = _engine.IsPaused ? "Resume" : "Pause";
            foreach (MenuItem item in scale.Items)
                item.IsChecked = Equals(item.Header, $"{_settings.Scale}×");
        };
        return menu;
    }

    private Forms.NotifyIcon CreateTray()
    {
        var menu = new Forms.ContextMenuStrip();
        var pause = new Forms.ToolStripMenuItem();
        pause.Click += (_, _) => Dispatcher.Invoke(TogglePause);
        menu.Items.Add(pause);
        var scale = new Forms.ToolStripMenuItem("Scale");
        for (int value = 1; value <= 4; value++)
        {
            int selected = value;
            var item = new Forms.ToolStripMenuItem($"{value}×");
            item.Click += (_, _) => Dispatcher.Invoke(() => SetScale(selected));
            scale.DropDownItems.Add(item);
        }
        menu.Items.Add(scale);
        menu.Items.Add("Reset Position", null, (_, _) => Dispatcher.Invoke(() => ApplyScaleAndPosition(reset: true)));
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Quit", null, (_, _) => Dispatcher.Invoke(Close));
        menu.Opening += (_, _) =>
        {
            pause.Text = _engine.IsPaused ? "Resume" : "Pause";
            foreach (Forms.ToolStripMenuItem item in scale.DropDownItems)
                item.Checked = item.Text == $"{_settings.Scale}×";
        };
        var tray = new Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "Chudley",
            ContextMenuStrip = menu,
            Visible = true
        };
        tray.DoubleClick += (_, _) => Dispatcher.Invoke(() =>
        {
            Show();
            Activate();
        });
        return tray;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _timer.Stop();
        if (_hwnd != IntPtr.Zero)
            SaveCurrentPosition();
        _tray.Visible = false;
        _tray.ContextMenuStrip?.Dispose();
        _tray.Dispose();
    }
}
