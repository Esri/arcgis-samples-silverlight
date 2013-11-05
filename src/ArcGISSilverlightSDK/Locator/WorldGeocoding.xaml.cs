using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class WorldGeocoding : UserControl
    {
        Locator _locatorTask;
        GraphicsLayer SearchOriginGraphicsLayer;
        GraphicsLayer FindResultLocationsGraphicsLayer;        

        public WorldGeocoding()
        {
            InitializeComponent();

            // Initialize Locator with ArcGIS Online World Geocoding Service.  See http://geocode.arcgis.com for details and doc.
            _locatorTask = new Locator("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");
            _locatorTask.FindCompleted += (s, a) =>
            {
                // When a Find operation is initiated, get the find results and add to a graphics layer for display in the map
                LocatorFindResult locatorFindResult = a.Result;
                foreach (Location location in locatorFindResult.Locations)
                {
                    FindResultLocationsGraphicsLayer.Graphics.Add(location.Graphic);
                }
            };
            _locatorTask.Failed += (s, e) =>
            {
                MessageBox.Show("Locator service failed: " + e.Error);
            };

            FindResultLocationsGraphicsLayer = MyMap.Layers["FindResultLocationsGraphicsLayer"] as GraphicsLayer;
            SearchOriginGraphicsLayer = MyMap.Layers["SearchOriginGraphicsLayer"] as GraphicsLayer; ;
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            FindResultLocationsGraphicsLayer.Graphics.Clear();
            SearchOriginGraphicsLayer.Graphics.Clear();

            // If locator already processing a request, cancel it.  Note, the request is not cancelled on the server.   
            if (_locatorTask.IsBusy)
                _locatorTask.CancelAsync();

            // If search text is empty, return
            if (string.IsNullOrEmpty(SearchTextBox.Text))
                return;

            // Search will return results based on a start location.  In this sample, the location is the center of the map.  
            // Add a graphic when a search is initiated to determine what location was used to search from and rank results.
            SearchOriginGraphicsLayer.Graphics.Add(new Graphic()
            {
                Geometry = MyMap.Extent.GetCenter()
            });

            // In this sample, the center of the map is used as the location from which results will be ranked and distance calculated. 
            // The distance from the location is optional.  Specifies the radius of an area around a point location which is used to boost
            // the rank of geocoding candidates so that candidates closest to the location are returned first. The distance value is in meters. 
            LocatorFindParameters locatorFindParams = new LocatorFindParameters()
            {
                Text = SearchTextBox.Text,
                Location = MyMap.Extent.GetCenter(),
                Distance = MyMap.Extent.Width / 2,
                MaxLocations = 5,
                OutSpatialReference = MyMap.SpatialReference
            };
            locatorFindParams.OutFields.AddRange(new string[]
            { "PlaceName", "City", "Region", "Country", "Score", "Distance", "Type"});
            
            _locatorTask.FindAsync(locatorFindParams);
        }

        private void GraphicsLayer_MouseEnter(object sender, GraphicMouseEventArgs e)
        {
            e.Graphic.Select();
        }

        private void GraphicsLayer_MouseLeave(object sender, GraphicMouseEventArgs e)
        {
            e.Graphic.UnSelect();
        }
    }
}








