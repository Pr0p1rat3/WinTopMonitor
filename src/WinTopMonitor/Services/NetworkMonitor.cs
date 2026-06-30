using System.Net.NetworkInformation;
using System.Net.Sockets;
using WinTopMonitor.Config;
using WinTopMonitor.Models;

namespace WinTopMonitor.Services;

public sealed class NetworkMonitor
{
    private readonly Dictionary<string, (long Sent, long Received, DateTimeOffset Timestamp)> _previous = new();

    public IReadOnlyList<NetworkAdapterSnapshot> Collect(AppConfig config)
    {
        var now = DateTimeOffset.UtcNow;
        var snapshots = new List<NetworkAdapterSnapshot>();

        foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (!config.ShowDisconnectedNetworkAdapters && adapter.OperationalStatus != OperationalStatus.Up)
            {
                continue;
            }

            if (adapter.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
            {
                continue;
            }

            var stats = adapter.GetIPv4Statistics();
            var key = adapter.Id;
            var upload = 0.0;
            var download = 0.0;
            if (_previous.TryGetValue(key, out var previous))
            {
                var seconds = Math.Max(0.001, (now - previous.Timestamp).TotalSeconds);
                upload = Math.Max(0, (stats.BytesSent - previous.Sent) / seconds);
                download = Math.Max(0, (stats.BytesReceived - previous.Received) / seconds);
            }

            _previous[key] = (stats.BytesSent, stats.BytesReceived, now);

            var addresses = adapter.GetIPProperties().UnicastAddresses
                .Where(address => address.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(address => address.Address.ToString())
                .ToArray();

            snapshots.Add(new NetworkAdapterSnapshot(
                adapter.Name,
                adapter.Description,
                adapter.OperationalStatus.ToString(),
                addresses,
                stats.BytesSent,
                stats.BytesReceived,
                upload,
                download));
        }

        return snapshots;
    }
}

