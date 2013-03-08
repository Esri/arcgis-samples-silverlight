using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;

namespace ArcGISSilverlightSDK
{
    public partial class IdentityManagerServices : UserControl
    {
        Dictionary<string, int> challengeAttemptsPerUrl = new Dictionary<string, int>();

        public IdentityManagerServices()
        {
            InitializeComponent();
            MyMap.Layers.LayersInitialized += Layers_LayersInitialized;

            IdentityManager.Current.ChallengeMethod = Challenge;
        }

        private void Challenge(string url,
            Action<IdentityManager.Credential, Exception> callback, IdentityManager.GenerateTokenOptions options)
        {
            LoginGrid.Visibility = System.Windows.Visibility.Visible;

            TitleTextBlock.Text = string.Format("Login to access: \n{0}", url);

            if (!challengeAttemptsPerUrl.ContainsKey(url))
                challengeAttemptsPerUrl.Add(url, 0);

            RoutedEventHandler handleClick = null;
            handleClick = (s, e) =>
            {
                IdentityManager.Current.GenerateCredentialAsync(url, UserTextBox.Text, PasswordTextBox.Text,
                (credential, ex) =>
                {
                    challengeAttemptsPerUrl[url]++;
                    if (ex == null || challengeAttemptsPerUrl[url] == 3)
                    {
                        LoginLoadLayerButton.Click -= handleClick;
                        callback(credential, ex);
                    }

                }, options);
            };
            LoginLoadLayerButton.Click += handleClick;

            System.Windows.Input.KeyEventHandler handleEnterKeyDown = null;
            handleEnterKeyDown = (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    PasswordTextBox.KeyDown -= handleEnterKeyDown;
                    handleClick(null, null);
                }
            };
            PasswordTextBox.KeyDown += handleEnterKeyDown;
        }

        void Layers_LayersInitialized(object sender, EventArgs args)
        {
            LoginGrid.Visibility = System.Windows.Visibility.Collapsed;
            ShadowGrid.Visibility = System.Windows.Visibility.Collapsed;
            LegendBorder.Visibility = System.Windows.Visibility.Visible;
        }

        private void Layer_InitializationFailed(object sender, EventArgs e) {}
           
    }
}
