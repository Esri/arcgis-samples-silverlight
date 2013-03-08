using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class Union : UserControl
    {
        private Draw MyDrawObject;
        private GraphicsLayer parcelGraphicsLayer;
        List<Graphic> selectedGraphics = new List<Graphic>();

        public Union()
        {
            InitializeComponent();

            MyMap.Layers.LayersInitialized += Layers_LayersInitialized;

            MyMap.MinimumResolution = double.Epsilon;

            MyDrawObject = new Draw(MyMap)
            {
                DrawMode = DrawMode.Point,
                IsEnabled = false,
            };
            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;

            parcelGraphicsLayer = MyMap.Layers["ParcelsGraphicsLayer"] as GraphicsLayer;
        }

        void Layers_LayersInitialized(object sender, EventArgs args)
        {
            if (parcelGraphicsLayer != null && parcelGraphicsLayer.Graphics.Count == 0)
            {
                QueryTask queryTask =
                    new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1");
                Query query = new Query();
                query.Geometry = MyMap.Extent;
                query.ReturnGeometry = true;
                queryTask.ExecuteCompleted += queryTask_ExecuteCompleted;
                queryTask.Failed += queryTask_Failed;
                queryTask.ExecuteAsync(query);
            }
        }

        void queryTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Query error: " + e.Error);
        }

        void queryTask_ExecuteCompleted(object sender, QueryEventArgs e)
        {
            foreach (Graphic g in e.FeatureSet.Features)
            {
                g.Symbol = LayoutRoot.Resources["BlueFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                g.Geometry.SpatialReference = MyMap.SpatialReference;
                parcelGraphicsLayer.Graphics.Add(g);
            }
            MyDrawObject.IsEnabled = true;
        }

        private void MyDrawObject_DrawComplete(object sender, DrawEventArgs args)
        {
            ESRI.ArcGIS.Client.Geometry.MapPoint point = args.Geometry as ESRI.ArcGIS.Client.Geometry.MapPoint;
            point.SpatialReference = MyMap.SpatialReference;
            System.Windows.Point screenPnt = MyMap.MapToScreen(point);

            // Account for difference between Map and application origin
            GeneralTransform generalTransform = MyMap.TransformToVisual(Application.Current.RootVisual);
            System.Windows.Point transformScreenPnt = generalTransform.Transform(screenPnt);

            IEnumerable<Graphic> selected =
                parcelGraphicsLayer.FindGraphicsInHostCoordinates(transformScreenPnt);

            foreach (Graphic g in selected)
                if (g.Selected) { g.UnSelect(); selectedGraphics.Remove(g); }
                else { g.Select(); selectedGraphics.Add(g); }

            if (selectedGraphics.Count > 1)
                UnionButton.IsEnabled = true;
            else
                UnionButton.IsEnabled = false;
        }

        private void UnionButton_Click(object sender, RoutedEventArgs e)
        {
            UnionButton.IsEnabled = false;
            MyDrawObject.IsEnabled = false;

            GeometryService geometryService =
                        new GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
            geometryService.UnionCompleted += GeometryService_UnionCompleted;
            geometryService.Failed += GeometryService_Failed;

            geometryService.UnionAsync(selectedGraphics);
        }

        void GeometryService_UnionCompleted(object sender, GeometryEventArgs e)
        {
            foreach (Graphic g in selectedGraphics)
                parcelGraphicsLayer.Graphics.Remove(g);
            selectedGraphics.Clear();

            parcelGraphicsLayer.Graphics.Add(new Graphic() { Geometry = e.Result, Symbol = LayoutRoot.Resources["BlueFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol });

            MyDrawObject.IsEnabled = true;
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
        }
    }
}
