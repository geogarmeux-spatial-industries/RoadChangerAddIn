// SPDX-License-Identifier: MIT
// Copyright (c) 2026 GeoGarmeux Spatial Industries

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace RoadChangerAddIn
{
    /// <summary>
    /// A map tool that draws a sizing reticle that follows the pointer: a true
    /// ground circle of the configured diameter (meters) plus full-view crosshair
    /// lines. Diameter comes from <see cref="SizingCursorState"/> (the Diameter box
    /// or a preset button). The circle is geodesic, so it represents the same real
    /// distance regardless of projection or zoom.
    /// </summary>
    internal class SizingCursorTool : MapTool
    {
        private IDisposable _circleOverlay;
        private IDisposable _crossOverlay;
        private CIMSymbolReference _lineSymRef;
        private System.Windows.Point? _lastClient;
        private bool _busy;

        public SizingCursorTool()
        {
            IsSketchTool = false;
            Cursor = Cursors.Cross;
        }

        protected override Task OnToolActivateAsync(bool hasMapViewChanged)
        {
            SizingCursorState.DiameterChanged += OnDiameterChanged;
            return QueuedTask.Run(() =>
            {
                _lineSymRef = SymbolFactory.Instance
                    .ConstructLineSymbol(SizingCursorState.CursorColor, 1.5)
                    .MakeSymbolReference();
            });
        }

        protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
        {
            SizingCursorState.DiameterChanged -= OnDiameterChanged;
            return QueuedTask.Run(() =>
            {
                _circleOverlay?.Dispose(); _circleOverlay = null;
                _crossOverlay?.Dispose();  _crossOverlay = null;
            });
        }

        // Redraw at the last known pointer position when the diameter changes.
        private void OnDiameterChanged(object sender, EventArgs e) => Draw();

        protected override void OnToolMouseMove(MapViewMouseEventArgs e)
        {
            _lastClient = e.ClientPoint;
            Draw();
        }

        private void Draw()
        {
            if (_busy || _lastClient == null) return;
            _busy = true;
            var client = _lastClient.Value;

            QueuedTask.Run(() =>
            {
                try
                {
                    var mv = MapView.Active;
                    if (mv == null) return;

                    var pt = mv.ClientToMap(client);
                    if (pt == null) return;
                    var sr = pt.SpatialReference ?? mv.Map.SpatialReference;
                    double radius = SizingCursorState.Diameter / 2.0;

                    // --- geodesic circle (true ground size) ---
                    var ell = new GeodesicEllipseParameter
                    {
                        Center = pt.Coordinate2D,
                        SemiAxis1Length = radius,
                        SemiAxis2Length = radius,
                        AxisDirection = 0.0,
                        LinearUnit = LinearUnit.Meters,
                        OutGeometryType = GeometryType.Polyline,
                        VertexCount = 120
                    };
                    var circle = GeometryEngine.Instance.GeodesicEllipse(ell, sr);

                    // --- full-view crosshair through the pointer ---
                    var ext = mv.Extent;
                    var pb = new PolylineBuilderEx(sr);
                    pb.AddPart(new[]
                    {
                        MapPointBuilderEx.CreateMapPoint(ext.XMin, pt.Y, sr),
                        MapPointBuilderEx.CreateMapPoint(ext.XMax, pt.Y, sr)
                    });
                    pb.AddPart(new[]
                    {
                        MapPointBuilderEx.CreateMapPoint(pt.X, ext.YMin, sr),
                        MapPointBuilderEx.CreateMapPoint(pt.X, ext.YMax, sr)
                    });
                    var cross = pb.ToGeometry();

                    if (_circleOverlay == null) _circleOverlay = AddOverlay(circle, _lineSymRef);
                    else                        UpdateOverlay(_circleOverlay, circle, _lineSymRef);

                    if (_crossOverlay == null)  _crossOverlay = AddOverlay(cross, _lineSymRef);
                    else                        UpdateOverlay(_crossOverlay, cross, _lineSymRef);
                }
                finally
                {
                    _busy = false;
                }
            });
        }
    }
}
