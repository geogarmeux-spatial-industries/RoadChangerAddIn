<#
    2-Sign-AddIn.ps1
    ----------------
    Signs the built .esriAddinX with ESRI's ArcGISSignAddIn.exe.
    Run AFTER you build the project (Release) so the .esriAddinX exists.

    Two signing modes:
      * By cert name  (default): uses the cert in your Windows store whose
                                 "Issued To" matches -IssuedTo.
      * By .pfx file  : pass -PfxPath (and you'll be prompted for its password).

    Examples:
      # sign the auto-deployed add-in using the cert created by script 1
      .\2-Sign-AddIn.ps1

      # sign a specific file, using the .pfx instead of the store
      .\2-Sign-AddIn.ps1 -AddInPath "C:\path\RoadChangerAddIn.esriAddinX" `
                         -PfxPath ".\RoadChangerAddIn-Signing.pfx"
#>

param(
    # Default: the copy ArcGIS Pro auto-deploys to the well-known AddIns folder.
    [string]$AddInPath = (Join-Path $env:USERPROFILE "Documents\ArcGIS\AddIns\ArcGISPro\RoadChangerAddIn.esriAddinX"),
    [string]$IssuedTo  = "RoadChangerAddIn Publisher",
    [string]$PfxPath   = "",
    [string]$ProBin    = "C:\Program Files\ArcGIS\Pro\bin"
)

$ErrorActionPreference = "Stop"

$tool = Join-Path $ProBin "ArcGISSignAddIn.exe"
if (-not (Test-Path $tool))     { throw "ArcGISSignAddIn.exe not found at '$tool'. Check your ArcGIS Pro install path (-ProBin)." }
if (-not (Test-Path $AddInPath)){ throw "Add-in not found at '$AddInPath'. Build the project first, or pass -AddInPath." }

if ($PfxPath) {
    if (-not (Test-Path $PfxPath)) { throw "PFX not found: $PfxPath" }
    $pfxPwd = Read-Host "Enter the .pfx password"
    Write-Host "Signing '$AddInPath' with PFX '$PfxPath' ..."
    & $tool "$AddInPath" "/c:$PfxPath" "/p:$pfxPwd" /s
} else {
    Write-Host "Signing '$AddInPath' with store certificate '$IssuedTo' ..."
    & $tool "$AddInPath" "/n:$IssuedTo" /s
}

if ($LASTEXITCODE -ne 0) { throw "ArcGISSignAddIn.exe returned exit code $LASTEXITCODE." }

Write-Host ""
Write-Host "Signed successfully." -ForegroundColor Green
Write-Host "Verify/inspect the signature any time with:"
Write-Host "  `"$tool`" `"$AddInPath`""
