using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace WinTopMonitor.Config;

public sealed class ConfigService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly ILogger<ConfigService> _logger;

    public ConfigService(ILogger<ConfigService> logger)
    {
        _logger = logger;
    }

    public async Task<AppConfig> LoadOrCreateAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(ConfigPaths.ConfigDirectory);

        if (!File.Exists(ConfigPaths.ConfigFile))
        {
            var defaultConfig = new AppConfig();
            await SaveAsync(defaultConfig, cancellationToken);
            _logger.LogInformation("Created default configuration at {ConfigFile}", ConfigPaths.ConfigFile);
            return defaultConfig;
        }

        try
        {
            await using var stream = File.OpenRead(ConfigPaths.ConfigFile);
            var config = await JsonSerializer.DeserializeAsync<AppConfig>(stream, JsonOptions, cancellationToken);
            return Normalize(config ?? new AppConfig());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read configuration, using defaults");
            return new AppConfig();
        }
    }

    public async Task SaveAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(ConfigPaths.ConfigDirectory);
        await using var stream = File.Create(ConfigPaths.ConfigFile);
        await JsonSerializer.SerializeAsync(stream, Normalize(config), JsonOptions, cancellationToken);
    }

    private static AppConfig Normalize(AppConfig config)
    {
        config.RefreshIntervalSeconds = Math.Clamp(config.RefreshIntervalSeconds, 0.2, 60);
        config.ProcessesToShow = Math.Clamp(config.ProcessesToShow, 1, 100);
        config.Thresholds ??= new ThresholdConfig();
        config.Thresholds.Cpu ??= new ResourceThreshold();
        config.Thresholds.Memory ??= new ResourceThreshold();
        config.Thresholds.Disk ??= new ResourceThreshold();
        return config;
    }
}

