using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class SDSMap : UserControl
    {
        private static ESRI.ArcGIS.Client.Projection.WebMercator mercator =
           new ESRI.ArcGIS.Client.Projection.WebMercator();

        public SDSMap()
        {
            InitializeComponent();

            ESRI.ArcGIS.Client.Geometry.Envelope initialExtent =
                    new ESRI.ArcGIS.Client.Geometry.Envelope(
                mercator.FromGeographic(
                    new ESRI.ArcGIS.Client.Geometry.MapPoint(-130, 20)) as MapPoint,
                mercator.FromGeographic(
                    new ESRI.ArcGIS.Client.Geometry.MapPoint(-65, 55)) as MapPoint);

            initialExtent.SpatialReference = new SpatialReference(102100);

            MyMap.Extent = initialExtent;
        }

        private void PolygonGraphicsLayer_Initialized(object sender, EventArgs e)
        {
            LoadPolygonGraphics();
        }

        private void LoadPolygonGraphics()
        {
            QueryTask queryTask =
                new QueryTask("http://servicesbeta5.esri.com/arcgis/rest/services/UnitedStates/FeatureServer/3");
            queryTask.ExecuteCompleted += queryTaskPolygon_ExecuteCompleted;

            Query query = new ESRI.ArcGIS.Client.Tasks.Query();
            query.OutSpatialReference = MyMap.SpatialReference;
            query.ReturnGeometry = true;
            query.Where = "1=1";

            query.OutFields.AddRange(new string[] { "STATE_NAME", "POP2000" });
            queryTask.ExecuteAsync(query);
        }

        void queryTaskPolygon_ExecuteCompleted(object sender, QueryEventArgs args)
        {
            FeatureSet featureSet = args.FeatureSet;

            if (featureSet == null || featureSet.Features.Count < 1)
            {
                MessageBox.Show("No features returned from query");
                return;
            }

            GraphicsLayer graphicsLayer = MyMap.Layers["MyPolygonGraphicsLayer"] as GraphicsLayer;

            Random random = new Random();

            // Random switch between class breaks and unique value renderers
            if (random.Next(0, 2) == 0)
            {
                ClassBreaksRenderer classBreakRenderer = new ClassBreaksRenderer();
                classBreakRenderer.Field = "POP2000";
                int classCount = 6;

                List<double> valueList = new List<double>();
                foreach (Graphic graphic in args.FeatureSet.Features)
                {
                    graphicsLayer.Graphics.Add(graphic);
                    valueList.Add((int)graphic.Attributes[classBreakRenderer.Field]);
                }

                // LINQ
                IEnumerable<double> valueEnumerator =
                   from aValue in valueList
                   orderby aValue
                   select aValue;

                int increment = Convert.ToInt32(Math.Ceiling(args.FeatureSet.Features.Count / classCount));
                int rgbFactor = 255 / classCount;
                int j = 255;

                for (int i = increment; i < valueList.Count; i += increment)
                {
                    ClassBreakInfo classBreakInfo = new ClassBreakInfo();

                    if (i == increment)
                        classBreakInfo.MinimumValue = 0;
                    else
                        classBreakInfo.MinimumValue = valueEnumerator.ElementAt(i - increment);

                    classBreakInfo.MaximumValue = valueEnumerator.ElementAt(i);

                    SimpleFillSymbol symbol = new SimpleFillSymbol()
                    {
                        Fill = new SolidColorBrush(Color.FromArgb(192, (byte)j, (byte)j, (byte)j)),
                        BorderBrush = new SolidColorBrush(Colors.Transparent),
                        BorderThickness = 1
                    };

                    classBreakInfo.Symbol = symbol;
                    classBreakRenderer.Classes.Add(classBreakInfo);

                    j = j - rgbFactor;
                }

                // Set maximum value for largest class break 
                classBreakRenderer.Classes[classBreakRenderer.Classes.Count - 1].MaximumValue = valueEnumerator.ElementAt(valueList.Count - 1) + 1;

                graphicsLayer.Renderer = classBreakRenderer;

            }
            else
            {
                UniqueValueRenderer uniqueValueRenderer = new UniqueValueRenderer();
                uniqueValueRenderer.DefaultSymbol = LayoutRoot.Resources["RedFillSymbol"] as Symbol;
                uniqueValueRenderer.Field = "STATE_NAME";

                foreach (Graphic graphic in args.FeatureSet.Features)
                {
                    graphicsLayer.Graphics.Add(graphic);
                    UniqueValueInfo uniqueValueInfo = new UniqueValueInfo();

                    SimpleFillSymbol symbol = new SimpleFillSymbol()
                    {
                        Fill = new SolidColorBrush(Color.FromArgb(192, (byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255))),
                        BorderBrush = new SolidColorBrush(Colors.Transparent),
                        BorderThickness = 1
                    };

                    uniqueValueInfo.Symbol = symbol;
                    uniqueValueInfo.Value = graphic.Attributes["STATE_NAME"];
                    uniqueValueRenderer.Infos.Add(uniqueValueInfo);
                }

                graphicsLayer.Renderer = uniqueValueRenderer;
            }
        }

        private void PointGraphicsLayer_Initialized(object sender, EventArgs e)
        {
            LoadPointGraphics(0, 1000);
        }

        private void LoadPointGraphics(int minLimitRange, int maxLimitRange)
        {
            QueryTask queryTask =
                new QueryTask("http://servicesbeta5.esri.com/arcgis/rest/services/UnitedStates/FeatureServer/0");
            queryTask.ExecuteCompleted += queryTaskPoint_ExecuteCompleted;
            queryTask.Failed += queryTask_Failed;

            Query query = new ESRI.ArcGIS.Client.Tasks.Query();
            query.OutSpatialReference = MyMap.SpatialReference;
            query.ReturnGeometry = true;
            query.OutFields.AddRange(new string[] { "POP2000", "AREANAME" });

            query.Where = string.Format("(OBJECTID >= {0}) AND (OBJECTID <= {1})",
                    minLimitRange, maxLimitRange);

            queryTask.ExecuteAsync(query);
        }

        void queryTaskPoint_ExecuteCompleted(object sender, QueryEventArgs args)
        {
            FeatureSet featureSet = args.FeatureSet;

            if (featureSet == null || featureSet.Features.Count < 1)
            {
                MessageBox.Show("No features returned from query");
                return;
            }

            GraphicsLayer graphicsLayer = MyMap.Layers["MyPointGraphicsLayer"] as GraphicsLayer;

            foreach (Graphic graphic in args.FeatureSet.Features)
            {
                graphic.Symbol = LayoutRoot.Resources["YellowMarkerSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                graphicsLayer.Graphics.Add(graphic);
            }

            if (featureSet.Features.Count == 1000)
            {
                DispatcherTimer UpdateTimer = new System.Windows.Threading.DispatcherTimer();
                UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
                UpdateTimer.Tick += (evtsender, a) =>
                {
                    LoadPointGraphics(graphicsLayer.Graphics.Count, graphicsLayer.Graphics.Count + 1000);
                    UpdateTimer.Stop();                    
                };
                UpdateTimer.Start();                
            }
        }

        void queryTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Query failed: " + e.Error);
        }
    }
}
