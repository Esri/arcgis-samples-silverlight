using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using System.Collections.Generic;

namespace ArcGISSilverlightSDK
{
    public partial class Intersect : UserControl
    {
        private Draw MyDrawObject;
        private GraphicsLayer parcelGraphicsLayer;
        private GraphicsLayer intersectGraphicsLayer;
        private GeometryService geometryService;
        private Random random;

        public Intersect()
        {
            InitializeComponent();

            MyMap.Layers.LayersInitialized += Layers_LayersInitialized;

            MyMap.MinimumResolution = double.Epsilon;

            MyDrawObject = new Draw(MyMap)
            {
                DrawMode = DrawMode.Polygon,
                IsEnabled = false,
                FillSymbol = LayoutRoot.Resources["CyanFillSymbol"] as ESRI.ArcGIS.Client.Symbols.FillSymbol
            };
            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;

            parcelGraphicsLayer = MyMap.Layers["ParcelsGraphicsLayer"] as GraphicsLayer;
            intersectGraphicsLayer = MyMap.Layers["IntersectGraphicsLayer"] as GraphicsLayer;

            geometryService =
              new GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");

            geometryService.SimplifyCompleted += GeometryService_SimplifyCompleted;
            geometryService.IntersectCompleted += GeometryService_IntersectCompleted;
            geometryService.Failed += GeometryService_Failed;

            random = new Random();
        }

        void Layers_LayersInitialized(object sender, EventArgs args)
        {
            if (parcelGraphicsLayer != null && parcelGraphicsLayer.Graphics.Count == 0)
            {
                QueryTask queryTask =
                    new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1");
                Query query = new Query();
                double contractRatio = MyMap.Extent.Width / 6;
                query.Geometry = new Envelope(MyMap.Extent.GetCenter().X - contractRatio,
                    MyMap.Extent.GetCenter().Y - contractRatio,
                    MyMap.Extent.GetCenter().X + contractRatio,
                    MyMap.Extent.GetCenter().Y + contractRatio) { SpatialReference = MyMap.SpatialReference };
                query.ReturnGeometry = true;
                queryTask.ExecuteCompleted += queryTask_ExecuteCompleted;
                queryTask.Failed += queryTask_Failed;
                queryTask.ExecuteAsync(query);
            }
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

            ESRI.ArcGIS.Client.Geometry.Polygon intersectPolygon = args.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon;
            intersectPolygon.SpatialReference = MyMap.SpatialReference;

            List<Graphic> graphicList = new List<Graphic>();
            graphicList.Add(new Graphic() { Geometry = intersectPolygon });
            geometryService.SimplifyAsync(graphicList);
        }

        void GeometryService_SimplifyCompleted(object sender, GraphicsEventArgs e)
        {
            doIntersect(e.Results[0].Geometry);
        }

        private void doIntersect(Geometry intersectGeometry)
        {
            geometryService.IntersectAsync(parcelGraphicsLayer.Graphics.ToList(), intersectGeometry);
        }

        void GeometryService_IntersectCompleted(object sender, GraphicsEventArgs e)
        {
            intersectGraphicsLayer.ClearGraphics();

            foreach (Graphic g in e.Results)
            {
                SimpleFillSymbol symbol = new SimpleFillSymbol()
                {
                    Fill = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromArgb(255, (byte)random.Next(0, 255), (byte)random.Next(0, 255), 
                        (byte)random.Next(0, 255))),
                    BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black),
                    BorderThickness = 1
                };
                g.Symbol = symbol;
                intersectGraphicsLayer.Graphics.Add(g);
            }       
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
        }
        
        void queryTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Query error: " + e.Error);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            intersectGraphicsLayer.ClearGraphics();
            MyDrawObject.IsEnabled = true;
        }
    }
}
