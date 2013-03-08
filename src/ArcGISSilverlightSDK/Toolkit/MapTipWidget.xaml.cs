using System;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Symbols;

namespace ArcGISSilverlightSDK
{
    public partial class MapTipWidget : UserControl
    {
        public MapTipWidget()
        {
            InitializeComponent();

            MyMap.Layers.LayersInitialized += Layers_LayersInitialized;
        }

        void Layers_LayersInitialized(object sender, EventArgs args)
        {
            ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query()
            {
                Geometry = MyMap.Extent,
				ReturnGeometry=true,
                OutSpatialReference = MyMap.SpatialReference
            };
            query.OutFields.Add("*");

            QueryTask queryTask = new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5");
            queryTask.ExecuteCompleted += QueryTask_ExecuteCompleted;
            queryTask.ExecuteAsync(query);
        }

        private void QueryTask_ExecuteCompleted(object sender, ESRI.ArcGIS.Client.Tasks.QueryEventArgs args)
        {
            FeatureSet featureSet = args.FeatureSet;
  
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            MyMapTip.GraphicsLayer = graphicsLayer;

            if (featureSet != null && featureSet.Features.Count > 0)
            {
                foreach (Graphic feature in featureSet.Features)
                {
                    feature.Symbol = LayoutRoot.Resources["DefaultFillSymbol"] as Symbol;
                    graphicsLayer.Graphics.Add(feature);
                }
            }
        }
    }
}
