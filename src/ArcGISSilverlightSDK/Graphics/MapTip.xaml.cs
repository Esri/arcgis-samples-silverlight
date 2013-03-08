using System;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
  public partial class GraphicsMapTip : UserControl
  {
    public GraphicsMapTip()
    {
      InitializeComponent();

      MyMap.PropertyChanged += MyMap_PropertyChanged;
    }

    void MyMap_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "SpatialReference")
      {
        StatesGraphicsLayerLoad();
        CitiesGraphicsLayerLoad();
        MyMap.PropertyChanged -= MyMap_PropertyChanged;
      }
    }

    void StatesGraphicsLayerLoad()
    {
      ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query()
      {
        Geometry = new ESRI.ArcGIS.Client.Geometry.Envelope(-180, 0, 0, 90) { SpatialReference = new ESRI.ArcGIS.Client.Geometry.SpatialReference(4326) },               
        ReturnGeometry = true,
        OutSpatialReference = MyMap.SpatialReference
      };
      query.OutFields.Add("*");


      QueryTask queryTask = new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5");
      queryTask.ExecuteCompleted += StatesGraphicsLayerQueryTask_ExecuteCompleted;
      queryTask.ExecuteAsync(query);
    }

    void StatesGraphicsLayerQueryTask_ExecuteCompleted(object sender, ESRI.ArcGIS.Client.Tasks.QueryEventArgs queryArgs)
    {
      if (queryArgs.FeatureSet == null)
        return;

      FeatureSet resultFeatureSet = queryArgs.FeatureSet;
      ESRI.ArcGIS.Client.GraphicsLayer graphicsLayer =
      MyMap.Layers["StatesGraphicsLayer"] as ESRI.ArcGIS.Client.GraphicsLayer;

      if (resultFeatureSet != null && resultFeatureSet.Features.Count > 0)
      {
        foreach (ESRI.ArcGIS.Client.Graphic graphicFeature in resultFeatureSet.Features)
        {
          graphicFeature.Symbol = LayoutRoot.Resources["DefaultFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
          graphicsLayer.Graphics.Add(graphicFeature);
        }
      }
    }

    private void CitiesGraphicsLayerLoad()
    {
      ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query()
      {
        Geometry = new ESRI.ArcGIS.Client.Geometry.Envelope(-180, 0, 0, 90) { SpatialReference = new ESRI.ArcGIS.Client.Geometry.SpatialReference(4326) },
        ReturnGeometry = true,
        OutSpatialReference = MyMap.SpatialReference
      };
      query.OutFields.Add("*");
      query.Where = "POP1990 > 100000";
      QueryTask queryTask = new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/0");
      queryTask.ExecuteCompleted += CitiesGraphicsLayerQueryTask_ExecuteCompleted;
      queryTask.ExecuteAsync(query);
    }

    void CitiesGraphicsLayerQueryTask_ExecuteCompleted(object sender, QueryEventArgs queryArgs)
    {
      if (queryArgs.FeatureSet == null)
        return;

      FeatureSet resultFeatureSet = queryArgs.FeatureSet;
      ESRI.ArcGIS.Client.GraphicsLayer graphicsLayer =
      MyMap.Layers["CitiesGraphicsLayer"] as ESRI.ArcGIS.Client.GraphicsLayer;

      if (resultFeatureSet != null && resultFeatureSet.Features.Count > 0)
      {
        foreach (ESRI.ArcGIS.Client.Graphic graphicFeature in resultFeatureSet.Features)
        {
          graphicFeature.Symbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
          graphicsLayer.Graphics.Add(graphicFeature);
        }
      }
    }
  }
}
