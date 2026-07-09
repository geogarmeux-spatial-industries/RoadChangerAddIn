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
    /// Map tool that draws a sizing reticle following the pointer: a geodesic circle
    /// of the current DIAMETER (a TDS spacing requirement) plus a full-view crosshair,
    /// in the current scale's color. Selection: left-click selects inside the circle;
    /// left-drag draws a rectangle marquee and selects inside it (Shift adds, Ctrl removes).
    /// </summary>
    internal class SizingCursorTool : MapTool
    {
        private IDisposable _circleOverlay;
        private IDisposable _crossOverlay;
        private CIMSymbolReference _lineSymRef;
        private System.Windows.Point? _lastClient;
        private bool _busy;
        private volatile bool _rebuildSymbol = true;

        public SizingCursorTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Rectangle;   // drag = marquee; click = tiny rect
            SketchOutputMode = SketchOutputMode.Map;
            Cursor = Cursors.Cross;
        }

        protected override Task OnToolActivateAsync(bool hasMapViewChanged)
        {
            _rebuildSymbol = true;
            SizingCursorState.DiameterChanged += OnDiameterChanged;
            SizingCursorState.ColorChanged += OnColorChanged;
            return Task.CompletedTask;
        }

        protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
        {
            SizingCursorState.DiameterChanged -= OnDiameterChanged;
            SizingCursorState.ColorChanged -= OnColorChanged;
            return QueuedTask.Run(() =>
            {
                _circleOverlay?.Dispose(); _circleOverlay = null;
                _crossOverlay?.Dispose();  _crossOverlay = null;
            });
        }

        private void OnDiameterChanged(object sender, EventArgs e) => Draw();
        private void OnColorChanged(object sender, EventArgs e) { _rebuildSymbol = true; Draw(); }

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

                    if (_rebuildSymbol || _lineSymRef == null)
                    {
                        _lineSymRef = SymbolFactory.Instance
                            .ConstructLineSymbol(SizingCursorState.CursorColor, 1.5)
                            .MakeSymbolReference();
                        _rebuildSymbol = false;
                    }

                    var pt = mv.ClientToMap(client);
                    if (pt == null) return;
                    var sr = pt.SpatialReference ?? mv.Map.SpatialReference;
                    double radius = SizingCursorState.Diameter / 2.0;

                    // --- geodesic circle of the set diameter ---
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

        // Sketch completes on click (tiny rectangle) or drag (real rectangle).
        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            return QueuedTask.Run(() =>
            {
                var mv = MapView.Active;
                if (mv == null || geometry == null) return true;

                // Only a real drag (non-empty rectangle) selects; a plain click does nothing.
                var env = geometry.Extent;
                if (env == null || (env.Width < 1e-6 && env.Height < 1e-6))
                    return true;

                var method = SelectionCombinationMethod.New;
                if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) method = SelectionCombinationMethod.Add;
                else if ((Keyboard.Modifiers & ModifierKeys.Control) != 0) method = SelectionCombinationMethod.Subtract;

                mv.SelectFeatures(geometry, method);   // rectangle marquee only
                return true;
            });
        }
    }
}
