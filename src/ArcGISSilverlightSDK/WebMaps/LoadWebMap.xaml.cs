using System;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.WebMap;

namespace ArcGISSilverlightSDK
{
    public partial class LoadWebMap : UserControl
    {
        public LoadWebMap()
        {
            InitializeComponent();

            Document webMap = new Document();
            webMap.GetMapCompleted += webMap_GetMapCompleted;

            webMap.GetMapAsync("00e5e70929e14055ab686df16c842ec1");
        }

        void webMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error == null)
                LayoutRoot.Children.Add(e.Map);
        }     
    }
}
