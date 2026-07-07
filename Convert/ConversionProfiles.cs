// SPDX-License-Identifier: MIT
// Copyright (c) 2026 GeoGarmeux Spatial Industries

using System.Collections.Generic;

namespace RoadChangerAddIn
{
    // =====================================================================
    //  One button per target class. Each declares only its ConversionProfile.
    //  Values seeded from the D05D_02 gdb modal values; edit here to tune.
    //  Fields not in Defaults/Clear are left as-is (carry from the source).
    //  Decode: RLE 2=Level | PCF 2=Intact | LOC 44=On Surface | ACC 1=Accurate
    //          CWT/CIW/THR 1000=FALSE/1001=TRUE
    //          RTY 3=Road 4=Street | RIN_ROI 5=Local | ZI016_ROC 3=Flexible Pavement 1=Unimproved
    // =====================================================================

    internal class MakeCartTrackButton : ConvertButtonBase
    {
        protected override string ClassLabel => "Cart Track";
        protected override ConversionProfile Profile => new ConversionProfile
        {
            SubtypeCode = 100150,
            FCode = "AP010",
            Defaults = TransFields.With(new Dictionary<string, object> { { "WID", 2.0 } }),
            Clear = TransFields.RoadOnly            // LZN + ZI026_CTUU carry automatically
        };
    }

    internal class MakeRoad1LaneButton : ConvertButtonBase
    {
        protected override string ClassLabel => "Road (1 Lane)";
        protected override ConversionProfile Profile => new ConversionProfile
        {
            SubtypeCode = 100152,
            FCode = "AP030",
            Defaults = TransFields.With(new Dictionary<string, object>
            {
                { "WID", 5.5 }, { "RTY", 3 }, { "RIN_ROI", 5 }, { "ZI016_ROC", 3 }, { "LTN", 1 }
            }),
            Clear = TransFields.CartOnly
        };
    }

    internal class MakeRoad2LaneButton : ConvertButtonBase
    {
        protected override string ClassLabel => "Road (2 Lane)";
        protected override ConversionProfile Profile => new ConversionProfile
        {
            SubtypeCode = 100152,
            FCode = "AP030",
            Defaults = TransFields.With(new Dictionary<string, object>
            {
                { "WID", 5.5 }, { "RTY", 3 }, { "RIN_ROI", 5 }, { "ZI016_ROC", 3 }, { "LTN", 2 }
            }),
            Clear = TransFields.CartOnly
        };
    }

    internal class MakeStreetButton : ConvertButtonBase
    {
        protected override string ClassLabel => "Street";
        protected override ConversionProfile Profile => new ConversionProfile
        {
            SubtypeCode = 100152,
            FCode = "AP030",
            Defaults = TransFields.With(new Dictionary<string, object>
            {
                { "WID", 5.5 }, { "RTY", 4 }, { "RIN_ROI", 5 }, { "ZI016_ROC", 3 }, { "LTN", 2 }
            }),
            Clear = TransFields.CartOnly
        };
    }

    internal class MakeDirtRoadButton : ConvertButtonBase
    {
        protected override string ClassLabel => "Dirt Road";
        protected override ConversionProfile Profile => new ConversionProfile
        {
            SubtypeCode = 100152,
            FCode = "AP030",
            Defaults = TransFields.With(new Dictionary<string, object>
            {
                { "WID", 5.5 }, { "RTY", 3 }, { "RIN_ROI", 5 }, { "ZI016_ROC", 1 }, { "LTN", 1 }
            }),
            Clear = TransFields.CartOnly
        };
    }
}
