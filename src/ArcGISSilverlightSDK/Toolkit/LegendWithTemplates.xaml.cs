using System.Windows.Controls;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Toolkit.Primitives;

namespace ArcGISSilverlightSDK
{
    public partial class LegendWithTemplates : UserControl
    {
        public LegendWithTemplates()
        {
            InitializeComponent();
        }

        private void Legend_Refreshed(object sender, Legend.RefreshedEventArgs e)
        {
            LayerItemViewModel removeLayerItemVM = null;

            // If a map layer has sublayers, iterate through them.
            if (e.LayerItem.LayerItems != null)
            {
                // Iterate through all the sublayer items.
                foreach (LayerItemViewModel layerItemVM in e.LayerItem.LayerItems)
                {
                    // Collapse all sublayers in the legend.
                    if (layerItemVM.IsExpanded)
                        layerItemVM.IsExpanded = false;

                    // Remove the sublayer named "states" from the legend.  The layer remains visible in the map.
                    if (layerItemVM.Label == "states")
                        removeLayerItemVM = layerItemVM;
                }

                if (removeLayerItemVM != null)
                    e.LayerItem.LayerItems.Remove(removeLayerItemVM);
            }
            else
            {
                // Collapse all map layers in the legend.
                e.LayerItem.IsExpanded = false;
            }
        }
    }
}
