using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class ExtractData : UserControl
    {
        private Draw _drawObject;
        Geoprocessor _geoprocessorTask;
        private DispatcherTimer _processingTimer;
        Stream _streamedDataFile;
        GraphicsLayer _graphicsLayer;
        

        public ExtractData()
        {
            InitializeComponent();
            _processingTimer = new System.Windows.Threading.DispatcherTimer();
            _processingTimer.Interval = new TimeSpan(0, 0, 0, 0, 800);
            _processingTimer.Tick += ProcessingTimer_Tick;

            _geoprocessorTask = new Geoprocessor("http://sampleserver4.arcgisonline.com/ArcGIS/rest/services/HomelandSecurity/Incident_Data_Extraction/GPServer/Extract%20Data%20Task");
            _geoprocessorTask.GetServiceInfoCompleted += _geoprocessorTask_GetServiceInfoCompleted;
            _geoprocessorTask.Failed += _geoprocessorTask_Failed;
            _geoprocessorTask.UpdateDelay = 5000;
            _geoprocessorTask.JobCompleted += _geoprocessorTask_JobCompleted;
            _geoprocessorTask.GetResultDataCompleted += _geoprocessorTask_GetResultDataCompleted;
            _geoprocessorTask.GetServiceInfoAsync();

            _drawObject = new Draw(MyMap)
            {
                LineSymbol = LayoutRoot.Resources["CustomAnimatedRedLineSymbol"] as LineSymbol,
                FillSymbol = LayoutRoot.Resources["CustomAnimatedRedFillSymbol"] as FillSymbol
            };
            _drawObject.DrawComplete += MyDrawObject_DrawComplete;

            _graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
        }

        private void DrawPolygon_Click(object sender, RoutedEventArgs e)
        {
            _graphicsLayer.ClearGraphics();
            if (FreehandCheckBox.IsChecked.Value)
                _drawObject.DrawMode = DrawMode.Freehand;
            else
                _drawObject.DrawMode = DrawMode.Polygon;

            _drawObject.IsEnabled = true;
        }

        private void FreehandCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (FreehandCheckBox.IsChecked.Value)
                _drawObject.DrawMode = DrawMode.Freehand;
            else
                _drawObject.DrawMode = DrawMode.Polygon;
        }

        void _geoprocessorTask_GetServiceInfoCompleted(object sender, GPServiceInfoEventArgs e)
        {
            LayersList.ItemsSource = e.GPServiceInfo.Parameters.FirstOrDefault(p => p.Name == "Layers_to_Clip").ChoiceList as object[];

            Formats.ItemsSource = e.GPServiceInfo.Parameters.FirstOrDefault(p => p.Name == "Feature_Format").ChoiceList as object[];
            if (Formats.ItemsSource != null && Formats.Items.Count > 0)
                Formats.SelectedIndex = 0;
        }

        private void MyDrawObject_DrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs args)
        {
            if (LayersList.SelectedItem == null)
            {
                MessageBox.Show("Please select layer(s) to extract");
                (MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer).ClearGraphics();
                return;
            }

            Geometry filterGeometry = args.Geometry;

            if (args.Geometry is Polyline)
            {
                Polyline freehandLine = args.Geometry as Polyline;
                freehandLine.Paths[0].Add(freehandLine.Paths[0][0].Clone());
                filterGeometry = new Polygon() { SpatialReference = MyMap.SpatialReference };
                (filterGeometry as Polygon).Rings.Add(freehandLine.Paths[0]);
            }

            ESRI.ArcGIS.Client.Graphic graphic = new ESRI.ArcGIS.Client.Graphic()
            {
                Geometry = filterGeometry
            };
            _graphicsLayer.Graphics.Add(graphic);
            _drawObject.IsEnabled = false;

            ProcessingTextBlock.Visibility = Visibility.Visible;
            _processingTimer.Start();

            List<GPParameter> parameters = new List<GPParameter>();

            var strLayerList = new List<GPString>();
            foreach (var itm in LayersList.SelectedItems)
            {
                strLayerList.Add(new GPString(itm.ToString(), itm.ToString()));
            }
            parameters.Add(new GPMultiValue<GPString>("Layers_to_Clip", strLayerList));
            parameters.Add(new GPFeatureRecordSetLayer("Area_of_Interest", _graphicsLayer.Graphics[0].Geometry));
            parameters.Add(new GPString("Feature_Format", Formats.SelectedValue.ToString()));

            _geoprocessorTask.SubmitJobAsync(parameters);
        }

      
        void _geoprocessorTask_JobCompleted(object sender, JobInfoEventArgs e)
        {
            if (e.JobInfo.JobStatus != esriJobStatus.esriJobSucceeded)
            {
                MessageBox.Show("Extract Data task failed to complete");
                return;
            }
            _geoprocessorTask.GetResultDataAsync(e.JobInfo.JobId, "Output_Zip_File");
        }

        void _geoprocessorTask_GetResultDataCompleted(object sender, GPParameterEventArgs ev1)
        {
            if (ev1.Parameter is GPDataFile)
            {
                GPDataFile ClipResultFile = ev1.Parameter as GPDataFile;

                if (String.IsNullOrEmpty(ClipResultFile.Url))
                    return;

                MessageBoxResult res = MessageBox.Show("Data file created. Would you like to download the file?", "Geoprocessing Task Success", MessageBoxButton.OKCancel);
                if (res == MessageBoxResult.OK)
                {
                    WebClient webClient = new WebClient();
                    webClient.OpenReadCompleted += (s, ev) =>
                    {
                        _streamedDataFile = ev.Result;
                        SaveFileButton.Visibility = Visibility.Visible;
                        ProcessingTextBlock.Text = "Download completed.  Click on 'Save data file' button to save to disk.";
                    };
                    webClient.OpenReadAsync(new Uri((ev1.Parameter as GPDataFile).Url), UriKind.Absolute);
                }
                else
                {
                    ProcessingTextBlock.Text = "";
                    ProcessingTextBlock.Visibility = Visibility.Collapsed;
                    SaveFileButton.Visibility = Visibility.Collapsed;
                }
            }
            _processingTimer.Stop();
        }

        void _geoprocessorTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Extract data task failed: " + e.Error.Message);
        }

        private void ResetButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            (MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer).ClearGraphics();
            ProcessingTextBlock.Text = "";
            ProcessingTextBlock.Visibility = Visibility.Collapsed;
            SaveFileButton.Visibility = Visibility.Collapsed;                    
        }

        void ProcessingTimer_Tick(object sender, EventArgs e)
        {
            if (ProcessingTextBlock.Text.Length > 20 || !ProcessingTextBlock.Text.StartsWith("Processing"))
                ProcessingTextBlock.Text = "Processing.";
            else
                ProcessingTextBlock.Text = ProcessingTextBlock.Text + ".";
        }

        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultFileName = "Output.zip";
            dialog.Filter = "Zip file (*.zip)|*.zip";

            bool? dialogResult = dialog.ShowDialog();

            if (dialogResult != true) return;
            try
            {
                using (Stream fs = (Stream)dialog.OpenFile())
                {
                    _streamedDataFile.CopyTo(fs);
                    fs.Flush();
                    fs.Close();
                    ProcessingTextBlock.Text = "Output file saved successfully!";
                    SaveFileButton.Visibility = Visibility.Collapsed;
                    _streamedDataFile = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving file :" + ex.Message);
            }
        }
    }
}
