using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Geometry;
using System.Windows.Media;

namespace ArcGISSilverlightSDK
{
    public partial class Offset : UserControl
    {
        private Draw MyDrawObject;
        private GraphicsLayer parcelGraphicsLayer;
        private GraphicsLayer offsetGraphicsLayer;
        private GeometryService geometryService;

        public Offset()
        {
            InitializeComponent();

            MyMap.Layers.LayersInitialized += Layers_LayersInitialized;

            MyMap.MinimumResolution = double.Epsilon;

            MyDrawObject = new Draw(MyMap)
            {
                DrawMode = DrawMode.Point,
                IsEnabled = false,
            };
            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;

            parcelGraphicsLayer = MyMap.Layers["ParcelsGraphicsLayer"] as GraphicsLayer;
            offsetGraphicsLayer = MyMap.Layers["OffsetGraphicsLayer"] as GraphicsLayer;
        }

        void Layers_LayersInitialized(object sender, EventArgs args)
        {
            if (parcelGraphicsLayer != null && parcelGraphicsLayer.Graphics.Count == 0)
            {
                QueryTask queryTask =
                    new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1");
                Query query = new Query();
                query.Geometry = new Envelope(-83.3188395774275, 42.61428312652851, -83.31295664068958, 42.61670913269855) { SpatialReference = new SpatialReference(4326) };
                query.ReturnGeometry = true;
                query.OutSpatialReference = MyMap.SpatialReference;
                queryTask.ExecuteCompleted += queryTask_ExecuteCompleted;
                queryTask.Failed += queryTask_Failed;
                queryTask.ExecuteAsync(query);
            }
        }

        void queryTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Query error: " + e.Error);
        }

        void queryTask_ExecuteCompleted(object sender, QueryEventArgs e)
        {
            foreach (Graphic g in e.FeatureSet.Features)
            {
                g.Symbol = LayoutRoot.Resources["BlueFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                g.Geometry.SpatialReference = MyMap.SpatialReference;
                parcelGraphicsLayer.Graphics.Add(g);
            }
            MyDrawObject.IsEnabled = true;
        }

        private void MyDrawObject_DrawComplete(object sender, DrawEventArgs args)
        {
            MyDrawObject.IsEnabled = false;
            offsetGraphicsLayer.ClearGraphics();

            ESRI.ArcGIS.Client.Geometry.MapPoint point = args.Geometry as ESRI.ArcGIS.Client.Geometry.MapPoint;
            point.SpatialReference = MyMap.SpatialReference;
            System.Windows.Point screenPnt = MyMap.MapToScreen(point);

            // Account for difference between Map and application origin
            GeneralTransform generalTransform = MyMap.TransformToVisual(Application.Current.RootVisual);
            System.Windows.Point transformScreenPnt = generalTransform.Transform(screenPnt);

            IEnumerable<Graphic> selected =
                parcelGraphicsLayer.FindGraphicsInHostCoordinates(transformScreenPnt);

            List<Graphic> selectedList = selected.ToList();

            if (selectedList.Count < 1)
            {
                MyDrawObject.IsEnabled = true;
                return;
            }

            geometryService =
              new GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");

            geometryService.OffsetCompleted += GeometryService_OffsetCompleted;
            geometryService.Failed += GeometryService_Failed;

            OffsetParameters offsetParameters = new OffsetParameters()
            {
                BevelRatio = 1.1,
                OffsetDistance = -30,
                OffsetHow = GeometryOffset.Bevelled,
                OffsetUnit = LinearUnit.Meter,
                Simplify = true
            };

            geometryService.OffsetAsync(selectedList, offsetParameters);
        }

        void GeometryService_OffsetCompleted(object sender, GraphicsEventArgs e)
        {            
            foreach (Graphic g in e.Results)
            {
                g.Symbol = LayoutRoot.Resources["CyanFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                offsetGraphicsLayer.Graphics.Add(g);
            }
            MyDrawObject.IsEnabled = true;
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
        }
    }
}
