using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISSilverlightSDK
{
    public partial class ToolkitFeatureDataForm : UserControl
    {
        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
               new ESRI.ArcGIS.Client.Projection.WebMercator();

        public ToolkitFeatureDataForm()
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

        private void FeatureLayer_MouseLeftButtonUp(object sender, GraphicMouseButtonEventArgs args)
        {
            FeatureLayer featureLayer = sender as FeatureLayer;

            foreach (Graphic g in featureLayer.Graphics)
              if (g.Selected)
                g.UnSelect();

            args.Graphic.Select();
            MyFeatureDataForm.GraphicSource = args.Graphic;

            FeatureDataFormBorder.Visibility = Visibility.Visible;
        }
    }
}
