using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISSilverlightSDK
{
    public partial class ToolkitFeatureDataGrid : UserControl
    {
        private Graphic _lastGraphic;
        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
            new ESRI.ArcGIS.Client.Projection.WebMercator();

        public ToolkitFeatureDataGrid()
        {
            InitializeComponent();

            ESRI.ArcGIS.Client.Geometry.Envelope initialExtent =
                    new ESRI.ArcGIS.Client.Geometry.Envelope(
                _mercator.FromGeographic(
                    new ESRI.ArcGIS.Client.Geometry.MapPoint(-122.4545596, 37.783443296)) as MapPoint,
                _mercator.FromGeographic(
                    new ESRI.ArcGIS.Client.Geometry.MapPoint(-122.4449924, 37.786447331)) as MapPoint);

            initialExtent.SpatialReference = new SpatialReference(102100);

            MyMap.Extent = initialExtent;
        }

        private void FeatureLayer_MouseLeftButtonUp(object sender, GraphicMouseButtonEventArgs e)
        {
            if (_lastGraphic != null)
                _lastGraphic.UnSelect();

            e.Graphic.Select();
            if (e.Graphic.Selected)
                MyDataGrid.ScrollIntoView(e.Graphic, null);

            _lastGraphic = e.Graphic;
        }
    }
}
