using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace ArcGISSilverlightSDK
{
    public partial class IntersectTaskAsync : UserControl
    {
        private Draw _myDrawObject;
        private GraphicsLayer _intersectGraphicsLayer;
        private GeometryService _geometryService;
        private CancellationTokenSource _cts;

        public IntersectTaskAsync()
        {
            InitializeComponent();

            _myDrawObject = new Draw(MyMap)
            {
                DrawMode = DrawMode.Polygon,
                IsEnabled = false,
                FillSymbol = LayoutRoot.Resources["CyanFillSymbol"] as ESRI.ArcGIS.Client.Symbols.FillSymbol
            };
            _myDrawObject.DrawComplete += MyDrawObject_DrawComplete;
            _myDrawObject.IsEnabled = true;

            _intersectGraphicsLayer = MyMap.Layers["IntersectGraphicsLayer"] as GraphicsLayer;

            _geometryService = new GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
        }

        private async void MyDrawObject_DrawComplete(object sender, DrawEventArgs args)
        {
            try
            {
                _myDrawObject.IsEnabled = false;

                if (_cts != null)
                    _cts.Cancel();

                _cts = new CancellationTokenSource();

                QueryTask queryTask =
                      new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1");
                Query query = new Query()
                {
                    Geometry = args.Geometry,
                    ReturnGeometry = true,
                    OutSpatialReference = MyMap.SpatialReference
                };

                QueryResult parcelsToIntersectResult = await queryTask.ExecuteTaskAsync(query, _cts.Token);

                List<Graphic> graphicList = new List<Graphic>();
                graphicList.Add(new Graphic() { Geometry = args.Geometry });
                SimplifyResult simplifiedIntersectGeometryResult = await _geometryService.SimplifyTaskAsync(graphicList, _cts.Token);

                IntersectResult intersectedParcelsResult = await _geometryService.IntersectTaskAsync(parcelsToIntersectResult.FeatureSet.ToList(), simplifiedIntersectGeometryResult.Results[0].Geometry, _cts.Token);

                Random random = new Random();
                foreach (Graphic g in intersectedParcelsResult.Results)
                {
                    SimpleFillSymbol symbol = new SimpleFillSymbol()
                    {
                        Fill = new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromArgb(255, (byte)random.Next(0, 255), (byte)random.Next(0, 255),
                            (byte)random.Next(0, 255))),
                        BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black),
                        BorderThickness = 1
                    };
                    g.Symbol = symbol;
                    _intersectGraphicsLayer.Graphics.Add(g);
                }
            }
            catch (Exception ex)
            {
                if (ex is ServiceException)
                {
                    MessageBox.Show(String.Format("{0}: {1}", (ex as ServiceException).Code.ToString(), (ex as ServiceException).Details[0]), "Error", MessageBoxButton.OK);
                    return;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _intersectGraphicsLayer.Graphics.Clear();
            _myDrawObject.IsEnabled = true;
        }
    }
}
