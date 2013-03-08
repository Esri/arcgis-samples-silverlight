using System.Json;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Bing;

namespace ArcGISSilverlightSDK
{
    public partial class BingImagery : UserControl
    {
        public BingImagery()
        {
            InitializeComponent();
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            ESRI.ArcGIS.Client.Bing.TileLayer tileLayer = MyMap.Layers["BingLayer"] as TileLayer;
            string layerTypeTag = (string)((RadioButton)sender).Tag;
            TileLayer.LayerType newLayerType = (TileLayer.LayerType)System.Enum.Parse(typeof(TileLayer.LayerType), layerTypeTag, true);
            tileLayer.LayerStyle = newLayerType;
        }

        private void BingKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text.Length >= 64)
                LoadMapButton.IsEnabled = true;
            else
                LoadMapButton.IsEnabled = false;
        }

        private void LoadMapButton_Click(object sender, RoutedEventArgs e)
        {
            WebClient webClient = new WebClient();
            string uri = string.Format("http://dev.virtualearth.net/REST/v1/Imagery/Metadata/Aerial?supressStatus=true&key={0}", BingKeyTextBox.Text);

            webClient.OpenReadCompleted += (s, a) =>
            {
                if (a.Error == null)
                {
                    JsonValue jsonResponse = JsonObject.Load(a.Result);
                    string authenticationResult = jsonResponse["authenticationResultCode"];
                    a.Result.Close();

                    if (authenticationResult == "ValidCredentials")
                    {
                        ESRI.ArcGIS.Client.Bing.TileLayer tileLayer = new TileLayer()
                        {
                            ID = "BingLayer",
                            LayerStyle = TileLayer.LayerType.Road,
                            ServerType = ServerType.Production,
                            Token = BingKeyTextBox.Text
                        };

                        MyMap.Layers.Add(tileLayer);

                        BingKeyGrid.Visibility = System.Windows.Visibility.Collapsed;
                        LayerStyleGrid.Visibility = System.Windows.Visibility.Visible;

                        InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Collapsed;

                    } else InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Visible;                    
                } else InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Visible;                
            };

            webClient.OpenReadAsync(new System.Uri(uri));
        }       
    }
}
