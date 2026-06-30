namespace WinTopMonitor.Utils;

public static class Formatters
{
    public static string Bytes(double bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB", "PB"];
        var value = Math.Max(0, bytes);
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value:0.##} {units[unit]}";
    }

    public static string Rate(double bytesPerSecond) => $"{Bytes(bytesPerSecond)}/s";

    public static string Percent(double value) => $"{value:0.0}%";

    public static string Duration(TimeSpan value)
    {
        if (value.TotalDays >= 1)
        {
            return $"{(int)value.TotalDays}d {value.Hours}h {value.Minutes}m";
        }

        return $"{value.Hours}h {value.Minutes}m {value.Seconds}s";
    }
}

