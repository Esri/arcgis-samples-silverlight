using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit;

namespace ArcGISSilverlightSDK
{
    public partial class InfoWindowSimple : UserControl
    {
        public InfoWindowSimple()
        {
            InitializeComponent();
        }

        private void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {
            FeatureLayer featureLayer = MyMap.Layers["MyFeatureLayer"] as FeatureLayer;
            System.Windows.Point screenPnt = MyMap.MapToScreen(e.MapPoint);

            // Account for difference between Map and application origin
            GeneralTransform generalTransform = MyMap.TransformToVisual(Application.Current.RootVisual);
            System.Windows.Point transformScreenPnt = generalTransform.Transform(screenPnt);

            IEnumerable<Graphic> selected =
                featureLayer.FindGraphicsInHostCoordinates(transformScreenPnt);

            foreach (Graphic g in selected)
            {

                MyInfoWindow.Anchor = e.MapPoint;
                MyInfoWindow.IsOpen = true;
                //Since a ContentTemplate is defined, Content will define the DataContext for the ContentTemplate
                MyInfoWindow.Content = g.Attributes;
                return;
            }

            InfoWindow window = new InfoWindow()
            {
                Anchor = e.MapPoint,
                Map = MyMap,
                IsOpen = true,
                Placement=InfoWindow.PlacementMode.Auto,
                ContentTemplate = LayoutRoot.Resources["LocationInfoWindowTemplate"] as System.Windows.DataTemplate,
                //Since a ContentTemplate is defined, Content will define the DataContext for the ContentTemplate
                Content = e.MapPoint 
            };
            LayoutRoot.Children.Add(window);
        }
        
        private void MyInfoWindow_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MyInfoWindow.IsOpen = false;
        }
    }
}
