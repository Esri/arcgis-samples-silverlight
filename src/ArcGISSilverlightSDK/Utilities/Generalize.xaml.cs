using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISSilverlightSDK
{
    public partial class Generalize : UserControl
    {
        GraphicsLayer originalGraphicsLayer;

        public Generalize()
        {
            InitializeComponent();

            MyMap.Layers.LayersInitialized += Layers_LayersInitialized;

            originalGraphicsLayer = MyMap.Layers["OriginalLineGraphicsLayer"] as GraphicsLayer;
        }

        void Layers_LayersInitialized(object sender, EventArgs args)
        {
            if (originalGraphicsLayer != null && originalGraphicsLayer.Graphics.Count == 0)
            {
                QueryTask queryTask =
                    new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/1");
                Query query = new Query();
                query.ReturnGeometry = true;
                query.OutSpatialReference = MyMap.SpatialReference;
                query.Where = "NAME = 'Mississippi'";

                queryTask.ExecuteCompleted += queryTask_ExecuteCompleted;
                queryTask.Failed += queryTask_Failed;
                queryTask.ExecuteAsync(query, query.OutSpatialReference);
            }
        }

        void queryTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Query error: " + e.Error);
        }

        void queryTask_ExecuteCompleted(object sender, QueryEventArgs e)
        {
            GraphicsLayer originalGraphicsLayer = MyMap.Layers["OriginalLineGraphicsLayer"] as GraphicsLayer;
            foreach (Graphic g in e.FeatureSet.Features)
            {
                g.Symbol = LayoutRoot.Resources["DefaultLineSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                g.Geometry.SpatialReference = e.UserState as SpatialReference;
                originalGraphicsLayer.Graphics.Add(g);

                foreach (ESRI.ArcGIS.Client.Geometry.PointCollection pc in (g.Geometry as Polyline).Paths)
                {
                    foreach (MapPoint point in pc)
                    {
                        Graphic vertice = new Graphic()
                        {
                            Symbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol,
                            Geometry = point
                        };
                        originalGraphicsLayer.Graphics.Add(vertice);
                    }
                }
            }
            GeneralizeButton.IsEnabled = true;
        }

        private void GeneralizeButton_Click(object sender, RoutedEventArgs e)
        {
            GeneralizeButton.IsEnabled = false;
            SliderStackPanel.Visibility = Visibility.Collapsed;

            GraphicsLayer originalGraphicsLayer = MyMap.Layers["OriginalLineGraphicsLayer"] as GraphicsLayer;

            GeometryService geometryService =
                        new GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
            geometryService.GeneralizeCompleted += GeometryService_GeneralizeCompleted;
            geometryService.Failed += GeometryService_Failed;
            
            GeneralizeParameters generalizeParameters = new GeneralizeParameters()
            {
                DeviationUnit = LinearUnit.SurveyMile,
                MaxDeviation = 10
            };

            geometryService.GeneralizeAsync(new List<Graphic>() { originalGraphicsLayer.Graphics[0] }, generalizeParameters);
        }

        void GeometryService_GeneralizeCompleted(object sender, GraphicsEventArgs e)
        {
            GraphicsLayer generalizedGraphicsLayer = MyMap.Layers["GeneralizedLineGraphicsLayer"] as GraphicsLayer;
            generalizedGraphicsLayer.Graphics.Clear();

            foreach (Graphic g in e.Results)
            {
                g.Symbol = LayoutRoot.Resources["NewLineSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                generalizedGraphicsLayer.Graphics.Add(g);

                ESRI.ArcGIS.Client.Geometry.Polyline p = g.Geometry as ESRI.ArcGIS.Client.Geometry.Polyline;

                foreach (ESRI.ArcGIS.Client.Geometry.PointCollection pc in p.Paths)
                {
                    foreach (MapPoint point in pc)
                    {
                        Graphic vertice = new Graphic()
                        {
                            Symbol = LayoutRoot.Resources["NewMarkerSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol,
                            Geometry = point
                        };
                        generalizedGraphicsLayer.Graphics.Add(vertice);
                    }
                }
            }
            generalizedGraphicsLayer.Opacity = 0.75;
            SliderStackPanel.Visibility = Visibility.Visible;
            GeneralizeButton.IsEnabled = true;
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
        }

        private void GeneralizeLayerOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MyMap != null)
                MyMap.Layers["GeneralizedLineGraphicsLayer"].Opacity = e.NewValue;
        }
    }
}
