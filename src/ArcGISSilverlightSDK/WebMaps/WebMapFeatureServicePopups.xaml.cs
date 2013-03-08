using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.WebMap;

namespace ArcGISSilverlightSDK
{
    public partial class WebMapFeatureServicePopups : UserControl
    {
        Dictionary<string, FrameworkElement> mapTipsElements = new Dictionary<string, FrameworkElement>();
        MapPoint lastPoint = null;

        public WebMapFeatureServicePopups()
        {
            InitializeComponent();
            Document webMap = new Document();
            webMap.GetMapCompleted += webMap_GetMapCompleted;

            webMap.GetMapAsync("921e9016d2a5423da8bd08eb306e4e0e");
        }

        void webMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                MyMap.Extent = e.Map.Extent;
                int i = 0;

                LayerCollection layerCollection = new LayerCollection();
                foreach (Layer layer in e.Map.Layers)
                {
                    layer.ID = i.ToString();
                    if (layer is FeatureLayer)
                    {
                        mapTipsElements.Add(layer.ID, (layer as FeatureLayer).MapTip);
                    }
                    layerCollection.Add(layer);
                    i++;
                }

                e.Map.Layers.Clear();
                MyMap.Layers = layerCollection;
            }
        }

        private void MapTipRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (MyMap != null)
            {
                MyInfoWindow.IsOpen = false;
                foreach (Layer layer in MyMap.Layers)
                {
                    if (layer is FeatureLayer)
                        (layer as FeatureLayer).MouseLeftButtonUp -= WebMapFeatureServicePopups_MouseLeftButtonUp;
                    if (mapTipsElements.ContainsKey(layer.ID))
                        (layer as FeatureLayer).MapTip = mapTipsElements[layer.ID];
                }
            }            
        }

        private void InfoWindowRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (Layer layer in MyMap.Layers)
                if (layer is FeatureLayer)
                {
                    (layer as FeatureLayer).MapTip = null;
                    (layer as FeatureLayer).MouseLeftButtonUp += WebMapFeatureServicePopups_MouseLeftButtonUp;
                }
        }

        void WebMapFeatureServicePopups_MouseLeftButtonUp(object sender, GraphicMouseButtonEventArgs e)
        {
            FeatureLayer flayer = sender as FeatureLayer;
            MapPoint clickPoint = MyMap.ScreenToMap(e.GetPosition(MyMap));

            if (clickPoint != lastPoint)
            {
                if (flayer.GetValue(Document.PopupTemplateProperty) != null)
                {
                    DataTemplate dt = flayer.GetValue(Document.PopupTemplateProperty) as DataTemplate;

                    MyInfoWindow.Anchor = clickPoint;
                    MyInfoWindow.ContentTemplate = dt;
                    MyInfoWindow.Content = e.Graphic.Attributes;
                    MyInfoWindow.IsOpen = true;
                    lastPoint = clickPoint;
                }
            }
        }
    }
}
