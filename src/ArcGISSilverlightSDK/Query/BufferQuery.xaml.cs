using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class BufferQuery : UserControl
    {
        GeometryService _geometryService;
        QueryTask _queryTask;
        GraphicsLayer _pointAndBufferGraphicsLayer;
        GraphicsLayer _resultsGraphicsLayer;

        public BufferQuery()
        {
            InitializeComponent();

            _geometryService =
            new GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
            _geometryService.BufferCompleted += GeometryService_BufferCompleted;
            _geometryService.Failed += GeometryService_Failed;

            _queryTask = new QueryTask("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/BloomfieldHillsMichigan/Parcels/MapServer/2");
            _queryTask.ExecuteCompleted += QueryTask_ExecuteCompleted;
            _queryTask.Failed += QueryTask_Failed;

            _pointAndBufferGraphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            _resultsGraphicsLayer = MyMap.Layers["MyResultsGraphicsLayer"] as GraphicsLayer;
        }

        private void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {
            _geometryService.CancelAsync();
            _queryTask.CancelAsync();

            Graphic clickGraphic = new Graphic();
            clickGraphic.Symbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol; 
            clickGraphic.Geometry = e.MapPoint;
            // Input spatial reference for buffer operation defined by first feature of input geometry array
            clickGraphic.Geometry.SpatialReference = MyMap.SpatialReference;

            _pointAndBufferGraphicsLayer.Graphics.Clear();
            _resultsGraphicsLayer.Graphics.Clear();

            clickGraphic.SetZIndex(2);
            _pointAndBufferGraphicsLayer.Graphics.Add(clickGraphic);

            // If buffer spatial reference is GCS and unit is linear, geometry service will do geodesic buffering
            ESRI.ArcGIS.Client.Tasks.BufferParameters bufferParams = new ESRI.ArcGIS.Client.Tasks.BufferParameters()
            {
                BufferSpatialReference = new SpatialReference(4326),
                OutSpatialReference = MyMap.SpatialReference,
                Unit = LinearUnit.Meter,
            };
            bufferParams.Distances.Add(100);
            bufferParams.Features.Add(clickGraphic);

            _geometryService.BufferAsync(bufferParams);
        }

        void GeometryService_BufferCompleted(object sender, GraphicsEventArgs args)
        {
            Graphic bufferGraphic = new Graphic();
            bufferGraphic.Geometry = args.Results[0].Geometry;
            bufferGraphic.Symbol = LayoutRoot.Resources["BufferSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol; 
            bufferGraphic.SetZIndex(1);

            _pointAndBufferGraphicsLayer.Graphics.Add(bufferGraphic);
            
            ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query();
            query.ReturnGeometry = true;
            query.OutSpatialReference = MyMap.SpatialReference;
            query.Geometry = bufferGraphic.Geometry;
            query.OutFields.Add("OWNERNME1");
            _queryTask.ExecuteAsync(query);
        }

        private void QueryTask_ExecuteCompleted(object sender, QueryEventArgs args)
        {
            if (args.FeatureSet.Features.Count < 1)
            {
                MessageBox.Show("No features found");
                return;
            }

            foreach (Graphic selectedGraphic in args.FeatureSet.Features)
            {
                selectedGraphic.Symbol = LayoutRoot.Resources["ParcelSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                _resultsGraphicsLayer.Graphics.Add(selectedGraphic);
            }
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs args)
        {
            MessageBox.Show("Geometry service failed: " + args.Error);
        }

        private void QueryTask_Failed(object sender, TaskFailedEventArgs args)
        {
            MessageBox.Show("Query failed: " + args.Error);
        }
    }
}
