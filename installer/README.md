# Installer Scaffolding

The WiX v4 project installs WinTop Monitor under Program Files, creates a Start Menu shortcut, and supports optional PATH registration through `scripts/package.ps1 -AddToPath`.

It does not install a Windows service, scheduled task, startup entry, driver, browser extension, or any persistence mechanism.

Build an MSI on Windows:

```powershell
dotnet tool install --global wix
.\scripts\package.ps1 -Configuration Release
```

Sign the MSI:

```powershell
.\scripts\sign.ps1 -File .\artifacts\installer\WinTopMonitor.msi -Thumbprint "<certificate thumbprint>"
```

