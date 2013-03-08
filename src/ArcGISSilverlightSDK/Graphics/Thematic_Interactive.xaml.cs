using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class Thematic_Interactive : UserControl
    {
        List<ThematicItem> ThematicItemList = new List<ThematicItem>();
        List<List<SolidColorBrush>> ColorList = new List<List<SolidColorBrush>>();
        int _colorShadeIndex = 0;
        int _thematicListIndex = 0;
        FeatureSet _featureSet = null;
        int _classType = 0; // EqualInterval = 1; Quantile = 0;
        int _classCount = 6;
        int _lastGeneratedClassCount = 0;
        bool _legendGridCollapsed;
        bool _classGridCollapsed;

        public Thematic_Interactive()
        {
            InitializeComponent();

            MyMap.PropertyChanged += MyMap_PropertyChanged;
        }

        void MyMap_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SpatialReference")
            {
                MyMap.PropertyChanged -= MyMap_PropertyChanged;

                _legendGridCollapsed = false;
                _classGridCollapsed = false;

                LegendCollapsedTriangle.MouseLeftButtonUp += Triangle_MouseLeftButtonUp;
                LegendExpandedTriangle.MouseLeftButtonUp += Triangle_MouseLeftButtonUp;
                ClassCollapsedTriangle.MouseLeftButtonUp += Triangle_MouseLeftButtonUp;
                ClassExpandedTriangle.MouseLeftButtonUp += Triangle_MouseLeftButtonUp;

                // Get start value for number of classifications in XAML.
                _lastGeneratedClassCount = Convert.ToInt32(((ComboBoxItem)ClassCountCombo.SelectedItem).Content);

                // Set query where clause to include features with an area greater than 70 square miles.  This 
                // will effectively exclude the District of Columbia from attributes to avoid skewing classifications.
                ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query()
                {
                    Where = "SQMI > 70",
                    OutSpatialReference = MyMap.SpatialReference,
                    ReturnGeometry = true
                };
                query.OutFields.Add("*");

                QueryTask queryTask = 
                    new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5");

                queryTask.ExecuteCompleted += (evtsender, args) =>
                {
                    if (args.FeatureSet == null)
                        return;
                    _featureSet = args.FeatureSet;
                    SetRangeValues();
                    RenderButton.IsEnabled = true;
                };

                queryTask.ExecuteAsync(query);

                CreateColorList();
                CreateThematicList();
            }
        }

        public struct ThematicItem
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string CalcField { get; set; }
            public double Min { get; set; }
            public double Max { get; set; }
            public string MinName { get; set; }
            public string MaxName { get; set; }
            public List<double> RangeStarts { get; set; }

        }

        private void CreateColorList()
        {
            ColorList = new List<List<SolidColorBrush>>();

            List<SolidColorBrush> BlueShades = new List<SolidColorBrush>();
            List<SolidColorBrush> RedShades = new List<SolidColorBrush>();
            List<SolidColorBrush> GreenShades = new List<SolidColorBrush>();
            List<SolidColorBrush> YellowShades = new List<SolidColorBrush>();
            List<SolidColorBrush> MagentaShades = new List<SolidColorBrush>();
            List<SolidColorBrush> CyanShades = new List<SolidColorBrush>();

            int rgbFactor = 255 / _classCount;

            for (int j = 0; j < 256; j = j + rgbFactor)
            {
                BlueShades.Add(new SolidColorBrush(Color.FromArgb(192, (byte)j, (byte)j, 255)));
                RedShades.Add(new SolidColorBrush(Color.FromArgb(192, 255, (byte)j, (byte)j)));
                GreenShades.Add(new SolidColorBrush(Color.FromArgb(192, (byte)j, 255, (byte)j)));
                YellowShades.Add(new SolidColorBrush(Color.FromArgb(192, 255, 255, (byte)j)));
                MagentaShades.Add(new SolidColorBrush(Color.FromArgb(192, 255, (byte)j, 255)));
                CyanShades.Add(new SolidColorBrush(Color.FromArgb(192, (byte)j, 255, 255)));
            }

            ColorList.Add(BlueShades);
            ColorList.Add(RedShades);
            ColorList.Add(GreenShades);
            ColorList.Add(YellowShades);
            ColorList.Add(MagentaShades);
            ColorList.Add(CyanShades);

            foreach (List<SolidColorBrush> brushList in ColorList)
            {
                brushList.Reverse();
            }

            List<SolidColorBrush> MixedShades = new List<SolidColorBrush>();
            if (_classCount > 5) MixedShades.Add(new SolidColorBrush(Color.FromArgb(192, 0, 255, 255)));
            if (_classCount > 4) MixedShades.Add(new SolidColorBrush(Color.FromArgb(192, 255, 0, 255)));
            if (_classCount > 3) MixedShades.Add(new SolidColorBrush(Color.FromArgb(192, 255, 255, 0)));
            MixedShades.Add(new SolidColorBrush(Color.FromArgb(192, 0, 255, 0)));
            MixedShades.Add(new SolidColorBrush(Color.FromArgb(192, 0, 0, 255)));
            MixedShades.Add(new SolidColorBrush(Color.FromArgb(192, 255, 0, 0)));
            ColorList.Add(MixedShades);

            _lastGeneratedClassCount = _classCount;
        }

        private void CreateThematicList()
        {
            ThematicItemList.Add(new ThematicItem() { Name = "POP2007", Description = "2007 Population ", CalcField = "" });
            ThematicItemList.Add(new ThematicItem() { Name = "POP07_SQMI", Description = "2007 Pop per Sq Mi", CalcField = "" });
            ThematicItemList.Add(new ThematicItem() { Name = "MALES", Description = "%Males", CalcField = "POP2007" });
            ThematicItemList.Add(new ThematicItem() { Name = "FEMALES", Description = "%Females", CalcField = "POP2007" });
            ThematicItemList.Add(new ThematicItem() { Name = "MED_AGE", Description = "Median age", CalcField = "" });
            ThematicItemList.Add(new ThematicItem() { Name = "SQMI", Description = "Total SqMi", CalcField = "" });
            foreach (ThematicItem items in ThematicItemList)
            {
                FieldCombo.Items.Add(items.Description);
            }
            FieldCombo.SelectedIndex = 0;
        }

        private void SetRangeValues()
        {
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;

            // if necessary, update ColorList based on current number of classes.
            if (_lastGeneratedClassCount != _classCount) CreateColorList();

            // Field on which to generate a classification scheme.  
            ThematicItem thematicItem = ThematicItemList[_thematicListIndex];

            // Calculate value for classification scheme
            bool useCalculatedValue = !string.IsNullOrEmpty(thematicItem.CalcField);

            // Store a list of values to classify
            List<double> valueList = new List<double>();

            // Get range, min, max, etc. from features
            for (int i = 0; i < _featureSet.Features.Count; i++)
            {
                Graphic graphicFeature = _featureSet.Features[i];

                double graphicValue = Convert.ToDouble(graphicFeature.Attributes[thematicItem.Name]);
                string graphicName = graphicFeature.Attributes["STATE_NAME"].ToString();

                if (useCalculatedValue)
                {
                    double calcVal = Convert.ToDouble(graphicFeature.Attributes[thematicItem.CalcField]);
                    graphicValue = Math.Round(graphicValue / calcVal * 100, 2);
                }

                if (i == 0)
                {
                    thematicItem.Min = graphicValue;
                    thematicItem.Max = graphicValue;
                    thematicItem.MinName = graphicName;
                    thematicItem.MaxName = graphicName;
                }
                else
                {
                    if (graphicValue < thematicItem.Min) { thematicItem.Min = graphicValue; thematicItem.MinName = graphicName; }
                    if (graphicValue > thematicItem.Max) { thematicItem.Max = graphicValue; thematicItem.MaxName = graphicName; }
                }

                valueList.Add(graphicValue);
            }

            // Set up range start values
            thematicItem.RangeStarts = new List<double>();

            double totalRange = thematicItem.Max - thematicItem.Min;
            double portion = totalRange / _classCount;

            thematicItem.RangeStarts.Add(thematicItem.Min);
            double startRangeValue = thematicItem.Min;

            // Equal Interval
            if (_classType == 1)
            {
                for (int i = 1; i < _classCount; i++)
                {
                    startRangeValue += portion;
                    thematicItem.RangeStarts.Add(startRangeValue);
                }
            }
            // Quantile
            else
            {
                // Enumerator of all values in ascending order
                IEnumerable<double> valueEnumerator =
                from aValue in valueList
                orderby aValue //"ascending" is default
                select aValue;

                int increment = Convert.ToInt32(Math.Ceiling(_featureSet.Features.Count / _classCount));
                for (int i = increment; i < valueList.Count; i += increment)
                {
                    double value = valueEnumerator.ElementAt(i);
                    thematicItem.RangeStarts.Add(value);
                }
            }

            // Create graphic features and set symbol using the class range which contains the value 
            List<SolidColorBrush> brushList = ColorList[_colorShadeIndex];
            if (_featureSet != null && _featureSet.Features.Count > 0)
            {
                // Clear previous graphic features
                graphicsLayer.ClearGraphics();

                for (int i = 0; i < _featureSet.Features.Count; i++)
                {
                    Graphic graphicFeature = _featureSet.Features[i];

                    double graphicValue = Convert.ToDouble(graphicFeature.Attributes[thematicItem.Name]);
                    if (useCalculatedValue)
                    {
                        double calcVal = Convert.ToDouble(graphicFeature.Attributes[thematicItem.CalcField]);
                        graphicValue = Math.Round(graphicValue / calcVal * 100, 2);
                    }

                    int brushIndex = GetRangeIndex(graphicValue, thematicItem.RangeStarts);

                    SimpleFillSymbol symbol = new SimpleFillSymbol()
                    {
                        Fill = brushList[brushIndex],
                        BorderBrush = new SolidColorBrush(Colors.Transparent),
                        BorderThickness = 1
                    };

                    Graphic graphic = new Graphic();
                    graphic.Geometry = graphicFeature.Geometry;
                    graphic.Attributes.Add("Name", graphicFeature.Attributes["STATE_NAME"].ToString());
                    graphic.Attributes.Add("Description", thematicItem.Description);
                    graphic.Attributes.Add("Value", graphicValue.ToString());
                    graphic.Symbol = symbol;

                    graphicsLayer.Graphics.Add(graphic);
                }


                // Create new legend with ranges and swatches.
                LegendStackPanel.Children.Clear();

                ListBox legendList = new ListBox();
                LegendTitle.Text = thematicItem.Description;

                for (int c = 0; c < _classCount; c++)
                {
                    Rectangle swatchRect = new Rectangle()
                    {
                        Width = 20,
                        Height = 20,
                        Stroke = new SolidColorBrush(Colors.Black),
                        Fill = brushList[c]
                    };

                    TextBlock classTextBlock = new TextBlock();

                    // First classification
                    if (c == 0)
                        classTextBlock.Text = String.Format("  Less than {0}", Math.Round(thematicItem.RangeStarts[1], 2));
                    // Last classification
                    else if (c == _classCount - 1)
                        classTextBlock.Text = String.Format("  {0} and above", Math.Round(thematicItem.RangeStarts[c], 2));
                    // Middle classifications
                    else
                        classTextBlock.Text = String.Format("  {0} to {1}", Math.Round(thematicItem.RangeStarts[c], 2), Math.Round(thematicItem.RangeStarts[c + 1], 2));

                    StackPanel classStackPanel = new StackPanel();
                    classStackPanel.Orientation = Orientation.Horizontal;
                    classStackPanel.Children.Add(swatchRect);
                    classStackPanel.Children.Add(classTextBlock);

                    legendList.Items.Add(classStackPanel);
                }

                TextBlock minTextBlock = new TextBlock();
                StackPanel minStackPanel = new StackPanel();
                minStackPanel.Orientation = Orientation.Horizontal;
                minTextBlock.Text = String.Format("Min: {0} ({1})", thematicItem.Min, thematicItem.MinName);
                minStackPanel.Children.Add(minTextBlock);
                legendList.Items.Add(minStackPanel);

                TextBlock maxTextBlock = new TextBlock();
                StackPanel maxStackPanel = new StackPanel();
                maxStackPanel.Orientation = Orientation.Horizontal;
                maxTextBlock.Text = String.Format("Max: {0} ({1})", thematicItem.Max, thematicItem.MaxName);
                maxStackPanel.Children.Add(maxTextBlock);
                legendList.Items.Add(maxStackPanel);

                LegendStackPanel.Children.Add(legendList);
            }
        }

        private int GetRangeIndex(double val, List<double> ranges)
        {
            int index = _classCount - 1;
            for (int r = 0; r < _classCount - 1; r++)
            {
                if (val >= ranges[r] && val < ranges[r + 1]) index = r;
            }
            return index;
        }

        public struct Values
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Value { get; set; }
        }

        private void ClassTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClassTypeCombo != null)
                _classType = ClassTypeCombo.SelectedIndex;
        }

        private void ColorBlendCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorBlendCombo != null)
                _colorShadeIndex = ColorBlendCombo.SelectedIndex;
        }

        private void ClassCountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClassCountCombo != null)
            {
                ComboBoxItem item = ClassCountCombo.SelectedItem as ComboBoxItem;
                _classCount = Convert.ToInt32(item.Content);
            }
        }

        private void FieldCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FieldCombo != null)
                _thematicListIndex = FieldCombo.SelectedIndex;
        }

        private void RenderButton_Click(object sender, RoutedEventArgs e)
        {
            SetRangeValues();
        }

        void Triangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement source = sender as FrameworkElement;

            switch (source.Name)
            {
                case "ClassExpandedTriangle":
                case "ClassCollapsedTriangle":
                    if (_classGridCollapsed)
                    {
                        ClassExpandedTriangle.Visibility = Visibility.Visible;
                        ClassCollapsedTriangle.Visibility = Visibility.Collapsed;
                        ClassCollapsedTitle.Visibility = Visibility.Collapsed;
                        ClassGrid.Height = Double.NaN;
                        ClassGrid.UpdateLayout();
                    }
                    else
                    {
                        ClassCollapsedTriangle.Visibility = Visibility.Visible;
                        ClassExpandedTriangle.Visibility = Visibility.Collapsed;
                        ClassCollapsedTitle.Visibility = Visibility.Visible;
                        ClassGrid.Height = 50;
                    }
                    _classGridCollapsed = !_classGridCollapsed;
                    break;
                case "LegendExpandedTriangle":
                case "LegendCollapsedTriangle":
                    if (_legendGridCollapsed)
                    {
                        LegendExpandedTriangle.Visibility = Visibility.Visible;
                        LegendCollapsedTriangle.Visibility = Visibility.Collapsed;
                        LegendCollapsedTitle.Visibility = Visibility.Collapsed;
                        LegendGrid.Height = Double.NaN;
                        LegendGrid.UpdateLayout();
                    }
                    else
                    {
                        LegendCollapsedTriangle.Visibility = Visibility.Visible;
                        LegendExpandedTriangle.Visibility = Visibility.Collapsed;
                        LegendCollapsedTitle.Visibility = Visibility.Visible;
                        LegendGrid.Height = 50;
                    }
                    _legendGridCollapsed = !_legendGridCollapsed;
                    break;
            }
        }
    }
}
