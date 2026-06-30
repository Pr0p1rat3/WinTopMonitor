using System.Diagnostics;
using WinTopMonitor.Models;

namespace WinTopMonitor.Services;

public sealed class DiskMonitor : IDisposable
{
    private readonly Dictionary<string, (PerformanceCounter Read, PerformanceCounter Write)> _counters = new(StringComparer.OrdinalIgnoreCase);

    public DiskMonitor()
    {
        try
        {
            foreach (var instance in new PerformanceCounterCategory("LogicalDisk").GetInstanceNames().Where(x => x != "_Total"))
            {
                _counters[instance] = (
                    new PerformanceCounter("LogicalDisk", "Disk Read Bytes/sec", instance, readOnly: true),
                    new PerformanceCounter("LogicalDisk", "Disk Write Bytes/sec", instance, readOnly: true));
            }
        }
        catch
        {
            // Disk activity is optional. Space metrics still work without performance counters.
        }
    }

    public IReadOnlyList<DiskSnapshot> Collect()
    {
        return DriveInfo.GetDrives()
            .Where(drive => drive.DriveType == DriveType.Fixed && drive.IsReady)
            .Select(ToSnapshot)
            .ToArray();
    }

    private DiskSnapshot ToSnapshot(DriveInfo drive)
    {
        var used = drive.TotalSize - drive.AvailableFreeSpace;
        var percent = drive.TotalSize == 0 ? 0 : used * 100.0 / drive.TotalSize;
        var key = drive.Name.TrimEnd('\\');
        var read = ReadCounter(key, counter => counter.Read);
        var write = ReadCounter(key, counter => counter.Write);

        return new DiskSnapshot(
            drive.Name,
            drive.VolumeLabel,
            drive.TotalSize,
            drive.AvailableFreeSpace,
            percent,
            read,
            write);
    }

    private double? ReadCounter(string key, Func<(PerformanceCounter Read, PerformanceCounter Write), PerformanceCounter> selector)
    {
        try
        {
            return _counters.TryGetValue(key, out var counterSet)
                ? Math.Max(0, selector(counterSet).NextValue())
                : null;
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        foreach (var pair in _counters.Values)
        {
            pair.Read.Dispose();
            pair.Write.Dispose();
        }
    }
}

