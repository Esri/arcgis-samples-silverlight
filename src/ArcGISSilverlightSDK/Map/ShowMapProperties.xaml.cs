using System.Text;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;

namespace ArcGISSilverlightSDK
{
    public partial class ShowMapProperties : UserControl
    {
        public ShowMapProperties()
        {
            InitializeComponent();

            MyMap.Layers.LayersInitialized += Layers_LayersInitialized;
        }

        void Layers_LayersInitialized(object sender, System.EventArgs args)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("Spatial Reference: {0}", MyMap.SpatialReference.WKT != null ? MyMap.SpatialReference.WKT : MyMap.SpatialReference.WKID.ToString()));
            sb.AppendLine(string.Format("Minimum Resolution: {0}", MyMap.MinimumResolution));
            sb.AppendLine(string.Format("Maximum Resolution: {0}", MyMap.MaximumResolution));
            sb.AppendLine(string.Format("Width (pixels): {0}", MyMap.ActualWidth));
            sb.AppendLine(string.Format("Height (pixels): {0}", MyMap.ActualHeight));
            sb.AppendLine();
            sb.AppendLine(string.Format("---Map Layers ({0})---", MyMap.Layers.Count));
            sb.AppendLine();

            foreach (Layer layer in MyMap.Layers)
            {
                sb.AppendLine(string.Format("ID: {0}", layer.ID));
                sb.AppendLine(string.Format("Type: {0}", layer.GetType().ToString()));
                sb.AppendLine(string.Format("Visibility : {0}", layer.Visible));
                sb.AppendLine(string.Format("Opacity : {0}", layer.Opacity));
                if (layer is ArcGISDynamicMapServiceLayer)
                {
                    ArcGISDynamicMapServiceLayer dynLayer = layer as ArcGISDynamicMapServiceLayer;
                    sb.AppendLine(string.Format("\t---Layers ({0})---", dynLayer.Layers.Length));
                    foreach (LayerInfo layerinfo in dynLayer.Layers)
                    {
                        sb.AppendLine(string.Format("\tID: {0}", layerinfo.ID));
                        sb.AppendLine(string.Format("\tName: {0}", layerinfo.Name));
                        sb.AppendLine(string.Format("\tDefault Visibility: {0}", layerinfo.DefaultVisibility));

                        sb.AppendLine(string.Format("\tMinimum Scale: {0}", layerinfo.MinScale));
                        sb.AppendLine(string.Format("\tMaximum Scale: {0}", layerinfo.MaxScale));
                        if (layerinfo.SubLayerIds != null)
                            sb.AppendLine(string.Format("\tSubLayer IDs: {0}", layerinfo.SubLayerIds.ToString()));
                        sb.AppendLine();
                    }
                }
                if (layer is ArcGISTiledMapServiceLayer)
                {
                    ArcGISTiledMapServiceLayer tiledLayer = layer as ArcGISTiledMapServiceLayer;
                    TileInfo ti = tiledLayer.TileInfo;
                    sb.AppendLine("Levels and Resolution :");
                    for (int i = 0; i < ti.Lods.Length; i++)
                    {
                        if (i < 10)
                            sb.Append(string.Format("Level: {0} \t \tResolution: {1}\r", i, ti.Lods[i].Resolution));
                        else
                            sb.Append(string.Format("Level: {0} \tResolution: {1}\r", i, ti.Lods[i].Resolution));

                    }
                }
                sb.AppendLine();
            }

            PropertiesTextBlock.Text = sb.ToString();
        }
    }
}
