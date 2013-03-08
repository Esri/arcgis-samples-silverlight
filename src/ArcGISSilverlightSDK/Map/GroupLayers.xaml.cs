using System.Windows.Controls;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Toolkit.Primitives;

namespace ArcGISSilverlightSDK
{
  public partial class GroupLayers : UserControl
  {
    public GroupLayers()
    {
      InitializeComponent();
    }

    private void Legend_Refreshed(object sender, Legend.RefreshedEventArgs e)
    {
      if (e.LayerItem.LayerItems != null)
        foreach (LayerItemViewModel layerItemVM in e.LayerItem.LayerItems)
          if (layerItemVM.IsExpanded)
            layerItemVM.IsExpanded = false;
    }
  }
}
