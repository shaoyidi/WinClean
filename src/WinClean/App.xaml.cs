using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using WinClean.Services;
using WinClean.ViewModels;
using WinClean.Views;

namespace WinClean;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/winclean-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<ICleanerService, SystemCleanerService>();
                services.AddSingleton<ILargeFileScannerService, LargeFileScannerService>();
                services.AddSingleton<IDuplicateFinderService, DuplicateFinderService>();
                services.AddSingleton<IDiskAnalyzerService, DiskAnalyzerService>();
                services.AddSingleton<IFileOperationService, FileOperationService>();

                services.AddSingleton<MainViewModel>();
                services.AddSingleton<SystemCleanerViewModel>();
                services.AddSingleton<LargeFileScannerViewModel>();
                services.AddSingleton<DuplicateFinderViewModel>();
                services.AddSingleton<DiskAnalyzerViewModel>();
                services.AddSingleton<SettingsViewModel>();

                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync();
        }
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
