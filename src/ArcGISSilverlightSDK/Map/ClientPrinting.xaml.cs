using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Printing;
using System.Windows.Threading;
using ESRI.ArcGIS.Client;

namespace ArcGISSilverlightSDK
{
    public partial class ClientPrinting
    {
        public ClientPrinting()
        {
            InitializeComponent();
        }

        private void ActivatePrintPreview(object sender, RoutedEventArgs e)
        {
            PrintPreview.Visibility = Visibility.Visible;
            MyMapPrinter.Map = MyMap; // sets the Map to print and initializes the PrintMap with a cloned map (as defined in the print style)
        }

        private void DesactivatePrintPreview(object sender, RoutedEventArgs e)
        {
            PrintPreview.Visibility = Visibility.Collapsed;
            MyMapPrinter.Map = null;  // cancel the current print and frees the cloned map
        }

        private void OnPreviewSizeChanged(object sender, SelectionChangedEventArgs e)
        {
            // Chnage the preview size of the print map.
            // Note that this size will be overwritten during the print process by the real print area of the printer (depending on print parameters: paper size, orientation,...)
            var previewSize = ((ComboBox)sender).SelectedItem as PreviewSize;
            if (previewSize != null && MyMapPrinter != null)
                MyMapPrinter.SetPrintableArea(previewSize.Height, previewSize.Width);
        }

        private void OnPrint(object sender, RoutedEventArgs e)
        {
            // Start the print process
            MyMapPrinter.Print();
        }
    }

    // Main print control that displays the map using the print template
    public class MapPrinter : Control, INotifyPropertyChanged
    {
        public MapPrinter()
        {
            _isPrinting = false;
            DataContext = this; // simplify binding in print styles
        }

        // Executed when the print template changed
        public override void OnApplyTemplate()
        {
            var extent = PrintMap == null ? null : PrintMap.Extent; // save the current print extent that will be lost after OnApplyTemplate (since the PrintMap changes)
            base.OnApplyTemplate();
            PrintMap = GetTemplateChild("PrintMap") as ESRI.ArcGIS.Client.Map;
            PrintMap.Extent = extent ?? Map.Extent; // restore previous print extent or init it with the current map extent
        }

        // Map to print (Dependency Property)
        public ESRI.ArcGIS.Client.Map Map
        {
            get { return (ESRI.ArcGIS.Client.Map)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }

        public static readonly DependencyProperty MapProperty = DependencyProperty.Register("Map", typeof(ESRI.ArcGIS.Client.Map), typeof(MapPrinter), new PropertyMetadata(null, OnMapChanged));

        private static void OnMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mapPrinter = d as MapPrinter;
            var newMap = e.NewValue as ESRI.ArcGIS.Client.Map;
            if (mapPrinter != null)
            {
                if (newMap != null && mapPrinter.PrintMap != null)
                    mapPrinter.PrintMap.Extent = newMap.Extent;
                if (newMap == null && mapPrinter.IsPrinting)
                    mapPrinter.IsCancelingPrint = true;
            }
        }

        // Title of the print document (Dependency Property)
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MapPrinter), new PropertyMetadata("Map Document"));

        // Flag indicating that the map must be rotated 90° 
        public bool RotateMap
        {
            get { return (bool)GetValue(RotateMapProperty); }
            set { SetValue(RotateMapProperty, value); }
        }

        public static readonly DependencyProperty RotateMapProperty =
                DependencyProperty.Register("RotateMap", typeof(bool), typeof(MapPrinter), new PropertyMetadata(false, OnRotateMapChanged));

        private static void OnRotateMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mapPrinter = d as MapPrinter;
            if (mapPrinter != null)
                mapPrinter.PrintMap.Rotation = ((bool)e.NewValue ? -90 : 0);
        }

        bool _isPrinting;
        // Indicates if a print task is going on.
        public bool IsPrinting
        {
            get { return _isPrinting; }
            private set
            {
                if (value != _isPrinting)
                {
                    _isPrinting = value;
                    NotifyPropertyChanged("IsPrinting");
                }
            }
        }

        // The print map (defined in the print template)
        public ESRI.ArcGIS.Client.Map PrintMap { get; private set; }

        // Gets the current date/time.
        public DateTime Now
        {
            get { return DateTime.Now; }
        }

        // Start the print process (by delagating either to the Silverlight print engine or to the WPF print engine)
        public void Print()
        {
            if (IsPrinting)
                return;

            // Create the print engine depending on silverlight/WPF
            var printEngine = new SilverlightPrintEngine(this);

            // Call the print engine doing the work
            try
            {
                printEngine.Print();
            }
            catch (Exception e)
            {
                EndPrint(e);
            }
        }

        // InotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        // Notifies the property changed.
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // Internal methods/properties
        internal bool IsCancelingPrint { get; set; }

        internal void BeginPrint()
        {
            IsCancelingPrint = false;
            IsPrinting = true;

            NotifyPropertyChanged("Now"); // in case time is displayed
        }

        internal void EndPrint(Exception error)
        {
            if (error != null && !IsCancelingPrint)
                MessageBox.Show(string.Format("Error during print: {0}", error.Message));
            IsPrinting = false;
            IsCancelingPrint = false;
        }

        internal void SetPrintableArea(double printableAreaHeight, double printableAreaWidth)
        {
            // Recalculate layout in order to fit printable area
            Height = printableAreaHeight;
            Width = printableAreaWidth;

            // Update map size
            UpdateLayout();
        }
    }

    // Collection of PreviewSize (creatable in XAML)
    public class PreviewSizes : ObservableCollection<PreviewSize> { }

    // Represents a Preview Size (creatable in XAML)
    public class PreviewSize
    {
        public string Id { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    // Silverlight PrintEngine class: Print a map by using a SL PrintDocument
    internal class SilverlightPrintEngine
    {
        private readonly MapPrinter _mapPrinter;

        // Used during print
        private bool _isLoading;
        private bool _isReady;
        private int _tryCount;
        private MapLoader _mapLoader;

        public SilverlightPrintEngine(MapPrinter mapPrinter)
        {
            _mapPrinter = mapPrinter;
        }

        public void Print()
        {
            var doc = new PrintDocument();

            doc.BeginPrint += (s, e) => BeginPrint();
            doc.PrintPage += PrintPage;
            doc.EndPrint += (s, e) => EndPrint(e != null ? e.Error : null);

            doc.Print(string.IsNullOrEmpty(_mapPrinter.Title) ? "Map Print" : _mapPrinter.Title, null);
        }

        private void BeginPrint()
        {
            _mapPrinter.BeginPrint();

            _mapLoader = new MapLoader(_mapPrinter.PrintMap);
            _mapLoader.Loaded += OnMapLoaderLoaded;
            _isLoading = false;
            _tryCount = 0;
            _isReady = false;
        }

        void OnMapLoaderLoaded(object sender, EventArgs e)
        {
            // All layers are loaded in the map --> ready to print (next time PrintPage will be called by SL framework)
            _isLoading = false;
            _mapPrinter.UpdateLayout(); // to be sure all tiles will be displayed
        }

        private void EndPrint(Exception error)
        {
            _mapPrinter.EndPrint(error);
            _mapLoader.Loaded -= OnMapLoaderLoaded;
            _mapLoader = null;
        }

        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            e.PageVisual = null;

            _tryCount++;
            if (_mapPrinter.IsCancelingPrint)
            {
                // Print has been canceled by user
                e.HasMorePages = false; //  Note that despite this setting to false, the framework will continue to call PrintPage 7 times
                return;
            }

            if (_tryCount == 1)
            {
                // Set the printable area size which is unknown before the print of the first page
                var extent = _mapPrinter.PrintMap.Extent;
                _mapPrinter.SetPrintableArea(e.PrintableArea.Height, e.PrintableArea.Width);

                // change the extent of the map and wait for all layers loaded (progress == 100)
                _isLoading = true;
                _mapLoader.WaitForLoaded();
                _mapPrinter.PrintMap.Extent = extent;
                e.HasMorePages = true; // retry later but nothing to print at this time
            }
            else
            {
                _mapPrinter.UpdateLayout();
                if (_isLoading && _tryCount <= 8)
                {
                    // Wait for loaded layers
                    e.HasMorePages = true; // retry later

                    Thread.Sleep(100 + 300 * _tryCount); // sleep to give a chance to load layers before the maximum of 7 tries
                }
                else
                {
                    if (_isReady || _tryCount > 8)
                    {
                        // Print the page
                        e.HasMorePages = false;
                        e.PageVisual = _mapPrinter;
                    }
                    else
                    {
                        // FeatureLayers in OnDemand mode need to be rendered once in order to be printable --> wait once more
                        e.HasMorePages = true;
                        _isReady = true;
                    }
                }
            }
        }
    }

    // Helper class to know when a map is loaded and so ready to print.
    // It's waiting for progress = 100 but sometimes this event never comes (e.g. with a dynamic layer when the image is in the cache)
    // So there is a timer to avoid infinite wait.
    internal class MapLoader
    {
        private readonly ESRI.ArcGIS.Client.Map _map;
        private readonly DispatcherTimer _timer;
        private bool _isProgressing; // some progress events came up 

        public MapLoader(ESRI.ArcGIS.Client.Map map)
        {
            _map = map;
            _timer = new DispatcherTimer();
            _timer.Tick += OnTimerTick;
            _isProgressing = false;
        }

        // Waits for the map loaded.
        public void WaitForLoaded()
        {
            _map.Progress += OnMapProgress; // subscribe to OnProgress event
            if (_timer.IsEnabled)
                _timer.Stop();
            _timer.Interval = TimeSpan.FromSeconds(10); // Wait 10 seconds before the first mapprogress event, after that, consider that the map was already ready
            _timer.Start();
        }

        /// Cancels the wait.
        public void CancelWait()
        {
            _timer.Stop();
            _map.Progress -= OnMapProgress;
        }

        // Occurs when the map is loaded.
        public event EventHandler<EventArgs> Loaded;
        private void OnLoaded()
        {
            CancelWait();
            var handler = Loaded;
            if (handler != null)
                handler(this, new EventArgs());
        }

        // Security timer to avoid infinite waiting (not useful with Silverlight which anyway calls PrintPage only 7 times)
        private void OnTimerTick(object sender, EventArgs e)
        {
            if (_isProgressing)
            {
                // Progress events are coming -> wait more
                _isProgressing = false;
                _timer.Interval = TimeSpan.FromSeconds(30);
            }
            else
            {
                // No progress event since last test --> stop and consider the map as loaded
                OnLoaded();
            }
        }

        private void OnMapProgress(object sender, ProgressEventArgs e)
        {
            _isProgressing = true;
            if (e.Progress == 100)
                OnLoaded(); // map is ready
        }
    }

    // Define an attached property allowing to initialize a map by cloning the layers of another map.
    public static class CloneMap
    {
        // Map to clone attached property
        public static String GetMap(DependencyObject obj)
        {
            return (String)obj.GetValue(MapProperty);
        }

        public static void SetMap(DependencyObject obj, String value)
        {
            obj.SetValue(MapProperty, value);
        }

        public static readonly DependencyProperty MapProperty = DependencyProperty.RegisterAttached("Map", typeof(ESRI.ArcGIS.Client.Map), typeof(CloneMap), new PropertyMetadata(null, OnMapChanged));

        private static void OnMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var map = d as ESRI.ArcGIS.Client.Map;
            if (map == null)
                return;
            var mapToClone = (ESRI.ArcGIS.Client.Map)e.NewValue;

            map.Layers.Clear();
            if (mapToClone != null)
                Clone(map, mapToClone);
        }

        // Clone a Map
        private static void Clone(ESRI.ArcGIS.Client.Map map, ESRI.ArcGIS.Client.Map mapToClone)
        {
            map.MinimumResolution = mapToClone.MinimumResolution;
            map.MaximumResolution = mapToClone.MaximumResolution;
            map.TimeExtent = mapToClone.TimeExtent;
            map.WrapAround = mapToClone.WrapAround;

            // Clone layers
            foreach (var toLayer in mapToClone.Layers.Select(CloneLayer).Where(toLayer => toLayer != null))
            {
                toLayer.InitializationFailed += (s, e) => { }; // to avoid crash if bad layer
                map.Layers.Add(toLayer); // use index in order to keep existing layers after cloned layers
            }
        }

        // Clone a Layer
        private static Layer CloneLayer(Layer layer)
        {
            Layer toLayer;
            var featureLayer = layer as FeatureLayer;

            if (layer is GraphicsLayer && (featureLayer == null || featureLayer.Url == null || featureLayer.Mode != FeatureLayer.QueryMode.OnDemand))
            {
                // Clone the layer and the graphics
                var fromLayer = layer as GraphicsLayer;
                var printLayer = new GraphicsLayer
                {
                    Renderer = fromLayer.Renderer,
                    Clusterer = fromLayer.Clusterer == null ? null : fromLayer.Clusterer.Clone(),
                    ShowLegend = fromLayer.ShowLegend,
                    RendererTakesPrecedence = fromLayer.RendererTakesPrecedence,
                    ProjectionService = fromLayer.ProjectionService
                };
                toLayer = printLayer;

                var graphicCollection = new GraphicCollection();
                foreach (var graphic in fromLayer.Graphics)
                {
                    var clone = new Graphic();

                    foreach (var kvp in graphic.Attributes)
                    {
                        if (kvp.Value is DependencyObject)
                        {
                            // If the attribute is a dependency object --> clone it
                            var clonedkvp = new KeyValuePair<string, object>(kvp.Key, (kvp.Value as DependencyObject).Clone());
                            clone.Attributes.Add(clonedkvp);
                        }
                        else
                            clone.Attributes.Add(kvp);
                    }
                    clone.Geometry = graphic.Geometry;
                    clone.Symbol = graphic.Symbol;
                    clone.Selected = graphic.Selected;
                    clone.TimeExtent = graphic.TimeExtent;
                    graphicCollection.Add(clone);
                }

                printLayer.Graphics = graphicCollection;

                toLayer.ID = layer.ID;
                toLayer.Opacity = layer.Opacity;
                toLayer.Visible = layer.Visible;
                toLayer.MaximumResolution = layer.MaximumResolution;
                toLayer.MinimumResolution = layer.MinimumResolution;
            }
            else
            {
                // Clone other layer types
                toLayer = layer.Clone();

                if (layer is GroupLayerBase)
                {
                    // Clone sublayers (not cloned in Clone() to avoid issue with graphicslayer)
                    var childLayers = new LayerCollection();
                    foreach (Layer subLayer in (layer as GroupLayerBase).ChildLayers)
                    {
                        var toSubLayer = CloneLayer(subLayer);

                        if (toSubLayer != null)
                        {
                            toSubLayer.InitializationFailed += (s, e) => { }; // to avoid crash if bad layer
                            childLayers.Add(toSubLayer);
                        }
                    }
                    ((GroupLayerBase)toLayer).ChildLayers = childLayers;
                }
            }
            return toLayer;
        }
    }

    // Generic class extention for cloning recursively a dependency object
    // Very limited implementation based on CLR properties
    // Attached properties are not taken in care except specific case for this sample
    // Is used to clone Layers and Elements of ElementLayer
    public static class CloneExtension
    {
        // Clones a dependency object.
        public static T Clone<T>(this T source) where T : DependencyObject
        {
            Type t = source.GetType(); // can be different from typeof(T)
            var clone = (T)Activator.CreateInstance(t);

            // Loop on CLR properties (except name, parent and graphics)
            foreach (PropertyInfo propertyInfo in t.GetProperties())
            {
                if (propertyInfo.Name == "Name" || propertyInfo.Name == "Parent" || propertyInfo.Name == "Graphics" || propertyInfo.Name == "ChildLayers" ||
                        !propertyInfo.CanRead || propertyInfo.GetGetMethod() == null ||
                        propertyInfo.GetIndexParameters().Length > 0)
                    continue;

                try
                {
                    Object value = propertyInfo.GetValue(source, null);
                    if (value != null)
                    {
                        if (propertyInfo.PropertyType.GetInterface("IList", true) != null && !propertyInfo.PropertyType.IsArray)
                        {
                            // Collection ==> loop on items and clone them (we suppose the collection itself is already initialized!)
                            var count = (int)propertyInfo.PropertyType.InvokeMember("get_Count", BindingFlags.InvokeMethod, null, value, null);
                            propertyInfo.PropertyType.InvokeMember("Clear", BindingFlags.InvokeMethod, null, propertyInfo.GetValue(clone, null), null); // without this line, text can be duplicated due to inlines objects added after text is set

                            for (int index = 0; index < count; index++)
                            {
                                object itemValue = propertyInfo.PropertyType.InvokeMember("get_Item", BindingFlags.InvokeMethod, null, propertyInfo.GetValue(source, null), new object[] { index });
                                propertyInfo.PropertyType.InvokeMember("Add", BindingFlags.InvokeMethod, null, propertyInfo.GetValue(clone, null), new[] { CloneDependencyObject(itemValue) });
                            }
                        }
                        else if (propertyInfo.CanWrite && propertyInfo.GetSetMethod() != null)
                        {
                            propertyInfo.SetValue(clone, CloneDependencyObject(value), null);
                        }
                    }
                }
                catch (Exception) { }
            }

            // Copy some useful attached properties (not done by reflection)
            if (source is UIElement)
            {
                DependencyProperty attachedProperty = ESRI.ArcGIS.Client.ElementLayer.EnvelopeProperty; // needed for ElementLayer
                SetDependencyProperty(attachedProperty, source, clone);
            }

            return clone;
        }

        static private object CloneDependencyObject(object source)
        {
            return source is DependencyObject && !(source is ControlTemplate || source is IRenderer) ? (source as DependencyObject).Clone() : source;
        }

        static private void SetDependencyProperty(DependencyProperty dp, DependencyObject source, DependencyObject clone)
        {
            Object value = source.GetValue(dp);
            if (value != null)
                clone.SetValue(dp, CloneDependencyObject(value));
        }
    }
}