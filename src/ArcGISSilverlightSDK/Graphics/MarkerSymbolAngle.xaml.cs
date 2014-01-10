using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Symbols;

namespace ArcGISSilverlightSDK
{
    public partial class MarkerSymbolAngle : UserControl
    {
        public MarkerSymbolAngle()
        {
            InitializeComponent();            
        }  

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {   
            string angleTag = (string)((RadioButton)sender).Tag;
            (LayoutRoot.Resources["MyAngleSymbol"] as MarkerSymbol).AngleAlignment = 
                (MarkerSymbol.MarkerAngleAlignment)System.Enum.Parse(typeof(MarkerSymbol.MarkerAngleAlignment), angleTag, true); 
        }
    }
}
