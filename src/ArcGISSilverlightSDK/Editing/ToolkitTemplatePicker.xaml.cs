using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISSilverlightSDK
{
    public partial class ToolkitTemplatePicker : UserControl
    {
        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
            new ESRI.ArcGIS.Client.Projection.WebMercator();
        
        public ToolkitTemplatePicker()
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

        private void MyTemplatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            string[] myLayerIDs = { "Points of Interest", "Evacuation Perimeter" };
            MyTemplatePicker.LayerIDs = myLayerIDs;
        }       
    }
}
