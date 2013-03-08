using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class Distance : UserControl
    {
        private Draw MyDrawObject;
        GraphicsLayer graphicsLayer;

        public Distance()
        {
            InitializeComponent();

            graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;

            MyDrawObject = new Draw(MyMap)
                {
                    DrawMode = DrawMode.Polyline,
                    IsEnabled = true
                };
            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;
            MyDrawObject.DrawBegin += MyDrawObject_DrawBegin;
        }

        private void MyDrawObject_DrawBegin(object sender, EventArgs args)
        {
            if (graphicsLayer.Graphics.Count >= 2)            
                graphicsLayer.ClearGraphics();
        }

        private void MyDrawObject_DrawComplete(object sender, DrawEventArgs args)
        {
            args.Geometry.SpatialReference = MyMap.SpatialReference;
            Graphic graphic = new Graphic(){ Geometry = args.Geometry };
            if (args.Geometry is Polyline)
              graphic.Symbol = LayoutRoot.Resources["DefaultLineSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
            else
              graphic.Symbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;

            graphicsLayer.Graphics.Add(graphic);

            if (graphicsLayer.Graphics.Count == 1)
                MyDrawObject.DrawMode = DrawMode.Point;      
            else if (graphicsLayer.Graphics.Count == 2)
            {
                MyDrawObject.IsEnabled = false;
                GeometryService geometryService =
                            new GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
                geometryService.DistanceCompleted += GeometryService_DistanceCompleted;
                geometryService.Failed += GeometryService_Failed;

                MyDrawObject.DrawMode = DrawMode.Polyline;

                DistanceParameters distanceParameters = new DistanceParameters()
                {
                    DistanceUnit = LinearUnit.SurveyMile,
                    Geodesic = true
                };

                geometryService.DistanceAsync(graphicsLayer.Graphics[0].Geometry, graphicsLayer.Graphics[1].Geometry,distanceParameters);
                ResponseTextBlock.Text = "The distance between geometries is... ";
            }                     
        }

        void GeometryService_DistanceCompleted(object sender, DistanceEventArgs e)
        {
            ResponseTextBlock.Text =
                String.Format("The distance between geometries is {0} miles", Math.Round(e.Distance, 3));
            MyDrawObject.IsEnabled = true;
        }       

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
            MyDrawObject.IsEnabled = true;
        }
    }
}
