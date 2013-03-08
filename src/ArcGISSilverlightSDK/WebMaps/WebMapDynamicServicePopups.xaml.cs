using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.WebMap;

namespace ArcGISSilverlightSDK
{
    public partial class WebMapDynamicServicePopups : UserControl
    {
        public WebMapDynamicServicePopups()
        {
            InitializeComponent();
            Document webMap = new Document();
            webMap.GetMapCompleted += webMap_GetMapCompleted;

            webMap.GetMapAsync("fd7fb514579f4422ab2698f47a7d4a46");
        }

        void webMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error == null)
            {               
                MyMap.Extent = e.Map.Extent;

                LayerCollection layerCollection = new LayerCollection();
                foreach (Layer layer in e.Map.Layers)
                    layerCollection.Add(layer);
             
                e.Map.Layers.Clear();
                MyMap.Layers = layerCollection;
            }
        }        

        private void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {
            MyInfoWindow.IsOpen = false;

            double mapScale = MyMap.Scale;

            ArcGISDynamicMapServiceLayer alayer = null;
            DataTemplate dt = null;
            int layid = 0;

            foreach (Layer layer in MyMap.Layers)
            {
                if (layer.GetValue(Document.PopupTemplatesProperty) != null)
                {
                    alayer = layer as ArcGISDynamicMapServiceLayer;
                    IDictionary<int, DataTemplate> idict = layer.GetValue(Document.PopupTemplatesProperty) as IDictionary<int, DataTemplate>;
                    foreach (LayerInfo linfo in alayer.Layers)
                    {
                        if (((mapScale > linfo.MaxScale // in scale range
                            && mapScale < linfo.MinScale) ||
                            (linfo.MaxScale == 0.0 // no scale dependency
                            && linfo.MinScale == 0.0) ||
                            (mapScale > linfo.MaxScale // minscale = 0.0 = infinity
                            && linfo.MinScale == 0.0)) &&
                            idict.ContainsKey(linfo.ID)) // id present in dictionary
                        {
                            layid = linfo.ID;
                            dt = idict[linfo.ID];
                            break;
                        }
                    }
                }
            }

            if (dt != null)
            {
                QueryTask qt = new QueryTask(string.Format("{0}/{1}", alayer.Url, layid));
                qt.ExecuteCompleted += (s, qe) =>
                {
                    if (qe.FeatureSet.Features.Count > 0)
                    {
                        Graphic g = qe.FeatureSet.Features[0];
                        MyInfoWindow.Anchor = e.MapPoint;
                        MyInfoWindow.ContentTemplate = dt;
                        MyInfoWindow.Content = g.Attributes;
                        MyInfoWindow.IsOpen = true;
                    }
                };

                ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query();
                double contractRatio = MyMap.Extent.Width / 20;
                Envelope inputEnvelope = new Envelope(e.MapPoint.X - contractRatio,
                    e.MapPoint.Y - contractRatio,
                    e.MapPoint.X + contractRatio,
                    e.MapPoint.Y + contractRatio);
                inputEnvelope.SpatialReference = MyMap.SpatialReference;
                query.Geometry = inputEnvelope;
                query.OutSpatialReference = MyMap.SpatialReference;
                query.OutFields.Add("*");

                qt.ExecuteAsync(query);
            }
        }
    }
}
