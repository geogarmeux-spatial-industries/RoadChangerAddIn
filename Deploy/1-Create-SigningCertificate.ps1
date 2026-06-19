<#
    1-Create-SigningCertificate.ps1
    -------------------------------
    Run ONCE, on your (the publisher's) machine, to create a self-signed
    code-signing certificate for the RoadChangerAddIn.

    Produces two files in this folder:
      * RoadChangerAddIn-Signing.pfx  -> PRIVATE. Used to sign. Keep secret.
      * RoadChangerAddIn-Public.cer   -> PUBLIC.  Ship this to end-user machines.

    The cert is also left in your CurrentUser\My store so you can sign by name
    (the /n switch) without touching the .pfx.

    NOTE: a self-signed cert is fine for internal/org use. For external
    distribution, buy a code-signing cert from a CA (DigiCert, etc.) instead —
    then end users don't need the Trusted Root step.

    Usage (PowerShell):
      .\1-Create-SigningCertificate.ps1
      # you'll be prompted for a password to protect the .pfx
#>

param(
    [string]$Subject = "RoadChangerAddIn Publisher",
    [string]$OutDir  = $PSScriptRoot,
    [int]   $ValidYears = 5
)

$ErrorActionPreference = "Stop"

$pwd = Read-Host -AsSecureString "Enter a password to protect the .pfx"

$cert = New-SelfSignedCertificate `
    -Type CodeSigningCert `
    -Subject "CN=$Subject" `
    -KeyUsage DigitalSignature `
    -KeyAlgorithm RSA -KeyLength 2048 `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -NotAfter (Get-Date).AddYears($ValidYears)

$pfx = Join-Path $OutDir "RoadChangerAddIn-Signing.pfx"
$cer = Join-Path $OutDir "RoadChangerAddIn-Public.cer"

Export-PfxCertificate -Cert $cert -FilePath $pfx -Password $pwd | Out-Null
Export-Certificate    -Cert $cert -FilePath $cer               | Out-Null

Write-Host ""
Write-Host "Certificate created." -ForegroundColor Green
Write-Host "  Issued To (use as -IssuedTo when signing): $Subject"
Write-Host "  Thumbprint : $($cert.Thumbprint)"
Write-Host "  Private    : $pfx   (keep secret)"
Write-Host "  Public     : $cer   (distribute to end users)"
Write-Host ""
Write-Host "Next: build the add-in, then run 2-Sign-AddIn.ps1"
