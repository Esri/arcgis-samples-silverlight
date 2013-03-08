using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class GenerateRenderer : UserControl
    {
        GenerateRendererTask generateRendererTask;

        public GenerateRenderer()
        {
            InitializeComponent();

            generateRendererTask = new GenerateRendererTask();
            generateRendererTask.Url = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/2";
            generateRendererTask.ExecuteCompleted += generateRendererTask_ExecuteCompleted;
            generateRendererTask.Failed += generateRendererTask_Failed;
        }

        private void GenerateRangeValueClick(object sender, RoutedEventArgs e)
        {
            ClassBreaksDefinition classBreaksDefinition = new ClassBreaksDefinition()
            {
                ClassificationField = "SQMI",
                ClassificationMethod = ClassificationMethod.StandardDeviation,
                BreakCount = 5,
                StandardDeviationInterval = ESRI.ArcGIS.Client.Tasks.StandardDeviationInterval.OneQuarter
            };
            classBreaksDefinition.ColorRamps.Add(new ColorRamp()
                {
                    From = Colors.Blue,
                    To = Colors.Red,
                    Algorithm = Algorithm.HSVAlgorithm
                });

            GenerateRendererParameters rendererParams = new GenerateRendererParameters()
            {
                ClassificationDefinition = classBreaksDefinition,
                Where = "STATE_NAME NOT IN ('Alaska', 'Hawaii')"
            };

            generateRendererTask.ExecuteAsync(rendererParams, rendererParams.Where);
        }

        private void GenerateUniqueValueClick(object sender, RoutedEventArgs e)
        {
            UniqueValueDefinition uniqueValueDefinition = new UniqueValueDefinition()
            {
                Fields = new List<string>() { "STATE_NAME" }
            };
            uniqueValueDefinition.ColorRamps.Add(new ColorRamp()
                {
                    From = Colors.Blue,
                    To = Colors.Red,
                    Algorithm = Algorithm.CIELabAlgorithm
                });

            GenerateRendererParameters rendererParams = new GenerateRendererParameters()
            {
                ClassificationDefinition = uniqueValueDefinition,
                Where = "STATE_NAME NOT IN ('Alaska', 'Hawaii')" 
            };

            generateRendererTask.ExecuteAsync(rendererParams, rendererParams.Where);
        }

        void generateRendererTask_ExecuteCompleted(object sender, GenerateRendererResultEventArgs e)
        {
            GenerateRendererResult rendererResult = e.GenerateRendererResult;

            LayerDrawingOptionsCollection options = (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).LayerDrawingOptions != null
                ? (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).LayerDrawingOptions : new LayerDrawingOptionsCollection();

            LayerDrawingOptions layerDrawingOptionsParcels = null;

            foreach (LayerDrawingOptions drawOption in options)
                if (drawOption.LayerID == 2)
                {
                    layerDrawingOptionsParcels = drawOption;
                    drawOption.Renderer = rendererResult.Renderer;
                }

            if (e.UserState != null)
            {
                LayerDefinition layerDefinition = new LayerDefinition()
                {
                    LayerID = 2,
                    Definition = e.UserState as string
                };

                (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).LayerDefinitions =
                    new System.Collections.ObjectModel.ObservableCollection<LayerDefinition>() { layerDefinition };
            }
            else       
                (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).LayerDefinitions = null;
            

            if (layerDrawingOptionsParcels == null)
                options.Add(new LayerDrawingOptions() { LayerID = 2, Renderer = rendererResult.Renderer });

            (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).LayerDrawingOptions = options;
            (MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).Refresh();
        }

        void generateRendererTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Error: " + e.Error.Message);
        }
    }
}
