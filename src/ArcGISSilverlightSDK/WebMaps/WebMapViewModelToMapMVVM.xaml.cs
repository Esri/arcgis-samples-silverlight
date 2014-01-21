using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Portal;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.WebMap;

namespace ArcGISSilverlightSDK
{
    public partial class WebMapViewModelToMapMVVM : UserControl, INotifyPropertyChanged
    {
        WebMapViewModel _webMapViewModel;

        public WebMapViewModelToMapMVVM()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadWebMap();
        }

        private async void LoadWebMap()
        {
            try
            {
                ArcGISPortal portal = new ArcGISPortal() 
                    { Url = "http://www.arcgis.com/sharing/rest" };
                ArcGISPortalItem portalItem = new ArcGISPortalItem(portal) 
                    { Id = "00e5e70929e14055ab686df16c842ec1" };

                WebMap webMap = await WebMap.FromPortalItemTaskAsync(portalItem);

                MyWebMapViewModel = await WebMapViewModel.LoadAsync(webMap, portalItem.ArcGISPortal);
            }
            catch (Exception ex)
            {
                if (ex is ServiceException)
                {
                    MessageBox.Show(String.Format("{0}: {1}", (ex as ServiceException).Code.ToString(), (ex as ServiceException).Details[0]), "Error", MessageBoxButton.OK);
                    return;
                }
            }
        }

        public WebMapViewModel MyWebMapViewModel
        {
            get { return _webMapViewModel; }
            set
            {
                if (_webMapViewModel != value)
                {
                    _webMapViewModel = value;
                    OnPropertyChanged("MyWebMapViewModel");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public static Envelope GetMapInitialExtent(DependencyObject obj)
        {
            return (Envelope)obj.GetValue(MapInitialExtentProperty);
        }

        public static void SetMapInitialExtent(DependencyObject obj, Envelope value)
        {
            obj.SetValue(MapInitialExtentProperty, value);
        }

        public static readonly DependencyProperty MapInitialExtentProperty =
          DependencyProperty.RegisterAttached("MapInitialExtent", typeof(Envelope),
          typeof(WebMapViewModelToMapMVVM), new PropertyMetadata(OnMapInitialExtentChanged));

        private static void OnMapInitialExtentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var map = d as ESRI.ArcGIS.Client.Map;
            var extent = e.NewValue as Envelope;
            if (map == null || extent == null)
                return;
            map.ZoomTo(extent);
        }
    }
}
