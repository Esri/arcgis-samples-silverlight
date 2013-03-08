using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class SDSGraphicsLayer : UserControl
    {
        public SDSGraphicsLayer()
        {
            InitializeComponent();

            // Wait for Map spatial reference to be set by first layer. Unnecessary if set explicitly.  
            MyMap.Layers.LayersInitialized += Layers_LayersInitialized;
        }

        void Layers_LayersInitialized(object sender, EventArgs args)
        {
            LoadGraphics();
        }

        private void LoadGraphics()
        {
            QueryTask queryTask =
                new QueryTask("http://servicesbeta5.esri.com/arcgis/rest/services/World/FeatureServer/0");

            queryTask.ExecuteCompleted += queryTask_ExecuteCompleted;
            queryTask.DisableClientCaching = true;

            Query query = new ESRI.ArcGIS.Client.Tasks.Query();
            query.OutSpatialReference = MyMap.SpatialReference;
            query.ReturnGeometry = true;
            query.Where = "POP_RANK < 4";
            // Note, query.Text is not supported for use with Spatial Data Services

            query.OutFields.AddRange(new string[] { "CITY_NAME" });
            queryTask.ExecuteAsync(query);
        }

        void queryTask_ExecuteCompleted(object sender, QueryEventArgs args)
        {
            FeatureSet featureSet = args.FeatureSet;

            if (featureSet == null || featureSet.Features.Count < 1)
            {
                MessageBox.Show("No features returned from query");
                return;
            }

            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            
            foreach (Graphic graphic in featureSet.Features)
            {
                graphic.Symbol = LayoutRoot.Resources["MediumMarkerSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                graphicsLayer.Graphics.Add(graphic);
            }
        }  
    }
}
