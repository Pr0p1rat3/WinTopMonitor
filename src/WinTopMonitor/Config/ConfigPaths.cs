namespace WinTopMonitor.Config;

public static class ConfigPaths
{
    public static string ConfigDirectory =>
        Environment.GetEnvironmentVariable("WINTOP_CONFIG_DIR")
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WinTopMonitor");

    public static string ConfigFile => Path.Combine(ConfigDirectory, "config.json");
}
