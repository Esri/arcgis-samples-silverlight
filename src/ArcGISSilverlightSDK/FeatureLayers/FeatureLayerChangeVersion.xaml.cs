using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client;

namespace ArcGISSilverlightSDK
{
  public partial class FeatureLayerChangeVersion : UserControl
  {
    FeatureLayer Fl;
    public FeatureLayerChangeVersion()
    {
      InitializeComponent();
      Geoprocessor gp_ListVersions = new Geoprocessor("http://sampleserver6.arcgisonline.com/arcgis/rest/services/GDBVersions/GPServer/ListVersions");

      gp_ListVersions.Failed += (s, a) =>
      {
        MessageBox.Show("Geoprocessing service failed: " + a.Error);
      };

      gp_ListVersions.ExecuteCompleted += (c, d) =>
      {
        VersionsCombo.DataContext = (d.Results.OutParameters[0] as GPRecordSet).FeatureSet;
        VersionsCombo.SelectedIndex = 0;
      };

      List<GPParameter> gpparams = new List<GPParameter>();
      gpparams.Add(new GPRecordSet("Versions", new FeatureSet()));
      gp_ListVersions.ExecuteAsync(gpparams);
    }

    private void VersionsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      Fl = (MyMap.Layers["ServiceConnections"] as FeatureLayer);
      Fl.GdbVersion = (e.AddedItems[0] as Graphic).Attributes["name"].ToString();
      Fl.Update();
    }
  }
}
