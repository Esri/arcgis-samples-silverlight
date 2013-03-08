using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.WebMap;
using System.Windows;

namespace ArcGISSilverlightSDK
{
    public partial class LoadWebMapDynamically : UserControl
    {
        public LoadWebMapDynamically()
        {
            InitializeComponent();
        }

        private void LoadWebMapButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(WebMapTextBox.Text))
            {
                Document webMap = new Document();
                webMap.GetMapCompleted += (s, a) =>
                    {
                        if (a.Error != null)
                            MessageBox.Show(string.Format("Unable to load webmap. {0}", a.Error.Message));
                        else
                        {
                            MyMap.Extent = a.Map.Extent;

                            LayerCollection layerCollection = new LayerCollection();
                            foreach (Layer layer in a.Map.Layers)
                                layerCollection.Add(layer);

                            a.Map.Layers.Clear();
                            MyMap.Layers = layerCollection;
                            WebMapPropertiesTextBox.DataContext = a.ItemInfo;
                        }
                    };

                webMap.GetMapAsync(WebMapTextBox.Text);
            }
        }
    }
}
