# RoadChangerAddIn — Signed Deployment Kit

Goal: sign the add-in once, then deploy to any machine by **installing one certificate**.
After that the signed `.esriAddinX` is trusted and will load — even if your org enforces
"only digitally signed add-ins."

ESRI signs Pro add-ins with its own tool, **`ArcGISSignAddIn.exe`** (ships in
`C:\Program Files\ArcGIS\Pro\bin`) — not Microsoft `signtool`. These scripts wrap it.

---

## One-time setup (you, the publisher)

1. **Create the certificate** — run in PowerShell:

   ```powershell
   .\1-Create-SigningCertificate.ps1
   ```

   Produces `RoadChangerAddIn-Signing.pfx` (private — keep secret) and
   `RoadChangerAddIn-Public.cer` (public — distribute). It also drops the cert in your
   `CurrentUser\My` store so you can sign by name.

   *For external distribution, buy a CA code-signing cert instead and skip straight to step 3.*

2. **Build the add-in** in Visual Studio (Release). Pro auto-copies
   `RoadChangerAddIn.esriAddinX` to `%USERPROFILE%\Documents\ArcGIS\AddIns\ArcGISPro`.

3. **Sign it:**

   ```powershell
   .\2-Sign-AddIn.ps1
   ```

   (Signs the auto-deployed copy with your store cert. Use `-AddInPath` / `-PfxPath` to
   point at a specific file or sign with the `.pfx`.)

---

## Per-machine deployment (each end user) — "the certificate install"

Ship these three files together in one folder:

- `Install-RoadChangerCertificate.ps1`
- `RoadChangerAddIn-Public.cer`
- `RoadChangerAddIn.esriAddinX`  *(the **signed** one)*

Then on each machine, **as Administrator**:

```powershell
powershell -ExecutionPolicy Bypass -File .\Install-RoadChangerCertificate.ps1 -DeployAddIn
```

That imports the public cert into **Trusted Publishers** (+ **Trusted Root**, because the
cert is self-signed) and copies the add-in into the user's AddIns folder. Start Pro — done.

- Already distributing the `.esriAddinX` another way? Drop `-DeployAddIn` and the script
  only installs the certificate.
- Using a commercial CA cert? Add `-SkipRoot` (the root is already trusted).

---

## Optional: auto-sign on every build

Add this to `RoadChangerAddIn.csproj` (right-click project → Edit Project), just before
`</Project>`, so each Release build signs automatically:

```xml
<Target Name="SignAddIn" AfterTargets="PackageArcGISContents">
  <Exec Command="&quot;C:\Program Files\ArcGIS\Pro\bin\ArcGISSignAddIn.exe&quot; $(TargetDir)$(TargetName).esriAddInX /n:&quot;RoadChangerAddIn Publisher&quot; /s" />
</Target>
```

(I can add this for you if you want it wired in.)

---

## Optional: enforce signed-only add-ins

To make Pro *require* trusted-signed add-ins (so unsigned ones are blocked org-wide):
**Project → Add-In Manager → Options →** "Require add-ins to be digitally signed by a
trusted publisher." Admins can also lock this via registry/installer switches — exact keys
are on ESRI's "ArcGIS Pro Registry Keys" page (linked below).

---

## Notes & references

- Self-signed certs are fine internally; for anything leaving your org, use a CA cert.
- Inspect/verify a signature any time: `"C:\Program Files\ArcGIS\Pro\bin\ArcGISSignAddIn.exe" <addin.esriAddinX>`.
- Re-sign whenever you rebuild — the signature covers the file contents.

Sources:
- [ProGuide: Digitally signed add-ins and configurations (ESRI)](https://github.com/Esri/arcgis-pro-sdk/wiki/ProGuide-Digitally-signed-add-ins-and-configurations)
- [ArcGIS Pro Registry Keys (ESRI)](https://github.com/Esri/arcgis-pro-sdk/wiki/ArcGIS-Pro-Registry-Keys)
