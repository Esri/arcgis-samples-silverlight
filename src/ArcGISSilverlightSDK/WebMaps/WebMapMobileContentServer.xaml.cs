using System.Windows.Controls;
using ESRI.ArcGIS.Client.WebMap;

namespace ArcGISSilverlightSDK
{
    public partial class WebMapMobileContentServer : UserControl
    {
        public WebMapMobileContentServer()
        {
            InitializeComponent();
            Document webMap = new Document();
            webMap.GetMapCompleted += webMap_GetMapCompleted;
            webMap.ServerBaseUrl = "http://arcgismobile.esri.com/arcgis/mobile/content";

            webMap.GetMapAsync("00ab0becb052428485a8d25e62afb86d");
        }

        void webMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error == null)                
                LayoutRoot.Children.Add(e.Map);           
        }
    }
}
