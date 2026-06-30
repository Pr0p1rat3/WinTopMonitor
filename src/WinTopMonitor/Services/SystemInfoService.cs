using System.Runtime.InteropServices;
using Microsoft.Win32;
using WinTopMonitor.Models;

namespace WinTopMonitor.Services;

public sealed class SystemInfoService
{
    private readonly string _hostName = Environment.MachineName;
    private readonly string _userName = Environment.UserName;
    private readonly string _osVersion = RuntimeInformation.OSDescription.Trim();
    private readonly string _architecture = RuntimeInformation.OSArchitecture.ToString();

    public SystemInfoSnapshot Collect()
    {
        return new SystemInfoSnapshot(
            _hostName,
            _userName,
            _osVersion,
            _architecture,
            TimeSpan.FromMilliseconds(Environment.TickCount64));
    }

    public string CpuModelName()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            return key?.GetValue("ProcessorNameString")?.ToString()?.Trim() ?? "Unknown CPU";
        }
        catch
        {
            return "Unknown CPU";
        }
    }

    public double? CpuFrequencyMhz()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            var value = key?.GetValue("~MHz");
            return value is null ? null : Convert.ToDouble(value);
        }
        catch
        {
            return null;
        }
    }
}

