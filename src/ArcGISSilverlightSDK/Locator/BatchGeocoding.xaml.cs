using System.Windows.Controls;
using ESRI.ArcGIS.Client.Tasks;
using System.Collections.Generic;
using System.Windows;
using ESRI.ArcGIS.Client;
using System.Linq;
using System.Collections.ObjectModel;
using System;
using ESRI.ArcGIS.Client.Geometry;


namespace ArcGISSilverlightSDK
{
    public partial class BatchGeocoding : UserControl
    {
        Locator _locatorTask;
        ObservableCollection<IDictionary<string, string>> batchaddresses = new ObservableCollection<IDictionary<string, string>>();
        GraphicsLayer geocodedResults;
        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
          new ESRI.ArcGIS.Client.Projection.WebMercator();

        public BatchGeocoding()
        {
            InitializeComponent();

            ESRI.ArcGIS.Client.Geometry.Envelope initialExtent =
                        new ESRI.ArcGIS.Client.Geometry.Envelope(
                    _mercator.FromGeographic(
                        new ESRI.ArcGIS.Client.Geometry.MapPoint(-117.387, 33.97)) as MapPoint,
                    _mercator.FromGeographic(
                        new ESRI.ArcGIS.Client.Geometry.MapPoint(-117.355, 33.988)) as MapPoint);

            initialExtent.SpatialReference = new SpatialReference(102100);

            MyMap.Extent = initialExtent;

            _locatorTask = new Locator("http://serverapps101.esri.com/arcgis/rest/services/USA_Geocode/GeocodeServer");
            _locatorTask.AddressesToLocationsCompleted += _locatorTask_AddressesToLocationsCompleted;
            _locatorTask.Failed += LocatorTask_Failed;

            geocodedResults = MyMap.Layers["LocationGraphicsLayer"] as GraphicsLayer;

            //List of addresses to geocode
            batchaddresses.Add(new Dictionary<string, string> { { "Street", "4409 Redwood Dr" }, { "Zip", "92501" } });
            batchaddresses.Add(new Dictionary<string, string> { { "Street", "3758 Cedar St" }, { "Zip", "92501" } });
            batchaddresses.Add(new Dictionary<string, string> { { "Street", "3486 Orange St" }, { "Zip", "92501" } });
            batchaddresses.Add(new Dictionary<string, string> { { "Street", "2999 4th St" }, { "Zip", "92507" } });
            batchaddresses.Add(new Dictionary<string, string> { { "Street", "3685 10th St" }, { "Zip", "92501" } });

            AddressListbox.ItemsSource = batchaddresses;
        }

        private void BatchGeocodeButton_Click(object sender, RoutedEventArgs e)
        {
            if (batchaddresses.Count > 0)
            {
                _locatorTask.AddressesToLocationsAsync(batchaddresses.ToList(), MyMap.SpatialReference);
            }
        }

        void _locatorTask_AddressesToLocationsCompleted(object sender, AddressesToLocationsEventArgs e)
        {
            if (e.Result != null && e.Result.AddressCandidates != null && e.Result.AddressCandidates.Count > 0)
            {
                geocodedResults.Graphics.Clear();
                foreach (AddressCandidate location in e.Result.AddressCandidates)
                {
                    Graphic graphic = new Graphic();
 
                    if (!string.IsNullOrEmpty(location.Address))
                    {
                        graphic.Geometry = location.Location;
                        graphic.Attributes.Add("X", location.Attributes["X"]);
                        graphic.Attributes.Add("Y", location.Attributes["Y"]);
                        graphic.Attributes.Add("Match_addr", location.Attributes["Match_addr"]);
                        graphic.Attributes.Add("Score", location.Attributes["Score"]);
                    }
                    else
                    {
                        graphic.Attributes.Add("Match_addr", "NO MATCH");
                        graphic.Attributes.Add("Score", location.Attributes["Score"]);
                    }
                    geocodedResults.Graphics.Add(graphic);
                }
            }
        }

        private void LocatorTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Locator service failed: " + e.Error);
        }

        private void addtolist_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(StreetTextBox.Text) || string.IsNullOrEmpty(ZipTextBox.Text))
            {
                MessageBox.Show("Value is required for all inputs");
                return;
            }

            int number;
            bool result = Int32.TryParse(ZipTextBox.Text, out number);

            if (!result)
            {
                MessageBox.Show("Incorrect Zip format");
                return;
            }
            batchaddresses.Add(new Dictionary<string, string> { { "Street", StreetTextBox.Text }, { "Zip", ZipTextBox.Text } });
        }

        private void ResetList_Click(object sender, RoutedEventArgs e)
        {
            batchaddresses.Clear();
            geocodedResults.Graphics.Clear();
        }

        private void GraphicsLayer_MouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs e)
        {
            e.Graphic.Selected = !e.Graphic.Selected;
        }
    }
}
