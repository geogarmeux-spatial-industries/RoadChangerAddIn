// SPDX-License-Identifier: MIT
// Copyright (c) 2026 GeoGarmeux Spatial Industries

using System;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Mapping;

namespace RoadChangerAddIn
{
    /// <summary>
    /// Shared state for the Sizing Cursor tool: the current circle diameter (in
    /// real-world meters) plus a change notification so the edit box, the preset
    /// buttons, and the active tool all stay in sync.
    /// </summary>
    internal static class SizingCursorState
    {
        public const double DefaultDiameter = 300.0;   // matches the box default

        private static double _diameter = DefaultDiameter;

        /// <summary>Raised whenever the diameter changes (preset click or typed value).</summary>
        public static event EventHandler DiameterChanged;

        /// <summary>Circle diameter in meters. Values &lt;= 0 are ignored.</summary>
        public static double Diameter
        {
            get => _diameter;
            set
            {
                if (value <= 0) return;
                if (Math.Abs(value - _diameter) < 1e-9) return;
                _diameter = value;
                DiameterChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Single fixed draw color for the circle + crosshair (per design choice).
        /// Red. Build symbols from this on the MCT.
        /// </summary>
        public static CIMColor CursorColor => ColorFactory.Instance.CreateRGBColor(230, 0, 0);
    }
}
