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
        GraphicsLayer graphicsLayer;
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
            myDrawObject.DrawBegin += drawObject_OnDrawBegin;

            webClient = new WebClient();
            webClient.OpenReadCompleted += wc_OpenReadCompleted;

            graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;

            colorRanges.Add(Colors.Blue);
            colorRanges.Add(Colors.Green);
            colorRanges.Add(Colors.Yellow);
            colorRanges.Add(Colors.Orange);
            colorRanges.Add(Colors.Red);
        }

        private void drawObject_OnDrawBegin(object sender, EventArgs args)
        {
            graphicsLayer.ClearGraphics();
            ElevationView.Visibility = Visibility.Collapsed;
        }

        void drawObject_DrawComplete(object sender, DrawEventArgs e)
        {
            if (e.Geometry.Extent.Height == 0 | e.Geometry.Extent.Width == 0)
            {
                MessageBox.Show("Please click and drag a box to define an extent", "Error", MessageBoxButton.OK);
                return;
            }

            myDrawObject.IsEnabled = false;

            ESRI.ArcGIS.Client.Graphic graphic = new ESRI.ArcGIS.Client.Graphic()
            {
                Geometry = e.Geometry,
                Symbol = LayoutRoot.Resources["DrawFillSymbol"] as Symbol
            };
            graphicsLayer.Graphics.Add(graphic);

            ESRI.ArcGIS.Client.Geometry.Envelope aoiEnvelope = e.Geometry as ESRI.ArcGIS.Client.Geometry.Envelope;

            string SOEurl = "http://sampleserver4.arcgisonline.com/ArcGIS/rest/services/Elevation/ESRI_Elevation_World/MapServer/exts/ElevationsSOE/ElevationLayers/1/GetElevationData?";

            SOEurl += string.Format(System.Globalization.CultureInfo.InvariantCulture, "Extent={{\"xmin\" : {0}, \"ymin\" : {1}, \"xmax\" : {2}, \"ymax\" :{3},\"spatialReference\" : {{\"wkid\" : {4}}}}}&Rows={5}&Columns={6}&f=json",
                aoiEnvelope.XMin, aoiEnvelope.YMin, aoiEnvelope.XMax, aoiEnvelope.YMax,
                MyMap.SpatialReference.WKID, HeightTextBox.Text, WidthTextBox.Text);

            webClient.OpenReadAsync(new Uri(SOEurl));
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

            ElevationView.Visibility = System.Windows.Visibility.Visible;
            ElevationImage.Source = writeableBitmapElevation;
            myDrawObject.IsEnabled = true;
        }

        void SetPixel(WriteableBitmap bitmap, int row, int col, int alpha, int red, int green, int blue)
        {
            int index = row * bitmap.PixelWidth + col;
            bitmap.Pixels[index] = alpha << 24 | red << 16 | green << 8 | blue;
        }

        private void CloseProfileImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ElevationView.Visibility = Visibility.Collapsed;
        }

        private void SizeProfileImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ElevationImage.Width == 150)
                ElevationImage.Width = ElevationImage.Height = 300;
            else
                ElevationImage.Width = ElevationImage.Height = 150;
        }
    }
}