using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISSilverlightSDK
{
    public partial class DriveTimes : UserControl
    {
        Geoprocessor _geoprocessorTask;
        string jobid;
        GraphicsLayer graphicsLayer;
        List<FillSymbol> bufferSymbols;
        MapPoint inputPoint;

        public DriveTimes()
        {
            InitializeComponent();

            graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            bufferSymbols = new List<FillSymbol>(
                        new FillSymbol[] { LayoutRoot.Resources["FillSymbol1"] as FillSymbol, 
                        LayoutRoot.Resources["FillSymbol2"] as FillSymbol, 
                        LayoutRoot.Resources["FillSymbol3"] as FillSymbol });

            _geoprocessorTask = new Geoprocessor("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/GPServer/Generate%20Service%20Areas");
            _geoprocessorTask.JobCompleted += GeoprocessorTask_JobCompleted;
            _geoprocessorTask.StatusUpdated += GeoprocessorTask_StatusUpdated;
            _geoprocessorTask.GetResultDataCompleted += GeoprocessorTask_GetResultDataCompleted;
            _geoprocessorTask.Failed += GeoprocessorTask_Failed;
        }

        private void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {           
            if (jobid != null)
            {
                inputPoint = e.MapPoint;
                _geoprocessorTask.CancelJobAsync(jobid);               
            }
            else
            {
                inputPoint = null;
                SubmitJob(e.MapPoint);               
            }
        }

        private void SubmitJob(MapPoint mp)
        {
            graphicsLayer.Graphics.Clear();

            Graphic graphic = new Graphic()
            {
                Symbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as Symbol,
                Geometry = mp
            };
            graphic.SetZIndex(1);
            graphicsLayer.Graphics.Add(graphic);

            List<GPParameter> parameters = new List<GPParameter>();
            parameters.Add(new GPFeatureRecordSetLayer("Facilities", new FeatureSet(graphicsLayer.Graphics)));
            parameters.Add(new GPString("Break_Values", "1 2 3"));
            parameters.Add(new GPString("Break_Units", "Minutes"));

            _geoprocessorTask.SubmitJobAsync(parameters);           
        }

        void GeoprocessorTask_StatusUpdated(object sender, JobInfoEventArgs e)
        {
            jobid = e.JobInfo.JobStatus == esriJobStatus.esriJobCancelled ||
                e.JobInfo.JobStatus == esriJobStatus.esriJobDeleted ||
                e.JobInfo.JobStatus == esriJobStatus.esriJobFailed
                ? null : e.JobInfo.JobId;
            if (e.JobInfo.JobStatus == esriJobStatus.esriJobCancelling && inputPoint != null)
            {
                SubmitJob(inputPoint);
                inputPoint = null;
            }
        }

        private void GeoprocessorTask_JobCompleted(object sender, JobInfoEventArgs e)
        {
            jobid = null;
            if (e.JobInfo.JobStatus == esriJobStatus.esriJobSucceeded)
                _geoprocessorTask.GetResultDataAsync(e.JobInfo.JobId, "ServiceAreas");
        }

        void GeoprocessorTask_GetResultDataCompleted(object sender, GPParameterEventArgs e)
        {
            if (e.Parameter is GPFeatureRecordSetLayer)
            {
                GPFeatureRecordSetLayer gpLayer = e.Parameter as GPFeatureRecordSetLayer;

                int count = 0;
                foreach (Graphic graphic in gpLayer.FeatureSet.Features)
                {
                    graphic.Symbol = bufferSymbols[count++];
                    graphicsLayer.Graphics.Add(graphic);
                }
            }
        }

        private void GeoprocessorTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geoprocessing service failed: " + e.Error);
        }
    }
}
