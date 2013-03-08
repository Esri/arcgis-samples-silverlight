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
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;

namespace ArcGISSilverlightSDK
{
    public partial class SymbolJson : UserControl
    {
        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
                new ESRI.ArcGIS.Client.Projection.WebMercator();

        public SymbolJson()
        {
            InitializeComponent();
            Button_SMS(null, null);
        }

        private void GraphicsLayer_Initialized(object sender, EventArgs e)
        {
            GraphicsLayer graphicsLayer = sender as GraphicsLayer;
            foreach (Graphic g in graphicsLayer.Graphics)
            {                
                g.Geometry = _mercator.FromGeographic(g.Geometry);

                if (g.Geometry is Polygon || g.Geometry is Envelope)
                    JsonTextBoxFillCurrent.Text = (g.Symbol as IJsonSerializable).ToJson();
                else if (g.Geometry is Polyline)
                    JsonTextBoxLineCurrent.Text = (g.Symbol as IJsonSerializable).ToJson();
                else
                    JsonTextBoxMarkerCurrent.Text = (g.Symbol as IJsonSerializable).ToJson();

                g.PropertyChanged += g_PropertyChanged;
            }
        }

        void g_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Symbol")
            {
                Graphic g = sender as Graphic;
                if (g.Geometry is Polygon || g.Geometry is Envelope)
                    JsonTextBoxFillCurrent.Text = (g.Symbol as IJsonSerializable).ToJson();
                else if (g.Geometry is Polyline)
                    JsonTextBoxLineCurrent.Text = (g.Symbol as IJsonSerializable).ToJson();
                else
                    JsonTextBoxMarkerCurrent.Text = (g.Symbol as IJsonSerializable).ToJson();  
            }
        }

        private void Button_Load(object sender, RoutedEventArgs e)
        {
            try
            {
                Symbol symbol = Symbol.FromJson(JsonTextBox.Text);

                GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;

                foreach (Graphic g in graphicsLayer.Graphics)
                {
                    if ((g.Geometry is Polygon || g.Geometry is Envelope) && symbol is FillSymbol)
                        g.Symbol = symbol;
                    else if (g.Geometry is Polyline && symbol is LineSymbol)
                        g.Symbol = symbol;
                    else if (g.Geometry is MapPoint && symbol is MarkerSymbol)
                        g.Symbol = symbol;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Deserializing JSON failed", MessageBoxButton.OK);
            }
        }

        private void Button_SMS(object sender, RoutedEventArgs e)
        {
            string jsonString = @"{
    ""type"": ""esriSMS"",
    ""style"": ""esriSMSSquare"",
    ""color"": [76,115,0,255],
    ""size"": 8,
    ""angle"": 0,
    ""xoffset"": 0,
    ""yoffset"": 0,
    ""outline"": 
    {
        ""color"": [152,230,0,255],
        ""width"": 1
    }
}
";
            JsonTextBox.Text = jsonString;
        }

        private void Button_PMS(object sender, RoutedEventArgs e)
        {
            string jsonString = @"{
	""type"" : ""esriPMS"", 
	""url"" : ""http://static.arcgis.com/images/Symbols/Basic/GreenStickpin.png"", 
	""contentType"" : ""image/png"", 
	""color"" : null, 
	""width"" : 28, 
	""height"" : 28, 
	""angle"" : 0, 
	""xoffset"" : 0, 
	""yoffset"" : 0
}
";
            JsonTextBox.Text = jsonString;
        }

        private void Button_SLS(object sender, RoutedEventArgs e)
        {
            string jsonString = @"{
    ""type"": ""esriSLS"",
    ""style"": ""esriSLSDot"",
    ""color"": [115,76,0,255],
    ""width"": 2
}
";
            JsonTextBox.Text = jsonString;
        }

        private void Button_SFS(object sender, RoutedEventArgs e)
        {
            string jsonString = @"{
    ""type"": ""esriSFS"",
    ""style"": ""esriSFSSolid"",
    ""color"": [250,76,0,150],
    ""outline"": 
    {
        ""type"": ""esriSLS"",
        ""style"": ""esriSLSSolid"",
        ""color"": [110,110,110,255],
        ""width"": 2
    }
}
";
            JsonTextBox.Text = jsonString;
        }

        private void Button_PFS(object sender, RoutedEventArgs e)
        {
            string jsonString = @"{
	""type"" : ""esriPFS"", 
	""url"" : ""http://static.arcgis.com/images/Symbols/Transportation/AmberBeacon.png"", 
	""contentType"" : ""image/png"", 
	""color"" : null, 
	""outline"" : 
	{
		""type"" : ""esriSLS"", 
		""style"" : ""esriSLSSolid"", 
		""color"" : [110,110,110,255], 
		""width"" : 1
	}, 
	""width"" : 12, 
	""height"" : 12, 
	""angle"" : 0, 
	""xoffset"" : 0, 
	""yoffset"" : 0, 
	""xscale"" : 1, 
	""yscale"" : 1
  }

";
            JsonTextBox.Text = jsonString;
        }
    }
}
