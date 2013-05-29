using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class Simplify : UserControl
    {
        private Graphic _unsimplifiedGraphic = new Graphic();

        public Simplify()
        {
            InitializeComponent();

            MyMap.Layers.LayersInitialized += Layers_LayersInitialized;
        }

        void Layers_LayersInitialized(object sender, EventArgs args)
        {
            if ((MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer).Graphics.Count < 1)
                drawPolygon();
        }

        private void drawPolygon()
        {
            MapPoint center = MyMap.Extent.GetCenter();
            double lat = center.Y;
            double lon = center.X + 300;
            double latOffset = 300;
            double lonOffset = 300;
            ESRI.ArcGIS.Client.Geometry.PointCollection points = new ESRI.ArcGIS.Client.Geometry.PointCollection()
            {
                new MapPoint(lon - lonOffset, lat),
                new MapPoint(lon, lat + latOffset),
                new MapPoint(lon + lonOffset, lat),
                new MapPoint(lon, lat - latOffset),
                new MapPoint(lon - lonOffset, lat),
                new MapPoint(lon - 2 * lonOffset, lat + latOffset),
                new MapPoint(lon - 3 * lonOffset, lat),
                new MapPoint(lon - 2 * lonOffset, lat - latOffset),
                new MapPoint(lon - 1.5 * lonOffset, lat + latOffset),
                new MapPoint(lon - lonOffset, lat)
            };
            ESRI.ArcGIS.Client.Geometry.Polygon polygon = new ESRI.ArcGIS.Client.Geometry.Polygon();
            polygon.Rings.Add(points);
            polygon.SpatialReference = MyMap.SpatialReference;
            _unsimplifiedGraphic.Geometry = polygon;

            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            _unsimplifiedGraphic.Symbol = LayoutRoot.Resources["PolygonFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
            graphicsLayer.Graphics.Add(_unsimplifiedGraphic);
        }

        private void QueryOnlyButton_Click(object sender, RoutedEventArgs e)
        {
            doQuery(_unsimplifiedGraphic.Geometry);
        }

        private void SimplifyAndQueryButton_Click(object sender, RoutedEventArgs e)
        {
            GeometryService geometryService =
              new GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
            geometryService.SimplifyCompleted += GeometryService_SimplifyCompleted;
            geometryService.Failed += GeometryService_Failed;

            List<Graphic> graphicList = new List<Graphic>();
            graphicList.Add(_unsimplifiedGraphic);
            geometryService.SimplifyAsync(graphicList);
        }

        private void GeometryService_SimplifyCompleted(object sender, GraphicsEventArgs args)
        {
            doQuery(args.Results[0].Geometry);
        }

        private void doQuery(ESRI.ArcGIS.Client.Geometry.Geometry geometry)
        {
            QueryTask queryTask = new QueryTask("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/BloomfieldHillsMichigan/Parcels/MapServer/2");
            queryTask.ExecuteCompleted += QueryTask_ExecuteCompleted;
            queryTask.Failed += QueryTask_Failed;

            ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query()
            {
                Geometry = geometry,
                SpatialRelationship = SpatialRelationship.esriSpatialRelContains,
                OutSpatialReference = MyMap.SpatialReference,
                ReturnGeometry = true
            };
            query.OutFields.Add("*");
            queryTask.ExecuteAsync(query);
        }

        private void QueryTask_ExecuteCompleted(object sender, QueryEventArgs args)
        {
            if (args.FeatureSet == null)
                return;
            FeatureSet featureSet = args.FeatureSet;
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.Graphics.Clear();

            if (featureSet != null && featureSet.Features.Count > 0)
            {
                foreach (Graphic feature in featureSet.Features)
                {
                    ESRI.ArcGIS.Client.Graphic graphic = new ESRI.ArcGIS.Client.Graphic()
                    {
                        Geometry = feature.Geometry,
                        Symbol = LayoutRoot.Resources["ParcelFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol
                    };
                    graphicsLayer.Graphics.Add(graphic);
                }
            }
            graphicsLayer.Graphics.Add(_unsimplifiedGraphic);
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
        }

        private void QueryTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Query failed: " + e.Error);
        }
    }
}
