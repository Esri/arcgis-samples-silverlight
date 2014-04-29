using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Toolkit.DataSources;

namespace ArcGISSilverlightSDK
{
  public partial class OSMTileServers : UserControl
  {
    OpenStreetMapLayer osmLayer;
    public OSMTileServers()
    {
      InitializeComponent();
      osmLayer = MyMap.Layers["OSMLayer"] as OpenStreetMapLayer;
    }
    private void RadioButton_Click(object sender, RoutedEventArgs e)
    {
      string layerTypeTag = (string)((RadioButton)sender).Tag;
      OpenStreetMapLayer.TileServerList newTileServers;

      switch (layerTypeTag)
      {
        case "MapQuest":
          //Collection is replaced
          newTileServers = new OpenStreetMapLayer.TileServerList();
          newTileServers.Add("https://otile1.mqcdn.com/tiles/1.0.0/osm");
          newTileServers.Add("https://otile2.mqcdn.com/tiles/1.0.0/osm");
          newTileServers.Add("https://otile3.mqcdn.com/tiles/1.0.0/osm");
          osmLayer.TileServers = newTileServers;
          break;
        case "Cloudmade":
          //Collection is replaced
          newTileServers = new OpenStreetMapLayer.TileServerList();
          newTileServers.Add("https://a.tile.cloudmade.com/fd093e52f0965d46bb1c6c6281022199/3/256");
          newTileServers.Add("https://b.tile.cloudmade.com/fd093e52f0965d46bb1c6c6281022199/3/256");
          newTileServers.Add("https://c.tile.cloudmade.com/fd093e52f0965d46bb1c6c6281022199/3/256");
          osmLayer.TileServers = newTileServers;
          break;
        case "CycleMap":
          //same collection is pre-populated and hence needs to be refreshed.
          osmLayer.TileServers.Clear();
          //osmLayer.TileServers.Add("https://a.tile.opencyclemap.org/cycle");
          //osmLayer.TileServers.Add("https://b.tile.opencyclemap.org/cycle");
          //osmLayer.TileServers.Add("https://c.tile.opencyclemap.org/cycle");
          osmLayer.Style = OpenStreetMapLayer.MapStyle.CycleMap;
          osmLayer.Refresh();
          break;
      }
    }
  }
}
