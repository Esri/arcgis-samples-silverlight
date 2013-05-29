using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using System.Json;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client.Symbols;

namespace ArcGISSilverlightSDK
{
    public partial class SOEElevationData : UserControl
    {
        Draw myDrawObject;
        WebClient webClient;        
        List<Color> colorRanges = new List<Color>(); 

        public SOEElevationData()
        {
            InitializeComponent();
            myDrawObject = new Draw(MyMap)
            {
                DrawMode = DrawMode.Rectangle,
                FillSymbol = LayoutRoot.Resources["DrawFillSymbol"] as ESRI.ArcGIS.Client.Symbols.FillSymbol,
                IsEnabled = true
            };
            myDrawObject.DrawComplete += drawObject_DrawComplete;

            webClient = new WebClient();
            webClient.OpenReadCompleted += wc_OpenReadCompleted;          

            colorRanges.Add(Colors.Blue);
            colorRanges.Add(Colors.Green);
            colorRanges.Add(Colors.Yellow);
            colorRanges.Add(Colors.Orange);
            colorRanges.Add(Colors.Red);
        }

        void drawObject_DrawComplete(object sender, DrawEventArgs e)
        {
            myDrawObject.IsEnabled = false;     

            ESRI.ArcGIS.Client.Geometry.Envelope aoiEnvelope = e.Geometry as ESRI.ArcGIS.Client.Geometry.Envelope;

            string SOEurl = "http://sampleserver4.arcgisonline.com/ArcGIS/rest/services/Elevation/ESRI_Elevation_World/MapServer/exts/ElevationsSOE/ElevationLayers/1/GetElevationData?";

            SOEurl += string.Format("Extent={{\"xmin\" : {0}, \"ymin\" : {1}, \"xmax\" : {2}, \"ymax\" :{3},\"spatialReference\" : {{\"wkid\" : {4}}}}}&Rows={5}&Columns={6}&f=json",
                aoiEnvelope.XMin, aoiEnvelope.YMin, aoiEnvelope.XMax, aoiEnvelope.YMax,
                MyMap.SpatialReference.WKID, HeightTextBox.Text, WidthTextBox.Text);

            webClient.OpenReadAsync(new Uri(SOEurl), aoiEnvelope);
        }

        void wc_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            JsonObject jsonObjectData = (JsonObject)JsonObject.Load(e.Result);
            e.Result.Close();

            if (jsonObjectData.ContainsKey("error"))
            {
                MessageBox.Show((int)jsonObjectData["error"]["code"] + ": " + jsonObjectData["error"]["message"]);
                myDrawObject.IsEnabled = true;
                return;
            }

            JsonArray elevData = (JsonArray)jsonObjectData["data"];

            int thematicMin, thematicMax;
            thematicMin = thematicMax = elevData[0];

            foreach (int elevValue in elevData)
            {
                if (elevValue < thematicMin) thematicMin = elevValue;
                if (elevValue > thematicMax) thematicMax = elevValue;
            }

            int totalRange = thematicMax - thematicMin;
            int portion = totalRange / 5;

            List<Color> cellColor = new List<Color>();

            foreach (int elevValue in elevData)
            {
                int startValue = thematicMin;
                for (int i = 0; i < 5; i++)
                {
                    if (Enumerable.Range(startValue, portion).Contains(elevValue))
                    {
                        cellColor.Add(colorRanges[i]);
                        break;
                    }
                    else if (i == 4)
                        cellColor.Add(colorRanges.Last());

                    startValue = startValue + portion;
                }
            }

            int rows = Convert.ToInt32(HeightTextBox.Text);
            int cols = Convert.ToInt32(WidthTextBox.Text);
            WriteableBitmap writeableBitmapElevation = new WriteableBitmap(rows, cols);

            int cell = 0;

            for (int x = 0; x < rows; x++)
            {
                for (int y = 0; y < cols; y++)
                {
                    SetPixel(writeableBitmapElevation, x, y, 255, cellColor[cell].R, cellColor[cell].G, cellColor[cell].B);
                    cell++;
                }
            }

            myDrawObject.IsEnabled = true;

            ESRI.ArcGIS.Client.ElementLayer elementLayer = MyMap.Layers["MyElementLayer"] as ESRI.ArcGIS.Client.ElementLayer;
            Image MyOutputImage = elementLayer.Children[0] as Image;
            MyOutputImage.Source = writeableBitmapElevation;
            MyOutputImage.SetValue(ESRI.ArcGIS.Client.ElementLayer.EnvelopeProperty, 
                e.UserState as ESRI.ArcGIS.Client.Geometry.Envelope);
        }

        void SetPixel(WriteableBitmap bitmap, int row, int col, int alpha, int red, int green, int blue)
        {
            int index = row * bitmap.PixelWidth + col;
            bitmap.Pixels[index] = alpha << 24 | red << 16 | green << 8 | blue;
        }
    }
}