// SPDX-License-Identifier: MIT
// Copyright (c) 2026 GeoGarmeux Spatial Industries

using System;
using System.Collections.Generic;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace RoadChangerAddIn
{
    /// <summary>
    /// Copy Template: captures every attribute (except identity / geometry /
    /// classification) from the single selected feature into TemplateStore.
    /// </summary>
    internal class CopyTemplateButton : Button
    {
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

                    var inspector = new Inspector();
                    inspector.Load(layer, oid);

                    var values = new Dictionary<string, object>();
                    foreach (var attr in inspector)
                    {
                        string field = attr.FieldName;
                        if (string.IsNullOrEmpty(field) || TemplateStore.Excluded.Contains(field))
                            continue;
                        values[field] = inspector[field];
                    }

                    TemplateStore.Set(values, "OID " + oid);
                    return "Template copied from OID " + oid + " - " + values.Count + " attribute(s).\n" +
                           "Select a target feature and click Paste Template.";
                });

                if (!string.IsNullOrEmpty(result))
                    MessageBox.Show(result, "Copy Template");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error: " + ex.Message, "Copy Template");
            }
        }

        protected override void OnUpdate()
        {
            Enabled = MapView.Active?.Map?.SelectionCount > 0;
        }
    }

    /// <summary>
    /// Paste Template: applies the stored template attributes to the single selected
    /// target feature in one undoable edit. Leaves the target's classification alone.
    /// </summary>
    internal class PasteTemplateButton : Button
    {
        protected override async void OnClick()
        {
            try
            {
                string result = await QueuedTask.Run(() =>
                {
                    if (!TemplateStore.HasTemplate)
                        return "No template captured yet - select a source feature and click Copy Template first.";

                    FeatureLayer layer;
                    long oid;
                    string error = TransSelection.FindSingle(out layer, out oid);
                    if (error != null) return error;

                    var op = new EditOperation
                    {
                        Name = "Apply template attributes",
                        SelectModifiedFeatures = true
                    };
                    op.Modify(layer, oid, TemplateStore.Snapshot());

                    if (op.IsEmpty)
                        return "Nothing to apply.";

                    return op.Execute()
                        ? "Applied " + TemplateStore.Count + " template attribute(s) to OID " + oid + "."
                        : "Edit failed: " + op.ErrorMessage;
                });

                if (!string.IsNullOrEmpty(result))
                    MessageBox.Show(result, "Paste Template");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error: " + ex.Message, "Paste Template");
            }
        }

        protected override void OnUpdate()
        {
            Enabled = TemplateStore.HasTemplate && MapView.Active?.Map?.SelectionCount > 0;
        }
    }
}
