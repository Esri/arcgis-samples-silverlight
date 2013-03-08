using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit;

namespace ArcGISSilverlightSDK
{
    public partial class OwnershipBasedEditing : UserControl
    {
        public OwnershipBasedEditing()
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
        { }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            SignOut();
        }

        private void SignOut()
        {
            var l = MyMap.Layers["SaveTheBayMarineLayer"] as FeatureLayer;
            var credential = IdentityManager.Current.FindCredential(l.Url, LoggedInUserTextBlock.Text);
            if (credential == null) return;
            ToolBorder.Visibility = System.Windows.Visibility.Collapsed;
            LoggedInGrid.Visibility = System.Windows.Visibility.Collapsed;
            IdentityManager.Current.RemoveCredential(credential);
            MyMap.Layers.Remove(l);
            l = new FeatureLayer()
            {
                ID = "SaveTheBayMarineLayer",
                DisplayName = "Save the Bay - Marine Layer",
                Url = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/SaveTheBay/FeatureServer/0",
                Mode = FeatureLayer.QueryMode.OnDemand
            };
            l.OutFields.Add("*");
            l.MouseLeftButtonDown += FeatureLayer_MouseLeftButtonDown;
            l.InitializationFailed += FeatureLayer_InitializationFailed;
            MyMap.Layers.Add(l);
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