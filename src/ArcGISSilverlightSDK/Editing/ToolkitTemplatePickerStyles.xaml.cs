using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISSilverlightSDK
{
    public partial class ToolkitTemplatePickerStyles : UserControl
    {
        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
            new ESRI.ArcGIS.Client.Projection.WebMercator();

        public ToolkitTemplatePickerStyles()
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

        private void MyTemplatePicker1_Loaded(object sender, RoutedEventArgs e)
        {
            string[] myLayerIDs = { "Points of Interest", "Evacuation Perimeter" };
            MyTemplatePicker1.LayerIDs = myLayerIDs;
        }

        private void MyTemplatePicker2_Loaded(object sender, RoutedEventArgs e)
        {
            string[] myLayerIDs = { "Points of Interest", "Evacuation Perimeter" };
            MyTemplatePicker2.LayerIDs = myLayerIDs;
        }

        private void MyTemplatePicker3_Loaded(object sender, RoutedEventArgs e)
        {
            string[] myLayerIDs = { "Points of Interest", "Evacuation Perimeter" };
            MyTemplatePicker3.LayerIDs = myLayerIDs;
        }
    }
}
