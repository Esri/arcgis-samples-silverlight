using System.Windows.Controls;
using System.Windows.Input;

namespace ArcGISSilverlightSDK
{
    public partial class Magnify : UserControl
    {
        public Magnify()
        {
            InitializeComponent();
        }

        private void MyMagnifyImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyMagnifier.Enabled = !MyMagnifier.Enabled;
        }
    }
}
