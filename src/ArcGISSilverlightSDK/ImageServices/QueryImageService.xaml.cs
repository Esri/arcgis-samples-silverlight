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
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class QueryImageService : UserControl
    {
        private Draw myDrawObject;
        private GraphicsLayer footprintsGraphicsLayer;

        public QueryImageService()
        {
            InitializeComponent();

            myDrawObject = new Draw(MyMap)
            {
                DrawMode = DrawMode.Rectangle,
                FillSymbol = LayoutRoot.Resources["DrawFillSymbol"] as ESRI.ArcGIS.Client.Symbols.FillSymbol,
                IsEnabled = true
            };

            myDrawObject.DrawComplete += MyDrawObject_DrawComplete;
            footprintsGraphicsLayer = MyMap.Layers["FootprintsGraphicsLayer"] as GraphicsLayer;
        }

        private void MyDrawObject_DrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs args)
        {
            myDrawObject.IsEnabled = false;

            QueryTask queryTask = new QueryTask("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Portland/Aerial/ImageServer/query");
            queryTask.ExecuteCompleted += QueryTask_ExecuteCompleted;
            queryTask.Failed += QueryTask_Failed;

            Query query = new ESRI.ArcGIS.Client.Tasks.Query();
            query.OutFields.Add("*");
            query.Geometry = args.Geometry;
            query.ReturnGeometry = true;
            query.OutSpatialReference = MyMap.SpatialReference;
            query.Where = "Category = 1";

            queryTask.ExecuteAsync(query);
        }

        private void QueryTask_ExecuteCompleted(object sender, ESRI.ArcGIS.Client.Tasks.QueryEventArgs args)
        {
            FeatureSet featureSet = args.FeatureSet;

            if (featureSet == null || featureSet.Features.Count < 1)
            {
                MessageBox.Show("No features returned from query");
                return;
            }
            
            if (featureSet != null && featureSet.Features.Count > 0)
            {
                foreach (Graphic graphic in featureSet.Features)
                {
                    graphic.Symbol = LayoutRoot.Resources["FootprintFillSymbol"] as ESRI.ArcGIS.Client.Symbols.FillSymbol;;
                    footprintsGraphicsLayer.Graphics.Add(graphic);
                }
            }

            myDrawObject.IsEnabled = true;
        }

        private void QueryTask_Failed(object sender, TaskFailedEventArgs args)
        {
            MessageBox.Show("Query failed: " + args.Error);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            footprintsGraphicsLayer.Graphics.Clear();
        }
    }
}
