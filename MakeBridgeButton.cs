using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace RoadChangerAddIn
{
    /// <summary>
    /// Codes the currently selected transportation segment as a bridge.
    ///
    /// Workflow:
    ///   1. User splits the trans line at the two crossing points (standard Split tool).
    ///   2. User selects the middle segment (exactly one).
    ///   3. User clicks this button -> the bridge attribute is written in one
    ///      undoable edit operation.
    ///
    /// NOTE: this is the original standalone bridge button. For the D05D_02 TDS
    /// schema, "bridge" is subtype-driven (FCSUBTYPE=100161 / F_CODE="AQ040"), not
    /// TUC=3 — see BridgeButton2 in TransportationButtons.cs for the corrected version.
    /// The constants below are kept as you configured them.
    /// </summary>
    internal class MakeBridgeButton : Button
    {
        // ============================================================
        //  Adjust these three to match your TDS schema, then rebuild.
        // ============================================================
        private const string TargetLayerName = "Transportation Ground";
        private const string BridgeField = "TUC";
        private static readonly object BridgeValue = 3;   // e.g. 3, or "AQ040"

        protected override async void OnClick()
        {
            try
            {
                string result = await QueuedTask.Run(() =>
                {
                    var mapView = MapView.Active;
                    if (mapView == null)
                        return "No active map.";

                    // Selected OIDs grouped by layer.
                    var selection = mapView.Map.GetSelection()
                                           .ToDictionary<BasicFeatureLayer>();

                    // Locate the target trans layer among the current selection.
                    var target = selection.Keys
                        .OfType<FeatureLayer>()
                        .FirstOrDefault(l =>
                            l.Name.Equals(TargetLayerName, StringComparison.OrdinalIgnoreCase));

                    if (target == null)
                        return $"Select one feature in '{TargetLayerName}' first.";

                    var oids = selection[target];
                    if (oids == null || oids.Count == 0)
                        return $"Nothing selected in '{TargetLayerName}'.";

                    // STRICT single-feature guard: code exactly one segment per click.
                    // Refuse anything else so a stray multi-select can never mass-recode.
                    if (oids.Count > 1)
                        return $"{oids.Count} features are selected in '{TargetLayerName}'. " +
                               "This tool codes one segment at a time — select exactly one and try again.";

                    // Single, atomic, undoable edit.
                    var op = new EditOperation
                    {
                        Name = "Code segment as bridge",
                        SelectModifiedFeatures = true
                    };

                    var attrs = new Dictionary<string, object> { { BridgeField, BridgeValue } };
                    foreach (var oid in oids)
                        op.Modify(target, oid, attrs);

                    if (op.IsEmpty)
                        return "Nothing to update.";

                    return op.Execute()
                        ? "Coded segment as bridge."
                        : $"Edit failed: {op.ErrorMessage}";
                });

                if (!string.IsNullOrEmpty(result))
                    MessageBox.Show(result, "Make Bridge");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Make Bridge");
            }
        }

        protected override void OnUpdate()
        {
            // Enable the button only when something is selected on the active map.
            Enabled = MapView.Active?.Map?.SelectionCount > 0;
        }
    }
}
