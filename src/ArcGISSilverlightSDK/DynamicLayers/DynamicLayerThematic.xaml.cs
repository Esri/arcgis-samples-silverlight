using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class DynamicLayerThematic : UserControl
    {
        GenerateRendererTask generateRendererTask;
        ColorRamp colorRamp;
        GenerateRendererParameters generateClassesParameters;

        public DynamicLayerThematic()
        {
            InitializeComponent();

            PopulateCombos();

            InitializeRenderingInfo();
        }

        private void FeatureLayer_Initialized(object sender, EventArgs e)
        {
            FeatureLayer featureLayer = sender as FeatureLayer;
            IEnumerable<Field> intandDoublefields =
              from fld in featureLayer.LayerInfo.Fields
              where fld.Type == Field.FieldType.Integer || fld.Type == Field.FieldType.Double
              select fld;
            if (intandDoublefields != null && intandDoublefields.Count() > 0)
            {
                ClassificationFieldCombo.ItemsSource = intandDoublefields;
                ClassificationFieldCombo.SelectedIndex = 1;
                NormalizationFieldCombo.ItemsSource = intandDoublefields;
                NormalizationFieldCombo.SelectedIndex = -1;
                RenderButton.IsEnabled = true;
            }
        }

        private void PopulateCombos()
        {
            AlgorithmCombo.ItemsSource = new object[] 
			{
			    Algorithm.CIELabAlgorithm, 
			    Algorithm.HSVAlgorithm, 
			    Algorithm.LabLChAlgorithm 
			};
            AlgorithmCombo.SelectedIndex = 1;

            ClassificationMethodCombo.ItemsSource = new object[] 
			{ 
			    ClassificationMethod.EqualInterval, 
			    ClassificationMethod.NaturalBreaks,
			    ClassificationMethod.Quantile, 
			    ClassificationMethod.StandardDeviation
			};
            ClassificationMethodCombo.SelectedIndex = 0;

            NormalizationTypeCombo.ItemsSource = new object[]
			{
			    NormalizationType.Field,
			    NormalizationType.Log,          
                NormalizationType.PercentOfTotal,
                NormalizationType.None
			};
            NormalizationTypeCombo.SelectedIndex = 3;

            IntervalCombo.ItemsSource = new object[]
			{
			    StandardDeviationInterval.One,
			    StandardDeviationInterval.OneHalf,
			    StandardDeviationInterval.OneQuarter,
			    StandardDeviationInterval.OneThird
			};
            IntervalCombo.SelectedIndex = 0;
        }

        private void InitializeRenderingInfo()
        {
            FeatureLayer statesFeatureLayer = MyMap.Layers["StatesFeatureLayer"] as FeatureLayer;

            generateRendererTask = new GenerateRendererTask(statesFeatureLayer.Url);

            generateClassesParameters = new GenerateRendererParameters();
            generateClassesParameters.Source = new LayerMapSource() { MapLayerID = 3 };

            generateRendererTask.Failed += (s, e) =>
            {
                MessageBox.Show(string.Format("GenerateRendererTask Failed: {0}", e.Error.Message));
            };
            generateRendererTask.ExecuteCompleted += (s, e) =>
            {
                statesFeatureLayer.Renderer = e.GenerateRendererResult.Renderer;
            };
        }

        private void RenderButton_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<ColorRamp> colorRamps = new ObservableCollection<ColorRamp>();
            colorRamp = new ColorRamp()
            {
                From = ((StartColorCombo.SelectedItem as ComboBoxItem).Background as SolidColorBrush).Color,
                To = ((EndColorCombo.SelectedItem as ComboBoxItem).Background as SolidColorBrush).Color,
                Algorithm = (Algorithm)AlgorithmCombo.SelectedItem,
            };
            colorRamps.Add(colorRamp);

            generateClassesParameters.ClassificationDefinition = new ClassBreaksDefinition()
            {
                BaseSymbol = new ESRI.ArcGIS.Client.Symbols.SimpleFillSymbol()
                {
                    Fill = (ColorRampCombo.SelectedItem as ComboBoxItem).Background
                },
                ClassificationField = ((ClassificationFieldCombo.SelectedItem) as Field).Name,
                ClassificationMethod = (ClassificationMethod)ClassificationMethodCombo.SelectedItem,

                BreakCount = int.Parse(BreakCountTb.Text.Trim()),
                ColorRamps = colorRamps,
            };

            ClassBreaksDefinition classBreakDef =
                generateClassesParameters.ClassificationDefinition as ClassBreaksDefinition;

            if (classBreakDef.ClassificationMethod == ClassificationMethod.StandardDeviation)
                classBreakDef.StandardDeviationInterval = (StandardDeviationInterval)IntervalCombo.SelectedItem;

            if (NormalizationTypeCombo.SelectedItem != null)
            {
                classBreakDef.NormalizationType = (NormalizationType)NormalizationTypeCombo.SelectedItem;

                if (classBreakDef.NormalizationType == NormalizationType.Field)
                {
                    if (NormalizationFieldCombo.SelectedItem != null)
                        classBreakDef.NormalizationField = ((NormalizationFieldCombo.SelectedItem) as Field).Name;
                    else
                    {
                        MessageBox.Show("Normalization Field must be selected");
                        return;
                    }
                }
            }
            if (generateRendererTask.IsBusy)
                generateRendererTask.CancelAsync();
            generateRendererTask.ExecuteAsync(generateClassesParameters);
        }

        private void ClassificationMethodCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClassificationMethod method = (ClassificationMethod)(sender as ComboBox).SelectedItem;
            IntervalCombo.IsEnabled = (method == ClassificationMethod.StandardDeviation) ? true : false;
            BreakCountTb.IsEnabled = (method == ClassificationMethod.StandardDeviation) ? false : true;
        }

        private void NormalizationTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NormalizationType normType = (NormalizationType)(sender as ComboBox).SelectedItem;
            NormalizationFieldCombo.IsEnabled = (normType == NormalizationType.Field) ? true : false;
        }
    }
}
