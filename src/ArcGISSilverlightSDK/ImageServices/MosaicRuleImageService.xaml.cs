using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using System;
using System.Windows;

namespace ArcGISSilverlightSDK
{
    public partial class MosaicRuleImageService : UserControl
    {
        public MosaicRuleImageService()
        {
            InitializeComponent();
        }

        private void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {
            try
            {
                ArcGISImageServiceLayer imageLayer = MyMap.Layers["ImageServiceLayer"] as ArcGISImageServiceLayer;
                MosaicRule mosaicRule = new MosaicRule();
                mosaicRule.MosaicMethod = "esriMosaicViewpoint";
                mosaicRule.Viewpoint = e.MapPoint;
                imageLayer.MosaicRule = mosaicRule;
                imageLayer.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
