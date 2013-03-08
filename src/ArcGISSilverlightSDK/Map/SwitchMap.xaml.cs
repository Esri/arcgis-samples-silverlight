using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;

namespace ArcGISSilverlightSDK
{
    public partial class SwitchMap : UserControl
    {
        public SwitchMap()
        {
            InitializeComponent();
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            ArcGISTiledMapServiceLayer arcgisLayer = MyMap.Layers["AGOLayer"] as ArcGISTiledMapServiceLayer;
            arcgisLayer.Url = ((RadioButton)sender).Tag as string;
        }

        private void MyMap_Loaded(object sender, RoutedEventArgs e)
        {
            MyMap.Focus();
        }
    }
}
