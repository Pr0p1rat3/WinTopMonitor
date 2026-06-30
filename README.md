# WinTop Monitor

WinTop Monitor is a lightweight Windows 10 and Windows 11 terminal-style system resource monitor inspired by the purpose of `btop` on Linux. It runs as a normal user by default and displays real-time CPU, memory, disk, network, process, and system health information in a clean console dashboard.

The application is transparent local monitoring software. It does not install a service, auto-start task, credential collector, browser data reader, keylogger, hidden window, persistence mechanism, evasion feature, security-control bypass, or unauthorized elevation path.

## Features

- Real-time terminal dashboard using Spectre.Console.
- Configurable refresh interval, default `1` second.
- System identity: date/time, uptime, hostname, OS version, logged-in user, and architecture.
- CPU: overall usage, per-core usage, CPU model, estimated frequency, and top CPU processes.
- Memory: total, used, available, usage percentage, and top memory processes.
- Disk: fixed drives, total/used/free space, usage percentage, optional read/write rates.
- Network: active adapters, IPv4 addresses, upload/download rates, and total bytes.
- Processes: process name, PID, CPU %, memory, and executable path when accessible.
- Graceful handling for access-denied process details.
- Keyboard shortcuts: `q`, `r`, `c`, `m`, `n`, `h`.
- Structured local JSONL logs.
- No external telemetry.

## Screenshots

<img width="1891" height="609" alt="image" src="https://github.com/user-attachments/assets/5ea6ff8e-8fe7-47ca-a0f9-c122c12fa15a" />

## Requirements

- Windows 10 or Windows 11.
- .NET 8 SDK to build.
- Windows SDK only when signing with `signtool.exe`.
- WiX Toolset v4 only when building the MSI installer.

## Build

```powershell
cd WinTopMonitor
dotnet build
```

Release publish:

```powershell
.\scripts\build.ps1 -Configuration Release -Runtime win-x64 -Publish
```

## Run

```powershell
dotnet run --project src\WinTopMonitor\WinTopMonitor.csproj
```

From a published folder:

```powershell
.\artifacts\publish\win-x64\WinTopMonitor.exe
```

## Keyboard Shortcuts

- `q`: quit
- `r`: refresh now
- `c`: sort primary process panel by CPU
- `m`: sort primary process panel by memory
- `n`: sort primary process panel by name
- `h`: toggle help

## Configuration

Configuration is stored at:

```text
%AppData%\WinTopMonitor\config.json
```

Default configuration is created automatically:

```json
{
  "refreshIntervalSeconds": 1,
  "processesToShow": 10,
  "defaultProcessSortMode": "cpu",
  "showDisconnectedNetworkAdapters": false,
  "thresholds": {
    "cpu": { "warning": 70, "critical": 90 },
    "memory": { "warning": 70, "critical": 90 },
    "disk": { "warning": 70, "critical": 90 }
  }
}
```

## Logs

Structured logs are written locally to:

```text
%LocalAppData%\WinTopMonitor\logs
```

Logs include startup, shutdown, and exception events. They do not collect sensitive information and are never transmitted externally by the application.

## Installer

The WiX installer scaffold is in `installer\Product.wxs`.

Build an MSI:

```powershell
dotnet tool install --global wix
.\scripts\package.ps1 -Configuration Release
```

Installer behavior:

- Installs under Program Files.
- Creates a Start Menu shortcut.
- Supports uninstall.
- Adds the app folder to PATH only when `-AddToPath` is passed to `package.ps1`.
- Does not install services, startup tasks, drivers, browser extensions, or background agents.
- Does not auto-start on boot.

## Code Signing

Sign an executable or MSI with a certificate thumbprint:

```powershell
.\scripts\sign.ps1 `
  -File .\artifacts\installer\WinTopMonitor.msi `
  -Thumbprint "<certificate thumbprint>" `
  -TimestampServer "http://timestamp.digicert.com"
```

Sign with a PFX:

```powershell
$password = Read-Host -AsSecureString "PFX password"
.\scripts\sign.ps1 `
  -File .\artifacts\publish\win-x64\WinTopMonitor.exe `
  -CertificatePath C:\certs\code-signing.pfx `
  -CertificatePassword $password
```

Verify:

```powershell
signtool verify /pa /v .\artifacts\installer\WinTopMonitor.msi
```

Code signing improves trust, but it does not guarantee the app will never be flagged. Reputation, safe behavior, clean packaging, and vendor submission processes may still be required.

## Security Design Notes

- Runs as normal user by default using `asInvoker`.
- Uses documented .NET, Windows performance counter, registry, network, drive, process, and memory APIs.
- Does not request elevation for basic functionality.
- Does not collect credentials, browser data, keystrokes, password hashes, or LSASS data.
- Does not disable Microsoft Defender, SmartScreen, EDR, PUA controls, or antivirus products.
- Does not include obfuscation, anti-analysis behavior, stealth behavior, or persistence.
- Admin rights should only be used by the operator for future explicit advanced features that require them.

## Troubleshooting

- Some process paths may show `access denied`; this is normal for protected or higher-integrity processes.
- CPU counters may display `0` briefly after startup while Windows performance counters warm up.
- Disk read/write activity depends on available Windows performance counters.
- If `wix` is not found, install WiX v4 with `dotnet tool install --global wix`.
- If `signtool.exe` is not found, install the Windows SDK and add the SDK tools folder to PATH.

## Known Limitations

- The TUI is optimized for Windows Terminal or a modern console host.
- No WinUI/WPF GUI is included in this MVP.
- MSIX packaging is documented as a future option; the included installer scaffold uses WiX MSI.
- Per-process CPU usage is sample-based and becomes accurate after the first refresh.

