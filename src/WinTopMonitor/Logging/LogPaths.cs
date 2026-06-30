namespace WinTopMonitor.Logging;

public static class LogPaths
{
    public static string LogDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinTopMonitor", "logs");
}

