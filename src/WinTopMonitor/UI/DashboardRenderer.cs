using Spectre.Console;
using Spectre.Console.Rendering;
using WinTopMonitor.Config;
using WinTopMonitor.Models;
using WinTopMonitor.Utils;

namespace WinTopMonitor.UI;

public sealed class DashboardRenderer
{
    public void Render(SystemSnapshot snapshot, AppConfig config, ProcessSortMode sortMode, bool showHelp)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(Header(snapshot));

        var grid = new Grid();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddRow(CpuPanel(snapshot.Cpu, config), MemoryPanel(snapshot.Memory, config));
        grid.AddRow(DiskPanel(snapshot.Disks, config), NetworkPanel(snapshot.NetworkAdapters));
        grid.AddRow(ProcessPanel("Processes", snapshot.TopCpuProcesses, sortMode), ProcessPanel("Memory Leaders", snapshot.TopMemoryProcesses, ProcessSortMode.Memory));
        AnsiConsole.Write(grid);

        if (showHelp)
        {
            AnsiConsole.Write(HelpPanel());
        }

        AnsiConsole.Write(Footer(sortMode, config));
    }

    private static Panel Header(SystemSnapshot snapshot)
    {
        var system = snapshot.System;
        var text =
            $"[bold aqua]WinTop Monitor[/]  {Markup.Escape(snapshot.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"))}\n" +
            $"{Markup.Escape(system.HostName)} | {Markup.Escape(system.UserName)} | {Markup.Escape(system.OsVersion)} | {Markup.Escape(system.Architecture)} | uptime {Markup.Escape(Formatters.Duration(system.Uptime))}";
        return new Panel(text).Border(BoxBorder.Rounded).Header("System");
    }

    private static Panel CpuPanel(CpuSnapshot cpu, AppConfig config)
    {
        var table = new Table().NoBorder();
        table.AddColumn("Metric");
        table.AddColumn("Value");
        table.AddRow("Model", Markup.Escape(cpu.ModelName));
        table.AddRow("Overall", Usage(cpu.OverallUsagePercent, config.Thresholds.Cpu));
        table.AddRow("Frequency", cpu.FrequencyMhz.HasValue ? $"{cpu.FrequencyMhz:0} MHz" : "n/a");

        for (var i = 0; i < cpu.PerCoreUsagePercent.Count; i++)
        {
            table.AddRow($"Core {i}", Usage(cpu.PerCoreUsagePercent[i], config.Thresholds.Cpu));
        }

        return new Panel(table).Border(BoxBorder.Rounded).Header("CPU");
    }

    private static Panel MemoryPanel(MemorySnapshot memory, AppConfig config)
    {
        var table = new Table().NoBorder();
        table.AddColumn("Metric");
        table.AddColumn("Value");
        table.AddRow("Used", $"{Formatters.Bytes(memory.UsedBytes)} / {Formatters.Bytes(memory.TotalBytes)}");
        table.AddRow("Available", Formatters.Bytes(memory.AvailableBytes));
        table.AddRow("Usage", Usage(memory.UsagePercent, config.Thresholds.Memory));
        return new Panel(table).Border(BoxBorder.Rounded).Header("Memory");
    }

    private static Panel DiskPanel(IReadOnlyList<DiskSnapshot> disks, AppConfig config)
    {
        var table = new Table().Expand();
        table.AddColumn("Drive");
        table.AddColumn("Used");
        table.AddColumn("Free");
        table.AddColumn("Usage");
        table.AddColumn("I/O");

        foreach (var disk in disks)
        {
            var io = disk.ReadBytesPerSecond.HasValue || disk.WriteBytesPerSecond.HasValue
                ? $"R {Formatters.Rate(disk.ReadBytesPerSecond ?? 0)} / W {Formatters.Rate(disk.WriteBytesPerSecond ?? 0)}"
                : "n/a";
            table.AddRow(
                Markup.Escape($"{disk.Name} {disk.VolumeLabel}".Trim()),
                Formatters.Bytes(disk.TotalBytes - disk.FreeBytes),
                Formatters.Bytes(disk.FreeBytes),
                Usage(disk.UsagePercent, config.Thresholds.Disk),
                Markup.Escape(io));
        }

        return new Panel(table).Border(BoxBorder.Rounded).Header("Disk");
    }

    private static Panel NetworkPanel(IReadOnlyList<NetworkAdapterSnapshot> adapters)
    {
        var table = new Table().Expand();
        table.AddColumn("Adapter");
        table.AddColumn("IPv4");
        table.AddColumn("Down");
        table.AddColumn("Up");
        table.AddColumn("Total");

        foreach (var adapter in adapters)
        {
            table.AddRow(
                Markup.Escape(adapter.Name),
                Markup.Escape(adapter.IPv4Addresses.Count == 0 ? "n/a" : string.Join(", ", adapter.IPv4Addresses)),
                Formatters.Rate(adapter.DownloadBytesPerSecond),
                Formatters.Rate(adapter.UploadBytesPerSecond),
                $"{Formatters.Bytes(adapter.BytesReceived)} / {Formatters.Bytes(adapter.BytesSent)}");
        }

        return new Panel(table).Border(BoxBorder.Rounded).Header("Network");
    }

    private static Panel ProcessPanel(string title, IReadOnlyList<ProcessSnapshot> processes, ProcessSortMode sortMode)
    {
        var table = new Table().Expand();
        table.AddColumn("Name");
        table.AddColumn("PID");
        table.AddColumn("CPU");
        table.AddColumn("Memory");
        table.AddColumn("Path");

        foreach (var process in processes)
        {
            table.AddRow(
                Markup.Escape(process.Name),
                process.ProcessId.ToString(),
                Formatters.Percent(process.CpuPercent),
                Formatters.Bytes(process.MemoryBytes),
                Markup.Escape(process.Path ?? "access denied"));
        }

        return new Panel(table).Border(BoxBorder.Rounded).Header($"{title} ({sortMode})");
    }

    private static Panel HelpPanel()
    {
        return new Panel("q quit | r refresh now | c sort by CPU | m sort by memory | n sort by name | h toggle help")
            .Border(BoxBorder.Rounded)
            .Header("Help");
    }

    private static IRenderable Footer(ProcessSortMode sortMode, AppConfig config)
    {
        return new Markup($"[grey]q quit | r refresh | c cpu | m memory | n name | h help | sort {sortMode} | refresh {config.RefreshIntervalSeconds:0.##}s[/]");
    }

    private static string Usage(double percent, ResourceThreshold threshold)
    {
        var level = ThresholdClassifier.Classify(percent, threshold);
        var color = level switch
        {
            ThresholdLevel.Critical => "red",
            ThresholdLevel.Warning => "yellow",
            _ => "green"
        };

        return $"[{color}]{Bar(percent)} {Formatters.Percent(percent)}[/]";
    }

    private static string Bar(double percent)
    {
        const int width = 20;
        var filled = (int)Math.Round(Math.Clamp(percent, 0, 100) / 100 * width);
        return "|" + new string('#', filled) + new string('-', width - filled) + "|";
    }
}
