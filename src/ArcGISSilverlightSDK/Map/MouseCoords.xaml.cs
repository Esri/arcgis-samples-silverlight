using System;
using System.Windows.Controls;

namespace ArcGISSilverlightSDK
{
    public partial class MouseCoords : UserControl
    {
        public MouseCoords()
        {
            InitializeComponent();
        }

        private void MyMap_MouseMove(object sender, System.Windows.Input.MouseEventArgs args)
        {
            if (MyMap.Extent != null)
            {
                System.Windows.Point screenPoint = args.GetPosition(MyMap);
                ScreenCoordsTextBlock.Text = string.Format("Screen Coords: X = {0}, Y = {1}", 
                    screenPoint.X, screenPoint.Y);

                ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint = MyMap.ScreenToMap(screenPoint);
                if (MyMap.WrapAroundIsActive)
                    mapPoint = ESRI.ArcGIS.Client.Geometry.Geometry.NormalizeCentralMeridian(mapPoint) as ESRI.ArcGIS.Client.Geometry.MapPoint;
                MapCoordsTextBlock.Text = string.Format("Map Coords: X = {0}, Y = {1}",
                    Math.Round(mapPoint.X, 4), Math.Round(mapPoint.Y, 4));
            }
        }
    }
}