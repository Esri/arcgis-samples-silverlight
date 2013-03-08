Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Symbols
Imports ESRI.ArcGIS.Client.Tasks
Imports System.Windows.Media


  Partial Public Class ServiceAreas
    Inherits UserControl

    Private myRouteTask As RouteTask
    Private facilitiesGraphicsLayer As GraphicsLayer
    Private barriersGraphicsLayer As GraphicsLayer
    Private pointBarriers As List(Of Graphic)
    Private polylineBarriers As List(Of Graphic)
    Private polygonBarriers As List(Of Graphic)
    Private random As Random

    Public Sub New()
      InitializeComponent()

      facilitiesGraphicsLayer = TryCast (MyMap.Layers("MyFacilityGraphicsLayer"), GraphicsLayer)
      barriersGraphicsLayer = TryCast (MyMap.Layers("MyBarrierGraphicsLayer"), GraphicsLayer)

      myRouteTask = New RouteTask("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Network/USA/NAServer/Service%20Area")
      AddHandler myRouteTask.SolveServiceAreaCompleted, AddressOf SolveServiceArea_Completed
      AddHandler myRouteTask.Failed, AddressOf SolveServiceArea_Failed

      pointBarriers = New List(Of Graphic)()
      polylineBarriers = New List(Of Graphic)()
      polygonBarriers = New List(Of Graphic)()

      random = New Random()
    End Sub

    Private Sub SolveButton_Click (ByVal sender As Object, ByVal e As RoutedEventArgs)
      Dim aps As New List(Of AttributeParameter)()
      Dim ap As AttributeParameter = GetAttributeParameterValue (AttributeParameterValues3.SelectionBoxItem.ToString().Trim())
            
      If ap IsNot Nothing Then
          aps.Add(ap)
      End If

      GenerateBarriers()

		Dim routeParams As New RouteServiceAreaParameters() With { _
			.Barriers = If(pointBarriers.Count > 0, pointBarriers, Nothing), _
			.PolylineBarriers = If(polylineBarriers.Count > 0, polylineBarriers, Nothing), _
			.PolygonBarriers = If(polygonBarriers.Count > 0, polygonBarriers, Nothing), _
			.Facilities = facilitiesGraphicsLayer.Graphics, _
			.DefaultBreaks = DefaultBreaks.Text, _
			.ExcludeSourcesFromPolygons = ExculdeSourcesFromPolygons.Text, _
			.MergeSimilarPolygonRanges = If(MergeSimilarPolygonRanges.IsChecked.HasValue, MergeSimilarPolygonRanges.IsChecked.Value, False), _
			.OutputLines = GetOutputLines(OutputLines3.SelectionBoxItem.ToString()), _
			.OutputPolygons = GetOutputPolygons(OutputPolygons.SelectionBoxItem.ToString()), _
			.OverlapLines = If(OverlapLines3.IsChecked.HasValue, OverlapLines3.IsChecked.Value, False), _
			.OverlapPolygons = If(OverlapPolygons3.IsChecked.HasValue, OverlapPolygons3.IsChecked.Value, False), _
			.SplitLineAtBreaks = If(SplitLinesAtBreaks.IsChecked.HasValue, SplitLinesAtBreaks.IsChecked.Value, False), _
			.SplitPolygonsAtBreaks = If(SplitPolygonsAtBreaks.IsChecked.HasValue, SplitPolygonsAtBreaks.IsChecked.Value, False), _
			.TravelDirection = GetFacilityTravelDirections(TravelDirections3.SelectionBoxItem.ToString().Trim()), _
			.TrimOuterPolygon = If(TrimOuterPolygons.IsChecked.HasValue, TrimOuterPolygons.IsChecked.Value, False), _
			.TrimPolygonDistance = If(String.IsNullOrEmpty(TrimPolygonDistance.Text), 0, Double.Parse(TrimPolygonDistance.Text)), _
			.TrimPolygonDistanceUnits = GetUnits(TrimPolygonDistanceUnits.SelectionBoxItem.ToString()), _
			.ReturnFacilities = If(ReturnFacilities3.IsChecked.HasValue, ReturnFacilities3.IsChecked.Value, False), _
			.ReturnBarriers = If(ReturnBarriers3.IsChecked.HasValue, ReturnBarriers3.IsChecked.Value, False), _
			.ReturnPolylineBarriers = If(ReturnPolylineBarriers3.IsChecked.HasValue, ReturnPolylineBarriers3.IsChecked.Value, False), _
			.ReturnPolygonBarriers = If(ReturnPolygonBarriers3.IsChecked.HasValue, ReturnPolygonBarriers3.IsChecked.Value, False), _
			.OutSpatialReference = If(String.IsNullOrEmpty(OutputSpatialReference3.Text), MyMap.SpatialReference, New SpatialReference(Integer.Parse(OutputSpatialReference3.Text))), _
			.AccumulateAttributes = If(String.IsNullOrEmpty(AccumulateAttributeNames3.Text), Nothing, AccumulateAttributeNames3.Text.Split(","c)), _
			.ImpedanceAttribute = If(String.IsNullOrEmpty(ImpedanceAttributeName3.Text), String.Empty, ImpedanceAttributeName3.Text), _
			.RestrictionAttributes = If(String.IsNullOrEmpty(RestrictionAttributeNames3.Text), Nothing, RestrictionAttributeNames3.Text.Split(","c)), _
			.RestrictUTurns = GetRestrictUTurns(RestrictUTurns3.SelectionBoxItem.ToString()), _
			.OutputGeometryPrecision = If(String.IsNullOrEmpty(OutputGeometryPrecision3.Text), 0, Double.Parse(OutputGeometryPrecision3.Text)), _
			.OutputGeometryPrecisionUnits = GetUnits(OutputGeometryPrecisionUnits3.SelectionBoxItem.ToString())}

      If myRouteTask.IsBusy Then
        myRouteTask.CancelAsync()
      End If

      myRouteTask.SolveServiceAreaAsync (routeParams)
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

    Private Sub SolveServiceArea_Completed (ByVal sender As Object, ByVal e As RouteEventArgs)
      Dim routeLayer As GraphicsLayer = TryCast(MyMap.Layers("MyServiceAreasGraphicsLayer"), GraphicsLayer)
      routeLayer.Graphics.Clear()
      If e.ServiceAreaPolygons IsNot Nothing Then
        For Each g As Graphic In e.ServiceAreaPolygons
          Dim symbol As New SimpleFillSymbol() With {.Fill = New SolidColorBrush(Color.FromArgb(100, CByte(random.Next(0, 255)), CByte(random.Next(0, 255)), CByte(random.Next(0, 255)))), .BorderBrush = New SolidColorBrush(Colors.Transparent), .BorderThickness = 1}

          g.Symbol = symbol
          routeLayer.Graphics.Add(g)
        Next g
      End If
      If e.ServiceAreaPolylines IsNot Nothing Then
        For Each g As Graphic In e.ServiceAreaPolylines
          Dim symbol As New SimpleLineSymbol() With {.Color = New SolidColorBrush(Color.FromArgb(100, CByte(random.Next(0, 255)), CByte(random.Next(0, 255)), CByte(random.Next(0, 255)))), .Width = 1}

          g.Symbol = symbol
          routeLayer.Graphics.Add(g)
        Next g
      End If
    End Sub

    Private Sub SolveServiceArea_Failed (ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      MessageBox.Show("Network Analysis error: " & e.Error.Message)
    End Sub

    Private Sub Editor_EditCompleted (ByVal sender As Object, ByVal e As Editor.EditEventArgs)
      If e.Action = Editor.EditAction.Add Then
        e.Edits.ElementAt(0).Graphic.Attributes.Add("FacilityNumber", (TryCast(e.Edits.ElementAt(0).Layer, GraphicsLayer)).Graphics.Count)

        SolveButton.IsEnabled = True
      End If
    End Sub

    Private Function GetAttributeParameterValue (ByVal attributeParamSelection As String) As AttributeParameter
      Return New AttributeParameter() With {.value = attributeParamSelection}
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

    Private Function GetOutputPolygons (outputPolygonSelection As String) As String
	    Dim result As String = "esriNAOutputPolygonNone"
	    Select Case outputPolygonSelection.ToLower()
		    Case "none"
			    result = "esriNAOutputPolygonNone"
			    Exit Select
		    Case "simplified"
			    result = "esriNAOutputPolygonSimplified"
			    Exit Select
		    Case "detailed"
			    result = "esriNAOutputPolygonDetailed"
			    Exit Select
		    Case Else
			    Exit Select
	    End Select
	    Return result
    End Function

    Private Function GetFacilityTravelDirections(directionSelection As String) As FacilityTravelDirection
	    Dim ftd As FacilityTravelDirection = FacilityTravelDirection.TravelDirectionToFacility
	    Select Case directionSelection.ToLower()
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

    Private Function GetUnits (unitsSelection As String) As esriUnits
	    Dim units As esriUnits = esriUnits.esriUnknownUnits
	    Select Case unitsSelection.ToLower()
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

    Private Function GetRestrictUTurns (restrictUTurns As String) As String
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
  End Class

