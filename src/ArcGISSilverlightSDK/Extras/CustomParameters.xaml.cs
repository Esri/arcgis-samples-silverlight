using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class CustomParameters : UserControl
    {
        GraphicsLayer _stopsGraphicsLayer;
        GraphicsLayer _routeGraphicsLayer;
        RouteTask _routeTask;
        RouteParameters _routeParams;
        CancellationTokenSource _cts;

        public CustomParameters()
        {
            InitializeComponent();

            _stopsGraphicsLayer = MyMap.Layers["MyStopsGraphicsLayer"] as GraphicsLayer;
            _routeGraphicsLayer = MyMap.Layers["MyRouteGraphicsLayer"] as GraphicsLayer;

            _routeTask = new RouteTask("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route");

            _routeParams = new RouteParameters()
            {
                Stops = _stopsGraphicsLayer,
                UseTimeWindows = false,
                OutSpatialReference = MyMap.SpatialReference
            };

            _routeTask.CustomParameters.Add("myParameterName", "0");
        }

        private async void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {
            try
            {
                if (_cts != null)
                    _cts.Cancel();

                _cts = new CancellationTokenSource();

                Graphic stop = new Graphic() { Geometry = e.MapPoint };

                _stopsGraphicsLayer.Graphics.Add(stop);

                if (_stopsGraphicsLayer.Graphics.Count > 1)
                {
                    if (_routeTask.IsBusy)
                    {
                        _cts.Cancel();
                        _stopsGraphicsLayer.Graphics.RemoveAt(_stopsGraphicsLayer.Graphics.Count - 1);
                    }

                    SolveRouteResult result = await _routeTask.SolveTaskAsync(_routeParams, _cts.Token);

                    _routeGraphicsLayer.Graphics.Clear();
                    _routeGraphicsLayer.Graphics.Add(result.RouteResults[0].Route);
                }
            }
            catch (Exception ex)
            {
                _stopsGraphicsLayer.Graphics.RemoveAt(_stopsGraphicsLayer.Graphics.Count - 1);

                if (ex is ServiceException)
                {
                    MessageBox.Show(String.Format("{0}: {1}", (ex as ServiceException).Code.ToString(), (ex as ServiceException).Details[0]), "Error", MessageBoxButton.OK);
                    return;
                }
            }
        }
    }
}
