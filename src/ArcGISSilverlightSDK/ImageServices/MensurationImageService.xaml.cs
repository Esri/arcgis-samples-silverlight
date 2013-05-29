using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class MensurationImageService : UserControl
    {
        Draw draw;
        GraphicsLayer drawGraphicsLayer;
        MensurationTask mensurationTask;
        MensurationOperation mensurationOperation;
        int clickCount = -1;

        public MensurationImageService()
        {
            InitializeComponent();

            draw = new Draw(MyMap);
            draw.DrawComplete += drawtool_DrawComplete;
            //draw.VertexAdded += new EventHandler<VertexAddedEventArgs>(drawtool_VertexAdded);
            drawGraphicsLayer = MyMap.Layers["DrawGraphicsLayer"] as GraphicsLayer;

            mensurationTask =
                new MensurationTask((MyMap.Layers["TiledImageServiceLayer"] as ArcGISTiledMapServiceLayer).Url);

            mensurationTask.AreaAndPerimeterCompleted += mt_TaskCompleted;
            mensurationTask.CentroidCompleted += mt_TaskCompleted;
            mensurationTask.DistanceAndAngleCompleted += mt_TaskCompleted;
            mensurationTask.HeightFromBaseAndTopCompleted += mt_TaskCompleted;
            mensurationTask.HeightFromBaseAndTopShadowCompleted += mt_TaskCompleted;
            mensurationTask.HeightFromTopAndTopShadowCompleted += mt_TaskCompleted;
            mensurationTask.PointCompleted += mt_TaskCompleted;
            mensurationTask.Failed += mt_TaskFailed;
        }

        void mt_TaskCompleted(object sender, TaskEventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            if (e is MensurationPointEventArgs)
            {
                MensurationPointResult result = (e as MensurationPointEventArgs).Result;

                if (result != null && result.Point != null)
                {
                    sb.Append(result.Point);
                    sb.Append("\n");
                }
            }
            else if (e is MensurationHeightEventArgs)
            {
                MensurationHeightResult result = (e as MensurationHeightEventArgs).Result;

                if (result != null)
                {
                    if (result.Height != null)
                    {
                        sb.Append("Height\n");
                        sb.AppendFormat("Value:\t\t{0}\n", result.Height.Value);
                        sb.AppendFormat("Display Value:\t{0}\n", result.Height.DisplayValue);
                        sb.AppendFormat("Uncertainty:\t{0}\n", result.Height.Uncertainty);
                        sb.AppendFormat("Unit:\t\t{0}\n", result.Height.LinearUnit);
                        sb.Append("\n");
                    }
                }
            }
            else if (e is MensurationLengthEventArgs)
            {
                MensurationLengthResult result = (e as MensurationLengthEventArgs).Result;

                if (result != null)
                {
                    if (result.Distance != null)
                    {
                        sb.Append("Distance\n");
                        sb.AppendFormat("Value:\t\t{0}\n", result.Distance.Value);
                        sb.AppendFormat("Display Value:\t{0}\n", result.Distance.DisplayValue);
                        sb.AppendFormat("Uncertainty:\t{0}\n", result.Distance.Uncertainty);
                        sb.AppendFormat("Unit:\t\t{0}\n", result.Distance.LinearUnit);
                        sb.Append("\n");
                    }
                    if (result.AzimuthAngle != null)
                    {
                        sb.Append("Azimuth Angle\n");
                        sb.AppendFormat("Value:\t\t{0}\n", result.AzimuthAngle.Value);
                        sb.AppendFormat("Display Value:\t{0}\n", result.AzimuthAngle.DisplayValue);
                        sb.AppendFormat("Uncertainty:\t{0}\n", result.AzimuthAngle.Uncertainty);
                        sb.AppendFormat("Unit:\t\t{0}\n", result.AzimuthAngle.AngularUnit);
                        sb.Append("\n");
                    }
                    if (result.ElevationAngle != null)
                    {
                        sb.Append("Elevation Angle\n");
                        sb.AppendFormat("Value:\t\t{0}\n", result.ElevationAngle.Value);
                        sb.AppendFormat("Display Value:\t{0}\n", result.ElevationAngle.DisplayValue);
                        sb.AppendFormat("Uncertainty:\t{0}\n", result.ElevationAngle.Uncertainty);
                        sb.AppendFormat("Unit:\t\t{0}\n", result.ElevationAngle.AngularUnit);
                        sb.Append("\n");
                    }
                }
            }
            else if (e is MensurationAreaEventArgs)
            {
                MensurationAreaResult result = (e as MensurationAreaEventArgs).Result;

                if (result != null)
                {
                    if (result.Area != null)
                    {
                        sb.Append("Area\n");
                        sb.AppendFormat("Value:\t\t{0}\n", result.Area.Value);
                        sb.AppendFormat("Display Value:\t{0}\n", result.Area.DisplayValue);
                        sb.AppendFormat("Uncertainty:\t{0}\n", result.Area.Uncertainty);
                        sb.AppendFormat("Unit:\t\t{0}\n", result.Area.AreaUnit);
                        sb.Append("\n");
                    }
                    if (result.Perimeter != null)
                    {
                        sb.Append("Perimeter\n");
                        sb.AppendFormat("Value:\t\t{0}\n", result.Perimeter.Value);
                        sb.AppendFormat("Display Value:\t{0}\n", result.Perimeter.DisplayValue);
                        sb.AppendFormat("Uncertainty:\t{0}\n", result.Perimeter.Uncertainty);
                        sb.AppendFormat("Unit:\t\t{0}\n", result.Perimeter.LinearUnit);
                        sb.Append("\n");
                    }
                }
            }

            MessageBox.Show(sb.ToString());
        }

        void mt_TaskFailed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show(string.Format(e.Error.Message));
        }

        void drawtool_VertexAdded(object sender, VertexAddedEventArgs e)
        {
            if (clickCount > 0)
                if (--clickCount == 0)
                {
                    draw.CompleteDraw();
                    clickCount = -1;
                }
        }

        private void ClearMeasureToolClick(object sender, RoutedEventArgs e)
        {
            drawGraphicsLayer.Graphics.Clear();
            draw.IsEnabled = false;
        }

        void drawtool_DrawComplete(object sender, DrawEventArgs e)
        {
            e.Geometry.SpatialReference = MyMap.SpatialReference;
            Graphic graphic = new Graphic() { Geometry = e.Geometry };

            if (e.Geometry is MapPoint)
                graphic.Symbol = LayoutRoot.Resources["DrawPointSymbol"] as SimpleMarkerSymbol;
            else if (e.Geometry is Polyline)
                graphic.Symbol = LayoutRoot.Resources["DrawPolylineSymbol"] as SimpleLineSymbol;
            else if (e.Geometry is Polygon || e.Geometry is Envelope)
                graphic.Symbol = LayoutRoot.Resources["DrawPolygonSymbol"] as SimpleFillSymbol;

            drawGraphicsLayer.Graphics.Add(graphic);
            draw.IsEnabled = false;

            Geometry fromGeometry = null;
            Geometry toGeometry = null;
            switch (mensurationOperation)
            {
                case MensurationOperation.DistanceAndAngle:
                case MensurationOperation.HeightFromBaseAndTop:
                case MensurationOperation.HeightFromBaseAndTopShadow:
                case MensurationOperation.HeightFromTopAndTopShadow:
                    fromGeometry = (e.Geometry as Polyline).Paths[0][0];
                    fromGeometry.SpatialReference = MyMap.SpatialReference;
                    toGeometry = (e.Geometry as Polyline).Paths[0][1];
                    toGeometry.SpatialReference = MyMap.SpatialReference;
                    break;
                case MensurationOperation.AreaAndPerimeter:
                case MensurationOperation.Centroid:
                case MensurationOperation.Point:
                    fromGeometry = e.Geometry;
                    break;
            }

            esriUnits? LinearUnit = null;
            switch (comboLinearUnit.SelectedIndex)
            {
                case 0:
                    LinearUnit = esriUnits.esriUnknownUnits;
                    break;
                case 1:
                    LinearUnit = esriUnits.esriInches;
                    break;
                case 2:
                    LinearUnit = esriUnits.esriPoints;
                    break;
                case 3:
                    LinearUnit = esriUnits.esriFeet;
                    break;
                case 4:
                    LinearUnit = esriUnits.esriYards;
                    break;
                case 5:
                    LinearUnit = esriUnits.esriMiles;
                    break;
                case 6:
                    LinearUnit = esriUnits.esriNauticalMiles;
                    break;
                case 7:
                    LinearUnit = esriUnits.esriMillimeters;
                    break;
                case 8:
                    LinearUnit = esriUnits.esriCentimeters;
                    break;
                case 9:
                    LinearUnit = esriUnits.esriMeters;
                    break;
                case 10:
                    LinearUnit = esriUnits.esriKilometers;
                    break;
                case 11:
                    LinearUnit = esriUnits.esriDecimalDegrees;
                    break;
                case 12:
                    LinearUnit = esriUnits.esriDecimeters;
                    break;
            }

            DirectionUnit AngularUnit = DirectionUnit.Default;
            switch (comboAngularUnit.SelectedIndex)
            {
                case 0:
                    AngularUnit = DirectionUnit.Default;
                    break;
                case 1:
                    AngularUnit = DirectionUnit.Radians;
                    break;
                case 2:
                    AngularUnit = DirectionUnit.DecimalDegrees;
                    break;
                case 3:
                    AngularUnit = DirectionUnit.DegreesMinutesSeconds;
                    break;
                case 4:
                    AngularUnit = DirectionUnit.Gradians;
                    break;
                case 5:
                    AngularUnit = DirectionUnit.Gons;
                    break;
            }

            AreaUnit AreaUnits = AreaUnit.Default;
            switch (comboAreaUnit.SelectedIndex)
            {
                case 0:
                    AreaUnits = AreaUnit.Default;
                    break;
                case 1:
                    AreaUnits = AreaUnit.SquareInches;
                    break;
                case 2:
                    AreaUnits = AreaUnit.SquareFeet;
                    break;
                case 3:
                    AreaUnits = AreaUnit.SquareYards;
                    break;
                case 4:
                    AreaUnits = AreaUnit.Acres;
                    break;
                case 5:
                    AreaUnits = AreaUnit.SquareMiles;
                    break;
                case 6:
                    AreaUnits = AreaUnit.SquareMillimeters;
                    break;
                case 7:
                    AreaUnits = AreaUnit.SquareCentimeters;
                    break;
                case 8:
                    AreaUnits = AreaUnit.SquareDecimeters;
                    break;
                case 9:
                    AreaUnits = AreaUnit.SquareMeters;
                    break;
                case 10:
                    AreaUnits = AreaUnit.Ares;
                    break;
                case 11:
                    AreaUnits = AreaUnit.Hectares;
                    break;
                case 12:
                    AreaUnits = AreaUnit.SquareKilometers;
                    break;
            }

            if (!mensurationTask.IsBusy)
            {
                switch (mensurationOperation)
                {
                    case MensurationOperation.AreaAndPerimeter:
                        MensurationAreaParameter p1 = new MensurationAreaParameter();
                        p1.LinearUnit = LinearUnit;
                        p1.AreaUnits = AreaUnits;
                        mensurationTask.AreaAndPerimeterAsync(fromGeometry as Polygon, p1);
                        break;
                    case MensurationOperation.Centroid:
                        MensurationPointParameter p3 = new MensurationPointParameter();
                        mensurationTask.CentroidAsync(fromGeometry as Polygon, p3);
                        break;
                    case MensurationOperation.DistanceAndAngle:
                        MensurationLengthParameter p5 = new MensurationLengthParameter();
                        p5.LinearUnit = LinearUnit;
                        p5.AngularUnit = AngularUnit;
                        mensurationTask.DistanceAndAngleAsync(fromGeometry as MapPoint, toGeometry as MapPoint, p5);
                        break;
                    case MensurationOperation.Point:
                        MensurationPointParameter p7 = new MensurationPointParameter();
                        mensurationTask.PointAsync(fromGeometry as MapPoint, p7);
                        break;
                    case MensurationOperation.HeightFromBaseAndTop:
                        MensurationHeightParameter p9 = new MensurationHeightParameter();
                        p9.LinearUnit = LinearUnit;
                        mensurationTask.HeightFromBaseAndTopAsync(fromGeometry as MapPoint, toGeometry as MapPoint, p9);
                        break;
                    case MensurationOperation.HeightFromBaseAndTopShadow:
                        MensurationHeightParameter p10 = new MensurationHeightParameter();
                        p10.LinearUnit = LinearUnit;
                        mensurationTask.HeightFromBaseAndTopShadowAsync(fromGeometry as MapPoint, toGeometry as MapPoint, p10);
                        break;
                    case MensurationOperation.HeightFromTopAndTopShadow:
                        MensurationHeightParameter p11 = new MensurationHeightParameter();
                        p11.LinearUnit = LinearUnit;
                        mensurationTask.HeightFromTopAndTopShadowAsync(fromGeometry as MapPoint, toGeometry as MapPoint, p11);
                        break;
                }
            }
        }

        private void ActivateMeasureToolClick(object sender, RoutedEventArgs e)
        {
            drawGraphicsLayer.Graphics.Clear();
            draw.IsEnabled = true;

            Button btn = sender as Button;
            switch ((string)btn.Tag)
            {
                case "AaP":
                    draw.DrawMode = DrawMode.Polygon;
                    mensurationOperation = MensurationOperation.AreaAndPerimeter;
                    break;
                case "Cen":
                    draw.DrawMode = DrawMode.Polygon;
                    mensurationOperation = MensurationOperation.Centroid;
                    break;
                case "DaA":
                    draw.DrawMode = DrawMode.LineSegment;
                    clickCount = 2;
                    mensurationOperation = MensurationOperation.DistanceAndAngle;
                    break;
                case "HFBaT":
                    draw.DrawMode = DrawMode.LineSegment;
                    clickCount = 2;
                    mensurationOperation = MensurationOperation.HeightFromBaseAndTop;
                    break;
                case "HFBaTS":
                    draw.DrawMode = DrawMode.LineSegment;
                    clickCount = 2;
                    mensurationOperation = MensurationOperation.HeightFromBaseAndTopShadow;
                    break;
                case "HFTaTS":
                    draw.DrawMode = DrawMode.LineSegment;
                    clickCount = 2;
                    mensurationOperation = MensurationOperation.HeightFromTopAndTopShadow;
                    break;
                case "Pnt":
                    draw.DrawMode = DrawMode.Point;
                    mensurationOperation = MensurationOperation.Point;
                    break;
            }            
        }
        internal enum MensurationOperation
        {
            AreaAndPerimeter,
            Centroid,
            DistanceAndAngle,
            HeightFromBaseAndTop,
            HeightFromBaseAndTopShadow,
            HeightFromTopAndTopShadow,
            Point
        }
    }
}

