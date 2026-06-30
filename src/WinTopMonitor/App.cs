using Microsoft.Extensions.Logging;
using WinTopMonitor.Config;
using WinTopMonitor.Models;
using WinTopMonitor.Services;
using WinTopMonitor.UI;

namespace WinTopMonitor;

public sealed class App
{
    private readonly ConfigService _configService;
    private readonly SystemSnapshotService _snapshotService;
    private readonly DashboardRenderer _renderer;
    private readonly ILogger<App> _logger;

    public App(
        ConfigService configService,
        SystemSnapshotService snapshotService,
        DashboardRenderer renderer,
        ILogger<App> logger)
    {
        _configService = configService;
        _snapshotService = snapshotService;
        _renderer = renderer;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var config = await _configService.LoadOrCreateAsync(cancellationToken);
        var sortMode = config.DefaultProcessSortMode;
        var showHelp = false;

        while (!cancellationToken.IsCancellationRequested)
        {
            var started = DateTimeOffset.UtcNow;
            var snapshot = await _snapshotService.CollectAsync(config, sortMode, cancellationToken);
            _renderer.Render(snapshot, config, sortMode, showHelp);

            var key = ReadKeyIfAvailable();
            if (key.HasValue)
            {
                switch (char.ToLowerInvariant(key.Value.KeyChar))
                {
                    case 'q':
                        _logger.LogInformation("Quit requested");
                        return;
                    case 'r':
                        continue;
                    case 'c':
                        sortMode = ProcessSortMode.Cpu;
                        break;
                    case 'm':
                        sortMode = ProcessSortMode.Memory;
                        break;
                    case 'n':
                        sortMode = ProcessSortMode.Name;
                        break;
                    case 'h':
                        showHelp = !showHelp;
                        break;
                }
            }

            var elapsed = DateTimeOffset.UtcNow - started;
            var delay = TimeSpan.FromSeconds(Math.Max(0.2, config.RefreshIntervalSeconds)) - elapsed;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    private static ConsoleKeyInfo? ReadKeyIfAvailable()
    {
        try
        {
            if (!Console.KeyAvailable)
            {
                return null;
            }

            return Console.ReadKey(intercept: true);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }
}
