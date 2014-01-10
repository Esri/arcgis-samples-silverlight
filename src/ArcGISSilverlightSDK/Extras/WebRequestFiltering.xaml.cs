using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;

namespace ArcGISSilverlightSDK
{
    public partial class WebRequestFiltering : UserControl
    {
        public WebRequestFiltering()
        {
            // Custom parameters for any url, permanent set for the lifetime of the app 
            //WebRequest.RegisterPrefix(
            //    "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer",
            //    new MyHttpRequestCreator(
            //        new Dictionary<string, string>() { 
            //            {"myParameterName", "0"} 
            //        }));

            InitializeComponent();

            var layer = (MyMap.Layers[1] as ArcGISDynamicMapServiceLayer);

            // Set value for initial request for dynamic map service layer info 
            SetCustomParameters(layer, new Dictionary<string, string> { { "myParameterName", "1" } });
        }

        private void ArcGISDynamicMapServiceLayer_Initialized(object sender, EventArgs e)
        {
            var layer = (sender as ArcGISDynamicMapServiceLayer);

            // Set value for subsequent dynamic map service layer requests  (eg. export)
            SetCustomParameters(layer, new Dictionary<string, string> { { "myParameterName", "2" } });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Change parameter value for subsequent requests to dynamic map service layer
            var layer = (MyMap.Layers[1] as ArcGISDynamicMapServiceLayer);
            SetCustomParameters(layer, new Dictionary<string, string> { { "myParameterName", "3" } });
            layer.Refresh();
        }

        public static IEnumerable<KeyValuePair<string, string>> GetCustomParameters(DependencyObject obj)
        {
            return (IEnumerable<KeyValuePair<string, string>>)obj.GetValue(CustomParametersProperty);
        }

        public static void SetCustomParameters(DependencyObject obj, IEnumerable<KeyValuePair<string, string>> value)
        {
            obj.SetValue(CustomParametersProperty, value);
        }

        private static readonly DependencyProperty WebRequestCreatorProperty =
            DependencyProperty.RegisterAttached("WebRequestCreator", typeof(MyHttpRequestCreator), typeof(WebRequestFiltering), null);

        public static readonly DependencyProperty CustomParametersProperty =
            DependencyProperty.RegisterAttached("CustomParameters", typeof(IEnumerable<KeyValuePair<string, string>>), typeof(WebRequestFiltering), new PropertyMetadata(null, OnCustomParametersPropertyChanged));

        private static void OnCustomParametersPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ArcGISDynamicMapServiceLayer)
            {
                var newValue = e.NewValue as IEnumerable<KeyValuePair<string, string>>;
                var creator = (d as ArcGISDynamicMapServiceLayer).GetValue(WebRequestCreatorProperty) as MyHttpRequestCreator;
                if (creator == null)
                {
                    creator = new MyHttpRequestCreator(newValue);
                    // Register prefix for a url with a custom http request creator
                    WebRequest.RegisterPrefix((d as ArcGISDynamicMapServiceLayer).Url, creator);
                    (d as ArcGISDynamicMapServiceLayer).SetValue(WebRequestCreatorProperty, creator);
                }
                else
                    creator.SetCallback(newValue);
            }
        }       
    }

    public class MyHttpRequestCreator : IWebRequestCreate
    {
        public MyHttpRequestCreator(IEnumerable<KeyValuePair<string, string>> parameters)
        {
            SetCallback(parameters);
        }

        public void SetCallback(IEnumerable<KeyValuePair<string, string>> parameters)
        {
            if (parameters == null)
                Callback = null;
            else
                Callback = (uri) =>
                {
                    string url = uri.OriginalString;
                    if (url.Contains("?"))
                    {
                        if (!url.EndsWith("&"))
                            url += "&";
                    }
                    else url += "?";
                    foreach (var p in parameters)
                    {
                        url += string.Format("{0}={1}&", p.Key, Uri.EscapeDataString(p.Value));
                    }

                    return new Uri(url, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
                };
        }

        public WebRequest Create(Uri uri)
        {
            return System.Net.Browser.WebRequestCreator.BrowserHttp.Create(Callback == null ? uri : Callback(uri));
        }

        public Func<Uri, Uri> Callback { get; set; }
    }
}
