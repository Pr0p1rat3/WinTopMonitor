using WinTopMonitor.Models;

namespace WinTopMonitor.Config;

public sealed class AppConfig
{
    public double RefreshIntervalSeconds { get; set; } = 1.0;
    public int ProcessesToShow { get; set; } = 10;
    public ProcessSortMode DefaultProcessSortMode { get; set; } = ProcessSortMode.Cpu;
    public bool ShowDisconnectedNetworkAdapters { get; set; } = false;
    public ThresholdConfig Thresholds { get; set; } = new();
}

public sealed class ThresholdConfig
{
    public ResourceThreshold Cpu { get; set; } = new();
    public ResourceThreshold Memory { get; set; } = new();
    public ResourceThreshold Disk { get; set; } = new();
}

public sealed class ResourceThreshold
{
    public double Warning { get; set; } = 70;
    public double Critical { get; set; } = 90;
}

