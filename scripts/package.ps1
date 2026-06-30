<#
.SYNOPSIS
Packages WinTop Monitor with WiX when WiX is installed.
#>
[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$AddToPath
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $root "artifacts\publish\$Runtime"
$installerDir = Join-Path $root "installer"
$wxs = Join-Path $installerDir "Product.wxs"
$msiOut = Join-Path $root "artifacts\installer"

& (Join-Path $PSScriptRoot "build.ps1") -Configuration $Configuration -Runtime $Runtime -Publish

if (-not (Get-Command wix -ErrorAction SilentlyContinue)) {
    throw "WiX Toolset v4 CLI was not found. Install with: dotnet tool install --global wix"
}

New-Item -ItemType Directory -Path $msiOut -Force | Out-Null

$definePath = if ($AddToPath) { "AddToPath=1" } else { "AddToPath=0" }
wix build $wxs `
    -d "PublishDir=$publishDir" `
    -d $definePath `
    -out (Join-Path $msiOut "WinTopMonitor.msi")

