using System.Runtime.InteropServices;
using WinTopMonitor.Models;

namespace WinTopMonitor.Services;

public sealed class MemoryMonitor
{
    public MemorySnapshot Collect()
    {
        var status = new MemoryStatusEx();
        if (!GlobalMemoryStatusEx(status))
        {
            return new MemorySnapshot(0, 0, 0, 0);
        }

        var used = status.TotalPhys - status.AvailPhys;
        var percent = status.TotalPhys == 0 ? 0 : used * 100.0 / status.TotalPhys;
        return new MemorySnapshot(status.TotalPhys, used, status.AvailPhys, percent);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatusEx lpBuffer);

    [StructLayout(LayoutKind.Sequential)]
    private sealed class MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhys;
        public ulong AvailPhys;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;

        public MemoryStatusEx()
        {
            Length = (uint)Marshal.SizeOf<MemoryStatusEx>();
        }
    }
}

