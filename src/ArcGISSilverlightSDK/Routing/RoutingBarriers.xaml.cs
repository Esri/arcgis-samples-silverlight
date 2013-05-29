using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class RoutingBarriers : UserControl
    {
        RouteTask _routeTask;
        List<Graphic> _stops = new List<Graphic>();
        List<Graphic> _barriers = new List<Graphic>();
        RouteParameters _routeParams = new RouteParameters();
        GraphicsLayer stopsLayer = null;
        GraphicsLayer barriersLayer = null;

        public RoutingBarriers()
        {
            InitializeComponent();

            _routeTask =
                new RouteTask("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route");
            _routeTask.SolveCompleted += routeTask_SolveCompleted;
            _routeTask.Failed += routeTask_Failed;

            _routeParams.Stops = _stops;
            _routeParams.Barriers = _barriers;
            _routeParams.UseTimeWindows = false;

            barriersLayer = MyMap.Layers["MyBarriersGraphicsLayer"] as GraphicsLayer;
            stopsLayer = MyMap.Layers["MyStopsGraphicsLayer"] as GraphicsLayer;
        }

        private void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {
            if (StopsRadioButton.IsChecked.Value)
            {
                Graphic stop = new Graphic() { Geometry = e.MapPoint, Symbol = LayoutRoot.Resources["StopSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol };
                stop.Attributes.Add("StopNumber", stopsLayer.Graphics.Count + 1);
                stopsLayer.Graphics.Add(stop);
                _stops.Add(stop);
            }
            else if (BarriersRadioButton.IsChecked.Value)
            {
                Graphic barrier = new Graphic() { Geometry = e.MapPoint, Symbol = LayoutRoot.Resources["BarrierSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol };
                barriersLayer.Graphics.Add(barrier);
                _barriers.Add(barrier);
            }
            if (_stops.Count > 1)
            {
                if (_routeTask.IsBusy)
                    _routeTask.CancelAsync();
                _routeParams.OutSpatialReference = MyMap.SpatialReference;
                _routeTask.SolveAsync(_routeParams);
            }
        }

        private void routeTask_Failed(object sender, TaskFailedEventArgs e)
        {
            string errorMessage = "Routing error: ";
            errorMessage += e.Error.Message;
            foreach (string detail in (e.Error as ServiceException).Details)
                errorMessage += "," + detail;

            MessageBox.Show(errorMessage);

            if ((_stops.Count) > 10)
            {
                stopsLayer.Graphics.RemoveAt(stopsLayer.Graphics.Count - 1);
                _stops.RemoveAt(_stops.Count - 1);
            }
        }

        private void routeTask_SolveCompleted(object sender, RouteEventArgs e)
        {
            GraphicsLayer routeLayer = MyMap.Layers["MyRouteGraphicsLayer"] as GraphicsLayer;
            RouteResult routeResult = e.RouteResults[0];
            routeResult.Route.Symbol = LayoutRoot.Resources["RouteSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;

            routeLayer.Graphics.Clear();
            Graphic lastRoute = routeResult.Route;

            routeLayer.Graphics.Add(lastRoute);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _stops.Clear();
            _barriers.Clear();

            foreach (Layer layer in MyMap.Layers)
                if (layer is GraphicsLayer)
                    (layer as GraphicsLayer).Graphics.Clear();
        }
    }
}
