using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISSilverlightSDK
{
    public partial class ToolkitEditorWidget : UserControl
    {
        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
                new ESRI.ArcGIS.Client.Projection.WebMercator();

        public ToolkitEditorWidget()
        {
            InitializeComponent();

            ESRI.ArcGIS.Client.Geometry.Envelope initialExtent =
                    new ESRI.ArcGIS.Client.Geometry.Envelope(
                _mercator.FromGeographic(
                    new ESRI.ArcGIS.Client.Geometry.MapPoint(-117.6690936441, 34.19871558256)) as MapPoint,
                _mercator.FromGeographic(
                    new ESRI.ArcGIS.Client.Geometry.MapPoint(-117.411944901, 34.37896002836)) as MapPoint);

            initialExtent.SpatialReference = new SpatialReference(102100);

            MyMap.Extent = initialExtent;
        }

        private void ArcGISDynamicMapServiceLayer_Initialized(object sender, EventArgs e)
        {
            ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer myArcGISDynamicMapServiceLayer = 
                MyMap.Layers["Fire Perimeter"] as ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer;
            int[] myVisibleLayers = {2};
            myArcGISDynamicMapServiceLayer.VisibleLayers = myVisibleLayers;
        }

        private void EditorWidget_Loaded(object sender, RoutedEventArgs e)
        {
            string[] myLayerIDs = {"Points of Interest", "Evacuation Perimeter"};
            MyEditorWidget.LayerIDs = myLayerIDs;
        }      

    }
}
