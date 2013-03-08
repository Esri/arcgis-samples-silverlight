using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.WebMap;

namespace ArcGISSilverlightSDK
{
    public partial class WebMapNotesPopups : UserControl
    {
        public WebMapNotesPopups()
        {
            InitializeComponent();

            Document webMap = new Document();
            webMap.GetMapCompleted += webMap_GetMapCompleted;

            webMap.GetMapAsync("2ccf901c5b414e5c98a346edb75e3c13");
        }

        void webMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                foreach (Layer layer in e.Map.Layers)
                {
                    if (layer is GraphicsLayer)
                    {
                        GraphicsLayer glayer = layer as GraphicsLayer;
                        // Modification of the default map tip style
                        Border border = glayer.MapTip as Border;
                        if (border != null)
                        {
                            border.Background = new SolidColorBrush(Color.FromArgb(200, 102, 150, 255));
                            border.CornerRadius = new CornerRadius(4);
                        }
                    }
                }
                LayoutRoot.Children.Add(e.Map);
            }
        }
    }
}