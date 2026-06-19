# RoadChangerAddIn

An ArcGIS Pro SDK add-in that recodes the selected transportation segment to a chosen
TDS classification in one undoable edit. Classification is subtype-driven on the
`TransportationGroundCrv` feature class (`FCSUBTYPE` + `F_CODE`); all other attribution
carries through untouched.

## Features
- One button per TDS subtype (Road, Cart Track, Trail, Railway, Bridge, Tunnel, Culvert,
  Ford, Ice Route, Sidewalk, Gate, Vehicle Barrier, and more), grouped on a custom
  **Transportation** ribbon tab.
- Road buttons also set `RTY` / `RIN_ROI` / `ZI016_ROC` (surface) and lane-count variants.
- Strict **single-feature** guard: refuses to run unless exactly one feature is selected,
  so a stray multi-select can never mass-recode.
- Layer matching keys off the underlying feature class, so it works with renamed layers
  and Subtype Group Layers.

## Project layout
| File | Purpose |
|------|---------|
| `ReclassifyButtonBase.cs` | Shared edit engine (matching, guard, EditOperation). |
| `TransportationButtons.cs` | One button class per subtype + `RoadFields` helper. |
| `TransportationModule.cs` | Add-in module. |
| `MakeBridgeButton.cs` | Original standalone bridge button (not wired in DAML). |
| `Config.daml` | Ribbon tab, groups, and button controls. |
| `Deploy/` | Signing + certificate-install kit (see `Deploy/README-Deployment.md`). |

## Build
Open `RoadChangerAddIn.slnx` in Visual Studio with the ArcGIS Pro SDK for .NET installed,
then Build (Release). Pro auto-deploys the `.esriAddinX` to
`%USERPROFILE%\Documents\ArcGIS\AddIns\ArcGISPro`.

## Deploy (signed)
See `Deploy/README-Deployment.md` — sign once, then install one certificate per machine.

## Notes
- Built/verified against `D05D_02.gdb` (TDS schema).
- Requires ArcGIS Pro 3.3+ ( .NET 8 ).
