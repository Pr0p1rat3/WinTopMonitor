namespace WinTopMonitor.Models;

public sealed record SystemSnapshot(
    DateTimeOffset Timestamp,
    SystemInfoSnapshot System,
    CpuSnapshot Cpu,
    MemorySnapshot Memory,
    IReadOnlyList<DiskSnapshot> Disks,
    IReadOnlyList<NetworkAdapterSnapshot> NetworkAdapters,
    IReadOnlyList<ProcessSnapshot> TopCpuProcesses,
    IReadOnlyList<ProcessSnapshot> TopMemoryProcesses);

public sealed record SystemInfoSnapshot(
    string HostName,
    string UserName,
    string OsVersion,
    string Architecture,
    TimeSpan Uptime);

public sealed record CpuSnapshot(
    string ModelName,
    double OverallUsagePercent,
    double? FrequencyMhz,
    IReadOnlyList<double> PerCoreUsagePercent);

public sealed record MemorySnapshot(
    ulong TotalBytes,
    ulong UsedBytes,
    ulong AvailableBytes,
    double UsagePercent);

public sealed record DiskSnapshot(
    string Name,
    string VolumeLabel,
    long TotalBytes,
    long FreeBytes,
    double UsagePercent,
    double? ReadBytesPerSecond,
    double? WriteBytesPerSecond);

public sealed record NetworkAdapterSnapshot(
    string Name,
    string Description,
    string OperationalStatus,
    IReadOnlyList<string> IPv4Addresses,
    long BytesSent,
    long BytesReceived,
    double UploadBytesPerSecond,
    double DownloadBytesPerSecond);

public sealed record ProcessSnapshot(
    int ProcessId,
    string Name,
    double CpuPercent,
    long MemoryBytes,
    string? Path);

