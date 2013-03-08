using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.WebMap;
using System.Json;
using System.Net;

namespace ArcGISSilverlightSDK
{
    public partial class LoadWebMapWithBing : UserControl
    {
        public LoadWebMapWithBing()
        {
            InitializeComponent();          
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

                    BingKeyGrid.Visibility = System.Windows.Visibility.Collapsed;
                    InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Collapsed;

                    if (authenticationResult == "ValidCredentials")
                    {
                        Document webMap = new Document();
                        webMap.BingToken = BingKeyTextBox.Text;
                        webMap.GetMapCompleted += (s1, e1) => 
                        {
                            if (e1.Error == null)
                                LayoutRoot.Children.Add(e1.Map);
                        };

                        webMap.GetMapAsync("75e222ec54b244a5b73aeef40ce76adc");
                    }
                    else InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Visible;
                }
                else InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Visible;
            };

            webClient.OpenReadAsync(new System.Uri(uri));
        }       
    }
}