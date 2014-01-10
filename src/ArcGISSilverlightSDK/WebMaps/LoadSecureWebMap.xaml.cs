using System.Windows.Controls;
using ESRI.ArcGIS.Client.WebMap;
using System.Net;
using System;
using System.Json;
using System.Windows;
using ESRI.ArcGIS.Client;

namespace ArcGISSilverlightSDK
{
    public partial class LoadSecureWebMap : UserControl
    {
        public LoadSecureWebMap()
        {
            InitializeComponent();
        }

        private void LoadWebMapButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IdentityManager.Current.GenerateCredentialAsync("https://www.arcgis.com/sharing", 
                UsernameTextBox.Text, PasswordTextBox.Password,
                  (credential, ex) =>
                  {
                      if (ex == null)
                      {
                          Document webMap = new Document();
                          webMap.Token = credential.Token;
                          webMap.GetMapCompleted += webMap_GetMapCompleted;
                          webMap.GetMapAsync(WebMapTextBox.Text);
                      }
                  }, null);
        }

        void webMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error != null)
                MessageBox.Show(string.Format("Unable to load webmap. {0}", e.Error.Message));
            else
                LayoutRoot.Children.Insert(0, e.Map);
        }
    }
}
