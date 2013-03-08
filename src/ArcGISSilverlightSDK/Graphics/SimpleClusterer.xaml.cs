using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
  public partial class SimpleClusterer : UserControl
  {
    public SimpleClusterer()
    {
      InitializeComponent();
    }

    void MyMap_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if ((e.PropertyName == "SpatialReference") &&
          ((sender as ESRI.ArcGIS.Client.Map).SpatialReference != null))
      {
        LoadGraphics();
        MyMap.PropertyChanged -= MyMap_PropertyChanged;
      }
    }

    private void LoadGraphics()
    {
      QueryTask queryTask =
          new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/0");
      queryTask.ExecuteCompleted += queryTask_ExecuteCompleted;

      Query query = new ESRI.ArcGIS.Client.Tasks.Query()
      {
        OutSpatialReference = MyMap.SpatialReference,
        ReturnGeometry = true,
        Where = "1=1"
      };

      query.OutFields.Add("CITY_NAME");

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
        graphicsLayer.Graphics.Add(graphic);
    }
  }
}
