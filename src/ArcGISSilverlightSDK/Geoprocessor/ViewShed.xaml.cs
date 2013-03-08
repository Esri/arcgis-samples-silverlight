using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Toolkit.Primitives;

namespace ArcGISSilverlightSDK
{
  public partial class ViewShed : UserControl
  {
    Geoprocessor _geoprocessorTask;
    bool _displayViewshedInfo;
    ArcGISDynamicMapServiceLayer resultLayer = null;
    GraphicsLayer graphicsLayer = null;

    public ViewShed()
    {
      InitializeComponent();
      _geoprocessorTask = new Geoprocessor("http://serverapps101.esri.com/arcgis/rest/services/ProbabilisticViewshedModel/GPServer/ProbabilisticViewshedModel");
      _geoprocessorTask.JobCompleted += GeoprocessorTask_JobCompleted;
      _geoprocessorTask.Failed += GeoprocessorTask_Failed;
      graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
    }

    private void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
    {
      MapPoint mapPoint = e.MapPoint;
      if (_displayViewshedInfo && resultLayer != null)
      {

        ESRI.ArcGIS.Client.Tasks.IdentifyParameters identifyParams = new IdentifyParameters()
        {
          Geometry = mapPoint,
          MapExtent = MyMap.Extent,
          Width = (int)MyMap.ActualWidth,
          Height = (int)MyMap.ActualHeight,
          LayerOption = LayerOption.visible,
          SpatialReference = MyMap.SpatialReference
        };

        IdentifyTask identifyTask = new IdentifyTask(resultLayer.Url);
        identifyTask.ExecuteCompleted += (s, ie) =>
        {
          if (ie.IdentifyResults.Count > 0)
          {
            foreach (var identifyresult in ie.IdentifyResults)
            {
              if (identifyresult.LayerId == 1)
              {
                Graphic g = identifyresult.Feature;
                MyInfoWindow.Anchor = e.MapPoint;
                MyInfoWindow.Content = g.Attributes;
                MyInfoWindow.IsOpen = true;
                break;
              }
            }
          }
        };
        identifyTask.ExecuteAsync(identifyParams);
      }
      else
      {
        _geoprocessorTask.CancelAsync();
       
        graphicsLayer.ClearGraphics();

        mapPoint.SpatialReference = new SpatialReference(102100);

        Graphic graphic = new Graphic()
        {
          Symbol = LayoutRoot.Resources["StartMarkerSymbol"] as Symbol,
          Geometry = mapPoint
        };
        graphicsLayer.Graphics.Add(graphic);

        MyMap.Cursor = System.Windows.Input.Cursors.Wait;

        List<GPParameter> parameters = new List<GPParameter>();
        parameters.Add(new GPFeatureRecordSetLayer("Input_Features", mapPoint));
        parameters.Add(new GPString("Height", HeightTextBox.Text));
        parameters.Add(new GPLinearUnit("Distance", esriUnits.esriMiles, Convert.ToDouble(MilesTextBox.Text)));


        _geoprocessorTask.OutputSpatialReference = new SpatialReference(102100);
        _geoprocessorTask.SubmitJobAsync(parameters);
      }

    }

    private void GeoprocessorTask_JobCompleted(object sender, JobInfoEventArgs e)
    {
      MyMap.Cursor = System.Windows.Input.Cursors.Hand;
      if (e.JobInfo.JobStatus == esriJobStatus.esriJobSucceeded)
      {
        Geoprocessor geoprocessorTask = sender as Geoprocessor;

        System.Threading.Thread.Sleep(2000);

        resultLayer = geoprocessorTask.GetResultMapServiceLayer(e.JobInfo.JobId);
        resultLayer.InitializationFailed += new EventHandler<EventArgs>(resultLayer_InitializationFailed);
        resultLayer.DisplayName = e.JobInfo.JobId;
        if (resultLayer != null)
        {
          _displayViewshedInfo = true;
          MyMap.Layers.Add(resultLayer);
        }
      }
      else
      {
        MessageBox.Show("Geoprocessor service failed");
        _displayViewshedInfo = false;
      }
    }

    void resultLayer_InitializationFailed(object sender, EventArgs e)
    {
    }
    private void GeoprocessorTask_Failed(object sender, TaskFailedEventArgs e)
    {
      MyMap.Cursor = System.Windows.Input.Cursors.Hand;
      _displayViewshedInfo = false;
      MessageBox.Show("Geoprocessor service failed: " + e.Error);
    }

    private void RemoveLayers_Click(object sender, RoutedEventArgs e)
    {
      //remove all previous results
      graphicsLayer.ClearGraphics();

      int idx = MyMap.Layers.Count - 1;
      while (idx > 1)
      {
        MyMap.Layers.RemoveAt(idx);
        idx--;
      }
      _displayViewshedInfo = false;
      MyInfoWindow.IsOpen = false;
    }

  }
}
