using Microsoft.Extensions.Logging.Abstractions;
using WinTopMonitor.Config;
using WinTopMonitor.Models;

namespace WinTopMonitor.Tests;

public sealed class ConfigServiceTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), "WinTopMonitorTests", Guid.NewGuid().ToString("N"));

    public ConfigServiceTests()
    {
        Environment.SetEnvironmentVariable("WINTOP_CONFIG_DIR", _tempDir);
    }

    [Fact]
    public async Task LoadOrCreateAsyncCreatesDefaultConfig()
    {
        var service = new ConfigService(NullLogger<ConfigService>.Instance);

        var config = await service.LoadOrCreateAsync();

        Assert.True(File.Exists(Path.Combine(_tempDir, "config.json")));
        Assert.Equal(1.0, config.RefreshIntervalSeconds);
        Assert.Equal(10, config.ProcessesToShow);
        Assert.Equal(ProcessSortMode.Cpu, config.DefaultProcessSortMode);
    }

    [Fact]
    public async Task LoadOrCreateAsyncNormalizesUnsafeValues()
    {
        Directory.CreateDirectory(_tempDir);
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "config.json"), """
        {
          "refreshIntervalSeconds": 0.01,
          "processesToShow": 500,
          "defaultProcessSortMode": "memory",
          "showDisconnectedNetworkAdapters": true
        }
        """);

        var service = new ConfigService(NullLogger<ConfigService>.Instance);
        var config = await service.LoadOrCreateAsync();

        Assert.Equal(0.2, config.RefreshIntervalSeconds);
        Assert.Equal(100, config.ProcessesToShow);
        Assert.Equal(ProcessSortMode.Memory, config.DefaultProcessSortMode);
        Assert.True(config.ShowDisconnectedNetworkAdapters);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("WINTOP_CONFIG_DIR", null);
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }
}

