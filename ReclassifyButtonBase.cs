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
    /// Shared engine for every "reclassify the selected transportation segment(s)"
    /// button. Each concrete button is its own class (see TransportationButtons.cs)
    /// and only declares WHAT to write — the edit plumbing lives here once.
    ///
    /// Workflow per click:
    ///   1. User has already split the line and selected the segment(s).
    ///   2. Button writes FCSUBTYPE + F_CODE (+ any ExtraAttributes) in ONE
    ///      undoable EditOperation. All other attributes carry through untouched.
    ///
    /// Classification in this TDS schema is subtype-driven: FCSUBTYPE is the
    /// ArcGIS subtype and F_CODE is the paired NFDD feature code. Writing only the
    /// fields in the dictionary leaves width, names, route numbers, etc. as-is.
    /// </summary>
    internal abstract class ReclassifyButtonBase : Button
    {
        // ---- Subclasses override these three ------------------------------

        /// <summary>FCSUBTYPE value for this classification (e.g. 100152 = ROAD_C).</summary>
        protected abstract int SubtypeCode { get; }

        /// <summary>F_CODE paired to the subtype (e.g. "AP030" for road).</summary>
        protected abstract string FCode { get; }

        /// <summary>Human label used in the edit-operation name and message box.</summary>
        protected abstract string ClassLabel { get; }

        // ---- Subclasses MAY override these --------------------------------

        /// <summary>
        /// TOC display name of the trans line layer — used only for the user-facing
        /// messages. Matching is done on the feature class (below), so this can be
        /// whatever your Contents pane shows.
        /// </summary>
        protected virtual string TargetLayerName => "TransportationGroundCurves";

        /// <summary>
        /// Underlying feature-class name to match (display-name independent). The
        /// real FC may be qualified in an enterprise gdb (e.g.
        /// "GEOGARMEUX.TransportationGroundCrv"), so we match on the trailing name.
        /// </summary>
        protected virtual string TargetFeatureClass => "TransportationGroundCrv";

        /// <summary>
        /// Extra fields to write alongside FCSUBTYPE/F_CODE (e.g. RTY, RIN_ROI,
        /// ZI016_ROC, LTN for road buttons). Default = none. NOTE: anything listed
        /// here OVERWRITES the existing value on every clicked feature.
        /// </summary>
        protected virtual IReadOnlyDictionary<string, object> ExtraAttributes => null;

        /// <summary>Field names — match the TransportationGroundCrv schema.</summary>
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

                    // Find the trans line layer among the current selection by its
                    // underlying feature class (robust to TOC renames like
                    // "TransportationGroundCurves" and to enterprise qualification).
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
                        return $"Select one or more features in '{TargetLayerName}' first.";

                    var oids = selection[target];
                    if (oids == null || oids.Count == 0)
                        return $"Nothing selected in '{TargetLayerName}'.";

                    // STRICT single-feature guard: this tool recodes exactly one
                    // segment per click. Refuse anything else so a stray multi-select
                    // can never mass-recode features.
                    if (oids.Count > 1)
                        return $"{oids.Count} features are selected in '{TargetLayerName}'. " +
                               "This tool recodes one segment at a time — select exactly one and try again.";

                    // Build the attribute payload: classification + optional extras.
                    var attrs = new Dictionary<string, object>
          