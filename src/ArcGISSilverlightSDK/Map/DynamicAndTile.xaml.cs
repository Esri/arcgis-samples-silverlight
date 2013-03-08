using System.Windows.Controls;
using System.Windows;
using ESRI.ArcGIS.Client;

namespace ArcGISSilverlightSDK
{
    public partial class DynamicAndTile : UserControl
    {
        public DynamicAndTile()
        {
            InitializeComponent();
        }

        private void Layer_InitializationFailed(object sender, System.EventArgs e)
        {
            Layer layer = sender as Layer;
            if (layer.InitializationFailure != null)
            {
                MessageBox.Show(layer.ID + ":" + layer.InitializationFailure.ToString());
            }
        }
    }
}
