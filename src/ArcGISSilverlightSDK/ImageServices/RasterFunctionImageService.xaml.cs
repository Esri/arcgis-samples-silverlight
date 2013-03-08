using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;

namespace ArcGISSilverlightSDK
{
    public partial class RasterFunctionImageService : UserControl
    {
        public RasterFunctionImageService()
        {
            InitializeComponent();
        }        	
      
        private void ArcGISImageServiceLayer_Initialized(object sender, EventArgs e)
        {
            RasterFunctionsComboBox.ItemsSource =
                (sender as ArcGISImageServiceLayer).RasterFunctionInfos;
            RasterFunctionsComboBox.SelectedIndex = 0;
        }

        private void RasterFunctionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ArcGISImageServiceLayer imageLayer = MyMap.Layers["MyImageLayer"] as ArcGISImageServiceLayer;
            var rasterFunction = (sender as ComboBox).SelectedItem as RasterFunctionInfo;
            if (rasterFunction != null)
            {
                RenderingRule renderingRule = new RenderingRule() { RasterFunctionName = rasterFunction.Name };
                imageLayer.RenderingRule = renderingRule;
                imageLayer.Refresh();
            }
        }
    }
}
