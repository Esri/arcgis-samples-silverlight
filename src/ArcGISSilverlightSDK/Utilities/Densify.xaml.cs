using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class Densify : UserControl
    {
        private Draw MyDrawObject;

        public Densify()
        {
            InitializeComponent();
            MyDrawObject = new Draw(MyMap)
                {
                    DrawMode = DrawMode.Polygon,
                    IsEnabled = true,
                    FillSymbol = LayoutRoot.Resources["DefaultFillSymbol"] as ESRI.ArcGIS.Client.Symbols.FillSymbol
                };
            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;
            MyDrawObject.DrawBegin += MyDrawObject_DrawBegin;
        }

        private void MyDrawObject_DrawBegin(object sender, EventArgs args)
        {
            GraphicsLayer graphicsLayerPolygon = MyMap.Layers["PolygonGraphicsLayer"] as GraphicsLayer;
            graphicsLayerPolygon.ClearGraphics();
            GraphicsLayer graphicsLayerVertices = MyMap.Layers["VerticesGraphicsLayer"] as GraphicsLayer;
            graphicsLayerVertices.ClearGraphics();
        }

        private void MyDrawObject_DrawComplete(object sender, DrawEventArgs args)
        {
            ESRI.ArcGIS.Client.Geometry.Polygon polygon = args.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon;
            polygon.SpatialReference = MyMap.SpatialReference;
            Graphic graphic = new Graphic()
            {
                Symbol = LayoutRoot.Resources["DefaultFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol,
                Geometry = polygon
            };

            GraphicsLayer graphicsLayerPolygon = MyMap.Layers["PolygonGraphicsLayer"] as GraphicsLayer;
            graphicsLayerPolygon.Graphics.Add(graphic);

            GraphicsLayer graphicsLayerVertices = MyMap.Layers["VerticesGraphicsLayer"] as GraphicsLayer;
            foreach (MapPoint point in polygon.Rings[0])
            {
                Graphic vertice = new Graphic()
                {
                    Symbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol,
                    Geometry = point
                };
                graphicsLayerVertices.Graphics.Add(vertice);
            }
            DensifyButton.IsEnabled = true;
        }

        private void DensifyButton_Click(object sender, RoutedEventArgs e)
        {
            DensifyButton.IsEnabled = false;

            GraphicsLayer graphicsLayerPolygon = MyMap.Layers["PolygonGraphicsLayer"] as GraphicsLayer;
            
            GeometryService geometryService =
                        new GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
            geometryService.DensifyCompleted += GeometryService_DensifyCompleted;
            geometryService.Failed += GeometryService_Failed;

            DensifyParameters densityParameters = new DensifyParameters()
            {
                LengthUnit = LinearUnit.Meter,
                Geodesic = true,
                MaxSegmentLength = MyMap.Resolution * 10
            };

            geometryService.DensifyAsync(graphicsLayerPolygon.Graphics.ToList(), densityParameters);
        }

        void GeometryService_DensifyCompleted(object sender, GraphicsEventArgs e)
        {
            GraphicsLayer graphicsLayerVertices = MyMap.Layers["VerticesGraphicsLayer"] as GraphicsLayer;
            foreach (Graphic g in e.Results)
            {
                ESRI.ArcGIS.Client.Geometry.Polygon p = g.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon;

                foreach (ESRI.ArcGIS.Client.Geometry.PointCollection pc in p.Rings)
                {
                    foreach (MapPoint point in pc)
                    {
                        Graphic vertice = new Graphic()
                        {
                            Symbol = LayoutRoot.Resources["NewMarkerSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol,
                            Geometry = point
                        };
                        graphicsLayerVertices.Graphics.Add(vertice);
                    }
                }
            }
            DensifyButton.IsEnabled = true;
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
        }
    }
}
