using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;

namespace ArcGISSilverlightSDK
{
    public partial class GeometryJson : UserControl
    {
        GraphicsLayer _myToJsonGraphicsLayer;
        GraphicsLayer _myFromJsonGraphicsLayer;
        Draw _myDrawObject;

        string jsonPoint = @"{""x"":-100.609,""y"":43.729,""spatialReference"":{""wkid"":4326}}";

        string jsonPolyline = @"{""paths"":[[[0,51.399],
[2.637,48.865],
[12.568,41.706],
[13.447,52.483],
[21.357,52.160],
[30.322,59.845]]],
""spatialReference"":{""wkid"":4326}}";

        string jsonPolygon = @"{""rings"":[[[110.039,-20.303],
[132.539,-7.0137],
[153.281,-13.923],
[162.773,-35.174],
[133.594,-43.180],
[111.797,-36.032],
[110.039,-20.303]]],
""spatialReference"":{""wkid"":4326}}";

        string jsonEnvelope = @"{""xmin"" : -109.55, ""ymin"" : 25.76, 
""xmax"" : -86.39, ""ymax"" : 49.94,
""spatialReference"" : {""wkid"" : 4326}}";

        public GeometryJson()
        {
            InitializeComponent();

            _myFromJsonGraphicsLayer = MyMap.Layers["MyFromJsonGraphicsLayer"] as GraphicsLayer;
            _myToJsonGraphicsLayer = MyMap.Layers["MyToJsonGraphicsLayer"] as GraphicsLayer;
            InJsonTextBox.Text = jsonPoint;

            _myDrawObject = new Draw(MyMap)
            {
                LineSymbol = LayoutRoot.Resources["DrawLineSymbol"] as LineSymbol,
                FillSymbol = LayoutRoot.Resources["DrawFillSymbol"] as FillSymbol
            };
            _myDrawObject.DrawComplete += MyDrawObject_DrawComplete;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ClearGraphicsLayers();
            InJsonTextBox.Text = "";
            OutJsonTextBox.Text = "";
        }

        private void PointJsonButton_Click(object sender, RoutedEventArgs e)
        {
            ClearGraphicsLayers();
            InJsonTextBox.Text = jsonPoint;
        }

        private void PolylineJsonButton_Click(object sender, RoutedEventArgs e)
        {
            ClearGraphicsLayers();
            InJsonTextBox.Text = jsonPolyline;
        }

        private void PolygonJsonButton_Click(object sender, RoutedEventArgs e)
        {
            ClearGraphicsLayers();
            InJsonTextBox.Text = jsonPolygon;
        }

        private void EnvelopeJsonButton_Click(object sender, RoutedEventArgs e)
        {
            ClearGraphicsLayers();
            InJsonTextBox.Text = jsonEnvelope;
        }

        private void ClearGraphicsLayers()
        {
            _myToJsonGraphicsLayer.Graphics.Clear();
            _myFromJsonGraphicsLayer.Graphics.Clear();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ClearGraphicsLayers();
            OutJsonTextBox.Text = "";

            try
            {
                // Convert from Geometry to ArcGIS REST geometry json
                Geometry geometry = Geometry.FromJson(InJsonTextBox.Text);

                Graphic graphic = new Graphic();

                if (geometry is MapPoint)
                    graphic.Symbol = LayoutRoot.Resources["RedMarkerSymbol"] as SimpleMarkerSymbol;
                else if (geometry is Polyline)
                    graphic.Symbol = LayoutRoot.Resources["RedLineSymbol"] as SimpleLineSymbol;
                else if (geometry is Polygon)
                    graphic.Symbol = LayoutRoot.Resources["RedFillSymbol"] as SimpleFillSymbol;
                else if (geometry is Envelope)
                    graphic.Symbol = LayoutRoot.Resources["RedFillSymbol"] as SimpleFillSymbol;

                if (graphic.Symbol != null)
                {
                    graphic.Geometry = geometry;
                    _myFromJsonGraphicsLayer.Graphics.Add(graphic);
                }
            }
            catch
            {
                MessageBox.Show("Unable to convert json into geometry");
            }
        }

        private void DrawGeometryButton_Click(object sender, RoutedEventArgs e)
        {
            ClearGraphicsLayers();
            OutJsonTextBox.Text = "";

            switch ((sender as Button).Tag as string)
            {
                case "DrawPoint":
                    _myDrawObject.DrawMode = DrawMode.Point;
                    break;
                case "DrawPolyline":
                    _myDrawObject.DrawMode = DrawMode.Polyline;
                    break;
                case "DrawPolygon":
                    _myDrawObject.DrawMode = DrawMode.Polygon;
                    break;
                case "DrawRectangle":
                    _myDrawObject.DrawMode = DrawMode.Rectangle;
                    break;
                default:
                    _myDrawObject.DrawMode = DrawMode.None;
                    ClearGraphicsLayers();
                    break;
            }
            _myDrawObject.IsEnabled = (_myDrawObject.DrawMode != DrawMode.None);
        }

        private void MyDrawObject_DrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs args)
        {
            ClearGraphicsLayers();

            Graphic graphic = new Graphic();
            if (args.Geometry is MapPoint)
                graphic.Symbol = LayoutRoot.Resources["BlueMarkerSymbol"] as SimpleMarkerSymbol;
            else if (args.Geometry is Polyline)
                graphic.Symbol = LayoutRoot.Resources["BlueLineSymbol"] as SimpleLineSymbol;
            else if (args.Geometry is Polygon)
                graphic.Symbol = LayoutRoot.Resources["BlueFillSymbol"] as SimpleFillSymbol;
            else if (args.Geometry is Envelope)
                graphic.Symbol = LayoutRoot.Resources["BlueFillSymbol"] as SimpleFillSymbol;

            if (graphic.Symbol != null)
            {
                graphic.Geometry = args.Geometry;
                _myToJsonGraphicsLayer.Graphics.Add(graphic);
            }

            // Convert from Geometry to ArcGIS REST geometry json
            OutJsonTextBox.Text = args.Geometry.ToJson();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_myFromJsonGraphicsLayer != null || _myToJsonGraphicsLayer != null)
            {
                ClearGraphicsLayers();
                OutJsonTextBox.Text = "";
            }
            else
                return;

            if ((((sender as TabControl).SelectedItem as TabItem).Header as string) == "From JSON")
                _myDrawObject.IsEnabled = false;
        }
    }
}
