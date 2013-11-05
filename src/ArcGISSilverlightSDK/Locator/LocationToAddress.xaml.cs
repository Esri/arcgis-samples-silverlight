using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISSilverlightSDK
{
    public partial class LocationToAddress : UserControl
    {
        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
                new ESRI.ArcGIS.Client.Projection.WebMercator();
        GraphicsLayer _locationGraphicsLayer;

        public LocationToAddress()
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

            _locationGraphicsLayer = MyMap.Layers["LocationGraphicsLayer"] as GraphicsLayer;
        }

        private void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {
            Locator locatorTask = new Locator("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");
            locatorTask.LocationToAddressCompleted += LocatorTask_LocationToAddressCompleted;
            locatorTask.Failed += LocatorTask_Failed;

            // Tolerance (distance) specified in meters
            double tolerance = 30;
            locatorTask.LocationToAddressAsync(e.MapPoint, tolerance, e.MapPoint);
        }

        private void LocatorTask_LocationToAddressCompleted(object sender, AddressEventArgs args)
        {
            Address address = args.Address;
            Dictionary<string, object> attributes = address.Attributes;

            Graphic graphic = new Graphic()
            {
                Geometry = args.UserState as MapPoint                
            };

            // Parameter names of address candidates may differ between geocode services
            string latlon = String.Format("{0}, {1}", address.Location.X, address.Location.Y);
            string address1 = attributes["Address"].ToString();
            string address2 = String.Format("{0}, {1} {2}", attributes["City"], attributes["Region"], attributes["Postal"]);

            graphic.Attributes.Add("LatLon", latlon);
            graphic.Attributes.Add("Address1", address1);
            graphic.Attributes.Add("Address2", address2);

            _locationGraphicsLayer.Graphics.Add(graphic);
        }

        private void LocatorTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Unable to find an address. Try selecting a location closer to a street.");
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
