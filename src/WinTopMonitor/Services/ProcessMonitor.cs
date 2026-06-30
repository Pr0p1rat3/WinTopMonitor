using System.Diagnostics;
using WinTopMonitor.Models;

namespace WinTopMonitor.Services;

public sealed class ProcessMonitor
{
    private readonly Dictionary<int, (TimeSpan CpuTime, DateTimeOffset Timestamp)> _previousCpu = new();

    public (IReadOnlyList<ProcessSnapshot> TopCpu, IReadOnlyList<ProcessSnapshot> TopMemory) Collect(int count, ProcessSortMode sortMode)
    {
        var now = DateTimeOffset.UtcNow;
        var processes = new List<ProcessSnapshot>();

        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                try
                {
                    var cpuTime = process.TotalProcessorTime;
                    var cpuPercent = CalculateCpuPercent(process.Id, cpuTime, now);
                    processes.Add(new ProcessSnapshot(
                        process.Id,
                        process.ProcessName,
                        cpuPercent,
                        process.WorkingSet64,
                        TryGetProcessPath(process)));
                }
                catch
                {
                    // Access-denied and short-lived processes are expected on normal-user runs.
                }
            }
        }

        var topCpu = processes
            .OrderByDescending(process => process.CpuPercent)
            .ThenBy(process => process.Name)
            .Take(count)
            .ToArray();

        var topMemory = processes
            .OrderByDescending(process => process.MemoryBytes)
            .ThenBy(process => process.Name)
            .Take(count)
            .ToArray();

        return (ApplySort(processes, sortMode).Take(count).ToArray(), topMemory.Length > 0 ? topMemory : topCpu);
    }

    private double CalculateCpuPercent(int processId, TimeSpan cpuTime, DateTimeOffset now)
    {
        if (!_previousCpu.TryGetValue(processId, out var previous))
        {
            _previousCpu[processId] = (cpuTime, now);
            return 0;
        }

        _previousCpu[processId] = (cpuTime, now);
        var elapsedMs = (now - previous.Timestamp).TotalMilliseconds;
        if (elapsedMs <= 0)
        {
            return 0;
        }

        var cpuMs = (cpuTime - previous.CpuTime).TotalMilliseconds;
        return Math.Clamp(cpuMs / elapsedMs / Environment.ProcessorCount * 100.0, 0, 100);
    }

    private static IEnumerable<ProcessSnapshot> ApplySort(IEnumerable<ProcessSnapshot> processes, ProcessSortMode sortMode)
    {
        return sortMode switch
        {
            ProcessSortMode.Memory => processes.OrderByDescending(process => process.MemoryBytes).ThenBy(process => process.Name),
            ProcessSortMode.Name => processes.OrderBy(process => process.Name),
            _ => processes.OrderByDescending(process => process.CpuPercent).ThenBy(process => process.Name)
        };
    }

    private static string? TryGetProcessPath(Process process)
    {
        try
        {
            return process.MainModule?.FileName;
        }
        catch
        {
            return null;
        }
    }
}

