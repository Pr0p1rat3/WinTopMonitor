<#
.SYNOPSIS
Signs an executable or installer with Authenticode signtool.exe.
#>
[CmdletBinding(DefaultParameterSetName = "Thumbprint")]
param(
    [Parameter(Mandatory = $true)]
    [string]$File,

    [Parameter(ParameterSetName = "Pfx", Mandatory = $true)]
    [string]$CertificatePath,

    [Parameter(ParameterSetName = "Pfx")]
    [securestring]$CertificatePassword,

    [Parameter(ParameterSetName = "Thumbprint", Mandatory = $true)]
    [string]$Thumbprint,

    [string]$TimestampServer = "http://timestamp.digicert.com"
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

if (-not (Test-Path $File)) {
    throw "File not found: $File"
}

$signtool = Get-Command signtool.exe -ErrorAction SilentlyContinue
if (-not $signtool) {
    throw "signtool.exe was not found. Install the Windows SDK and ensure signtool.exe is in PATH."
}

if ($PSCmdlet.ParameterSetName -eq "Pfx") {
    $args = @("sign", "/fd", "SHA256", "/tr", $TimestampServer, "/td", "SHA256", "/f", $CertificatePath)
    if ($CertificatePassword) {
        $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($CertificatePassword)
        try {
            $plain = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
            $args += @("/p", $plain)
        }
        finally {
            [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
        }
    }
    $args += $File
} else {
    $args = @("sign", "/fd", "SHA256", "/tr", $TimestampServer, "/td", "SHA256", "/sha1", $Thumbprint, $File)
}

& $signtool.Source @args
& $signtool.Source verify /pa /v $File
