using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class AreasAndLengths : UserControl
    {
        private Draw MyDrawObject;
        private static ESRI.ArcGIS.Client.Projection.WebMercator mercator = 
            new ESRI.ArcGIS.Client.Projection.WebMercator();

        public AreasAndLengths()
        {
            InitializeComponent();

            ESRI.ArcGIS.Client.Geometry.Envelope initialExtent =
                    new ESRI.ArcGIS.Client.Geometry.Envelope(
                mercator.FromGeographic(
                    new ESRI.ArcGIS.Client.Geometry.MapPoint(-130, 20)) as MapPoint,
                mercator.FromGeographic(
                    new ESRI.ArcGIS.Client.Geometry.MapPoint(-65, 55)) as MapPoint);

            initialExtent.SpatialReference = new SpatialReference(102100);

            MyMap.Extent = initialExtent;

            MyDrawObject = new Draw(MyMap)
            {
                DrawMode = DrawMode.Polygon,
                IsEnabled = true,
                FillSymbol = LayoutRoot.Resources["DefaultFillSymbol"] as ESRI.ArcGIS.Client.Symbols.FillSymbol
            };
            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;
            MyDrawObject.DrawBegin += MyDrawObject_DrawBegin;

            calculationTypeCombo.ItemsSource = Enum.GetValues(typeof(CalculationType));
            calculationTypeCombo.SelectedIndex = 0;

        }

        private void MyDrawObject_DrawBegin(object sender, EventArgs args)
        {
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.ClearGraphics();
        }

        private void MyDrawObject_DrawComplete(object sender, DrawEventArgs args)
        {
            ESRI.ArcGIS.Client.Geometry.Polygon polygon = args.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon;
            polygon.SpatialReference = MyMap.SpatialReference;
            Graphic graphic = new Graphic()
            {
                Symbol = LayoutRoot.Resources["DefaultFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol,
                Geometry = polygon,
            };

            GeometryService geometryService =
                new GeometryService("http://serverapps101.esri.com/arcgis/rest/services/Geometry/GeometryServer");
            geometryService.AreasAndLengthsCompleted += GeometryService_AreasAndLengthsCompleted;
            geometryService.Failed += GeometryService_Failed;

            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.Graphics.Add(graphic);

            List<Graphic> graphicList = new List<Graphic>();
            graphicList.Add(graphic);

            // Since there are multiple overloads for AreasAndLengthsAsync, make sure to use appropriate signature with correct parameter types.
            geometryService.AreasAndLengthsAsync(graphicList,null,null, (CalculationType)calculationTypeCombo.SelectedValue );

            // GeometryService.AreasAndLengths returns distances and areas in the units of the spatial reference.
            // The units in the map view's projection is decimal degrees. 
            // Use the Project method to convert graphic points to a projection that uses a measured unit (e.g. meters).
            // If the map units are in measured units, the call to Project is unnecessary. 
            // Important: Use a projection appropriate for your area of interest.
        }

        private void GeometryService_AreasAndLengthsCompleted(object sender, AreasAndLengthsEventArgs args)
        {
            // convert results from meters into miles and sq meters into sq miles for our display
            double miles = args.Results.Lengths[0] * 0.0006213700922;
            double sqmi = Math.Abs(args.Results.Areas[0]) * 0.0000003861003;
            ResponseTextBlock.Text = String.Format("Polygon area: {0} sq. miles\nPolygon perimeter: {1} miles.", Math.Round(sqmi, 3), Math.Round(miles, 3));
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
        }
    }
}
