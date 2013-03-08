using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Portal;
using ESRI.ArcGIS.Client.WebMap;

namespace ArcGISSilverlightSDK
{
    public partial class BasemapSwitcher : UserControl
    {
        ESRI.ArcGIS.Client.Map _map;

        public BasemapSwitcher()
        {
            InitializeComponent();
            LoadWebMap();
            LoadPortalBasemaps();
        }
        private void LoadWebMap()
        {
            // Downloads a web map from ArcGIS Online that contains a basemap and operational layer (federal land holdings)
            Document doc = new Document();
            doc.GetMapCompleted += (s, e) =>
            {
                _map = e.Map;
                Grid.SetColumn(_map, 1);
                LayoutRoot.Children.Insert(0, _map);
            };
            doc.GetMapAsync("3679c136c2694d0b95bb5e6c3f2b480e");
        }

        private void LoadPortalBasemaps()
        {
            // Initialize a portal to get the query to return web maps in the basemap gallery
            ArcGISPortal portal = new ArcGISPortal();
            portal.InitializeAsync("http://www.arcgis.com/sharing/rest", (s, e) =>
            {
                if (portal.ArcGISPortalInfo != null)
                {
                    // Return the first 20 basemaps in the gallery
                    SearchParameters parameters = new SearchParameters() { Limit = 20 };
                    // Use the advertised query to search the basemap gallery and return web maps as portal items
                    portal.ArcGISPortalInfo.SearchBasemapGalleryAsync(parameters, (result, error) =>
                    {
                        if (error == null && result != null)
                        {
                            basemapList.ItemsSource = result.Results;
                        }
                    });
                }
            });
        }

        private void BaseMapButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as ArcGISPortalItem;
            ESRI.ArcGIS.Client.WebMap.WebMap.FromPortalItemAsync(item, (webmap, err) =>
            {
                var basemaps = webmap.BaseMap;
                //Create a map instance from the webmap
                Document doc = new Document()
                {
                    BingToken = ""//Required if you want to load bing basemaps
                };
                doc.GetMapCompleted += (s, result) =>
                {
                    // Remove basemap layers in current map control
                    foreach (var layer in _map.Layers.ToList())
                        if (Document.GetIsBaseMap(layer))
                            _map.Layers.Remove(layer);

                    // Get the basemap layers from the result map control from the call to GetMapAsync
                    var newBaseLayers = result.Map.Layers.Where(layer => Document.GetIsBaseMap(layer)).ToList();

                    // To add the basemap layer retrieved in the previous line to the current map control, remove them from 
                    // the result map control
                    result.Map.Layers.Clear();

                    // Use an index to determine where to insert basemap reference layers
                    int idx = 0;
                    foreach (var layer in newBaseLayers)
                    {
                        // If basemap contains a Bing tile layer and no Bing key is available, skip adding the basemap layer.
                        if (layer is ESRI.ArcGIS.Client.Bing.TileLayer && string.IsNullOrEmpty(doc.BingToken))
                        {
                            MessageBox.Show("Bing key not available. Bing layer cannot be used as a basemap.");
                            break;
                        }

                        // Returns json definition for the layer
                        var data = layer.GetValue(Document.WebMapDataProperty) as IDictionary<string, object>;

                        // Reference layers go on top of other layers (e.g. labels)
                        if (data.ContainsKey("isReference"))
                            _map.Layers.Add(layer);
                        else
                        {
                            // Basemap layers go below all other layers
                            _map.Layers.Insert(idx++, layer);
                        }
                    }
                };
                doc.GetMapAsync(webmap);
            });
        }
    }
}

