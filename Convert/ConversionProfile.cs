// SPDX-License-Identifier: MIT
// Copyright (c) 2026 GeoGarmeux Spatial Industries

using System;
using System.Collections.Generic;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace RoadChangerAddIn
{
    /// <summary>
    /// A per-target-class conversion recipe. Sets the class (FCSUBTYPE/F_CODE),
    /// stamps the Defaults, and clears the Clear fields to noInformation. Any field
    /// NOT listed is left untouched, so it "carries" from whatever the feature was.
    /// </summary>
    internal class ConversionProfile
    {
        public const int NoInfo = -999999;

        public int SubtypeCode;
        public string FCode;
        public Dictionary<string, object> Defaults;   // fixed values to write
        public string[] Clear;                          // fields to reset to NoInfo
    }

    /// <summary>Shared field sets and the common ground-transport defaults.</summary>
    internal static class TransFields
    {
        // Road-only fields, cleared when converting to a non-road class (cart track, trail).
        public static readonly string[] RoadOnly =
            { "RTY", "RIN_ROI", "ZI016_ROC", "LTN", "FCO", "MES", "ONE", "RMWC", "ROR", "SEP", "SGCC", "SPM", "RIN_RTN" };

        // Cart-track-only fields, cleared when converting to a road class.
        public static readonly string[] CartOnly = { "TRS", "TRS2" };

        // Defaults shared by most classes (verified as the modal values in the gdb).
        public static Dictionary<string, object> Common()
        {
            return new Dictionary<string, object>
            {
                { "RLE", 2 },     // Level
                { "PCF", 2 },     // Intact
                { "LOC", 44 },    // On Surface
                { "ACC", 1 },     // Accurate
                { "CWT", 1000 },  // FALSE
                { "CIW", 1000 },  // FALSE
                { "THR", 1000 }   // FALSE
            };
        }

        // Combine Common() with class-specific overrides.
        public static Dictionary<string, object> With(Dictionary<string, object> extra)
        {
            var d = Common();
            foreach (var kv in extra) d[kv.Key] = kv.Value;
            return d;
        }
    }

    /// <summary>
    /// Base for the one-click "convert this segment to class X" buttons. Applies the
    /// Profile in a single undoable edit. Requires exactly one selected feature.
    /// </summary>
    internal abstract class ConvertButtonBase : Button
    {
        protected abstract ConversionProfile Profile { get; }
        protected abstract string ClassLabel { get; }

        protected override async void OnClick()
        {
            try
            {
                string result = await QueuedTask.Run(() =>
                {
                    FeatureLayer layer;
                    long oid;
                    string error = TransSelection.FindSingle(out layer, out oid);
                    if (error != null) return error;

                    var p = Profile;
                    var attrs = new Dictionary<string, object>
                    {
                        { "FCSUBTYPE", p.SubtypeCode },
                        { "F_CODE",    p.FCode }
                    };
                    if (p.Defaults != null)
                        foreach (var kv in p.Defaults) attrs[kv.Key] = kv.Value;
                    if (p.Clear != null)
                        foreach (var f in p.Clear) attrs[f] = ConversionProfile.NoInfo;

                    var op = new EditOperation
                    {
                        Name = "Convert to " + ClassLabel,
                        SelectModifiedFeatures = true
                    };
                    op.Modify(layer, oid, attrs);

                    if (op.IsEmpty)
                        return "Nothing to update.";

                    return op.Execute()
                        ? "Converted segment to " + ClassLabel + "."
                        : "Edit failed: " + op.ErrorMessage;
                });

                if (!string.IsNullOrEmpty(result))
                    MessageBox.Show(result, "Convert Transportation");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error: " + ex.Message, "Convert Transportation");
            }
        }

        protected override void OnUpdate()
        {
            Enabled = MapView.Active?.Map?.SelectionCount > 0;
        }
    }
}
