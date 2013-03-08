using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Toolkit.DataSources;

namespace ArcGISSilverlightSDK
{
    public partial class OpenStreetMapSimple : UserControl
    {
        public OpenStreetMapSimple()
        {
            InitializeComponent();
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            OpenStreetMapLayer osmLayer = MyMap.Layers["OSMLayer"] as OpenStreetMapLayer;
            string layerTypeTag = (string)((RadioButton)sender).Tag;
            OpenStreetMapLayer.MapStyle newLayerType = (OpenStreetMapLayer.MapStyle)System.Enum.Parse(typeof(OpenStreetMapLayer.MapStyle), 
                layerTypeTag, true);
            osmLayer.Style = newLayerType;
        }
    }
}
