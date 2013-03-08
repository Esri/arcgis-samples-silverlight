using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.ValueConverters;

namespace ArcGISSilverlightSDK
{
    public partial class SpatialQuery : UserControl
    {
        private Draw MyDrawObject;
        GraphicsLayer selectionGraphicslayer;

        public SpatialQuery()
        {
            InitializeComponent();

            selectionGraphicslayer = MyMap.Layers["MySelectionGraphicsLayer"] as GraphicsLayer;

            MyDrawObject = new Draw(MyMap)
            {
                LineSymbol = LayoutRoot.Resources["DefaultLineSymbol"] as SimpleLineSymbol,
                FillSymbol = LayoutRoot.Resources["DefaultFillSymbol"] as FillSymbol
            };
            MyDrawObject.DrawComplete += MyDrawSurface_DrawComplete;
        }

        private void UnSelectTools()
        {
            foreach (UIElement element in MyStackPanel.Children)
                if (element is Button)
                    VisualStateManager.GoToState((element as Button), "UnSelected", false);
        }

        private void Tool_Click(object sender, RoutedEventArgs e)
        {
            UnSelectTools();

            VisualStateManager.GoToState(sender as Button, "Selected", false);

            switch ((sender as Button).Tag as string)
            {
                case "DrawPoint":
                    MyDrawObject.DrawMode = DrawMode.Point;
                    break;
                case "DrawPolyline":
                    MyDrawObject.DrawMode = DrawMode.Polyline;
                    break;
                case "DrawPolygon":
                    MyDrawObject.DrawMode = DrawMode.Polygon;
                    break;
                case "DrawRectangle":
                    MyDrawObject.DrawMode = DrawMode.Rectangle;
                    break;
                case "DrawFreehand":
                    MyDrawObject.DrawMode = DrawMode.Freehand;
                    break;
                case "DrawCircle":
                    MyDrawObject.DrawMode = DrawMode.Circle;
                    break;
                case "DrawEllipse":
                    MyDrawObject.DrawMode = DrawMode.Ellipse;
                    break;
                default:
                    MyDrawObject.DrawMode = DrawMode.None;
                    selectionGraphicslayer.ClearGraphics();
                    QueryDetailsDataGrid.ItemsSource = null;
                    ResultsDisplay.Visibility = Visibility.Collapsed;
                    break;
            }
            MyDrawObject.IsEnabled = (MyDrawObject.DrawMode != DrawMode.None);
        }

        private void MyDrawSurface_DrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs args)
        {
            ResultsDisplay.Visibility = Visibility.Collapsed;
            MyDrawObject.IsEnabled = false;
            selectionGraphicslayer.ClearGraphics();

            QueryTask queryTask = new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5");
            queryTask.ExecuteCompleted += QueryTask_ExecuteCompleted;
            queryTask.Failed += QueryTask_Failed;

            // Bind data grid to query results
            Binding resultFeaturesBinding = new Binding("LastResult.Features");
            resultFeaturesBinding.Source = queryTask;
            QueryDetailsDataGrid.SetBinding(DataGrid.ItemsSourceProperty, resultFeaturesBinding);
            Query query = new ESRI.ArcGIS.Client.Tasks.Query();

            // Specify fields to return from query
            query.OutFields.AddRange(new string[] { "STATE_NAME", "SUB_REGION", "STATE_FIPS", "STATE_ABBR", "POP2000", "POP2007" });
            query.Geometry = args.Geometry;

            // Return geometry with result features
            query.ReturnGeometry = true;
            query.OutSpatialReference = MyMap.SpatialReference;

            queryTask.ExecuteAsync(query);
        }

        private void QueryTask_ExecuteCompleted(object sender, ESRI.ArcGIS.Client.Tasks.QueryEventArgs args)
        {
            FeatureSet featureSet = args.FeatureSet;

            if (featureSet == null || featureSet.Features.Count < 1)
            {
                MessageBox.Show("No features returned from query");               
                return;
            }

            if (featureSet != null && featureSet.Features.Count > 0)
            {
                foreach (Graphic feature in featureSet.Features)
                {
                    feature.Symbol = LayoutRoot.Resources["ResultsFillSymbol"] as FillSymbol;
                    selectionGraphicslayer.Graphics.Insert(0, feature);
                }
                ResultsDisplay.Visibility = Visibility.Visible;
            }
            MyDrawObject.IsEnabled = true;
        }

        private void QueryTask_Failed(object sender, TaskFailedEventArgs args)
        {
            MessageBox.Show("Query failed: " + args.Error);
            MyDrawObject.IsEnabled = true;
        }

        private void GraphicsLayer_MouseEnter(object sender, GraphicMouseEventArgs args)
        {
            QueryDetailsDataGrid.Focus();
            QueryDetailsDataGrid.SelectedItem = args.Graphic;
            QueryDetailsDataGrid.CurrentColumn = QueryDetailsDataGrid.Columns[0];
            QueryDetailsDataGrid.ScrollIntoView(QueryDetailsDataGrid.SelectedItem, QueryDetailsDataGrid.Columns[0]);
        }

        private void GraphicsLayer_MouseLeave(object sender, GraphicMouseEventArgs args)
        {
            QueryDetailsDataGrid.Focus();
            QueryDetailsDataGrid.SelectedItem = null;
        }

        private void QueryDetailsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (Graphic g in e.AddedItems)
                g.Select();

            foreach (Graphic g in e.RemovedItems)
                g.UnSelect();
        }

        private void QueryDetailsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.MouseEnter += Row_MouseEnter;
            e.Row.MouseLeave += Row_MouseLeave;
        }

        void Row_MouseEnter(object sender, MouseEventArgs e)
        {
            (((System.Windows.FrameworkElement)(sender)).DataContext as Graphic).Select();
        }

        void Row_MouseLeave(object sender, MouseEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            Graphic g = ((System.Windows.FrameworkElement)(sender)).DataContext as Graphic;

            if (!QueryDetailsDataGrid.SelectedItems.Contains(g))
                g.UnSelect();
        }
    }
}
