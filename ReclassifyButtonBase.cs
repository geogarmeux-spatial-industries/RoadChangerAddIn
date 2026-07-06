// SPDX-License-Identifier: MIT
// Copyright (c) 2026 GeoGarmeux Spatial Industries

using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace RoadChangerAddIn
{
    /// <summary>
    /// Shared engine for every "reclassify the selected transportation segment"
    /// button. Each concrete button only declares SubtypeCode / FCode / ClassLabel
    /// (+ optional ExtraAttributes); the edit plumbing lives here once. A partial
    /// Modify writes only FCSUBTYPE + F_CODE (+ extras), so all other attribution
    /// carries through. Strict single-feature guard. Matches the layer by feature
    /// class, so it works with renamed / Subtype Group Layers.
    /// </summary>
    internal abstract class ReclassifyButtonBase : Button
    {
        protected abstract int SubtypeCode { get; }
        protected abstract string FCode { get; }
        protected abstract string ClassLabel { get; }

        protected virtual string TargetLayerName => "TransportationGroundCurves";
        protected virtual string TargetFeatureClass => "TransportationGroundCrv";
        protected virtual IReadOnlyDictionary<string, object> ExtraAttributes => null;

        protected const string SubtypeField = "FCSUBTYPE";
        protected const string FCodeField = "F_CODE";

        protected override async void OnClick()
        {
            try
            {
                string result = await QueuedTask.Run(() =>
                {
                    var mapView = MapView.Active;
                    if (mapView == null)
                        return "No active map.";

                    var selection = mapView.Map.GetSelection()
                                           .ToDictionary<BasicFeatureLayer>();

                    FeatureLayer target = null;
                    foreach (var fl in selection.Keys.OfType<FeatureLayer>())
                    {
                        if (fl.Name.Equals(TargetLayerName, StringComparison.OrdinalIgnoreCase))
                        {
                            target = fl;
                            break;
                        }
                        using (var fc = fl.GetFeatureClass())
                        {
                            var fcName = fc?.GetName();
                            if (!string.IsNullOrEmpty(fcName) &&
                                fcName.EndsWith(TargetFeatureClass, StringComparison.OrdinalIgnoreCase))
                            {
                                target = fl;
                                break;
                            }
                        }
                    }

                    if (target == null)
                        return "Select one or more features in '" + TargetLayerName + "' first.";

                    var oids = selection[target];
                    if (oids == null || oids.Count == 0)
                        return "Nothing selected in '" + TargetLayerName + "'.";

                    if (oids.Count > 1)
                        return oids.Count + " features are selected in '" + TargetLayerName +
                               "'. This tool recodes one segment at a time - select exactly one and try again.";

                    var attrs = new Dictionary<string, object>
                    {
                        { SubtypeField, SubtypeCode },
                        { FCodeField,  FCode }
                    };
                    if (ExtraAttributes != null)
                        foreach (var kv in ExtraAttributes)
                            attrs[kv.Key] = kv.Value;

                    var op = new EditOperation
                    {
                        Name = "Code segment as " + ClassLabel,
                        SelectModifiedFeatures = true
                    };

                    foreach (var oid in oids)
                        op.Modify(target, oid, attrs);

                    if (op.IsEmpty)
                        return "Nothing to update.";

                    return op.Execute()
                        ? "Coded segment as " + ClassLabel + "."
                        : "Edit failed: " + op.ErrorMessage;
                });

                if (!string.IsNullOrEmpty(result))
                    MessageBox.Show(result, "Reclassify Transportation");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error: " + ex.Message, "Reclassify Transportation");
            }
        }

        protected override void OnUpdate()
        {
            Enabled = MapView.Active?.Map?.SelectionCount > 0;
        }
    }
}
