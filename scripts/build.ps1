<#
.SYNOPSIS
Builds WinTop Monitor with the dotnet CLI.
#>
[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$Publish
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "src\WinTopMonitor\WinTopMonitor.csproj"

dotnet restore $project
dotnet build $project --configuration $Configuration --no-restore

if ($Publish) {
    dotnet publish $project `
        --configuration $Configuration `
        --runtime $Runtime `
        --self-contained false `
        -p:PublishSingleFile=true `
        -p:DebugType=embedded `
        --output (Join-Path $root "artifacts\publish\$Runtime")
}

