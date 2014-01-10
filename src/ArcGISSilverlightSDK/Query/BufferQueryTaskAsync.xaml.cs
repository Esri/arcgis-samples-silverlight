using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class BufferQueryTaskAsync : UserControl
    {
        GeometryService _geometryService;
        QueryTask _queryTask;
        GraphicsLayer _pointAndBufferGraphicsLayer;
        GraphicsLayer _resultsGraphicsLayer;
        CancellationTokenSource _cts;

        public BufferQueryTaskAsync()
        {
            InitializeComponent();

            _geometryService = new GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");

            _queryTask = new QueryTask("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/BloomfieldHillsMichigan/Parcels/MapServer/2");

            _pointAndBufferGraphicsLayer = MyMap.Layers["MyBufferPointGraphicsLayer"] as GraphicsLayer;
            _resultsGraphicsLayer = MyMap.Layers["MyResultsGraphicsLayer"] as GraphicsLayer;
        }

        private async void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {
            try
            {
                if (_cts != null)
                    _cts.Cancel();

                _cts = new CancellationTokenSource();

                Graphic clickGraphic = new Graphic();
                clickGraphic.Symbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                clickGraphic.Geometry = e.MapPoint;
                clickGraphic.Geometry.SpatialReference = MyMap.SpatialReference;

                _pointAndBufferGraphicsLayer.Graphics.Clear();
                _resultsGraphicsLayer.Graphics.Clear();

                clickGraphic.SetZIndex(2);
                _pointAndBufferGraphicsLayer.Graphics.Add(clickGraphic);

                ESRI.ArcGIS.Client.Tasks.BufferParameters bufferParams = new ESRI.ArcGIS.Client.Tasks.BufferParameters()
                {
                    BufferSpatialReference = new SpatialReference(4326),
                    OutSpatialReference = MyMap.SpatialReference,
                    Unit = LinearUnit.Meter,
                };
                bufferParams.Distances.Add(100);
                bufferParams.Features.Add(clickGraphic);

                BufferResult bufferResult = await _geometryService.BufferTaskAsync(bufferParams, _cts.Token);

                Graphic bufferGraphic = new Graphic();
                bufferGraphic.Geometry = bufferResult.Results[0].Geometry;
                bufferGraphic.Symbol = LayoutRoot.Resources["BufferSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                bufferGraphic.SetZIndex(1);

                _pointAndBufferGraphicsLayer.Graphics.Add(bufferGraphic);

                ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query();
                query.ReturnGeometry = true;
                query.OutSpatialReference = MyMap.SpatialReference;
                query.Geometry = bufferGraphic.Geometry;
                query.OutFields.Add("OWNERNME1");

                QueryResult queryResult = await _queryTask.ExecuteTaskAsync(query, _cts.Token);

                if (queryResult.FeatureSet.Features.Count < 1)
                {
                    MessageBox.Show("No features found");
                    return;
                }

                foreach (Graphic selectedGraphic in queryResult.FeatureSet.Features)
                {
                    selectedGraphic.Symbol = LayoutRoot.Resources["ParcelSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                    _resultsGraphicsLayer.Graphics.Add(selectedGraphic);
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
    }
}
