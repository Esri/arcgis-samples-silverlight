using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Bing.RouteService;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using System.Net;
using System.Json;

namespace ArcGISSilverlightSDK
{
    public partial class BingRouting : UserControl
    {
        private Draw myDrawObject;
        private GraphicsLayer waypointGraphicsLayer;
        private GraphicsLayer routeResultsGraphicsLayer;
        private ESRI.ArcGIS.Client.Bing.Routing routing;
        private static ESRI.ArcGIS.Client.Projection.WebMercator mercator =
           new ESRI.ArcGIS.Client.Projection.WebMercator();

        public BingRouting()
        {
            InitializeComponent();            
        }

        private void BingKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text.Length >= 64)
                LoadMapButton.IsEnabled = true;
            else
                LoadMapButton.IsEnabled = false;
        }

        private void LoadMapButton_Click(object sender, RoutedEventArgs e)
        {
            WebClient webClient = new WebClient();
            string uri = string.Format("http://dev.virtualearth.net/REST/v1/Imagery/Metadata/Aerial?supressStatus=true&key={0}", BingKeyTextBox.Text);

            webClient.OpenReadCompleted += (s, a) =>
            {
                if (a.Error == null)
                {
                    JsonValue jsonResponse = JsonObject.Load(a.Result);
                    string authenticationResult = jsonResponse["authenticationResultCode"];
                    a.Result.Close();

                    if (authenticationResult == "ValidCredentials")
                    {
                        ESRI.ArcGIS.Client.Bing.TileLayer tileLayer = new TileLayer()
                        {
                            ID = "BingLayer",
                            LayerStyle = TileLayer.LayerType.Road,
                            ServerType = ServerType.Production,
                            Token = BingKeyTextBox.Text
                        };

                        MyMap.Layers.Insert(0, tileLayer);

                        // Add your Bing Maps key in the constructor for the Routing class. Use http://www.bingmapsportal.com to generate a key.  
                        routing = new ESRI.ArcGIS.Client.Bing.Routing(BingKeyTextBox.Text);
                        routing.ServerType = ServerType.Production;

                        myDrawObject = new Draw(MyMap)
                        {
                            DrawMode = DrawMode.Point,
                            IsEnabled = true
                        };

                        myDrawObject.DrawComplete += MyDrawObject_DrawComplete;
                        waypointGraphicsLayer = MyMap.Layers["WaypointGraphicsLayer"] as GraphicsLayer;
                        routeResultsGraphicsLayer = MyMap.Layers["RouteResultsGraphicsLayer"] as GraphicsLayer;

                        ESRI.ArcGIS.Client.Geometry.Envelope initialExtent =
                                new ESRI.ArcGIS.Client.Geometry.Envelope(
                            mercator.FromGeographic(
                                new ESRI.ArcGIS.Client.Geometry.MapPoint(-130, 20)) as MapPoint,
                            mercator.FromGeographic(
                                new ESRI.ArcGIS.Client.Geometry.MapPoint(-65, 55)) as MapPoint);

                        initialExtent.SpatialReference = new SpatialReference(102100);

                        MyMap.Extent = initialExtent;

                        BingKeyGrid.Visibility = System.Windows.Visibility.Collapsed;
                        RouteGrid.Visibility = System.Windows.Visibility.Visible;

                        InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Collapsed;

                    }
                    else InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Visible;
                }
                else InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Visible;
            };

            webClient.OpenReadAsync(new System.Uri(uri));
        }   

        private void MyDrawObject_DrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs args)
        {
            myDrawObject.IsEnabled = false;

            Graphic waypointGraphic = new Graphic()
            {
                Geometry = MyMap.WrapAroundIsActive ? Geometry.NormalizeCentralMeridian(args.Geometry) : args.Geometry,
                Symbol = LayoutRoot.Resources["UserStopSymbol"] as Symbol
            };

            waypointGraphic.Attributes.Add("StopNumber", waypointGraphicsLayer.Graphics.Count + 1);
            waypointGraphicsLayer.Graphics.Add(waypointGraphic);

            if (waypointGraphicsLayer.Graphics.Count > 1)
                RouteButton.IsEnabled = true;
            myDrawObject.IsEnabled = true;
        }

        private void RouteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            myDrawObject.IsEnabled = false;
            routing.Optimization = RouteOptimization.MinimizeTime;
            routing.TrafficUsage = TrafficUsage.None;
            routing.TravelMode = TravelMode.Driving;
            routing.Route(GraphicsToMapPoints(), Route_Complete);
        }

        private List<MapPoint> GraphicsToMapPoints()
        {
            List<MapPoint> mapPoints = new List<MapPoint>();
            foreach (Graphic g in waypointGraphicsLayer.Graphics)
                mapPoints.Add(g.Geometry as MapPoint);

            return mapPoints;
        }

        private void Route_Complete(object sender, CalculateRouteCompletedEventArgs args)
        {
            myDrawObject.IsEnabled = true;
            routeResultsGraphicsLayer.ClearGraphics();
            waypointGraphicsLayer.ClearGraphics();

            StringBuilder directions = new StringBuilder();

            ObservableCollection<RouteLeg> routeLegs = args.Result.Result.Legs;
            int numLegs = routeLegs.Count;
            int instructionCount = 0;
            for (int n = 0; n < numLegs; n++)
            {
                if ((n % 2) == 0)
                {
                    AddStopPoint(mercator.FromGeographic(new MapPoint(routeLegs[n].ActualStart.Longitude, routeLegs[n].ActualStart.Latitude)) as MapPoint);
                    AddStopPoint(mercator.FromGeographic(new MapPoint(routeLegs[n].ActualEnd.Longitude, routeLegs[n].ActualEnd.Latitude)) as MapPoint);
                }
                else if (n == (numLegs - 1))
                {
                    AddStopPoint(mercator.FromGeographic(new MapPoint(routeLegs[n].ActualEnd.Longitude, routeLegs[n].ActualEnd.Latitude)) as MapPoint);
                }

                directions.Append(string.Format("--Leg #{0}--\n", n + 1));

                foreach (ItineraryItem item in routeLegs[n].Itinerary)
                {
                    instructionCount++;
                    directions.Append(string.Format("{0}. {1}\n", instructionCount, item.Text));
                }
            }

            Regex regex = new Regex("<[/a-zA-Z:]*>",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);

            DirectionsContentTextBlock.Text = regex.Replace(directions.ToString(), string.Empty);
            DirectionsGrid.Visibility = Visibility.Visible;

            RoutePath routePath = args.Result.Result.RoutePath;

            Polyline line = new Polyline();
            line.Paths.Add(new PointCollection());

            foreach (ESRI.ArcGIS.Client.Bing.RouteService.Location location in routePath.Points)
                line.Paths[0].Add(mercator.FromGeographic(new MapPoint(location.Longitude, location.Latitude)) as MapPoint);

            Graphic graphic = new Graphic()
            {
                Geometry = line,
                Symbol = LayoutRoot.Resources["RoutePathSymbol"] as Symbol
            };
            routeResultsGraphicsLayer.Graphics.Add(graphic);
        }

        private void AddStopPoint(MapPoint mapPoint)
        {
            Graphic graphic = new Graphic()
            {
                Geometry = mapPoint,
                Symbol = LayoutRoot.Resources["ResultStopSymbol"] as Symbol
            };
            graphic.Attributes.Add("StopNumber", waypointGraphicsLayer.Graphics.Count + 1);
            waypointGraphicsLayer.Graphics.Add(graphic);
        }

        private void ClearRouteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            waypointGraphicsLayer.ClearGraphics();
            routeResultsGraphicsLayer.ClearGraphics();
            DirectionsContentTextBlock.Text = "";
            DirectionsGrid.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
