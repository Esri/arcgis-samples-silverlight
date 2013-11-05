using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows;

namespace ArcGISSilverlightSDK
{
    public partial class DrawGraphics : UserControl
    {
        private Draw MyDrawObject;
        private Symbol _activeSymbol = null;
        GraphicsLayer graphicsLayer;
        Graphic activeGraphic;
        Editor editor;

        public DrawGraphics()
        {
            InitializeComponent();

            graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;

            MyDrawObject = new Draw(MyMap)
            {
                LineSymbol = LayoutRoot.Resources["DrawLineSymbol"] as LineSymbol,
                FillSymbol = LayoutRoot.Resources["DrawFillSymbol"] as FillSymbol
            };
            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;

            editor = LayoutRoot.Resources["MyEditor"] as Editor;
        }

        private void MyDrawObject_DrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs args)
        {
            ESRI.ArcGIS.Client.Graphic graphic = new ESRI.ArcGIS.Client.Graphic()
            {
                Geometry = args.Geometry,
                Symbol = _activeSymbol,
            };
            graphicsLayer.Graphics.Add(graphic);
        }

        private void GraphicsLayer_MouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs e)
        {
            if (EnableEditVerticesScaleRotate.IsChecked.Value)
            {                            
                if (e.Graphic != null && !(e.Graphic.Geometry is ESRI.ArcGIS.Client.Geometry.MapPoint))
                {
                    MyDrawObject.DrawMode = DrawMode.None;
                    UnSelectTools();    
                    editor.EditVertices.Execute(e.Graphic);
                    activeGraphic = e.Graphic;
                }
            }
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

            if (activeGraphic != null)
            {
                if (editor.CancelActive.CanExecute(null))
                    editor.CancelActive.Execute(null);
                activeGraphic = null;
            }

            switch ((sender as Button).Tag as string)
            {
                case "DrawPoint":
                    MyDrawObject.DrawMode = DrawMode.Point;
                    _activeSymbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as Symbol;
                    break;
                case "DrawPolyline":
                    MyDrawObject.DrawMode = DrawMode.Polyline;
                    _activeSymbol = LayoutRoot.Resources["DefaultLineSymbol"] as Symbol;
                    break;
                case "DrawlineSegment":
                    MyDrawObject.DrawMode = DrawMode.LineSegment;
                    _activeSymbol = LayoutRoot.Resources["DefaultLineSymbol"] as Symbol;
                    break;
                case "DrawPolygon":
                    MyDrawObject.DrawMode = DrawMode.Polygon;
                    _activeSymbol = LayoutRoot.Resources["DefaultFillSymbol"] as Symbol;
                    break;
                case "DrawRectangle":
                    MyDrawObject.DrawMode = DrawMode.Rectangle;
                    _activeSymbol = LayoutRoot.Resources["DefaultFillSymbol"] as Symbol;
                    break;
                case "DrawFreehand":
                    MyDrawObject.DrawMode = DrawMode.Freehand;
                    _activeSymbol = LayoutRoot.Resources["DefaultLineSymbol"] as Symbol;
                    break;
                case "DrawArrow":
                    MyDrawObject.DrawMode = DrawMode.Arrow;
                    _activeSymbol = LayoutRoot.Resources["DefaultFillSymbol"] as Symbol;
                    break;
                case "DrawTriangle":
                    MyDrawObject.DrawMode = DrawMode.Triangle;
                    _activeSymbol = LayoutRoot.Resources["DefaultFillSymbol"] as Symbol;
                    break;
                case "DrawCircle":
                    MyDrawObject.DrawMode = DrawMode.Circle;
                    _activeSymbol = LayoutRoot.Resources["DefaultFillSymbol"] as Symbol;
                    break;
                case "DrawEllipse":
                    MyDrawObject.DrawMode = DrawMode.Ellipse;
                    _activeSymbol = LayoutRoot.Resources["DefaultFillSymbol"] as Symbol;
                    break;
                default:
                    MyDrawObject.DrawMode = DrawMode.None;
                    graphicsLayer.Graphics.Clear();
                    break;
            }
            MyDrawObject.IsEnabled = (MyDrawObject.DrawMode != DrawMode.None);
        }
    }
}
