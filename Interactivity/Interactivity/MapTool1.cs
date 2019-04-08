using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace Interactivity
{
    internal class MapTool1 : MapTool
    {
        private Dockpane1ViewModel pane = FrameworkApplication.DockPaneManager.Find("Interactivity_Dockpane1") as Dockpane1ViewModel;

        public MapTool1()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Rectangle;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            var mv = MapView.Active;
            var identifyResult = await QueuedTask.Run(() =>
            {
                var sb = new StringBuilder();

                // Get the features that intersect the sketch geometry.
                var features = mv.GetFeatures(geometry);
                mv.SelectFeatures(geometry);
                // Get all layer definitions.
                var lyrs = mv.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
                foreach (var lyr in lyrs)
                {
                    var fCnt = features.ContainsKey(lyr) ? features[lyr].Count : 0;
                    sb.AppendLine($@"{fCnt} {(fCnt <= 1 ? "record" : "records")} for {lyr.Name}");
                }

                return sb.ToString();
            });
            pane.SelectedFeaturesText = identifyResult;
            return true;
        }

        private static bool bIsMouseMoveActive = false;

        protected override void OnToolMouseMove(MapViewMouseEventArgs e)
        {
            // bIsMouseMoveActive is preventing the creation of too many threads  via QueuedTask.Run
            // especially if imagery is via a remote service
            if (bIsMouseMoveActive) return;
            bIsMouseMoveActive = true;

            MapPoint map_point = null;
            // IPoint point = activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y);
            Task t = QueuedTask.Run(() =>
            {
                map_point = MapView.Active.ClientToMap(e.ClientPoint);
            });
            t.Wait();

            pane.PositionLabel = string.Format("{0:0.###} {1:0.###}", map_point.X, map_point.Y);
            bIsMouseMoveActive = false;
        }

        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
        {
            MapPoint map_point = null;
            Task t = QueuedTask.Run(() =>
            {
                map_point = MapView.Active.ClientToMap(e.ClientPoint);
            });
            t.Wait();

            pane.PositionLabel = string.Format("Clicked @ ({0:0.###}, {1:0.###})!", map_point.X, map_point.Y);
        }
    }
}
