using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Toolkit;

namespace ArcGISSilverlightSDK
{
    public partial class EditorTracking : UserControl
    {
        public EditorTracking()
        {
            InitializeComponent();
            IdentityManager.Current.ChallengeMethod = Challenge;
        }

        private void Challenge(string url,
            Action<IdentityManager.Credential, Exception> callback, IdentityManager.GenerateTokenOptions options)
        {
            SignInDialog.DoSignIn(url, (credential, error) =>
            {
                if (error == null)
                {
                    ToolBorder.Visibility = System.Windows.Visibility.Visible;
                    LoggedInGrid.Visibility = System.Windows.Visibility.Visible;
                    LoggedInUserTextBlock.Text = credential.UserName;
                }
                callback(credential, error);
            }
            , options);
        }       

        private void FeatureLayer_InitializationFailed(object sender, EventArgs e)
        {}

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            SignOut();
        }

        private void SignOut()
        {
            var featureLayer = MyMap.Layers["WildfireLayer"] as FeatureLayer;
            var credential = IdentityManager.Current.FindCredential(featureLayer.Url, LoggedInUserTextBlock.Text);
            if (credential == null) return;
            ToolBorder.Visibility = System.Windows.Visibility.Collapsed;
            LoggedInGrid.Visibility = System.Windows.Visibility.Collapsed;
            IdentityManager.Current.RemoveCredential(credential);
            MyMap.Layers.Remove(featureLayer);
            featureLayer = new FeatureLayer()
            {
                ID = "WildfireLayer",
                DisplayName = "Wildfire Layer",
                Url = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Wildfire_secure/FeatureServer/0",
                Mode = FeatureLayer.QueryMode.OnDemand
            };
            featureLayer.OutFields.Add("*");
            featureLayer.MouseLeftButtonDown += FeatureLayer_MouseLeftButtonDown;
            featureLayer.InitializationFailed += FeatureLayer_InitializationFailed;
            MyMap.Layers.Add(featureLayer);
        }

        private void FeatureLayer_MouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs e)
        {

            if (e.Graphic != null && !e.Graphic.Selected && (sender as FeatureLayer).IsUpdateAllowed(e.Graphic))
            {
                Editor editor = LayoutRoot.Resources["MyEditor"] as Editor;
                if ((sender as FeatureLayer).IsUpdateAllowed(e.Graphic))
                {
                    if (editor.EditVertices.CanExecute(null))
                        editor.EditVertices.Execute(null);
                }
                else
                    if (editor.CancelActive.CanExecute(null))
                        editor.CancelActive.Execute(null);
            }
            (sender as FeatureLayer).ClearSelection();
            e.Graphic.Select();
            MyDataGrid.ScrollIntoView(e.Graphic, null);
        }
    }
}