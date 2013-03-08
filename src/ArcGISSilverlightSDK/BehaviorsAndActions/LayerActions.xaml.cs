using System.Windows.Controls;
using ESRI.ArcGIS.Client.Actions;
using ESRI.ArcGIS.Client;

namespace ArcGISSilverlightSDK
{
    public partial class LayerActions : UserControl
    {
        public LayerActions()
        {
            InitializeComponent();
        }

        private void EventTrigger_PreviewInvoke(object sender, System.Windows.Interactivity.PreviewInvokeEventArgs e)
        {            
            FeatureLayer featureLayer = MyMap.Layers["MyFeatureLayer"] as FeatureLayer;
            featureLayer.Where = featureLayer.Where == null ? "POP2000 > 1000" : null;
        }
    }
}
