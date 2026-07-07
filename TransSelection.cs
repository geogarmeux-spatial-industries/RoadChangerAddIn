// SPDX-License-Identifier: MIT
// Copyright (c) 2026 GeoGarmeux Spatial Industries

using System;
using System.Linq;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;

namespace RoadChangerAddIn
{
    /// <summary>
    /// Locates the transportation line layer among the current selection by its
    /// underlying feature class (robust to renames / Subtype Group Layers / enterprise
    /// qualification), requiring exactly one selected feature. Call on the MCT.
    /// Returns null on success (out params set); otherwise an error message.
    /// </summary>
    internal static class TransSelection
    {
        public const string TargetFeatureClass = "TransportationGroundCrv";

        public static string FindSingle(out FeatureLayer layer, out long oid)
        {
            layer = null;
            oid = 0;

            var mv = MapView.Active;
            if (mv == null) return "No active map.";

            var selection = mv.Map.GetSelection().ToDictionary<BasicFeatureLayer>();

            FeatureLayer target = null;
            foreach (var fl in selection.Keys.OfType<FeatureLayer>())
            {
                using (var fc = fl.GetFeatureClass())
                {
                    var name = fc?.GetName();
                    if (!string.IsNullOrEmpty(name) &&
                        name.EndsWith(TargetFeatureClass, StringComparison.OrdinalIgnoreCase))
                    {
                        target = fl;
                        break;
                    }
                }
            }

            if (target == null)
                return "Select a feature in the '" + TargetFeatureClass + "' layer first.";

            var oids = selection[target];
            if (oids == null || oids.Count == 0)
                return "Nothing selected in the target layer.";
            if (oids.Count > 1)
                return oids.Count + " features are selected - select exactly one and try again.";

            layer = target;
            oid = oids[0];
            return null;
        }
    }
}
