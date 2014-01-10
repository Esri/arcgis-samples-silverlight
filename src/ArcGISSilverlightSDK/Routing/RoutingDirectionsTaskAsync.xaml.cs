using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class RoutingDirectionsTaskAsync : UserControl
    {
        RouteTask _routeTask;
        Locator _locator;
        List<Graphic> _stops = new List<Graphic>();
        Graphic _activeSegmentGraphic;
        RouteParameters _routeParams;
        DirectionsFeatureSet _directionsFeatureSet;
        CancellationTokenSource _cts;
        GraphicsLayer _routeGraphicsLayer;

        public RoutingDirectionsTaskAsync()
        {
            InitializeComponent();

            _locator = new Locator("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

            _routeTask = new RouteTask("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route");

            _routeParams = new RouteParameters()
            {
                ReturnRoutes = false,
                ReturnDirections = true,
                DirectionsLengthUnits = esriUnits.esriMiles,
                Stops = _stops,
                UseTimeWindows = false,
            };

            _routeGraphicsLayer = (MyMap.Layers["MyRouteGraphicsLayer"] as GraphicsLayer);            
        }

        private async void GetDirections_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_cts != null)
                    _cts.Cancel();

                _cts = new CancellationTokenSource();

                DirectionsStackPanel.Children.Clear();
                _stops.Clear();
                _routeGraphicsLayer.Graphics.Clear();

                //Geocode from address
                LocatorTaskFindResult fromAddress = await _locator.FindTaskAsync(ParseSearchText(FromTextBox.Text), _cts.Token);
                // if no result?
                Graphic fromLocation = fromAddress.Result.Locations[0].Graphic;
                fromLocation.Geometry.SpatialReference = MyMap.SpatialReference;
                fromLocation.Attributes.Add("name", fromAddress.Result.Locations[0].Name);
                _stops.Add(fromLocation);

                fromLocation.Symbol = LayoutRoot.Resources["FromSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                _routeGraphicsLayer.Graphics.Add(fromLocation);

                //Geocode to address
                LocatorTaskFindResult toAddress = await _locator.FindTaskAsync(ParseSearchText(ToTextBox.Text), _cts.Token);
                Graphic toLocation = toAddress.Result.Locations[0].Graphic;
                toLocation.Geometry.SpatialReference = MyMap.SpatialReference;
                toLocation.Attributes.Add("name", toAddress.Result.Locations[0].Name);
                _stops.Add(toLocation);

                toLocation.Symbol = LayoutRoot.Resources["ToSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                _routeGraphicsLayer.Graphics.Add(toLocation);

                //Get route between from and to
                _routeParams.OutSpatialReference = MyMap.SpatialReference;
                SolveRouteResult solveRouteResult = await _routeTask.SolveTaskAsync(_routeParams, _cts.Token);

                RouteResult routeResult = solveRouteResult.RouteResults[0];
                _directionsFeatureSet = routeResult.Directions;

                _routeGraphicsLayer.Graphics.Add(new Graphic() { Geometry = _directionsFeatureSet.MergedGeometry, Symbol = LayoutRoot.Resources["RouteSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol });
                TotalDistanceTextBlock.Text = string.Format("Total Distance: {0}", FormatDistance(_directionsFeatureSet.TotalLength, "miles"));
                TotalTimeTextBlock.Text = string.Format("Total Time: {0}", FormatTime(_directionsFeatureSet.TotalTime));
                TitleTextBlock.Text = _directionsFeatureSet.RouteName;

                int i = 1;
                foreach (Graphic graphic in _directionsFeatureSet.Features)
                {
                    System.Text.StringBuilder text = new System.Text.StringBuilder();
                    text.AppendFormat("{0}. {1}", i, graphic.Attributes["text"]);
                    if (i > 1 && i < _directionsFeatureSet.Features.Count)
                    {
                        string distance = FormatDistance(Convert.ToDouble(graphic.Attributes["length"]), "miles");
                        string time = null;
                        if (graphic.Attributes.ContainsKey("time"))
                        {
                            time = FormatTime(Convert.ToDouble(graphic.Attributes["time"]));
                        }
                        if (!string.IsNullOrEmpty(distance) || !string.IsNullOrEmpty(time))
                            text.Append(" (");
                        text.Append(distance);
                        if (!string.IsNullOrEmpty(distance) && !string.IsNullOrEmpty(time))
                            text.Append(", ");
                        text.Append(time);
                        if (!string.IsNullOrEmpty(distance) || !string.IsNullOrEmpty(time))
                            text.Append(")");
                    }
                    TextBlock textBlock = new TextBlock() { Text = text.ToString(), Tag = graphic, Margin = new Thickness(4), Cursor = Cursors.Hand };
                    textBlock.MouseLeftButtonDown += new MouseButtonEventHandler(directionsSegment_MouseLeftButtonDown);
                    DirectionsStackPanel.Children.Add(textBlock);
                    i++;
                }
                MyMap.ZoomTo(Expand(_directionsFeatureSet.Extent));
            }
            catch (Exception ex)
            {
                if (ex is ServiceException)
                {
                    MessageBox.Show(String.Format("{0}: {1}", (ex as ServiceException).Code.ToString(), (ex as ServiceException).Details[0]), "Error", MessageBoxButton.OK);
                    return;
                }
            }
        }

        private void directionsSegment_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            Graphic feature = textBlock.Tag as Graphic;
            MyMap.ZoomTo(Expand(feature.Geometry.Extent));
            if (_activeSegmentGraphic == null)
            {
                _activeSegmentGraphic = new Graphic() { Symbol = LayoutRoot.Resources["SegmentSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol };
                _routeGraphicsLayer.Graphics.Add(_activeSegmentGraphic);
            }
            _activeSegmentGraphic.Geometry = feature.Geometry;
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_directionsFeatureSet != null)
                MyMap.ZoomTo(Expand(_directionsFeatureSet.Extent));
        }

        private LocatorFindParameters ParseSearchText(string searchText)
        {
            LocatorFindParameters locatorFindParams = new LocatorFindParameters()
            {
                Text = searchText,
                Location = MyMap.Extent.GetCenter(),
                Distance = MyMap.Extent.Width / 2,
                MaxLocations = 1,
                OutSpatialReference = MyMap.SpatialReference
            };
            return locatorFindParams;
        }

        private string FormatDistance(double dist, string units)
        {
            string result = "";
            double formatDistance = Math.Round(dist, 2);
            if (formatDistance != 0)
            {
                result = formatDistance + " " + units;
            }
            return result;
        }

        private string FormatTime(double minutes)
        {
            TimeSpan time = TimeSpan.FromMinutes(minutes);
            string result = "";
            int hours = (int)Math.Floor(time.TotalHours);
            if (hours > 1)
                result = string.Format("{0} hours ", hours);
            else if (hours == 1)
                result = string.Format("{0} hour ", hours);
            if (time.Minutes > 1)
                result += string.Format("{0} minutes ", time.Minutes);
            else if (time.Minutes == 1)
                result += string.Format("{0} minute ", time.Minutes);
            return result;
        }

        private Envelope Expand(Envelope e)
        {
            double factor = 0.6;
            MapPoint centerMapPoint = e.GetCenter();
            return new Envelope(centerMapPoint.X - e.Width * factor, centerMapPoint.Y - e.Height * factor,
                centerMapPoint.X + e.Width * factor, centerMapPoint.Y + e.Height * factor);
        }
    }
}
