using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class Identify : UserControl
    {
        private List<DataItem> _dataItems = null;

        public Identify()
        {
            InitializeComponent();
        }

        private void QueryPoint_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {
            ESRI.ArcGIS.Client.Geometry.MapPoint clickPoint = e.MapPoint;

            ESRI.ArcGIS.Client.Tasks.IdentifyParameters identifyParams = new IdentifyParameters()
            {
                Geometry = clickPoint,
                MapExtent = MyMap.Extent,
                Width = (int)MyMap.ActualWidth,
                Height = (int)MyMap.ActualHeight,
                LayerOption = LayerOption.visible,
                SpatialReference = MyMap.SpatialReference
            };

            IdentifyTask identifyTask = new IdentifyTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/" +
                "Demographics/ESRI_Census_USA/MapServer");
            identifyTask.ExecuteCompleted += IdentifyTask_ExecuteCompleted;
            identifyTask.Failed += IdentifyTask_Failed;
            identifyTask.ExecuteAsync(identifyParams);

            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.Graphics.Clear();
            ESRI.ArcGIS.Client.Graphic graphic = new ESRI.ArcGIS.Client.Graphic()
            {
                Geometry = clickPoint,
                Symbol = LayoutRoot.Resources["DefaultPictureSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol
            };
            graphicsLayer.Graphics.Add(graphic);
        }

        public void ShowFeatures(List<IdentifyResult> results)
        {
            _dataItems = new List<DataItem>();

            if (results != null && results.Count > 0)
            {
                IdentifyComboBox.Items.Clear();
                foreach (IdentifyResult result in results)
                {
                    Graphic feature = result.Feature;
                    string title = result.Value.ToString() + " (" + result.LayerName + ")";
                    _dataItems.Add(new DataItem()
                    {
                        Title = title,
                        Data = feature.Attributes
                    });
                    IdentifyComboBox.Items.Add(title);
                }
                IdentifyComboBox.SelectedIndex = 0;
            }
        }

        void cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = IdentifyComboBox.SelectedIndex;
            if (index > -1)
                IdentifyDetailsDataGrid.ItemsSource = _dataItems[index].Data;
        }

        private void IdentifyTask_ExecuteCompleted(object sender, IdentifyEventArgs args)
        {
            IdentifyDetailsDataGrid.ItemsSource = null;

            if (args.IdentifyResults != null && args.IdentifyResults.Count > 0)
            {
                IdentifyResultsPanel.Visibility = Visibility.Visible;

                ShowFeatures(args.IdentifyResults);
            }
            else
            {
                IdentifyComboBox.Items.Clear();
                IdentifyComboBox.UpdateLayout();

                IdentifyResultsPanel.Visibility = Visibility.Collapsed;
            }
        }

        public class DataItem
        {
            public string Title { get; set; }
            public IDictionary<string, object> Data { get; set; }
        }

        void IdentifyTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Identify failed. Error: " + e.Error);
        }
    }
}

