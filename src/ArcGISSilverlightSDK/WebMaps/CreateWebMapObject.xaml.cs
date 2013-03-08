using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.WebMap;
using System.Collections.Generic;
using System.Collections;
using ESRI.ArcGIS.Client.Tasks;
using System.Windows;

namespace ArcGISSilverlightSDK
{
    public partial class CreateWebMapObject : UserControl
    {
        WebMap webmap;
        List<WebMapLayer> operationLayers = new List<WebMapLayer>();
        BaseMap basemap;

        public CreateWebMapObject()
        {
            InitializeComponent();

            //Define BaseMap Layer
            basemap = new BaseMap()
           {
               Layers = new List<WebMapLayer> { new WebMapLayer { Url = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer" } }
           };

            //Add a ArcGISDynamicMapService 
            operationLayers.Add(new WebMapLayer
            {
                Url = "http://serverapps10.esri.com/ArcGIS/rest/services/California/MapServer",
                VisibleLayers = new List<object> { 0, 1, 3, 6, 9 }
            });

            //Define popup
            IList<FieldInfo> fieldinfos = new List<FieldInfo>();
            fieldinfos.Add(new FieldInfo() { FieldName = "STATE_NAME", Label = "State", Visible = true });

            IList<MediaInfo> mediainfos = new List<MediaInfo>();
            MediaInfoValue infovalue = new MediaInfoValue();
            infovalue.Fields = new string[] { "POP2000,POP2007" };
            mediainfos.Add(new MediaInfo() { Type = MediaType.PieChart, Value = infovalue });

            PopupInfo popup = new PopupInfo() { FieldInfos = fieldinfos, MediaInfos = mediainfos, Title = "Population Change between 2000 and 2007", };

            //Add a Feature Layer with popup
            operationLayers.Add(new WebMapLayer
            {
                Url = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3",
                Mode = FeatureLayer.QueryMode.OnDemand,
                PopupInfo = popup
            });

            //Perform Query to get a featureSet and add to webmap as featurecollection
            QueryTask qt = new QueryTask() { Url = "http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Earthquakes/EarthquakesFromLastSevenDays/MapServer/0" };
            qt.ExecuteCompleted += qt_ExecuteCompleted;

            ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query();
            query.OutFields.Add("*");
            query.Where = "magnitude > 3.5";
            query.ReturnGeometry = true;
            qt.Failed += (a, b) =>
              {
                  MessageBox.Show("QueryTask failed to execute:" + b.Error);
              };
            qt.ExecuteAsync(query);

        }

        void qt_ExecuteCompleted(object sender, QueryEventArgs e)
        {
            #region Since featureset does not include layerdefinition, we would have to populate it with appropriate drawinginfo

            Dictionary<string, object> layerdef = new Dictionary<string, object>();
            Dictionary<string, object> defdictionary = new Dictionary<string, object>() 
      { 
        { "id", 0 }, 
        { "name", "Earthquakes from last 7 days" } 
      };

            Dictionary<string, object> renderer = new Dictionary<string, object>();
            renderer.Add("type", "simple");
            renderer.Add("style", "esriSMSCircle");

            int[] color = new int[] { 255, 0, 0, 255 };
            renderer.Add("color", color);
            renderer.Add("size", 4);

            int[] outlinecolor = new int[] { 0, 0, 0, 255 };

            defdictionary.Add("drawingInfo", renderer);

            layerdef.Add("layerDefinition", defdictionary);
            #endregion

            //Add a FeatureCollection to operational layers
            FeatureCollection featureCollection = null;

            if (e.FeatureSet.Features.Count > 0)
            {
                var sublayer = new WebMapSubLayer();
                sublayer.FeatureSet = e.FeatureSet;

                sublayer.AddCustomProperty("layerDefinition", layerdef);
                featureCollection = new FeatureCollection { SubLayers = new List<WebMapSubLayer> { sublayer } };
            }

            if (featureCollection != null)
                operationLayers.Add(new WebMapLayer { FeatureCollection = featureCollection });


            //Create a new webmap object and add base map and operational layers
            webmap = new WebMap() { BaseMap = basemap, OperationalLayers = operationLayers };

            Document webmapdoc = new Document();
            webmapdoc.GetMapCompleted += (a, b) =>
              {
                  if (b.Error == null)
                  {
                      b.Map.Extent = new ESRI.ArcGIS.Client.Geometry.Envelope(-20000000, 1100000, -3900000, 11000000);
                      LayoutRoot.Children.Add(b.Map);
                  }
              };
            webmapdoc.GetMapAsync(webmap);
        }
    }
}