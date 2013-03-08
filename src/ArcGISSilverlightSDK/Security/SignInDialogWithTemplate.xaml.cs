using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;

namespace ArcGISSilverlightSDK
{
    public partial class SignInDialogWithTemplate : UserControl
    {
        public SignInDialogWithTemplate()
        {
            InitializeComponent();

            IdentityManager.Current.ChallengeMethod = Challenge;

            Unloaded += new RoutedEventHandler(SignInDialogWithTemplate_Unloaded);
        }

        void SignInDialogWithTemplate_Unloaded(object sender, RoutedEventArgs e)
        {
            MySignInDialog.Visibility = System.Windows.Visibility.Collapsed;
            MySignInDialog.IsActive = false;            
        }

        private void Challenge(string url,
          Action<IdentityManager.Credential, Exception> callback, IdentityManager.GenerateTokenOptions options)
        {
            // Option 1: Access to url, callback, and options passed to SignInDialog
            //SignInDialog.DoSignIn(url, callback, options);

            // Option 2: Use Popup to contain SignInDialog
            //var popup = new Popup
            //{
            //    HorizontalOffset = 200,
            //    VerticalOffset = 200
            //};

            //SignInDialog signInDialog = new SignInDialog()
            //{
            //    Width = 300,
            //    Url = url,
            //    IsActive = true,
            //    Callback = (credential, ex) =>
            // {
            //     callback(credential, ex);
            //     popup.IsOpen = false;
            // }
            //};
            //popup.Child = signInDialog;
            //popup.IsOpen = true;

            // Option 3: Use a template to define SignInDialog content             
            MySignInDialog.Url = url;
            MySignInDialog.IsActive = true;
            MySignInDialog.Visibility = System.Windows.Visibility.Visible;

            MySignInDialog.Callback = (credential, ex) =>
            {
                callback(credential, ex);
                MySignInDialog.Visibility = System.Windows.Visibility.Collapsed;
                MySignInDialog.IsActive = false;
            };
        }

        private void Layer_InitializationFailed(object sender, System.EventArgs e) { }

        private void Layer_Initialized(object sender, System.EventArgs e)
        {
            MyMap.Extent = (sender as Layer).FullExtent;
        }
    }
}
