// SPDX-License-Identifier: MIT
// Copyright (c) 2026 GeoGarmeux Spatial Industries

using System.Globalization;
using ArcGIS.Desktop.Framework.Contracts;

namespace RoadChangerAddIn
{
    /// <summary>
    /// Editable Radius (m) box = the active spacing distance. Reflects the shared
    /// radius and pushes typed values back on Enter (manual override).
    /// </summary>
    internal class RadiusEditBox : EditBox
    {
        public RadiusEditBox()
        {
            Text = Format(SizingCursorState.Radius);
            SizingCursorState.RadiusChanged += (s, e) => Text = Format(SizingCursorState.Radius);
        }

        protected override void OnEnter()
        {
            if (double.TryParse(Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double d) && d > 0)
                SizingCursorState.Radius = d;
            else
                Text = Format(SizingCursorState.Radius);  // revert invalid input
        }

        private static string Format(double d) => d.ToString("0.###", CultureInfo.InvariantCulture);
    }

    // ---- Scale buttons: set the active scale + reticle color ----
    internal abstract class ScaleButtonBase : Button
    {
        protected abstract SizeScale Scale { get; }
        protected override void OnClick() => SizingCursorState.SetScale(Scale);
    }

    internal class Scale12kButton  : ScaleButtonBase { protected override SizeScale Scale => SizeScale.S12_5K; }
    internal class Scale50kButton  : ScaleButtonBase { protected override SizeScale Scale => SizeScale.S50K;   }
    internal class Scale250kButton : ScaleButtonBase { protected override SizeScale Scale => SizeScale.S250K;  }
    internal class Scale500kButton : ScaleButtonBase { protected override SizeScale Scale => SizeScale.S500K;  }

    // ---- Requirement buttons: set the radius from the active scale's row ----
    internal abstract class ReqButtonBase : Button
    {
        protected abstract SizeReq Req { get; }
        protected override void OnClick() => SizingCursorState.SetRequirement(Req);
    }

    internal class ReqRoadSpacingButton : ReqButtonBase { protected override SizeReq Req => SizeReq.RoadSpacing; }
    internal class ReqRoadBuaButton     : ReqButtonBase { protected override SizeReq Req => SizeReq.RoadBUA;     }
    internal class ReqCartSparseButton  : ReqButtonBase { protected override SizeReq Req => SizeReq.CartSparse;  }
    internal class ReqCartDenseButton   : ReqButtonBase { protected override SizeReq Req => SizeReq.CartDense;   }
}
