<#
    Install-RoadChangerCertificate.ps1   <-- THE "certificate install" you drop on each machine
    ----------------------------------
    Run as ADMINISTRATOR on each end-user machine. It:
      1. Installs the PUBLIC cert (RoadChangerAddIn-Public.cer) into
         Trusted Publishers (so ArcGIS Pro trusts the signed add-in), and into
         Trusted Root (needed only because the cert is self-signed).
      2. Optionally copies the signed .esriAddinX into the user's AddIns folder.

    Ship this script together with:
      * RoadChangerAddIn-Public.cer
      * RoadChangerAddIn.esriAddinX   (the SIGNED add-in)
    ...in the same folder, then have the user right-click > Run with PowerShell
    (as admin), or run:  powershell -ExecutionPolicy Bypass -File .\Install-RoadChangerCertificate.ps1 -DeployAddIn

    For a COMMERCIAL CA cert you can skip the Trusted Root import (-SkipRoot).
#>

param(
    [string]$CerPath   = (Join-Path $PSScriptRoot "RoadChangerAddIn-Public.cer"),
    [string]$AddInPath = (Join-Path $PSScriptRoot "RoadChangerAddIn.esriAddinX"),
    [switch]$DeployAddIn,
    [switch]$SkipRoot
)

$ErrorActionPreference = "Stop"

# --- require elevation (LocalMachine stores need admin) ---
$principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Please run this script as Administrator (right-click > Run as administrator)."
}

if (-not (Test-Path $CerPath)) { throw "Public certificate not found: $CerPath" }

Import-Certificate -FilePath $CerPath -CertStoreLocation "Cert:\LocalMachine\TrustedPublisher" | Out-Null
Write-Host "Installed into Trusted Publishers (LocalMachine)." -ForegroundColor Green

if (-not $SkipRoot) {
    Import-Certificate -FilePath $CerPath -CertStoreLocation "Cert:\LocalMachine\Root" | Out-Null
    Write-Host "Installed into Trusted Root CA (LocalMachine)." -ForegroundColor Green
}

if ($DeployAddIn) {
    if (-not (Test-Path $AddInPath)) { throw "Signed add-in not found: $AddInPath" }
    $dest = Join-Path $env:USERPROFILE "Documents\ArcGIS\AddIns\ArcGISPro"
    New-Item -ItemType Directory -Force -Path $dest | Out-Null
    Copy-Item $AddInPath $dest -Force
    Write-Host "Deployed add-in to: $dest" -ForegroundColor Green
}

Write-Host ""
Write-Host "Done. Start ArcGIS Pro; the RoadChangerAddIn is now a trusted publisher." -ForegroundColor Green
Write-Host "If your org enforces signed-only add-ins, this add-in will now load."
