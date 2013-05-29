using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Portal;
using System.Linq;

namespace ArcGISSilverlightSDK
{
    public partial class PortalSpatialSearch : UserControl
    {
        ArcGISPortal arcgisPortal;
        ESRI.ArcGIS.Client.Projection.WebMercator mercator =
           new ESRI.ArcGIS.Client.Projection.WebMercator();

        GraphicsLayer webmapGraphicsLayer;

        public PortalSpatialSearch()
        {
            InitializeComponent();

            // Search public web maps on www.arcgis.com
            arcgisPortal = new ArcGISPortal() { Url = "http://www.arcgis.com/sharing/rest" };
            webmapGraphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
        }

        private void FindWebMapsButton_Click(object sender, RoutedEventArgs e)
        {
            webmapGraphicsLayer.Graphics.Clear();

            // Search envelope must be in geographic (WGS84).  Convert the current map extent from Web Mercator
            // to geographic. 
            ESRI.ArcGIS.Client.Geometry.Geometry geom = mercator.ToGeographic(MyMap.Extent);

            SpatialSearchParameters parameters = new SpatialSearchParameters
            {
                Limit = String.IsNullOrEmpty(resultLimit.Text) == true ? 15 : Convert.ToInt32(resultLimit.Text),
                SearchExtent = geom.Extent,
                QueryString = String.Format("{0} And type:Web Map", searchText.Text)
            };

            arcgisPortal.SearchItemsAsync(parameters, (result, error) =>
            {
                if (error == null)
                {
                    // Set the ItemsSource for the Listbox to an IEnumerable of ArcGISPortalItems.  
                    // Bindings setup in the Listbox item template in XAML will enable the discovery and 
                    // display individual result items.  
                    WebMapsListBox.ItemsSource = result.Results;

                    // For each web map returned, add the center (point) of the web map extent as a graphic.
                    // Add the ArcGISPortalItem instance as an attribute.  This will be used to select graphics
                    // in the map.  
                    foreach (var item in result.Results)
                    {
                        Graphic graphic = new Graphic();
                        graphic.Attributes.Add("PortalItem", item);
                        MapPoint extentCenter = item.Extent.GetCenter();
                        graphic.Geometry = new MapPoint(extentCenter.X, extentCenter.Y, new SpatialReference(4326));
                        webmapGraphicsLayer.Graphics.Add(graphic);
                    }
                }
            });
        }

        // Click on a graphic in the map and select it.  Scroll to the web map item in the Listbox. 
        private void GraphicsLayer_MouseLeftButtonUp(object sender, GraphicMouseButtonEventArgs e)
        {
            webmapGraphicsLayer.ClearSelection();
            Graphic graphic = e.Graphic;
            graphic.Selected = true;
            ArcGISPortalItem portalitem = graphic.Attributes["PortalItem"] as ArcGISPortalItem;
            WebMapsListBox.SelectedItem = portalitem;
            WebMapsListBox.ScrollIntoView(portalitem);
        }

        // Click on a web map item in the Listbox, zoom to and select the respective graphic in the map. 
        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            webmapGraphicsLayer.ClearSelection();
            ArcGISPortalItem portalItem = (sender as Image).DataContext as ArcGISPortalItem;
            if (portalItem.Extent != null)
            {
                MyMap.ZoomTo(mercator.FromGeographic(portalItem.Extent));
            }

            // Use LINQ to select the graphic where the portal item attribute equals the portal item instance
            // in the Listbox.
            var queryPortalItemGraphics = from g in webmapGraphicsLayer.Graphics
                                          where g.Attributes["PortalItem"] == portalItem
                                          select g;

            // Get the first graphic in the IEnumerable of graphics and set it selected.
            foreach (var graphic in queryPortalItemGraphics)
            {
                graphic.Selected = true;
                return;
            }
        }

        // Listen for when the FullExtent property changes for the graphics layer that stores web map extent center points.
        // When FullExtent changes (e.g. new search results are returned) zoom to an extent that contains all the results.  
        private void GraphicsLayer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == "FullExtent") && ((sender as GraphicsLayer).FullExtent != null))
                MyMap.ZoomTo((sender as GraphicsLayer).FullExtent);
        }
    }
}
