using System.Windows.Controls;

namespace ArcGISSilverlightSDK
{
    public partial class FeatureLayerSelection : UserControl
    {
        private static ESRI.ArcGIS.Client.Projection.WebMercator mercator = new ESRI.ArcGIS.Client.Projection.WebMercator();

        ESRI.ArcGIS.Client.Geometry.Envelope initialExtent = new ESRI.ArcGIS.Client.Geometry.Envelope(
            mercator.FromGeographic(new ESRI.ArcGIS.Client.Geometry.MapPoint(-117.190346717, 34.0514888762)) as ESRI.ArcGIS.Client.Geometry.MapPoint,
            mercator.FromGeographic(new ESRI.ArcGIS.Client.Geometry.MapPoint(-117.160305976, 34.072946548)) as ESRI.ArcGIS.Client.Geometry.MapPoint)
        {
            SpatialReference = new ESRI.ArcGIS.Client.Geometry.SpatialReference(102100)
        };

        public FeatureLayerSelection()
        {
            InitializeComponent();

            MyMap.Extent = initialExtent;
        }
    }
}
