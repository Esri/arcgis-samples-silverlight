Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Tasks


    Partial Public Class ClosestFacility
        Inherits UserControl

        Private myRouteTask As RouteTask
        Private facilitiesGraphicsLayer As GraphicsLayer
        Private IncidentsGraphicsLayer As GraphicsLayer
        Private barriersGraphicsLayer As GraphicsLayer
        Private routeGraphicsLayer As GraphicsLayer
        Private pointBarriers As List(Of Graphic)
        Private polylineBarriers As List(Of Graphic)
        Private polygonBarriers As List(Of Graphic)
        Private random As Random

        Public Sub New()
            InitializeComponent()

            facilitiesGraphicsLayer = TryCast(MyMap.Layers("MyFacilitiesGraphicsLayer"), GraphicsLayer)
            IncidentsGraphicsLayer = TryCast(MyMap.Layers("MyIncidentsGraphicsLayer"), GraphicsLayer)
            barriersGraphicsLayer = TryCast(MyMap.Layers("MyBarriersGraphicsLayer"), GraphicsLayer)
            routeGraphicsLayer = TryCast(MyMap.Layers("MyRoutesGraphicsLayer"), GraphicsLayer)

        myRouteTask = New RouteTask("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/ClosestFacility")
            AddHandler myRouteTask.SolveClosestFacilityCompleted, AddressOf SolveClosestFacility_Completed
            AddHandler myRouteTask.Failed, AddressOf SolveClosestFacility_Failed

            pointBarriers = New List(Of Graphic)()
            polylineBarriers = New List(Of Graphic)()
            polygonBarriers = New List(Of Graphic)()

            random = New Random()
        End Sub

        Private Sub SolveButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Dim aps As New List(Of AttributeParameter)()
            Dim ap As AttributeParameter = GetAttributeParameterValue(AttributeParameter2.SelectionBoxItem.ToString().Trim())
            
            If ap IsNot Nothing Then
                aps.Add(ap)
            End If

            GenerateBarriers()
            
		Dim routeParams As New RouteClosestFacilityParameters() With { _
				.Incidents = IncidentsGraphicsLayer.Graphics, _
				.Barriers = If(pointBarriers.Count > 0, pointBarriers, Nothing), _
				.PolylineBarriers = If(polylineBarriers.Count > 0, polylineBarriers, Nothing), _
				.PolygonBarriers = If(polygonBarriers.Count > 0, polygonBarriers, Nothing), _
				.Facilities = facilitiesGraphicsLayer.Graphics, _
				.AttributeParameterValues = aps, _
				.ReturnDirections = If(ReturnDirections2.IsChecked.HasValue, ReturnDirections2.IsChecked.Value, False), _
				.DirectionsLanguage = If(String.IsNullOrEmpty(DirectionsLanguage2.Text), New System.Globalization.CultureInfo("en-US"), _
																 New System.Globalization.CultureInfo(DirectionsLanguage2.Text)), _
				.DirectionsLengthUnits = GetDirectionsLengthUnits(DirectionsLengthUnits2.SelectionBoxItem.ToString().Trim()), _
				.DirectionsTimeAttribute = DirectionsTimeAttributeName2.Text, _
				.ReturnRoutes = If(ReturnRoutes2.IsChecked.HasValue, ReturnRoutes2.IsChecked.Value, False), _
				.ReturnFacilities = If(ReturnFacilities2.IsChecked.HasValue, ReturnFacilities2.IsChecked.Value, False), _
				.ReturnIncidents = If(ReturnIncidents2.IsChecked.HasValue, ReturnIncidents2.IsChecked.Value, False), _
				.ReturnBarriers = If(ReturnBarriers2.IsChecked.HasValue, ReturnBarriers2.IsChecked.Value, False), _
				.ReturnPolylineBarriers = If(ReturnPolylineBarriers2.IsChecked.HasValue, ReturnPolylineBarriers2.IsChecked.Value, False), _
				.ReturnPolygonBarriers = If(ReturnPolygonBarriers2.IsChecked.HasValue, ReturnPolygonBarriers2.IsChecked.Value, False), _
				.FacilityReturnType = FacilityReturnType.ServerFacilityReturnAll, _
				.OutputLines = GetOutputLines(OutputLines2.SelectionBoxItem.ToString().Trim()), _
				.DefaultCutoff = If(String.IsNullOrEmpty(DefaultCutoff2.Text), 100, Double.Parse(DefaultCutoff2.Text)), _
				.DefaultTargetFacilityCount = If(String.IsNullOrEmpty(DefaultTargetFacilityCount2.Text), 1, Integer.Parse(DefaultTargetFacilityCount2.Text)), _
				.TravelDirection = GetFacilityTravelDirections(TravelDirection2.SelectionBoxItem.ToString().Trim()), _
				.OutSpatialReference = If(String.IsNullOrEmpty(OutputSpatialReference2.Text), MyMap.SpatialReference, New SpatialReference(Integer.Parse(OutputSpatialReference2.Text))), _
				.AccumulateAttributes = AccumulateAttributeNames2.Text.Split(","c), _
				.ImpedanceAttribute = ImpedanceAttributeName2.Text, _
				.RestrictionAttributes = RestrictionAttributeNames2.Text.Split(","c), _
				.RestrictUTurns = GetRestrictUTurns(RestrictUTurns2.SelectionBoxItem.ToString().Trim()), _
				.UseHierarchy = If(UseHierarchy2.IsChecked.HasValue, UseHierarchy2.IsChecked.Value, False), _
				.OutputGeometryPrecision = If(String.IsNullOrEmpty(OutputGeometryPrecision2.Text), 0, Double.Parse(OutputGeometryPrecision2.Text)), _
				.OutputGeometryPrecisionUnits = GetGeometryPrecisionUnits(OutputGeometryPrecisionUnits2.SelectionBoxItem.ToString().Trim())}

            If myRouteTask.IsBusy Then
                myRouteTask.CancelAsync()
            End If

            myRouteTask.SolveClosestFacilityAsync(routeParams)
        End Sub

        Public Sub GenerateBarriers()
            For Each g As Graphic In barriersGraphicsLayer
                Dim gType As Type = g.Geometry.GetType()

                If gType Is GetType(MapPoint) Then
                    pointBarriers.Add(g)
                ElseIf gType Is GetType(Polyline) Then
                    polylineBarriers.Add(g)
                ElseIf gType Is GetType(Polygon) OrElse gType Is GetType(Envelope) Then
                    polygonBarriers.Add(g)
                End If
            Next g
        End Sub

        Private Sub SolveClosestFacility_Completed (ByVal sender As Object, ByVal e As RouteEventArgs)
            routeGraphicsLayer.Graphics.Clear()

            If e.RouteResults IsNot Nothing Then
                For Each route As RouteResult In e.RouteResults
                    Dim g As Graphic = route.Route
                    routeGraphicsLayer.Graphics.Add(g)
                Next route
            End If
        End Sub

        Private Sub SolveClosestFacility_Failed (ByVal sender As Object, ByVal e As TaskFailedEventArgs)
            MessageBox.Show("Network Analysis error: " & e.Error.Message)
        End Sub

        Private Sub Editor_EditCompleted (ByVal sender As Object, ByVal e As Editor.EditEventArgs)
            Dim editor As Editor = TryCast(sender, Editor)

            If e.Action = editor.EditAction.Add Then
                If editor.LayerIDs.ElementAt(0) = "MyFacilitiesGraphicsLayer" Then
                    e.Edits.ElementAt(0).Graphic.Attributes.Add("FacilityNumber", (TryCast(e.Edits.ElementAt(0).Layer, GraphicsLayer)).Graphics.Count)
                ElseIf editor.LayerIDs.ElementAt(0) = "MyIncidentsGraphicsLayer" Then
                    e.Edits.ElementAt(0).Graphic.Attributes.Add("IncidentNumber", (TryCast(e.Edits.ElementAt(0).Layer, GraphicsLayer)).Graphics.Count)
                End If

                If facilitiesGraphicsLayer.Graphics.Count > 0 AndAlso IncidentsGraphicsLayer.Graphics.Count > 0 Then
                    SolveButton.IsEnabled = True
                End If
            End If
        End Sub

        Private Sub ClearButton_Click (ByVal sender As Object, ByVal e As RoutedEventArgs)
            For Each layer As Layer In MyMap.Layers
                If TypeOf layer Is GraphicsLayer Then
                    TryCast(layer, GraphicsLayer).Graphics.Clear()
                End If
            Next layer
            SolveButton.IsEnabled = False
        End Sub

        Private Function GetAttributeParameterValue (ByVal attributeParamSelection As String) As AttributeParameter

        If (attributeParamSelection.Equals("None")) Then
            Return Nothing
        End If

        If (attributeParamSelection.Equals("Other Roads")) Then
            Return New AttributeParameter() With
            {
                .attributeName = "Time",
                .parameterName = "OtherRoads",
                .value = "5"
            }
        End If

        Return New AttributeParameter() With
        {
            .attributeName = "Time",
            .parameterName = attributeParamSelection,
            .value = attributeParamSelection.Replace(" MPH", "")
        }

        End Function

        Private Function GetDirectionsLengthUnits (ByVal directionsLengthUnits As String) As esriUnits
            Dim units As esriUnits = esriUnits.esriUnknownUnits
            If directionsLengthUnits.Equals(String.Empty) Then
                Return units
            End If

            Select Case directionsLengthUnits.ToLower()
                Case "kilometers"
                    units = esriUnits.esriKilometers
                    Exit Select
                Case "meters"
                    units = esriUnits.esriMeters
                    Exit Select
                Case "miles"
                    units = esriUnits.esriMiles
                    Exit Select
                Case "nautical miles"
                    units = esriUnits.esriNauticalMiles
                    Exit Select
                Case "unknown"
                    units = esriUnits.esriUnknownUnits
                    Exit Select
                Case Else
                    Exit Select
            End Select
            Return units
        End Function

        Private Function GetOutputLines (ByVal outputLines As String) As String
            Dim result As String = ""
            If outputLines.Equals(String.Empty) Then
                Return result
            End If

            Select Case outputLines.ToLower()
                Case "none"
                    result = "esriNAOutputLineNone"
                    Exit Select
                Case "straight"
                    result = "esriNAOutputLineStraight"
                    Exit Select
                Case "true shape"
                    result = "esriNAOutputLineTrueShape"
                    Exit Select
                Case Else
                    Exit Select
            End Select
            Return result
        End Function

        Private Function GetFacilityTravelDirections (ByVal direction As String) As FacilityTravelDirection
            Dim ftd As FacilityTravelDirection = FacilityTravelDirection.TravelDirectionToFacility
            Select Case direction.ToLower()
                Case "to facility"
                    ftd = FacilityTravelDirection.TravelDirectionToFacility
                    Exit Select
                Case "from facility"
                    ftd = FacilityTravelDirection.TravelDirectionFromFacility
                    Exit Select
                Case Else
                    Exit Select
            End Select
            Return ftd
        End Function

        Private Function GetRestrictUTurns (ByVal restrictUTurns As String) As String
            Dim result As String = "esriNFSBAllowBacktrack"
            Select Case restrictUTurns.ToLower()
                Case "allow backtrack"
                    result = "esriNFSBAllowBacktrack"
                    Exit Select
                Case "at dead ends only"
                    result = "esriNFSBAtDeadEndsOnly"
                    Exit Select
                Case "no backtrack"
                    result = "esriNFSBNoBacktrack"
                    Exit Select
                Case Else
                    Exit Select
            End Select
            Return result
        End Function

        Private Function GetGeometryPrecisionUnits (ByVal outputGeometryPrecisionUnits As String) As esriUnits
            Dim units As esriUnits = esriUnits.esriUnknownUnits
            Select Case outputGeometryPrecisionUnits.ToLower()
                Case "unknown"
                    units = esriUnits.esriUnknownUnits
                    Exit Select
                Case "decimal degrees"
                    units = esriUnits.esriDecimalDegrees
                    Exit Select
                Case "kilometers"
                    units = esriUnits.esriKilometers
                    Exit Select
                Case "meters"
                    units = esriUnits.esriMeters
                    Exit Select
                Case "miles"
                    units = esriUnits.esriMiles
                    Exit Select
                Case "nautical miles"
                    units = esriUnits.esriNauticalMiles
                    Exit Select
                Case "inches"
                    units = esriUnits.esriInches
                    Exit Select
                Case "points"
                    units = esriUnits.esriPoints
                    Exit Select
                Case "feet"
                    units = esriUnits.esriFeet
                    Exit Select
                Case "yards"
                    units = esriUnits.esriYards
                    Exit Select
                Case "millimeters"
                    units = esriUnits.esriMillimeters
                    Exit Select
                Case "centimeters"
                    units = esriUnits.esriCentimeters
                    Exit Select
                Case "decimeters"
                    units = esriUnits.esriDecimeters
                    Exit Select
                Case Else
                    Exit Select
            End Select
            Return units
        End Function
    End Class

