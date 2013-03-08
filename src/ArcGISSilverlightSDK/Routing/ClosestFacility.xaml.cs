using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class ClosestFacility : UserControl
    {
        private RouteTask myRouteTask;
        private GraphicsLayer facilitiesGraphicsLayer;
        private GraphicsLayer IncidentsGraphicsLayer;
        private GraphicsLayer barriersGraphicsLayer;
        private GraphicsLayer routeGraphicsLayer;
        List<Graphic> pointBarriers;
        List<Graphic> polylineBarriers;
        List<Graphic> polygonBarriers;
        Random random;

        public ClosestFacility()
        {
            InitializeComponent();

            facilitiesGraphicsLayer = MyMap.Layers["MyFacilitiesGraphicsLayer"] as GraphicsLayer;
            IncidentsGraphicsLayer = MyMap.Layers["MyIncidentsGraphicsLayer"] as GraphicsLayer;
            barriersGraphicsLayer = MyMap.Layers["MyBarriersGraphicsLayer"] as GraphicsLayer;
            routeGraphicsLayer = MyMap.Layers["MyRoutesGraphicsLayer"] as GraphicsLayer;

            myRouteTask = new RouteTask("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Network/USA/NAServer/Closest%20Facility");
            myRouteTask.SolveClosestFacilityCompleted += SolveClosestFacility_Completed;
            myRouteTask.Failed += SolveClosestFacility_Failed;

            pointBarriers = new List<Graphic>();
            polylineBarriers = new List<Graphic>();
            polygonBarriers = new List<Graphic>();

            random = new Random();
        }

        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            List<AttributeParameter> aps = new List<AttributeParameter>();
            AttributeParameter ap = GetAttributeParameterValue (AttributeParameter2.SelectionBoxItem.ToString().Trim());
            if (ap != null)
                aps.Add( ap );

            GenerateBarriers();

            RouteClosestFacilityParameters routeParams = new RouteClosestFacilityParameters()
            {
                Incidents = IncidentsGraphicsLayer.Graphics,
                Barriers = pointBarriers.Count > 0 ? pointBarriers : null,
                PolylineBarriers = polylineBarriers.Count > 0 ? polylineBarriers : null,
                PolygonBarriers = polygonBarriers.Count > 0 ? polygonBarriers : null,
                Facilities = facilitiesGraphicsLayer.Graphics,

                AttributeParameterValues = aps,
                ReturnDirections = ReturnDirections2.IsChecked.HasValue ? ReturnDirections2.IsChecked.Value : false,
                DirectionsLanguage = String.IsNullOrEmpty(DirectionsLanguage2.Text) ? new System.Globalization.CultureInfo("en-US") : new System.Globalization.CultureInfo(DirectionsLanguage2.Text),
                DirectionsLengthUnits = GetDirectionsLengthUnits (DirectionsLengthUnits2.SelectionBoxItem.ToString().Trim()),
                DirectionsTimeAttribute = DirectionsTimeAttributeName2.Text,
                
                ReturnRoutes = ReturnRoutes2.IsChecked.HasValue ? ReturnRoutes2.IsChecked.Value : false,
                ReturnFacilities = ReturnFacilities2.IsChecked.HasValue ? ReturnFacilities2.IsChecked.Value : false,
                ReturnIncidents = ReturnIncidents2.IsChecked.HasValue ? ReturnIncidents2.IsChecked.Value : false,
                ReturnBarriers = ReturnBarriers2.IsChecked.HasValue ? ReturnBarriers2.IsChecked.Value : false,
                ReturnPolylineBarriers = ReturnPolylineBarriers2.IsChecked.HasValue ? ReturnPolylineBarriers2.IsChecked.Value : false,
                ReturnPolygonBarriers = ReturnPolygonBarriers2.IsChecked.HasValue ? ReturnPolygonBarriers2.IsChecked.Value : false,
                
                FacilityReturnType = FacilityReturnType.ServerFacilityReturnAll,
                OutputLines = GetOutputLines (OutputLines2.SelectionBoxItem.ToString().Trim()),
                DefaultCutoff = string.IsNullOrEmpty(DefaultCutoff2.Text) ? 100 : double.Parse(DefaultCutoff2.Text),
                DefaultTargetFacilityCount = string.IsNullOrEmpty(DefaultTargetFacilityCount2.Text) ? 1 : int.Parse(DefaultTargetFacilityCount2.Text),
                TravelDirection = GetFacilityTravelDirections (TravelDirection2.SelectionBoxItem.ToString().Trim()),
                OutSpatialReference = string.IsNullOrEmpty(OutputSpatialReference2.Text) ? MyMap.SpatialReference : new SpatialReference(int.Parse(OutputSpatialReference2.Text)),
                
                AccumulateAttributes = AccumulateAttributeNames2.Text.Split(','),
                ImpedanceAttribute = ImpedanceAttributeName2.Text,
                RestrictionAttributes = RestrictionAttributeNames2.Text.Split(','),
                RestrictUTurns = GetRestrictUTurns (RestrictUTurns2.SelectionBoxItem.ToString().Trim()),
                UseHierarchy = UseHierarchy2.IsChecked.HasValue ? UseHierarchy2.IsChecked.Value : false,
                OutputGeometryPrecision = string.IsNullOrEmpty(OutputGeometryPrecision2.Text) ? 0 : double.Parse(OutputGeometryPrecision2.Text),
                OutputGeometryPrecisionUnits = GetGeometryPrecisionUnits (OutputGeometryPrecisionUnits2.SelectionBoxItem.ToString().Trim())
            };

            if (myRouteTask.IsBusy)
                myRouteTask.CancelAsync();

            myRouteTask.SolveClosestFacilityAsync(routeParams);
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

        void SolveClosestFacility_Completed (object sender, RouteEventArgs e)
        {
            routeGraphicsLayer.Graphics.Clear();

            if (e.RouteResults != null)
            {
                foreach (RouteResult route in e.RouteResults)
                {
                    Graphic g = route.Route;
                    routeGraphicsLayer.Graphics.Add(g);
                }
            }
        }

        void SolveClosestFacility_Failed (object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Network Analysis error: " + e.Error.Message);
        }

        private void Editor_EditCompleted (object sender, Editor.EditEventArgs e)
        {
            Editor editor = sender as Editor;

            if (e.Action == Editor.EditAction.Add)
            {
                if (editor.LayerIDs.ElementAt(0) == "MyFacilitiesGraphicsLayer")
                {
                    e.Edits.ElementAt(0).Graphic.Attributes.Add("FacilityNumber",
                        (e.Edits.ElementAt(0).Layer as GraphicsLayer).Graphics.Count);
                }
                else if (editor.LayerIDs.ElementAt(0) == "MyIncidentsGraphicsLayer")
                {
                    e.Edits.ElementAt(0).Graphic.Attributes.Add("IncidentNumber",
                        (e.Edits.ElementAt(0).Layer as GraphicsLayer).Graphics.Count);
                }

                if (facilitiesGraphicsLayer.Graphics.Count > 0 && IncidentsGraphicsLayer.Graphics.Count > 0)
                    SolveButton.IsEnabled = true;
            }
        }

        private void ClearButton_Click (object sender, RoutedEventArgs e)
        {
            foreach (Layer layer in MyMap.Layers)
                if (layer is GraphicsLayer)
                    (layer as GraphicsLayer).ClearGraphics();

            SolveButton.IsEnabled = false;
        }

        private AttributeParameter GetAttributeParameterValue (string attributeParamSelection)
        {
            // See Attribute Parameter Values list at 
            // http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Network/USA/NAServer/Closest%20Facility
            if (attributeParamSelection.Equals("None"))
                return null;

            if (attributeParamSelection.Equals("Other Roads"))
                return new AttributeParameter
                {
                    attributeName = "Time",
                    parameterName = "OtherRoads",
                    value = "5"
                };

            return new AttributeParameter
            {
                attributeName = "Time",
                parameterName = attributeParamSelection,
                value = attributeParamSelection.Replace(" MPH", "")
            };
        }

        private esriUnits GetDirectionsLengthUnits (string directionsLengthUnits)
        {
            esriUnits units = esriUnits.esriUnknownUnits;
            if (directionsLengthUnits.Equals( string.Empty ))
                return units;

            switch (directionsLengthUnits.ToLower())
            { 
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
                case "unknown":
                    units = esriUnits.esriUnknownUnits;
                    break;
                default:
                    break;
            }

            return units;
        }

        private string GetOutputLines (string outputLines)
        {
            string result = "";
            if (outputLines.Equals( string.Empty ))
                return result;

            switch (outputLines.ToLower())
            {
                case "none":
                    result = "esriNAOutputLineNone";
                    break;
                case "straight":
                    result = "esriNAOutputLineStraight";
                    break;
                case "true shape":
                    result = "esriNAOutputLineTrueShape";
                    break;
                default:
                    break;
            }
            return result;
        }

        private FacilityTravelDirection GetFacilityTravelDirections (string direction)
        {
            FacilityTravelDirection ftd = FacilityTravelDirection.TravelDirectionToFacility;
            switch (direction.ToLower())
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

        private esriUnits GetGeometryPrecisionUnits (string outputGeometryPrecisionUnits)
        {
            esriUnits units = esriUnits.esriUnknownUnits;
            switch (outputGeometryPrecisionUnits.ToLower())
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
    }
}
