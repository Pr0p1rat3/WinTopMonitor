using System.Diagnostics;
using Microsoft.Extensions.Logging;
using WinTopMonitor.Models;

namespace WinTopMonitor.Services;

public sealed class CpuMonitor : IDisposable
{
    private readonly ILogger<CpuMonitor> _logger;
    private readonly SystemInfoService _systemInfo;
    private readonly PerformanceCounter? _totalCounter;
    private readonly List<PerformanceCounter> _coreCounters = [];

    public CpuMonitor(SystemInfoService systemInfo, ILogger<CpuMonitor> logger)
    {
        _systemInfo = systemInfo;
        _logger = logger;

        try
        {
            _totalCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", readOnly: true);
            foreach (var instance in new PerformanceCounterCategory("Processor").GetInstanceNames().Where(x => x != "_Total").OrderBy(x => x))
            {
                _coreCounters.Add(new PerformanceCounter("Processor", "% Processor Time", instance, readOnly: true));
            }

            _ = _totalCounter.NextValue();
            foreach (var counter in _coreCounters)
            {
                _ = counter.NextValue();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CPU performance counters unavailable");
        }
    }

    public CpuSnapshot Collect()
    {
        var overall = ReadCounter(_totalCounter);
        var perCore = _coreCounters.Count == 0
            ? Enumerable.Repeat(overall, Environment.ProcessorCount).ToArray()
            : _coreCounters.Select(ReadCounter).ToArray();

        return new CpuSnapshot(
            _systemInfo.CpuModelName(),
            overall,
            _systemInfo.CpuFrequencyMhz(),
            perCore);
    }

    private static double ReadCounter(PerformanceCounter? counter)
    {
        if (counter is null)
        {
            return 0;
        }

        try
        {
            return Math.Clamp(counter.NextValue(), 0, 100);
        }
        catch
        {
            return 0;
        }
    }

    public void Dispose()
    {
        _totalCounter?.Dispose();
        foreach (var counter in _coreCounters)
        {
            counter.Dispose();
        }
    }
}

