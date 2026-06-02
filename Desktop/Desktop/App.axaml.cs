using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Desktop.ViewModels;
using Desktop.Views;

namespace Desktop;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Меняем режим закрытия, чтобы приложение не закрывалось
            // при закрытии окна подключения до открытия главного окна
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var connectionVm = new ConnectionViewModel();
            var connectionWindow = new ConnectionWindow
            {
                DataContext = connectionVm
            };

            // При успешном подключении — открываем MainWindow
            connectionVm.ConnectionSucceeded += () =>
            {
                connectionWindow.Close();

                var mainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
                desktop.MainWindow = mainWindow;
                mainWindow.Show();

                // Теперь приложение закрывается при закрытии главного окна
                desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            };

            // Если пользователь закрыл окно подключения без подключения — выходим
            connectionWindow.Closed += (_, _) =>
            {
                if (!connectionVm.IsConnected)
                {
                    desktop.Shutdown();
                }
            };

            connectionWindow.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }
}