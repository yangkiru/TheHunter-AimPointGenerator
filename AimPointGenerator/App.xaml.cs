using System.IO;
using System.Windows;
using System.Windows.Threading;
using AimPointGenerator.Models;
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

        if (Environment.GetCommandLineArgs().Contains("--generate-readme-sample"))
        {
            GenerateReadmeSample();
            Shutdown();
        }
    }

    private static void GenerateReadmeSample()
    {
        var sample = new AimPointData
        {
            AmmunitionName = ".308 Soft Point",
            EffectiveRange = 200,
            ScopeName = "Hyperion 4-8x42",
            LabelFontSize = 8,
            AimPointItems = new List<AimPointItem>
            {
                new AimPointItem { TargetDistance = 75, ZeroingDistance = 75, AimPosition = -2, IsMinZoom = false },
                new AimPointItem { TargetDistance = 100, ZeroingDistance = 100, AimPosition = -1, IsMinZoom = false },
                new AimPointItem { TargetDistance = 150, ZeroingDistance = 150, AimPosition = 0.5, IsMinZoom = false },
                new AimPointItem { TargetDistance = 200, ZeroingDistance = 200, AimPosition = 1, IsMinZoom = false },
                new AimPointItem { TargetDistance = 250, ZeroingDistance = 200, AimPosition = 2, IsMinZoom = false },
                new AimPointItem { TargetDistance = 300, ZeroingDistance = 200, AimPosition = 3, IsMinZoom = false },
            }
        };

        var docsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docs"));
        Directory.CreateDirectory(docsDir);
        var savePath = Path.Combine(docsDir, "example.png");

        var bitmap = AimPointImageGenerator.Generate(sample);
        AimPointImageGenerator.SaveToFile(bitmap, savePath);
    }
}
