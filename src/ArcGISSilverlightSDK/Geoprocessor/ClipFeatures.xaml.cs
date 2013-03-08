using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
  public partial class ClipFeatures : UserControl
  {
    private DispatcherTimer _processingTimer;
    private Draw MyDrawObject;

    public ClipFeatures()
    {
      InitializeComponent();

      _processingTimer = new System.Windows.Threading.DispatcherTimer();
      _processingTimer.Interval = new TimeSpan(0, 0, 0, 0, 800);
      _processingTimer.Tick += ProcessingTimer_Tick;

      MyDrawObject = new Draw(MyMap)
      {
        DrawMode = DrawMode.Polyline,
        IsEnabled = true,
        LineSymbol = LayoutRoot.Resources["ClipLineSymbol"] as ESRI.ArcGIS.Client.Symbols.LineSymbol
      };
      MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;
    }

    private void MyDrawObject_DrawComplete(object sender, DrawEventArgs args)
    {
      MyDrawObject.IsEnabled = false;

      ProcessingTextBlock.Visibility = Visibility.Visible;
      _processingTimer.Start();

      GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;

      Graphic graphic = new Graphic()
      {
        Symbol = LayoutRoot.Resources["ClipLineSymbol"] as ESRI.ArcGIS.Client.Symbols.LineSymbol,
        Geometry = args.Geometry
      };
      graphicsLayer.Graphics.Add(graphic);

      Geoprocessor geoprocessorTask = new Geoprocessor("http://serverapps10.esri.com/ArcGIS/rest/services/" +
          "SamplesNET/USA_Data_ClipTools/GPServer/ClipCounties");

      //geoprocessorTask.ProxyURL = "http://servicesbeta3.esri.com/SilverlightDemos/ProxyPage/proxy.ashx";

      geoprocessorTask.UpdateDelay = 5000;
      geoprocessorTask.JobCompleted += GeoprocessorTask_JobCompleted;

      List<GPParameter> parameters = new List<GPParameter>();
      parameters.Add(new GPFeatureRecordSetLayer("Input_Features", args.Geometry));
      parameters.Add(new GPLinearUnit("Linear_unit", esriUnits.esriMiles, Int32.Parse(DistanceTextBox.Text)));

      geoprocessorTask.SubmitJobAsync(parameters);
    }

    private void GeoprocessorTask_JobCompleted(object sender, JobInfoEventArgs e)
    {
      Geoprocessor geoprocessorTask = sender as Geoprocessor;

      geoprocessorTask.GetResultDataCompleted += (s1, ev1) =>
      {
        GraphicsLayer graphicsLayer = MyMap.Layers["MyResultGraphicsLayer"] as GraphicsLayer;

        if (ev1.Parameter is GPFeatureRecordSetLayer)
        {
          GPFeatureRecordSetLayer gpLayer = ev1.Parameter as GPFeatureRecordSetLayer;
          if (gpLayer.FeatureSet.Features.Count == 0)
          {
            geoprocessorTask.GetResultImageLayerCompleted += (s2, ev2) =>
            {
              GPResultImageLayer gpImageLayer = ev2.GPResultImageLayer;
              gpImageLayer.Opacity = 0.5;
              MyMap.Layers.Add(gpImageLayer);

              ProcessingTextBlock.Text = "Greater than 500 features returned.  Results drawn using map service.";
              _processingTimer.Stop();
            };

            geoprocessorTask.GetResultImageLayerAsync(e.JobInfo.JobId, "Clipped_Counties");
            return;
          }

          foreach (Graphic graphic in gpLayer.FeatureSet.Features)
          {
            graphic.Symbol = LayoutRoot.Resources["ClipFeaturesFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
            graphicsLayer.Graphics.Add(graphic);
          }
        }

        ProcessingTextBlock.Visibility = Visibility.Collapsed;
        _processingTimer.Stop();
      };

      geoprocessorTask.GetResultDataAsync(e.JobInfo.JobId, "Clipped_Counties");
    }

    private void GeoprocessorTask_Failed(object sender, TaskFailedEventArgs e)
    {
      MessageBox.Show("Geoprocessor service failed: " + e.Error);
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
      List<Layer> gpResultImageLayers = new List<Layer>();

      foreach (Layer layer in MyMap.Layers)
        if (layer is GraphicsLayer)
          (layer as GraphicsLayer).ClearGraphics();
        else if (layer is GPResultImageLayer)
          gpResultImageLayers.Add(layer);
      for (int i = 0; i < gpResultImageLayers.Count; i++)
        MyMap.Layers.Remove(gpResultImageLayers[i]);

      MyDrawObject.IsEnabled = true;

      ProcessingTextBlock.Text = "";
      ProcessingTextBlock.Visibility = Visibility.Collapsed;
    }

    void ProcessingTimer_Tick(object sender, EventArgs e)
    {
      if (ProcessingTextBlock.Text.Length > 20 || !ProcessingTextBlock.Text.StartsWith("Processing"))
        ProcessingTextBlock.Text = "Processing.";
      else
        ProcessingTextBlock.Text = ProcessingTextBlock.Text + ".";
    }
  }
}
