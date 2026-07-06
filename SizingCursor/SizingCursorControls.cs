// SPDX-License-Identifier: MIT
// Copyright (c) 2026 GeoGarmeux Spatial Industries

using System.Globalization;
using ArcGIS.Desktop.Framework.Contracts;

namespace RoadChangerAddIn
{
    /// <summary>
    /// Editable Diameter (m) box. Reflects the shared diameter and pushes typed
    /// values back into it on Enter. Stays available alongside the presets so the
    /// size can always be adjusted manually.
    /// </summary>
    internal class DiameterEditBox : EditBox
    {
        public DiameterEditBox()
        {
            Text = Format(SizingCursorState.Diameter);
            // Keep the box in sync when a preset button changes the diameter.
            SizingCursorState.DiameterChanged += (s, e) => Text = Format(SizingCursorState.Diameter);
        }

        protected override void OnEnter()
        {
            if (double.TryParse(Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double d) && d > 0)
                SizingCursorState.Diameter = d;
            else
                Text = Format(SizingCursorState.Diameter);  // revert invalid input
        }

        private static string Format(double d) => d.ToString("0.###", CultureInfo.InvariantCulture);
    }

    /// <summary>Base for the quick-size preset buttons; each just sets a diameter.</summary>
    internal abstract class SizingPresetButtonBase : Button
    {
        protected abstract double PresetDiameter { get; }
        protected override void OnClick() => SizingCursorState.Diameter = PresetDiameter;
    }

    // Preset diameters (meters): 12.5k < 300, 50k = 300, 250k = 1,000, 500k = 3,000.
    // 12.5k set to 100 m (your "<300"); change here if you want a different value.
    internal class Preset12kButton  : SizingPresetButtonBase { protected override double PresetDiameter => 100.0;  }
    internal class Preset50kButton  : SizingPresetButtonBase { protected override double PresetDiameter => 300.0;  }
    internal class Preset250kButton : SizingPresetButtonBase { protected override double PresetDiameter => 1000.0; }
    internal class Preset500kButton : SizingPresetButtonBase { protected override double PresetDiameter => 3000.0; }
}
