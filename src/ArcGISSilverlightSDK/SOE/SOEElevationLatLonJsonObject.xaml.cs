using System;
using System.Net;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Geometry;
using System.Json;

namespace ArcGISSilverlightSDK
{
    public partial class SOEElevationLatLonJsonObject : UserControl
    {
        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
            new ESRI.ArcGIS.Client.Projection.WebMercator();

        public SOEElevationLatLonJsonObject()
        {
            InitializeComponent();
        }

        private void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {
            MapPoint geographicPoint = _mercator.ToGeographic(e.MapPoint) as MapPoint; 

            string SOEurl = "http://sampleserver4.arcgisonline.com/ArcGIS/rest/services/Elevation/ESRI_Elevation_World/MapServer/exts/ElevationsSOE/ElevationLayers/1/GetElevationAtLonLat";
            SOEurl += string.Format(System.Globalization.CultureInfo.InvariantCulture, "?lon={0}&lat={1}&f=json", geographicPoint.X, geographicPoint.Y);

            WebClient webClient = new WebClient();

            webClient.OpenReadCompleted += (s, a) =>
            {               
                JsonValue jsonResponse = JsonObject.Load(a.Result);
                double elevation = jsonResponse["elevation"];
                a.Result.Close();

                MyInfoWindow.Anchor = e.MapPoint;
                MyInfoWindow.Content = string.Format("Elevation: {0} meters", elevation.ToString("0"));
                MyInfoWindow.IsOpen = true;
            };

            webClient.OpenReadAsync(new Uri(SOEurl));
        }
    }
}
