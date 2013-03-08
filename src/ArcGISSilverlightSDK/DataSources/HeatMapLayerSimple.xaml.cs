using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client.Toolkit.DataSources;

namespace ArcGISSilverlightSDK
{
    public partial class HeatMapLayerSimple : UserControl
    {
        public HeatMapLayerSimple()
        {
            InitializeComponent();
            MyMap.Layers.LayersInitialized += Layers_LayersInitialized;
        }

        void Layers_LayersInitialized(object sender, EventArgs args)
        {
            //Add 1000 random points to the heat map layer
            //Replace this with "real" data points that you want to display
            //in the heat map.
            HeatMapLayer layer = MyMap.Layers["RandomHeatMapLayer"] as HeatMapLayer;
            Random rand = new Random();
            for (int i = 0; i < 1000; i++)
            {
                double x = rand.NextDouble() * MyMap.Extent.Width - MyMap.Extent.Width/2;
                double y = rand.NextDouble() * MyMap.Extent.Height - MyMap.Extent.Height/2;
                layer.HeatMapPoints.Add(new ESRI.ArcGIS.Client.Geometry.MapPoint(x, y));
            }
        }
    }
}
