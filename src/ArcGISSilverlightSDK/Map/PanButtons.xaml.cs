using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISSilverlightSDK
{
    public partial class PanButtons : UserControl
    {
        public PanButtons()
        {
            InitializeComponent();
        }

        private void PanClick(object sender, RoutedEventArgs e)
        {
            Envelope extent = MyMap.Extent;
            if (extent == null) return;
            MapPoint center = extent.GetCenter();

            switch ((sender as Button).Tag.ToString())
            {
                case "W":
                    MyMap.PanTo(new MapPoint(extent.XMin, center.Y)); break;
                case "E":
                    MyMap.PanTo(new MapPoint(extent.XMax, center.Y)); break;               
                case "N":
                    MyMap.PanTo(new MapPoint(center.X, extent.YMax)); break;
                case "S":
                    MyMap.PanTo(new MapPoint(center.X, extent.YMin)); break;                
                case "NE":
                    MyMap.PanTo(new MapPoint(extent.XMax, extent.YMax)); break;
                case "SE":
                    MyMap.PanTo(new MapPoint(extent.XMax, extent.YMin)); break;
                case "SW":
                    MyMap.PanTo(new MapPoint(extent.XMin, extent.YMin)); break;
                case "NW":
                    MyMap.PanTo(new MapPoint(extent.XMin, extent.YMax)); break;                
                default: break;
            }
        }
    }
}
