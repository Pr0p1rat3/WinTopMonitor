using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WinTopMonitor.Config;
using WinTopMonitor.Logging;
using WinTopMonitor.Services;
using WinTopMonitor.UI;

namespace WinTopMonitor;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };

        var services = new ServiceCollection();
        services.AddSingleton<ConfigService>();
        services.AddSingleton<FileLoggerProvider>();
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(new FileLoggerProvider(LogPaths.LogDirectory));
            builder.SetMinimumLevel(LogLevel.Information);
        });
        services.AddSingleton<SystemInfoService>();
        services.AddSingleton<CpuMonitor>();
        services.AddSingleton<MemoryMonitor>();
        services.AddSingleton<DiskMonitor>();
        services.AddSingleton<NetworkMonitor>();
        services.AddSingleton<ProcessMonitor>();
        services.AddSingleton<SystemSnapshotService>();
        services.AddSingleton<DashboardRenderer>();
        services.AddSingleton<App>();

        using var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

        try
        {
            logger.LogInformation("WinTop Monitor starting");
            var app = provider.GetRequiredService<App>();
            await app.RunAsync(cancellation.Token);
            logger.LogInformation("WinTop Monitor stopped");
            return 0;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("WinTop Monitor cancelled");
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            Console.Error.WriteLine($"WinTop Monitor failed: {ex.Message}");
            return 1;
        }
    }
}
