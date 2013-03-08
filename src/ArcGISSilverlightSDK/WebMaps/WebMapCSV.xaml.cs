using System.Windows.Controls;
using ESRI.ArcGIS.Client.WebMap;

namespace ArcGISSilverlightSDK
{
    public partial class WebMapCSV : UserControl
    {
        public WebMapCSV()
        {
            InitializeComponent();
                        
            Document webMap = new Document();
            webMap.GetMapCompleted += webMap_GetMapCompleted;

            webMap.GetMapAsync("e64c82296b5a48acb0a7f18e3f556607");
        }

        void webMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error == null)
                LayoutRoot.Children.Add(e.Map);
        }  
    }
}
