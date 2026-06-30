using WinTopMonitor.Config;
using WinTopMonitor.Models;

namespace WinTopMonitor.Services;

public sealed class SystemSnapshotService
{
    private readonly SystemInfoService _systemInfo;
    private readonly CpuMonitor _cpuMonitor;
    private readonly MemoryMonitor _memoryMonitor;
    private readonly DiskMonitor _diskMonitor;
    private readonly NetworkMonitor _networkMonitor;
    private readonly ProcessMonitor _processMonitor;

    public SystemSnapshotService(
        SystemInfoService systemInfo,
        CpuMonitor cpuMonitor,
        MemoryMonitor memoryMonitor,
        DiskMonitor diskMonitor,
        NetworkMonitor networkMonitor,
        ProcessMonitor processMonitor)
    {
        _systemInfo = systemInfo;
        _cpuMonitor = cpuMonitor;
        _memoryMonitor = memoryMonitor;
        _diskMonitor = diskMonitor;
        _networkMonitor = networkMonitor;
        _processMonitor = processMonitor;
    }

    public Task<SystemSnapshot> CollectAsync(AppConfig config, ProcessSortMode sortMode, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var processSets = _processMonitor.Collect(config.ProcessesToShow, sortMode);
        var snapshot = new SystemSnapshot(
            DateTimeOffset.Now,
            _systemInfo.Collect(),
            _cpuMonitor.Collect(),
            _memoryMonitor.Collect(),
            _diskMonitor.Collect(),
            _networkMonitor.Collect(config),
            processSets.TopCpu,
            processSets.TopMemory);

        return Task.FromResult(snapshot);
    }
}

