// SPDX-License-Identifier: MIT
// Copyright (c) 2026 GeoGarmeux Spatial Industries

using System;
using System.Collections.Generic;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Mapping;

namespace RoadChangerAddIn
{
    /// <summary>TDS reference scales; each drives a reticle color.</summary>
    internal enum SizeScale { S12_5K, S50K, S250K, S500K }

    /// <summary>TDS spacing requirements; the circle radius uses one of these.</summary>
    internal enum SizeReq { RoadSpacing, RoadBUA, CartSparse, CartDense }

    /// <summary>
    /// Shared state for the Sizing Cursor. The circle RADIUS (meters) is the TDS
    /// spacing requirement for the current scale, and the reticle COLOR is the scale.
    /// The box, scale/requirement buttons, and the tool stay in sync via RadiusChanged
    /// (size) and ColorChanged (scale color) events.
    /// </summary>
    internal static class SizingCursorState
    {
        // Radius (meters) per scale per requirement — from the TDS quick-reference cards.
        private static readonly Dictionary<SizeScale, Dictionary<SizeReq, double>> Table =
            new Dictionary<SizeScale, Dictionary<SizeReq, double>>
            {
                { SizeScale.S12_5K, new Dictionary<SizeReq, double> { { SizeReq.RoadSpacing, 150 },  { SizeReq.RoadBUA, 100 },  { SizeReq.CartSparse, 150 },  { SizeReq.CartDense, 1000 } } },
                { SizeScale.S50K,   new Dictionary<SizeReq, double> { { SizeReq.RoadSpacing, 300 },  { SizeReq.RoadBUA, 200 },  { SizeReq.CartSparse, 300 },  { SizeReq.CartDense, 1000 } } },
                { SizeScale.S250K,  new Dictionary<SizeReq, double> { { SizeReq.RoadSpacing, 1500 }, { SizeReq.RoadBUA, 1000 }, { SizeReq.CartSparse, 1500 }, { SizeReq.CartDense, 5000 } } },
                { SizeScale.S500K,  new Dictionary<SizeReq, double> { { SizeReq.RoadSpacing, 3000 }, { SizeReq.RoadBUA, 2000 }, { SizeReq.CartSparse, 3000 }, { SizeReq.CartDense, 10000 } } },
            };

        // Per-scale reticle color: 12.5K black, 50K yellow, 250K green, 500K purple.
        private static readonly Dictionary<SizeScale, int[]> Rgb =
            new Dictionary<SizeScale, int[]>
            {
                { SizeScale.S12_5K, new[] { 0, 0, 0 } },
                { SizeScale.S50K,   new[] { 255, 215, 0 } },
                { SizeScale.S250K,  new[] { 0, 176, 80 } },
                { SizeScale.S500K,  new[] { 128, 0, 128 } },
            };

        private static double _radius = 300.0;

        public static SizeScale CurrentScale { get; private set; } = SizeScale.S50K;
        public static SizeReq CurrentReq { get; private set; } = SizeReq.RoadSpacing;

        /// <summary>Raised when the radius changes (redraw + refresh the box).</summary>
        public static event EventHandler RadiusChanged;

        /// <summary>Raised when the scale color changes (rebuild symbol + redraw).</summary>
        public static event EventHandler ColorChanged;

        /// <summary>Circle radius in meters (= the active spacing requirement).</summary>
        public static double Radius
        {
            get => _radius;
            set
            {
                if (value <= 0) return;
                if (Math.Abs(value - _radius) < 1e-9) return;
                _radius = value;
                RadiusChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        /// <summary>Set the active scale: changes color and re-applies the radius for the current requirement.</summary>
        public static void SetScale(SizeScale scale)
        {
            CurrentScale = scale;
            _radius = Table[scale][CurrentReq];
            ColorChanged?.Invoke(null, EventArgs.Empty);
            RadiusChanged?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>Set the active requirement: radius = that requirement at the current scale.</summary>
        public static void SetRequirement(SizeReq req)
        {
            CurrentReq = req;
            _radius = Table[CurrentScale][req];
            RadiusChanged?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>Reticle color for the current scale. Build symbols from this on the MCT.</summary>
        public static CIMColor CursorColor
        {
            get
            {
                var c = Rgb[CurrentScale];
                return ColorFactory.Instance.CreateRGBColor(c[0], c[1], c[2]);
            }
        }
    }
}
