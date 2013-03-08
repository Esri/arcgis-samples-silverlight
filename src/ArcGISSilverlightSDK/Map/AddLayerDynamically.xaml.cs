using System.Windows;
using System.Windows.Controls;

namespace ArcGISSilverlightSDK
{
    public partial class AddLayerDynamically : UserControl
    {
        public AddLayerDynamically()
        {
            InitializeComponent();           
        }

        private void AddLayerButton_Click(object sender, RoutedEventArgs e)
        {
            MyMap.Layers.Clear();

            ESRI.ArcGIS.Client.ArcGISTiledMapServiceLayer NewTiledLayer = new ESRI.ArcGIS.Client.ArcGISTiledMapServiceLayer();

            MyMap.Layers.LayersInitialized += (evtsender, args) =>
            {
                MyMap.ZoomTo(NewTiledLayer.InitialExtent);
            };

            NewTiledLayer.Url = UrlTextBox.Text;
            MyMap.Layers.Add(NewTiledLayer);
        }

        void MyMap_ExtentChange(object sender, ESRI.ArcGIS.Client.ExtentEventArgs args)
        {            
            setExtentText(args.NewExtent);
        }

        private void setExtentText(ESRI.ArcGIS.Client.Geometry.Envelope newExtent)
        {
            ExtentTextBlock.Text = string.Format("MinX: {0}\nMinY: {1}\nMaxX: {2}\nMaxY: {3}",
                newExtent.XMin, newExtent.YMin, newExtent.XMax, newExtent.YMax);
            ExtentGrid.Visibility = Visibility.Visible;
            ExtentTextBox.Text = string.Format("{0},{1},{2},{3}", newExtent.XMin.ToString("#0.000"), newExtent.YMin.ToString("#0.000"),
                newExtent.XMax.ToString("#0.000"), newExtent.YMax.ToString("#0.000"));    
        }
    }
}
