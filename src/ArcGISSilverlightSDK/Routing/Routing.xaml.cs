using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class Routing : UserControl
    {
        GraphicsLayer stopsGraphicsLayer;
        GraphicsLayer routeGraphicsLayer;
        RouteTask routeTask;

        public Routing()
        {
            InitializeComponent();

            stopsGraphicsLayer = MyMap.Layers["MyStopsGraphicsLayer"] as GraphicsLayer;
            routeGraphicsLayer = MyMap.Layers["MyRouteGraphicsLayer"] as GraphicsLayer;
            routeTask = LayoutRoot.Resources["MyRouteTask"] as RouteTask;
        }

        private void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {  
            Graphic stop = new Graphic() { Geometry = e.MapPoint };

            stopsGraphicsLayer.Graphics.Add(stop);

            if (stopsGraphicsLayer.Graphics.Count > 1)
            {                
                if (routeTask.IsBusy)
                {
                    routeTask.CancelAsync();
                    stopsGraphicsLayer.Graphics.RemoveAt(stopsGraphicsLayer.Graphics.Count - 1);
                }
                routeTask.SolveAsync(new RouteParameters() { Stops = stopsGraphicsLayer, 
                    UseTimeWindows = false, OutSpatialReference = MyMap.SpatialReference });
            }
        }

        private void MyRouteTask_Failed(object sender, TaskFailedEventArgs e)
        {
            string errorMessage = "Routing error: ";
            errorMessage += e.Error.Message;
            foreach (string detail in (e.Error as ServiceException).Details)            
                errorMessage += "," + detail;
            
            MessageBox.Show(errorMessage);

            stopsGraphicsLayer.Graphics.RemoveAt(stopsGraphicsLayer.Graphics.Count - 1);
        }

        private void MyRouteTask_SolveCompleted(object sender, RouteEventArgs e)
        {
            routeGraphicsLayer.Graphics.Clear();

            RouteResult routeResult = e.RouteResults[0];
            
            Graphic lastRoute = routeResult.Route;

            decimal totalTime = (decimal)lastRoute.Attributes["Total_Time"];
            string tip = string.Format("{0} minutes", totalTime.ToString("#0.000"));
            lastRoute.Attributes.Add("TIP", tip);

            routeGraphicsLayer.Graphics.Add(lastRoute);
        }
    }
}
