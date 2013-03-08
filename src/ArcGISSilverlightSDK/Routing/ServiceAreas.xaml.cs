using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class ServiceAreas : UserControl
    {
        private RouteTask myRouteTask;
        private GraphicsLayer facilitiesGraphicsLayer;
        private GraphicsLayer barriersGraphicsLayer;
        List<Graphic> pointBarriers;
        List<Graphic> polylineBarriers;
        List<Graphic> polygonBarriers;
        Random random;

        public ServiceAreas()
        {
            InitializeComponent();

            facilitiesGraphicsLayer = MyMap.Layers["MyFacilityGraphicsLayer"] as GraphicsLayer;
            barriersGraphicsLayer = MyMap.Layers["MyBarrierGraphicsLayer"] as GraphicsLayer;

            myRouteTask = new RouteTask("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Network/USA/NAServer/Service%20Area");
            myRouteTask.SolveServiceAreaCompleted += SolveServiceArea_Completed;
            myRouteTask.Failed += SolveServiceArea_Failed;

            pointBarriers = new List<Graphic>();
            polylineBarriers = new List<Graphic>();
            polygonBarriers = new List<Graphic>();

            random = new Random();
        }

        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            List<AttributeParameter> aps = new List<AttributeParameter>();
            AttributeParameter ap = GetAttributeParameterValue( AttributeParameterValues3.SelectionBoxItem.ToString().Trim() );
            if (ap != null)
                aps.Add( ap );

            GenerateBarriers();

            try
            {
                RouteServiceAreaParameters routeParams = new RouteServiceAreaParameters()
                {
                    Barriers = pointBarriers.Count > 0 ? pointBarriers : null,
                    PolylineBarriers = polylineBarriers.Count > 0 ? polylineBarriers : null,
                    PolygonBarriers = polygonBarriers.Count > 0 ? polygonBarriers : null,
                    Facilities = facilitiesGraphicsLayer.Graphics,
                    //AttributeParameterValues = aps,
                    DefaultBreaks = DefaultBreaks.Text,
                    ExcludeSourcesFromPolygons = ExculdeSourcesFromPolygons.Text,
                    MergeSimilarPolygonRanges = MergeSimilarPolygonRanges.IsChecked.HasValue ? MergeSimilarPolygonRanges.IsChecked.Value : false,

                    OutputLines = GetOutputLines (OutputLines3.SelectionBoxItem.ToString()),
                    OutputPolygons = GetOutputPolygons (OutputPolygons.SelectionBoxItem.ToString()),
                    OverlapLines = OverlapLines3.IsChecked.HasValue ? OverlapLines3.IsChecked.Value : false,
                    OverlapPolygons = OverlapPolygons3.IsChecked.HasValue ? OverlapPolygons3.IsChecked.Value : false,

                    SplitLineAtBreaks = SplitLinesAtBreaks.IsChecked.HasValue ? SplitLinesAtBreaks.IsChecked.Value : false,
                    SplitPolygonsAtBreaks = SplitPolygonsAtBreaks.IsChecked.HasValue ? SplitPolygonsAtBreaks.IsChecked.Value : false,
                    TravelDirection = GetFacilityTravelDirections (TravelDirections3.SelectionBoxItem.ToString().Trim()),
                    TrimOuterPolygon = TrimOuterPolygons.IsChecked.HasValue ? TrimOuterPolygons.IsChecked.Value : false,
                    TrimPolygonDistance = string.IsNullOrEmpty(TrimPolygonDistance.Text) ? 0 : double.Parse(TrimPolygonDistance.Text),
                    TrimPolygonDistanceUnits = GetUnits (TrimPolygonDistanceUnits.SelectionBoxItem.ToString().Trim()),

                    ReturnFacilities = ReturnFacilities3.IsChecked.HasValue ? ReturnFacilities3.IsChecked.Value : false,
                    ReturnBarriers = ReturnBarriers3.IsChecked.HasValue ? ReturnBarriers3.IsChecked.Value : false,
                    ReturnPolylineBarriers = ReturnPolylineBarriers3.IsChecked.HasValue ? ReturnPolylineBarriers3.IsChecked.Value : false,
                    ReturnPolygonBarriers = ReturnPolygonBarriers3.IsChecked.HasValue ? ReturnPolygonBarriers3.IsChecked.Value : false,

                    OutSpatialReference = string.IsNullOrEmpty(OutputSpatialReference3.Text) ? MyMap.SpatialReference : new SpatialReference(int.Parse( OutputSpatialReference3.Text )),
                    AccumulateAttributes = string.IsNullOrEmpty(AccumulateAttributeNames3.Text) ? null : AccumulateAttributeNames3.Text.Split(','),
                    ImpedanceAttribute = string.IsNullOrEmpty(ImpedanceAttributeName3.Text) ? string.Empty : ImpedanceAttributeName3.Text,
                    RestrictionAttributes = string.IsNullOrEmpty(RestrictionAttributeNames3.Text) ? null : RestrictionAttributeNames3.Text.Split(','),
                    RestrictUTurns = GetRestrictUTurns (RestrictUTurns3.SelectionBoxItem.ToString()),
                    OutputGeometryPrecision = string.IsNullOrEmpty(OutputGeometryPrecision3.Text) ? 0 : double.Parse(OutputGeometryPrecision3.Text),
                    OutputGeometryPrecisionUnits = GetUnits (OutputGeometryPrecisionUnits3.SelectionBoxItem.ToString().Trim())
                };

                if (myRouteTask.IsBusy)
                    myRouteTask.CancelAsync();

                myRouteTask.SolveServiceAreaAsync(routeParams);
            }
            catch (Exception ex)
            {
                MessageBox.Show( ex.Message + '\n' + ex.StackTrace);
            }
        }

        public void GenerateBarriers()
        {           
            foreach (Graphic g in barriersGraphicsLayer)
            {
                Type gType = g.Geometry.GetType();

                if (gType == typeof(MapPoint))
                    pointBarriers.Add(g);
                else if (gType == typeof(Polyline))
                    polylineBarriers.Add(g);
                else if (gType == typeof(Polygon) || gType == typeof(Envelope))
                    polygonBarriers.Add(g); 
            }
        }

        private AttributeParameter GetAttributeParameterValue (string attributeParamSelection)
        {
            return new AttributeParameter { value = attributeParamSelection };
        }
        
        private void SolveServiceArea_Completed (object sender, RouteEventArgs e)
        {            
            GraphicsLayer routeLayer = MyMap.Layers["MyServiceAreasGraphicsLayer"] as GraphicsLayer;
            routeLayer.Graphics.Clear();
            if (e.ServiceAreaPolygons != null)
            {
                foreach (Graphic g in e.ServiceAreaPolygons)
                {
                    SimpleFillSymbol symbol = new SimpleFillSymbol()
                    {
                        Fill = new SolidColorBrush(Color.FromArgb(100, (byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255))),
                        BorderBrush = new SolidColorBrush(Colors.Transparent),
                        BorderThickness = 1
                    };

                    g.Symbol = symbol;
                    routeLayer.Graphics.Add(g);
                }
            }
            if (e.ServiceAreaPolylines != null)
            {
                foreach (Graphic g in e.ServiceAreaPolylines)
                {
                    SimpleLineSymbol symbol = new SimpleLineSymbol()
                    {
                        Color = new SolidColorBrush(Color.FromArgb(100, (byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255))),
                        Width = 1,
                    };

                    g.Symbol = symbol;                    
                    routeLayer.Graphics.Add(g);
                }
            }
        }

        private void SolveServiceArea_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Network Analysis error: " + e.Error.Message);
        }

        private void Editor_EditCompleted(object sender, Editor.EditEventArgs e)
        {
            if (e.Action == Editor.EditAction.Add)
            {
                e.Edits.ElementAt(0).Graphic.Attributes.Add("FacilityNumber",
                    (e.Edits.ElementAt(0).Layer as GraphicsLayer).Graphics.Count);

                SolveButton.IsEnabled = true;
            }
        }

        private string GetOutputLines (string outputLineSelection)
        {
            string result = "esriNAOutputLineNone";
            switch (outputLineSelection.ToLower())
            { 
                case "none":
                    result = "esriNAOutputLineNone";
                    break;
                case "true shape":
                    result = "esriNAOutputLineTrueShape";
                    break;
                default:
                    break;
            }
            return result;
        }

        private string GetOutputPolygons (string outputPolygonSelection)
        {
            string result = "esriNAOutputPolygonNone";
            switch (outputPolygonSelection.ToLower())
            { 
                case "none":
                    result = "esriNAOutputPolygonNone";
                    break;
                case "simplified":
                    result = "esriNAOutputPolygonSimplified";
                    break;
                case "detailed":
                    result = "esriNAOutputPolygonDetailed";
                    break;
                default:
                    break;
            }
            return result;
        }

        private FacilityTravelDirection GetFacilityTravelDirections (string directionSelection)
        {
            FacilityTravelDirection ftd = FacilityTravelDirection.TravelDirectionToFacility;
            switch (directionSelection.ToLower())
            { 
                case "to facility":
                    ftd = FacilityTravelDirection.TravelDirectionToFacility;
                    break;
                case "from facility":
                    ftd = FacilityTravelDirection.TravelDirectionFromFacility;
                    break;
                default:
                    break;
            }
            return ftd;
        }

        private esriUnits GetUnits (string unitsSelection)
        {
            esriUnits units = esriUnits.esriUnknownUnits;
            switch (unitsSelection.ToLower())
            {
                case "unknown":
                    units = esriUnits.esriUnknownUnits;
                    break;
                case "decimal degrees":
                    units = esriUnits.esriDecimalDegrees;
                    break;
                case "kilometers":
                    units = esriUnits.esriKilometers;
                    break;
                case "meters":
                    units = esriUnits.esriMeters;
                    break;
                case "miles":
                    units = esriUnits.esriMiles;
                    break;
                case "nautical miles":
                    units = esriUnits.esriNauticalMiles;
                    break;
                case "inches":
                    units = esriUnits.esriInches;
                    break;
                case "points":
                    units = esriUnits.esriPoints;
                    break;
                case "feet":
                    units = esriUnits.esriFeet;
                    break;
                case "yards":
                    units = esriUnits.esriYards;
                    break;
                case "millimeters":
                    units = esriUnits.esriMillimeters;
                    break;
                case "centimeters":
                    units = esriUnits.esriCentimeters;
                    break;
                case "decimeters":
                    units = esriUnits.esriDecimeters;
                    break;
                default:
                    break;
            }
            return units;
        }

        private string GetRestrictUTurns (string restrictUTurns)
        {
            string result = "esriNFSBAllowBacktrack";
            switch (restrictUTurns.ToLower())
            {
                case "allow backtrack":
                    result = "esriNFSBAllowBacktrack";
                    break;
                case "at dead ends only":
                    result = "esriNFSBAtDeadEndsOnly";
                    break;
                case "no backtrack":
                    result = "esriNFSBNoBacktrack";
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}
