// SPDX-License-Identifier: MIT
// Copyright (c) 2026 GeoGarmeux Spatial Industries

namespace RoadChangerAddIn
{
    // Simple one-value reclassify buttons for classes NOT covered by the Convert
    // profiles (structures, rail, ancillary). Road / Road 1-Lane / Road 2-Lane /
    // Cart Track were retired in favor of the Convert group. Each sets FCSUBTYPE +
    // F_CODE only; all other attribution carries through.

    internal class TrailButton : ReclassifyButtonBase
    {
        protected override int SubtypeCode => 100156;
        protected override string FCode => "AP050";
        protected override string ClassLabel => "Trail";
    }

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

    internal class BridgeButton2 : ReclassifyButtonBase
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
