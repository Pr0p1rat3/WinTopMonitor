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

            var elapsed = DateTimeOffset.UtcNow - started;
            var delay = TimeSpan.FromSeconds(Math.Max(0.2, config.RefreshIntervalSeconds)) - elapsed;

            var waitStart = DateTimeOffset.UtcNow;
            var forceRefresh = false;

            do
            {
                var key = ReadKeyIfAvailable();
                if (key.HasValue)
                {
                    switch (char.ToLowerInvariant(key.Value.KeyChar))
                    {
                        case 'q':
                            _logger.LogInformation("Quit requested");
                            return;
                        case 'r':
                            forceRefresh = true;
                            break;
                        case 'c':
                            sortMode = ProcessSortMode.Cpu;
                            forceRefresh = true;
                            break;
                        case 'm':
                            sortMode = ProcessSortMode.Memory;
                            forceRefresh = true;
                            break;
                        case 'n':
                            sortMode = ProcessSortMode.Name;
                            forceRefresh = true;
                            break;
                        case 'h':
                            showHelp = !showHelp;
                            forceRefresh = true;
                            break;
                    }
                }

                if (forceRefresh)
                {
                    break;
                }

                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(Math.Min(50, (int)delay.TotalMilliseconds), cancellationToken);
                }
            }
            while (!cancellationToken.IsCancellationRequested && (DateTimeOffset.UtcNow - waitStart) < delay);
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
