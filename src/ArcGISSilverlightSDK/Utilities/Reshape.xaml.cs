using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using System.Collections.Generic;
using System.Windows.Media;

namespace ArcGISSilverlightSDK
{
    public partial class Reshape : UserControl
    {
        private Draw MyDrawObject;
        private GraphicsLayer parcelGraphicsLayer;
        private Graphic selectedGraphic;

        public Reshape()
        {
            InitializeComponent();

            MyMap.Layers.LayersInitialized += Layers_LayersInitialized;

            MyMap.MinimumResolution = double.Epsilon;

            MyDrawObject = new Draw(MyMap)
            {
                DrawMode = DrawMode.Point,
                LineSymbol = LayoutRoot.Resources["RedLineSymbol"] as ESRI.ArcGIS.Client.Symbols.LineSymbol,
                IsEnabled = false
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
            MyDrawObject.IsEnabled = false;

            if (MyDrawObject.DrawMode == DrawMode.Point)
            {
                ESRI.ArcGIS.Client.Geometry.MapPoint point = args.Geometry as ESRI.ArcGIS.Client.Geometry.MapPoint;
                point.SpatialReference = MyMap.SpatialReference;
                System.Windows.Point screenPnt = MyMap.MapToScreen(point);

                // Account for difference between Map and application origin
                GeneralTransform generalTransform = MyMap.TransformToVisual(Application.Current.RootVisual);
                System.Windows.Point transformScreenPnt = generalTransform.Transform(screenPnt);

                IEnumerable<Graphic> selected =
                    parcelGraphicsLayer.FindGraphicsInHostCoordinates(transformScreenPnt);

                if (selected.ToArray().Length <= 0)
                {
                    MyDrawObject.IsEnabled = true;
                    return;
                }

                selectedGraphic = selected.ToList()[0] as Graphic;

                selectedGraphic.Select();

                MyDrawObject.DrawMode = DrawMode.Polyline;
                MyDrawObject.IsEnabled = true;

                InfoTextBlock.Text = LayoutRoot.Resources["EndText"] as string;
            }
            else
            {
                ESRI.ArcGIS.Client.Geometry.Polyline polyline = args.Geometry as ESRI.ArcGIS.Client.Geometry.Polyline;
                polyline.SpatialReference = MyMap.SpatialReference;

                GeometryService geometryService =
                  new GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");

                geometryService.ReshapeCompleted += GeometryService_ReshapeCompleted;
                geometryService.Failed += GeometryService_Failed;

                geometryService.ReshapeAsync(selectedGraphic.Geometry, polyline);
            }
        }

        void GeometryService_ReshapeCompleted(object sender, GeometryEventArgs e)
        {
            parcelGraphicsLayer.Graphics.Remove(selectedGraphic);

            Graphic graphic = new Graphic()
            {
                Symbol = LayoutRoot.Resources["BlueFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol,
                Geometry = e.Result
            };

            parcelGraphicsLayer.Graphics.Add(graphic);

            MyDrawObject.DrawMode = DrawMode.Point;
            MyDrawObject.IsEnabled = true;
            InfoTextBlock.Text = LayoutRoot.Resources["StartText"] as string; 
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
        }
    }
}

