using System;
using System.Threading;
using System.Windows;
using Chudley.Core;

namespace Chudley.Desktop;

public partial class App : System.Windows.Application
{
    private Mutex? _instanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _instanceMutex = new Mutex(true, "Local\\Chudley.Desktop", out bool createdNew);
        if (!createdNew)
        {
            _instanceMutex.Dispose();
            _instanceMutex = null;
            Shutdown();
            return;
        }

        try
        {
            var store = new PetSettingsStore();
            var window = new PetWindow(store, store.Load());
            MainWindow = window;
            window.Show();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Chudley could not start.\n\n{ex.Message}", "Chudley", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_instanceMutex is not null)
        {
            _instanceMutex.ReleaseMutex();
            _instanceMutex.Dispose();
        }
        base.OnExit(e);
    }
}
