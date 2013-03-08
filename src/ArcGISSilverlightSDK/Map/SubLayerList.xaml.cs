using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ArcGISSilverlightSDK
{
    public partial class SubLayerList : UserControl
    {
        public SubLayerList()
        {
            InitializeComponent();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox tickedCheckBox = sender as CheckBox;

            string serviceName = tickedCheckBox.Name;
            bool visible = (bool)tickedCheckBox.IsChecked;

            int layerIndex = (int)tickedCheckBox.Tag;

            ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer dynamicServiceLayer = MyMap.Layers[serviceName] as
                ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer;

            List<int> visibleLayerList =
                dynamicServiceLayer.VisibleLayers != null
                ? dynamicServiceLayer.VisibleLayers.ToList() : new List<int>();

            if (visible)
            {
                if (!visibleLayerList.Contains(layerIndex))
                    visibleLayerList.Add(layerIndex);
            }
            else
            {
                if (visibleLayerList.Contains(layerIndex))
                    visibleLayerList.Remove(layerIndex);
            }
            		
		    dynamicServiceLayer.VisibleLayers = visibleLayerList.ToArray();
        }

        private void ArcGISDynamicMapServiceLayer_Initialized(object sender, EventArgs e)
        {
            ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer dynamicServiceLayer =
                sender as ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer;
            if (dynamicServiceLayer.VisibleLayers == null)
                dynamicServiceLayer.VisibleLayers = GetDefaultVisibleLayers(dynamicServiceLayer);
        }

        private int[] GetDefaultVisibleLayers(ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer dynamicService)
        {
            List<int> visibleLayerIDList = new List<int>();

            ESRI.ArcGIS.Client.LayerInfo[] layerInfoArray = dynamicService.Layers;

            for (int index = 0; index < layerInfoArray.Length; index++)
            {
                if (layerInfoArray[index].DefaultVisibility)
                    visibleLayerIDList.Add(index);
            }
            return visibleLayerIDList.ToArray();
        }
    }
}
