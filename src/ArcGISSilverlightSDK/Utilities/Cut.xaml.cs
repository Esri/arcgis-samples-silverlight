using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class Cut : UserControl
    {
        private Draw MyDrawObject;
        private GraphicsLayer parcelGraphicsLayer;

        public Cut()
        {
            InitializeComponent();

            MyMap.Layers.LayersInitialized += Layers_LayersInitialized;  

            MyMap.MinimumResolution = double.Epsilon;

            MyDrawObject = new Draw(MyMap)
            {
                DrawMode = DrawMode.Polyline,
                IsEnabled = false,
                LineSymbol = LayoutRoot.Resources["RedLineSymbol"] as ESRI.ArcGIS.Client.Symbols.LineSymbol
            };
            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;

            parcelGraphicsLayer = MyMap.Layers["ParcelsGraphicsLayer"] as GraphicsLayer;
        }

        void Layers_LayersInitialized(object sender, EventArgs args)
        {
            if (parcelGraphicsLayer != null && parcelGraphicsLayer.Graphics.Count == 0)
            {
                QueryTask queryTask =
                    new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1");
                Query query = new Query();
                query.Geometry = MyMap.Extent;
                query.ReturnGeometry = true;
                queryTask.ExecuteCompleted += queryTask_ExecuteCompleted;
                queryTask.Failed += queryTask_Failed;
                queryTask.ExecuteAsync(query);
            }
        }

        void queryTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Query error: " + e.Error);
        }

        void queryTask_ExecuteCompleted(object sender, QueryEventArgs e)
        {            
            foreach (Graphic g in e.FeatureSet.Features)
            {
                g.Symbol = LayoutRoot.Resources["BlueFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                g.Geometry.SpatialReference = MyMap.SpatialReference;
                parcelGraphicsLayer.Graphics.Add(g);
            }
            MyDrawObject.IsEnabled = true;
        }

        private void MyDrawObject_DrawComplete(object sender, DrawEventArgs args)
        {
            MyDrawObject.IsEnabled = false;

            ESRI.ArcGIS.Client.Geometry.Polyline polyline = args.Geometry as ESRI.ArcGIS.Client.Geometry.Polyline;
            polyline.SpatialReference = MyMap.SpatialReference;

            GeometryService geometryService =
              new GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
                        
            geometryService.CutCompleted += GeometryService_CutCompleted;
            geometryService.Failed += GeometryService_Failed;

            geometryService.CutAsync(parcelGraphicsLayer.Graphics.ToList(), polyline);
        }
        
        void GeometryService_CutCompleted(object sender, CutEventArgs e)
        {
            parcelGraphicsLayer.Graphics.Clear();

            foreach (Graphic g in e.Results)
            {
                g.Symbol = LayoutRoot.Resources["BlueFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                parcelGraphicsLayer.Graphics.Add(g);
            }
            MyDrawObject.IsEnabled = true;
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
        }
    }
}
