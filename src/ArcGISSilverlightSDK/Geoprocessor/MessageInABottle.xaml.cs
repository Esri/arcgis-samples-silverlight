using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class MessageInABottle : UserControl
    {
        GraphicsLayer _graphicsLayer = null;
        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
            new ESRI.ArcGIS.Client.Projection.WebMercator();

        public MessageInABottle()
        {
            InitializeComponent();

            _graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
        }

        private void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {
            Graphic graphic = new Graphic()
            {
                Symbol =  LayoutRoot.Resources["StartMarkerSymbol"] as Symbol,
                Geometry = e.MapPoint
            };
            _graphicsLayer.Graphics.Add(graphic);

            Geoprocessor geoprocessorTask = new Geoprocessor("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_Currents_World/GPServer/MessageInABottle");
            geoprocessorTask.ExecuteCompleted += GeoprocessorTask_ExecuteCompleted;
            geoprocessorTask.Failed += GeoprocessorTask_Failed;
            geoprocessorTask.OutputSpatialReference = MyMap.SpatialReference;

            List<GPParameter> parameters = new List<GPParameter>();
            parameters.Add(new GPFeatureRecordSetLayer("Input_Point", _mercator.ToGeographic(e.MapPoint)));
            parameters.Add(new GPDouble("Days", Convert.ToDouble(DaysTextBox.Text)));
            
            geoprocessorTask.ExecuteAsync(parameters);
        }

        private void GeoprocessorTask_ExecuteCompleted(object sender, GPExecuteCompleteEventArgs e)
        {
            foreach (GPParameter gpParameter in e.Results.OutParameters)
            {
                if (gpParameter is GPFeatureRecordSetLayer)
                {
                    GPFeatureRecordSetLayer gpLayer = gpParameter as GPFeatureRecordSetLayer;
                    foreach (Graphic graphic in gpLayer.FeatureSet.Features)
                    {
                      graphic.Symbol = LayoutRoot.Resources["PathLineSymbol"] as Symbol;
                      _graphicsLayer.Graphics.Add(graphic);
                    }
                }
            }
        }

        private void GeoprocessorTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geoprocessor service failed: " + e.Error);
        }
    }
}
