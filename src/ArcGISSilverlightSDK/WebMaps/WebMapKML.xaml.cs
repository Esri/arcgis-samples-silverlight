using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit.DataSources;
using ESRI.ArcGIS.Client.WebMap;

namespace ArcGISSilverlightSDK
{
    public partial class WebMapKML : UserControl
    {
        public WebMapKML()
        {
            InitializeComponent();

            Document webMap = new Document();
            webMap.GetMapCompleted += webMap_GetMapCompleted;
            webMap.ProxyUrl = "http://servicesbeta3.esri.com/SilverlightDemos/ProxyPage/proxy.ashx";
            webMap.GetMapAsync("d2cb7cac8b1947c7b57ed8edd6b045bb");
        }

        void webMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                foreach (Layer layer in e.Map.Layers)
                    if (layer is KmlLayer)
                        (layer as KmlLayer).Initialized += kmllayer_Initialized;

                e.Map.WrapAround = true;
                LayoutRoot.Children.Add(e.Map);
            }
        }

        void kmllayer_Initialized(object sender, System.EventArgs e)
        {
            foreach (Layer layer in (sender as KmlLayer).ChildLayers)
            {
                layer.Visible = true;

                Border border = new Border()
                {
                    Background = new SolidColorBrush(Colors.White),
                    BorderBrush = new SolidColorBrush(Colors.Black),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5)
                };

                StackPanel stackPanelChild = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(5)
                };

                TextBlock textValue = new TextBlock();
                Binding valueBinding = new Binding(string.Format("[{0}]", "name"));
                textValue.SetBinding(TextBlock.TextProperty, valueBinding);

                stackPanelChild.Children.Add(textValue);

                border.Child = stackPanelChild;
                (layer as KmlLayer).MapTip = border;
            }
        }
    }
}
