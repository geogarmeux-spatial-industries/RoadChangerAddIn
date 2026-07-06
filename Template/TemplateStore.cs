// SPDX-License-Identifier: MIT
// Copyright (c) 2026 GeoGarmeux Spatial Industries

using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;

namespace RoadChangerAddIn
{
    /// <summary>
    /// Holds attribute values copied from a "template" feature so they can be
    /// stamped onto other features (a one-click replacement for copy + paste-special).
    /// Classification (FCSUBTYPE / F_CODE) and identity/geometry fields are NOT copied.
    /// </summary>
    internal static class TemplateStore
    {
        public static readonly HashSet<string> Excluded =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "OBJECTID", "OID", "GlobalID", "hc_GlobalID", "Version",
                "SHAPE", "Shape", "SHAPE_Length", "Shape_Length",
                "FCSUBTYPE", "F_CODE"
            };

        private static Dictionary<string, object> _values;

        public static bool HasTemplate => _values != null && _values.Count > 0;
        public static int Count => _values?.Count ?? 0;
        public static string SourceLabel { get; private set; }
        public static event EventHandler Changed;

        public static void Set(Dictionary<string, object> values, string sourceLabel)
        {
            _values = values;
            SourceLabel = sourceLabel;
            Changed?.Invoke(null, EventArgs.Empty);
        }

        public static Dictionary<string, object> Snapshot()
        {
            return _values == null
                ? new Dictionary<string, object>()
                : new Dictionary<string, object>(_values);
        }
    }

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
