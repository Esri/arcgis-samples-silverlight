using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class ElevationProfile : UserControl
    {
        Draw _myDrawObject;
        GraphicsLayer _lineGraphicLayer;
        GraphicsLayer _pointGraphicLayer;
        CancellationTokenSource _cts;

        public ElevationProfile()
        {
            InitializeComponent();

            _lineGraphicLayer = MyMap.Layers["LineGraphicsLayer"] as GraphicsLayer;
            _lineGraphicLayer.Graphics.Add(new Graphic()
            {
                Geometry = null
            });

            _pointGraphicLayer = MyMap.Layers["PointGraphicsLayer"] as GraphicsLayer;
            _pointGraphicLayer.Graphics.Add(new Graphic()
            {
                Geometry = new MapPoint(Double.NaN, Double.NaN)
            });

            _myDrawObject = new Draw(MyMap)
            {
                LineSymbol = (LayoutRoot.Resources["LineRenderer"] as SimpleRenderer).Symbol as LineSymbol
            };
            _myDrawObject.DrawComplete += _myDrawObject_DrawComplete;
        }

        private void Tool_Click(object sender, RoutedEventArgs e)
        {
            ChartContainer.Visibility = System.Windows.Visibility.Collapsed;
            _lineGraphicLayer.Graphics[0].Geometry = null;
            (_pointGraphicLayer.Graphics[0].Geometry as MapPoint).X = Double.NaN;
            (_pointGraphicLayer.Graphics[0].Geometry as MapPoint).Y = Double.NaN;

            switch ((sender as Button).Tag as string)
            {
                case "DrawPolyline":
                    _myDrawObject.DrawMode = DrawMode.Polyline;
                    _myDrawObject.IsEnabled = true;
                    break;
                case "DrawFreehand":
                    _myDrawObject.DrawMode = DrawMode.Freehand;
                    _myDrawObject.IsEnabled = true;
                    break;
                default:
                    _myDrawObject.DrawMode = DrawMode.None;
                    break;
            }
        }

        async void _myDrawObject_DrawComplete(object sender, DrawEventArgs e)
        {
            if (e.Geometry == null)
            {
                ChartContainer.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

            try
            {
                _myDrawObject.IsEnabled = false;

                this.Cursor = Cursors.Wait;

                _lineGraphicLayer.Graphics[0].Geometry = e.Geometry;

                if (_cts != null)
                    _cts.Cancel();

                _cts = new CancellationTokenSource();

                Geoprocessor geoprocessorTask = new Geoprocessor(
                    "http://elevation.arcgis.com/arcgis/rest/services/Tools/ElevationSync/GPServer/Profile");

                List<GPParameter> parameters = new List<GPParameter>();
                parameters.Add(new GPFeatureRecordSetLayer("InputLineFeatures", e.Geometry));
                parameters.Add(new GPString("returnM", "true"));
                parameters.Add(new GPString("returnZ", "true"));

                GPExecuteResults results = await geoprocessorTask.ExecuteTaskAsync(parameters, _cts.Token);

                if (results == null || results.OutParameters.Count == 0 || (results.OutParameters[0] as GPFeatureRecordSetLayer).FeatureSet.Features.Count == 0)
                {
                    MessageBox.Show("Fail to get elevation data. Draw another line");
                    return;
                }

                ESRI.ArcGIS.Client.Geometry.Polyline elevationLine =
                    (results.OutParameters[0] as GPFeatureRecordSetLayer).FeatureSet.Features[0].Geometry
                    as ESRI.ArcGIS.Client.Geometry.Polyline;

                foreach (MapPoint p in elevationLine.Paths[0])
                {
                    p.M = Math.Round(p.M / 1000, 2);
                    p.Z = Math.Round(p.Z, 2);
                }

                MapPoint lastPoint = elevationLine.Paths[0][elevationLine.Paths[0].Count - 1];

                lblDistance.Text = string.Format("Total Distance {0} Kilometers", lastPoint.M.ToString());

                (ElevationChart.Series[0] as LineSeries).ItemsSource = elevationLine.Paths[0];

                ChartContainer.Visibility = System.Windows.Visibility.Visible;
            }
            catch (Exception ex)
            {
                if (ex is ServiceException)
                {
                    MessageBox.Show(String.Format("{0}: {1}", (ex as ServiceException).Code.ToString(),
                        (ex as ServiceException).Details[0]), "Error", MessageBoxButton.OK);
                    return;
                }
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        void ElevationChart_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(((System.Windows.FrameworkElement)(((System.Windows.RoutedEventArgs)(e)).OriginalSource)) is Ellipse))
            {
                (_pointGraphicLayer.Graphics[0].Geometry as MapPoint).X = Double.NaN;
                (_pointGraphicLayer.Graphics[0].Geometry as MapPoint).Y = Double.NaN;
            }
            else
            {
                Ellipse chartPoint = ((System.Windows.FrameworkElement)(((System.Windows.RoutedEventArgs)(e)).OriginalSource)) as Ellipse;
                MapPoint mapPoint = chartPoint.DataContext as MapPoint;

                (_pointGraphicLayer.Graphics[0].Geometry as MapPoint).X = mapPoint.X;
                (_pointGraphicLayer.Graphics[0].Geometry as MapPoint).Y = mapPoint.Y;
            }
        }
    }
}
