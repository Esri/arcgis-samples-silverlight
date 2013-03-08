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
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISSilverlightSDK
{
    public partial class DynamicLayer : UserControl
    {
        public DynamicLayer()
        {
            InitializeComponent();          
        }

        private void ApplyRangeValueClick(object sender, RoutedEventArgs e)
        {
            ClassBreaksRenderer newClassBreaksRenderer = new ClassBreaksRenderer();
            newClassBreaksRenderer.Field = "POP00_SQMI";

            newClassBreaksRenderer.Classes.Add(new ClassBreakInfo()
            {
                MinimumValue = 0,
                MaximumValue = 12,
                Symbol = new SimpleFillSymbol()
                {
                    Fill = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0))
                }
            });

            newClassBreaksRenderer.Classes.Add(new ClassBreakInfo()
            {
                MaximumValue = 31.3,
                Symbol = new SimpleFillSymbol()
                {
                    Fill = new SolidColorBrush(Color.FromArgb(255, 100, 255, 100))
                }
            });

            newClassBreaksRenderer.Classes.Add(new ClassBreakInfo()
            {
                MaximumValue = 59.7,
                Symbol = new SimpleFillSymbol()
                {
                    Fill = new SolidColorBrush(Color.FromArgb(255, 0, 255, 200))
                }
            });

            newClassBreaksRenderer.Classes.Add(new ClassBreakInfo()
            {
                MaximumValue = 146.2,
                Symbol = new SimpleFillSymbol()
                {
                    Fill = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
                }
            });

            newClassBreaksRenderer.Classes.Add(new ClassBreakInfo()
            {
                MaximumValue = 57173,
                Symbol = new SimpleFillSymbol()
                {
                    Fill = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255))
                }
            });

            LayerDrawingOptions layerDrawOptions = new LayerDrawingOptions();
            layerDrawOptions.LayerID = 3;
            layerDrawOptions.Renderer = newClassBreaksRenderer;

            (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).LayerDrawingOptions =
                new LayerDrawingOptionsCollection() { layerDrawOptions };
            (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).VisibleLayers = new int[] { 3 };
            // Changing VisibleLayers will refresh the layer, otherwise an explicit call to Refresh is needed.
            //(MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).Refresh();
        }

        private void ApplyUniqueValueClick(object sender, RoutedEventArgs e)
        {
            UniqueValueRenderer newUniqueValueRenderer = new UniqueValueRenderer()
            {
                DefaultSymbol = new SimpleFillSymbol()
                {
                    Fill = new SolidColorBrush(Colors.Gray)
                },
                Field = "SUB_REGION"
            };

            newUniqueValueRenderer.Infos.Add(new UniqueValueInfo()
            {
                Value = "Pacific",
                Symbol = new SimpleFillSymbol()
                {
                    Fill = new SolidColorBrush(Colors.Purple),
                    BorderBrush = new SolidColorBrush(Colors.Transparent)
                }
            });

            newUniqueValueRenderer.Infos.Add(new UniqueValueInfo()
            {
                Value = "W N Cen",
                Symbol = new SimpleFillSymbol()
                {
                    Fill = new SolidColorBrush(Colors.Green),
                    BorderBrush = new SolidColorBrush(Colors.Transparent)
                }
            });

            newUniqueValueRenderer.Infos.Add(new UniqueValueInfo()
            {
                Value = "W S Cen",
                Symbol = new SimpleFillSymbol()
                {
                    Fill = new SolidColorBrush(Colors.Cyan),
                    BorderBrush = new SolidColorBrush(Colors.Transparent)
                }
            });

            newUniqueValueRenderer.Infos.Add(new UniqueValueInfo()
            {
                Value = "E N Cen",
                Symbol = new SimpleFillSymbol()
                {
                    Fill = new SolidColorBrush(Colors.Yellow),
                    BorderBrush = new SolidColorBrush(Colors.Transparent)
                }
            });

            newUniqueValueRenderer.Infos.Add(new UniqueValueInfo()
            {
                Value = "Mtn",
                Symbol = new SimpleFillSymbol()
                {
                    Fill = new SolidColorBrush(Colors.Blue),
                    BorderBrush = new SolidColorBrush(Colors.Transparent)
                }
            });

            newUniqueValueRenderer.Infos.Add(new UniqueValueInfo()
            {
                Value = "N Eng",
                Symbol = new SimpleFillSymbol()
                {
                    Fill = new SolidColorBrush(Colors.Red),
                    BorderBrush = new SolidColorBrush(Colors.Transparent)
                }
            });

            newUniqueValueRenderer.Infos.Add(new UniqueValueInfo()
            {
                Value = "E S Cen",
                Symbol = new SimpleFillSymbol()
                {
                    Fill = new SolidColorBrush(Colors.Brown),
                    BorderBrush = new SolidColorBrush(Colors.Transparent)
                }
            });

            newUniqueValueRenderer.Infos.Add(new UniqueValueInfo()
            {
                Value = "Mid Atl",
                Symbol = new SimpleFillSymbol()
                {
                    Fill = new SolidColorBrush(Colors.Magenta),
                    BorderBrush = new SolidColorBrush(Colors.Transparent)
                }
            });

            newUniqueValueRenderer.Infos.Add(new UniqueValueInfo()
            {
                Value = "S Atl",
                Symbol = new SimpleFillSymbol()
                {
                    Fill = new SolidColorBrush(Colors.Orange),
                    BorderBrush = new SolidColorBrush(Colors.Transparent)
                }
            });

            LayerDrawingOptions layerDrawOptions = new LayerDrawingOptions();
            layerDrawOptions.LayerID = 2;
            layerDrawOptions.Renderer = newUniqueValueRenderer;

            (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).LayerDrawingOptions =
                new LayerDrawingOptionsCollection() { layerDrawOptions };
            (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).VisibleLayers = new int[] { 2 };
            // Changing VisibleLayers will refresh the layer, otherwise an explicit call to Refresh is needed.
            //(MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).Refresh();

        }

        private void ChangeLayerOrderClick(object sender, RoutedEventArgs e)
        {
            (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).LayerDrawingOptions = null;

            DynamicLayerInfoCollection myDynamicLayerInfos = (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).DynamicLayerInfos;
            if (myDynamicLayerInfos == null)
                myDynamicLayerInfos = (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).CreateDynamicLayerInfosFromLayerInfos();

            var aDynamicLayerInfo = myDynamicLayerInfos[0];
            myDynamicLayerInfos.RemoveAt(0);
            myDynamicLayerInfos.Add(aDynamicLayerInfo);

            (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).DynamicLayerInfos = myDynamicLayerInfos;
            (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).VisibleLayers = null;
            // Changing VisibleLayers will refresh the layer, otherwise an explicit call to Refresh is needed.
            //(MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).Refresh();
        }

        private void AddLayerClick(object sender, RoutedEventArgs e)
        {
            (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).LayerDrawingOptions = null;

            DynamicLayerInfoCollection myDynamicLayerInfos = (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).DynamicLayerInfos;
            if (myDynamicLayerInfos == null)
                myDynamicLayerInfos = (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).CreateDynamicLayerInfosFromLayerInfos();

            DynamicLayerInfo dli = new DynamicLayerInfo()
            {
                ID = 4,
                Source = new LayerDataSource()
                {
                    DataSource = new TableDataSource()
                    {
                        WorkspaceID = "MyDatabaseWorkspaceIDSSR2",
                        DataSourceName = "ss6.gdb.Lakes"
                    }
                }
            };

            LayerDrawingOptions layerDrawOptions = new LayerDrawingOptions();
            layerDrawOptions.LayerID = 4;
            layerDrawOptions.Renderer = new SimpleRenderer()
            {
                Symbol = new SimpleFillSymbol()
                {
                    Fill = new SolidColorBrush(Color.FromArgb((int)255, (int)0, (int)0, (int)255))
                }
            };

            (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).LayerDrawingOptions =
                new LayerDrawingOptionsCollection() { layerDrawOptions };

            myDynamicLayerInfos.Insert(0, dli);
            (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).DynamicLayerInfos = myDynamicLayerInfos;
            (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).VisibleLayers = new int[] { 3,4 };
            // Changing VisibleLayers will refresh the layer, otherwise an explicit call to Refresh is needed.
            //(MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).Refresh();
        }
    }
}
