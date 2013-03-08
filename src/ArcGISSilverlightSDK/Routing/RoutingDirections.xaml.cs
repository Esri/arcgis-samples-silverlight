using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class RoutingDirections : UserControl
    {
        RouteTask _routeTask;
        Locator _locator;
        List<Graphic> _stops = new List<Graphic>();
        Graphic _activeSegmentGraphic;
        RouteParameters _routeParams;
        DirectionsFeatureSet _directionsFeatureSet;

        public RoutingDirections()
        {
            InitializeComponent();

            _routeParams = new RouteParameters()
            {
                ReturnRoutes = false,
                ReturnDirections = true,
                DirectionsLengthUnits = esriUnits.esriMiles,
                Stops = _stops,
                UseTimeWindows = false
            };

            _routeTask =
                new RouteTask("http://tasks.arcgisonline.com/ArcGIS/rest/services/NetworkAnalysis/ESRI_Route_NA/NAServer/Route");
            _routeTask.SolveCompleted += routeTask_SolveCompleted;
            _routeTask.Failed += task_Failed;

            _locator =
                new Locator("http://tasks.arcgisonline.com/ArcGIS/rest/services/Locators/TA_Address_NA/GeocodeServer");
            _locator.AddressToLocationsCompleted += locator_AddressToLocationsCompleted;
            _locator.Failed += task_Failed;
        }
        
        private void GetDirections_Click(object sender, RoutedEventArgs e)
        {
            //Reset
            DirectionsStackPanel.Children.Clear();
            _stops.Clear();

            (MyMap.Layers["MyRouteGraphicsLayer"] as GraphicsLayer).ClearGraphics();
            _locator.CancelAsync();
            _routeTask.CancelAsync();

            //Geocode from address
            _locator.AddressToLocationsAsync(ParseAddress(FromTextBox.Text), "from");

        }

        void locator_AddressToLocationsCompleted(object sender, AddressToLocationsEventArgs e)
        {
            GraphicsLayer graphicsLayer = (MyMap.Layers["MyRouteGraphicsLayer"] as GraphicsLayer);
            if (e.Results.Count > 0)
            {
                AddressCandidate address = e.Results[0];
                Graphic graphicLocation = new Graphic() { Geometry = address.Location };
                graphicLocation.Attributes.Add("address", address.Address);
                graphicLocation.Attributes.Add("score", address.Score);

                _stops.Add(graphicLocation);
                if ((string)e.UserState == "from")
                {
                    graphicLocation.Symbol = LayoutRoot.Resources["FromSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                    //Geocode to address
                    _locator.AddressToLocationsAsync(ParseAddress(ToTextBox.Text), "to");
                }
                else
                {
                    graphicLocation.Symbol = LayoutRoot.Resources["ToSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol; 
                    //Get route between from and to
                    _routeParams.OutSpatialReference = MyMap.SpatialReference;
                    _routeTask.SolveAsync(_routeParams);
                }

                graphicsLayer.Graphics.Add(graphicLocation);
            }
        }

        private void routeTask_SolveCompleted(object sender, RouteEventArgs e)
        {
            GraphicsLayer graphicsLayer = MyMap.Layers["MyRouteGraphicsLayer"] as GraphicsLayer;
            RouteResult routeResult = e.RouteResults[0];
            _directionsFeatureSet = routeResult.Directions;

            graphicsLayer.Graphics.Add(new Graphic() { Geometry = _directionsFeatureSet.MergedGeometry, Symbol = LayoutRoot.Resources["RouteSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol });
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

        private void task_Failed(object sender, TaskFailedEventArgs e)
        {
            string errorMessage = "Routing error: ";
            errorMessage += e.Error.Message;
            foreach (string detail in (e.Error as ServiceException).Details)
                errorMessage += "," + detail;

            MessageBox.Show(errorMessage);
        }

        private void directionsSegment_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            Graphic feature = textBlock.Tag as Graphic;
            MyMap.ZoomTo(Expand(feature.Geometry.Extent));
            if (_activeSegmentGraphic == null)
            {
                _activeSegmentGraphic = new Graphic() {Symbol = LayoutRoot.Resources["SegmentSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol};
                GraphicsLayer graphicsLayer = MyMap.Layers["MyRouteGraphicsLayer"] as GraphicsLayer;
                graphicsLayer.Graphics.Add(_activeSegmentGraphic);
            }
            _activeSegmentGraphic.Geometry = feature.Geometry;
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_directionsFeatureSet != null)
            {
                GraphicsLayer graphicsLayer = MyMap.Layers["MyRouteGraphicsLayer"] as GraphicsLayer;
                MyMap.ZoomTo(Expand(_directionsFeatureSet.Extent));
            }
        }

        private AddressToLocationsParameters ParseAddress(string address)
        {
            string[] fromArray = address.Split(new char[] { ',' });
            AddressToLocationsParameters fromAddress = new AddressToLocationsParameters();
            fromAddress.OutFields.Add("Loc_name");
            fromAddress.Address.Add("Address", fromArray[0]);
            fromAddress.Address.Add("City", fromArray[1]);
            fromAddress.Address.Add("State", fromArray[2]);
            fromAddress.Address.Add("Zip", fromArray[3]);
            fromAddress.Address.Add("Country", "USA");
            return fromAddress;
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
