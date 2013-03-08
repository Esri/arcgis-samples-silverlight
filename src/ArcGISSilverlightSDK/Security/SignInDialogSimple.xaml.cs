using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit;

namespace ArcGISSilverlightSDK
{
    public partial class SignInDialogSimple : UserControl
    {
        public SignInDialogSimple()
        {
            InitializeComponent();

            IdentityManager.Current.ChallengeMethod = SignInDialog.DoSignIn;
        }

        private void Layer_InitializationFailed(object sender, System.EventArgs e) { }

        private void Layer_Initialized(object sender, System.EventArgs e)
        {
            MyMap.Extent = (sender as Layer).FullExtent;
        }
    }
}
