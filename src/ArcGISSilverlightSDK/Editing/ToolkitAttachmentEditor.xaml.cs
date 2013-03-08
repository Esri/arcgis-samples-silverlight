using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISSilverlightSDK
{
    public partial class ToolkitAttachmentEditor : UserControl
    {
        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
            new ESRI.ArcGIS.Client.Projection.WebMercator();

        public ToolkitAttachmentEditor()
        {
            InitializeComponent();

            ESRI.ArcGIS.Client.Geometry.Envelope initialExtent =
                    new ESRI.ArcGIS.Client.Geometry.Envelope(
                _mercator.FromGeographic(
                    new ESRI.ArcGIS.Client.Geometry.MapPoint(-122.4306073721, 37.7666097907)) as MapPoint,
                _mercator.FromGeographic(
                    new ESRI.ArcGIS.Client.Geometry.MapPoint(-122.4230971868, 37.77197420877)) as MapPoint);

            initialExtent.SpatialReference = new SpatialReference(102100);

            MyMap.Extent = initialExtent;
        }

        private void FeatureLayer_MouseLeftButtonUp(object sender, GraphicMouseButtonEventArgs e)
        {
            FeatureLayer featureLayer = sender as FeatureLayer;

            foreach (Graphic g in featureLayer.Graphics)
              if (g.Selected)
                g.UnSelect();

            e.Graphic.Select();
            MyAttachmentEditor.GraphicSource = e.Graphic;
        }

        private void MyAttachmentEditor_UploadFailed(object sender, ESRI.ArcGIS.Client.Toolkit.AttachmentEditor.UploadFailedEventArgs e)
        {
            MessageBox.Show(e.Result.Message);
        }
    }
}
