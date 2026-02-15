using System.IO;
using System.Windows;
using System.Windows.Threading;
using AimPointGenerator.Services;

namespace AimPointGenerator;

public partial class App : Application
{
    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogCrash(e.ExceptionObject as Exception, "Unhandled");
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogCrash(e.Exception, "Dispatcher");
        e.Handled = true;
    }

    private static void LogCrash(Exception? ex, string source)
    {
        try
        {
            var dir = Path.GetDirectoryName(Environment.ProcessPath) ?? Environment.CurrentDirectory;
            var path = Path.Combine(dir, "crash.log");
            var msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}\r\n{ex}\r\n\r\n";
            File.AppendAllText(path, msg);
        }
        catch { /* ignore */ }
    }

    private void App_Startup(object sender, StartupEventArgs e)
    {
        try
        {
            ThemeManager.Load();
        }
        catch
        {
            // 기본 테마로 계속 진행
        }
    }
}
