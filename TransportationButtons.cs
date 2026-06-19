// SPDX-License-Identifier: MIT
// Copyright (c) 2026 GeoGarmeux Spatial Industries

using System.Collections.Generic;

namespace RoadChangerAddIn
{
    // ===================================================================
    //  One Button class per TransportationGroundCrv subtype.
    //  Each only declares SubtypeCode / FCode / ClassLabel (+ extras).
    //  All edit plumbing is inherited from ReclassifyButtonBase.
    //
    //  FCSUBTYPE / F_CODE values were read directly from D05D_02.gdb.
    //
    //  ROAD buttons also write RTY / RIN_ROI / ZI016_ROC, per your request.
    //  WARNING: those three OVERWRITE existing values on every click. They are
    //  pulled out as editable constants below — change them, or delete the field
    //  from RoadAttrs(...) to stop writing it. Decoded domains (AP030):
    //    RTY     1 Motorway | 2 Limited Access Motorway | 3 Road | 4 Street | 999 Other
    //    RIN_ROI 1 International | 2 National Motorway | 3 National | 4 Secondary | 5 Local
    //    ZI016_ROC (surface) 1 Unimproved | 2 Stabilized Earth | 3 Flexible Pavement |
    //              4 Aggregate | 7 Rigid Pavement | 9 Asphalt | 17 Ice | 18 Snow | ...
    //    LTN     lane/track count (literal integer, e.g. 1 or 2)
    // ===================================================================

    internal static class RoadFields
    {
        public const string RTY = "RTY";
        public const string RIN_ROI = "RIN_ROI";
        public const string ROC = "ZI016_ROC";
        public const string LTN = "LTN";

        // Defaults applied by the generic Road buttons (most common values in the data).
        public const int Default_RTY = 3;       // Road
        public const int Default_RIN_ROI = 5;   // Local
        public const int Default_ROC = 3;       // Flexible Pavement

        public static Dictionary<string, object> RoadAttrs(int? lanes = null)
        {
            var d = new Dictionary<string, object>
            {
                { RTY,     Default_RTY },
                { RIN_ROI, Default_RIN_ROI },
                { ROC,     Default_ROC }
            };
            if (lanes.HasValue) d[LTN] = lanes.Value;
            return d;
        }
    }

    // ---------------- Roads (AP030, subtype 100152) ----------------------

    internal class RoadButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100152;
        protected override string FCode => "AP030";
        protected override string ClassLabel => "Road";
        protected override IReadOnlyDictionary<string, object> ExtraAttributes => RoadFields.RoadAttrs();
    }

    internal class Road1LaneButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100152;
        protected override string FCode => "AP030";
        protected override string ClassLabel => "Road (1 lane)";
        protected override IReadOnlyDictionary<string, object> ExtraAttributes => RoadFields.RoadAttrs(lanes: 1);
    }

    internal class Road2LaneButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100152;
        protected override string FCode => "AP030";
        protected override string ClassLabel => "Road (2 lane)";
        protected override IReadOnlyDictionary<string, object> ExtraAttributes => RoadFields.RoadAttrs(lanes: 2);
    }

    // ---------------- Tracks / trails ------------------------------------

    internal class CartTrackButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100150;
        protected override string FCode => "AP010";
        protected override string ClassLabel => "Cart Track";
    }

    internal class TrailButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100156;
        protected override string FCode => "AP050";
        protected override string ClassLabel => "Trail";
    }

    // ---------------- Rail -----------------------------------------------

    internal class RailwayButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100143;
        protected override string FCode => "AN010";
        protected override string ClassLabel => "Railway";
    }

    internal class RailwaySidetrackButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100144;
        protected override string FCode => "AN050";
        protected override string ClassLabel => "Railway Sidetrack";
    }

    // ---------------- Structures along the route -------------------------

    internal class BridgeButton2 : ReclassifyButtonBase   // renamed to avoid clashing with your existing MakeBridgeButton
    {
        protected override int SubtypeCode => 100161;
        protected override string FCode => "AQ040";
        protected override string ClassLabel => "Bridge";
    }

    internal class BridgeSpanButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100162;
        protected override string FCode => "AQ045";
        protected override string ClassLabel => "Bridge Span";
    }

    internal class TunnelButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100187;
        protected override string FCode => "AQ130";
        protected override string ClassLabel => "Tunnel";
    }

    internal class CulvertButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100170;
        protected override string FCode => "AQ065";
        protected override string ClassLabel => "Culvert";
    }

    internal class FordButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100302;
        protected override string FCode => "BH070";
        protected override string ClassLabel => "Ford";
    }

    internal class IceRouteButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100173;
        protected override string FCode => "AQ075";
        protected override string ClassLabel => "Ice Route";
    }

    internal class CausewayStructureButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 130381;
        protected override string FCode => "AQ063";
        protected override string ClassLabel => "Causeway Structure";
    }

    internal class ArcadeButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100192;
        protected override string FCode => "AQ151";
        protected override string ClassLabel => "Arcade";
    }

    internal class CablewayButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100206;
        protected override string FCode => "AT041";
        protected override string ClassLabel => "Cableway";
    }

    internal class TransRouteProtectStructButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 130921;
        protected override string FCode => "AL211";
        protected override string ClassLabel => "Transport Route Protection Structure";
    }

    internal class OverheadObstructionButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100112;
        protected override string FCode => "AL155";
        protected override string ClassLabel => "Overhead Obstruction";
    }

    // ---------------- Ancillary line features ----------------------------

    internal class SidewalkButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100159;
        protected override string FCode => "AQ035";
        protected override string ClassLabel => "Sidewalk";
    }

    internal class GateButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100154;
        protected override string FCode => "AP040";
        protected override string ClassLabel => "Gate";
    }

    internal class VehicleBarrierButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100155;
        protected override string FCode => "AP041";
        protected override string ClassLabel => "Vehicle Barrier";
    }
}
