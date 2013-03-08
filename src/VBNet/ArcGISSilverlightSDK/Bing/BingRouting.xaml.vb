Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Bing
Imports ESRI.ArcGIS.Client.Bing.RouteService
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Symbols
Imports System.Net
Imports System.Json

Partial Public Class BingRouting
	Inherits UserControl
	Private myDrawObject As Draw
	Private waypointGraphicsLayer As GraphicsLayer
	Private routeResultsGraphicsLayer As GraphicsLayer
	Private routing As ESRI.ArcGIS.Client.Bing.Routing
	Private Shared mercator As New ESRI.ArcGIS.Client.Projection.WebMercator()

	Public Sub New()
		InitializeComponent()
	End Sub

	Private Sub BingKeyTextBox_TextChanged(ByVal sender As Object, ByVal e As TextChangedEventArgs)
		If (TryCast(sender, TextBox)).Text.Length >= 64 Then
			LoadMapButton.IsEnabled = True
		Else
			LoadMapButton.IsEnabled = False
		End If
	End Sub

	Private Sub LoadMapButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
		Dim webClient As New WebClient()
		Dim uri As String = String.Format("http://dev.virtualearth.net/REST/v1/Imagery/Metadata/Aerial?supressStatus=true&key={0}", BingKeyTextBox.Text)

		AddHandler webClient.OpenReadCompleted, Sub(s, a)
																							If a.Error Is Nothing Then
																								Dim jsonResponse As JsonValue = JsonObject.Load(a.Result)
																								Dim authenticationResult As String = jsonResponse("authenticationResultCode")
																								a.Result.Close()

																								If authenticationResult = "ValidCredentials" Then
																									Dim tileLayer As ESRI.ArcGIS.Client.Bing.TileLayer = New TileLayer() With
																									{
																											.ID = "BingLayer",
																											.LayerStyle = tileLayer.LayerType.Road,
																											.ServerType = ServerType.Production,
																											.Token = BingKeyTextBox.Text
																									}

																									MyMap.Layers.Insert(0, tileLayer)

																									' Add your Bing Maps key in the constructor for the Routing class. Use http://www.bingmapsportal.com to generate a key.  
																									routing = New ESRI.ArcGIS.Client.Bing.Routing(BingKeyTextBox.Text)
																									routing.ServerType = ServerType.Production

																									myDrawObject = New Draw(MyMap) With {.DrawMode = DrawMode.Point, .IsEnabled = True}

																									AddHandler myDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete
																									waypointGraphicsLayer = TryCast(MyMap.Layers("WaypointGraphicsLayer"), GraphicsLayer)
																									routeResultsGraphicsLayer = TryCast(MyMap.Layers("RouteResultsGraphicsLayer"), GraphicsLayer)

																									Dim initialExtent As New ESRI.ArcGIS.Client.Geometry.Envelope(TryCast(mercator.FromGeographic(New ESRI.ArcGIS.Client.Geometry.MapPoint(-130, 20)), MapPoint), TryCast(mercator.FromGeographic(New ESRI.ArcGIS.Client.Geometry.MapPoint(-65, 55)), MapPoint))

																									initialExtent.SpatialReference = New SpatialReference(102100)

																									MyMap.Extent = initialExtent

																									BingKeyGrid.Visibility = System.Windows.Visibility.Collapsed
																									RouteGrid.Visibility = System.Windows.Visibility.Visible

																									InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Collapsed

																								Else
																									InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Visible
																								End If
																							Else
																								InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Visible
																							End If
																						End Sub

		webClient.OpenReadAsync(New System.Uri(uri))
	End Sub

	Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As ESRI.ArcGIS.Client.DrawEventArgs)
		myDrawObject.IsEnabled = False

		Dim waypointGraphic As New Graphic() With {.Geometry = If(MyMap.WrapAroundIsActive, Geometry.NormalizeCentralMeridian(args.Geometry), args.Geometry), .Symbol = TryCast(LayoutRoot.Resources("UserStopSymbol"), Symbol)}

		waypointGraphic.Attributes.Add("StopNumber", waypointGraphicsLayer.Graphics.Count + 1)
		waypointGraphicsLayer.Graphics.Add(waypointGraphic)

		If waypointGraphicsLayer.Graphics.Count > 1 Then
			RouteButton.IsEnabled = True
		End If
		myDrawObject.IsEnabled = True
	End Sub

	Private Sub RouteButton_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
		myDrawObject.IsEnabled = False
		routing.Optimization = RouteOptimization.MinimizeTime
		routing.TrafficUsage = TrafficUsage.None
		routing.TravelMode = TravelMode.Driving
		routing.Route(GraphicsToMapPoints(), AddressOf Route_Complete)
	End Sub

	Private Function GraphicsToMapPoints() As List(Of MapPoint)
		Dim mapPoints As New List(Of MapPoint)()
		For Each g As Graphic In waypointGraphicsLayer.Graphics
			mapPoints.Add(TryCast(g.Geometry, MapPoint))
		Next g

		Return mapPoints
	End Function

	Private Sub Route_Complete(ByVal sender As Object, ByVal args As CalculateRouteCompletedEventArgs)
		myDrawObject.IsEnabled = True
		routeResultsGraphicsLayer.ClearGraphics()
		waypointGraphicsLayer.ClearGraphics()

		Dim directions As New StringBuilder()

		Dim routeLegs As ObservableCollection(Of RouteLeg) = args.Result.Result.Legs
		Dim numLegs As Integer = routeLegs.Count
		Dim instructionCount As Integer = 0
		For n As Integer = 0 To numLegs - 1
			If (n Mod 2) = 0 Then
				AddStopPoint(TryCast(mercator.FromGeographic(New MapPoint(routeLegs(n).ActualStart.Longitude, routeLegs(n).ActualStart.Latitude)), MapPoint))
				AddStopPoint(TryCast(mercator.FromGeographic(New MapPoint(routeLegs(n).ActualEnd.Longitude, routeLegs(n).ActualEnd.Latitude)), MapPoint))
			ElseIf n = (numLegs - 1) Then
				AddStopPoint(TryCast(mercator.FromGeographic(New MapPoint(routeLegs(n).ActualEnd.Longitude, routeLegs(n).ActualEnd.Latitude)), MapPoint))
			End If

			directions.Append(String.Format("--Leg #{0}--" & vbLf, n + 1))

			For Each item As ItineraryItem In routeLegs(n).Itinerary
				instructionCount += 1
				directions.Append(String.Format("{0}. {1}" & vbLf, instructionCount, item.Text))
			Next item
		Next n

		Dim regex As New Regex("<[/a-zA-Z:]*>", RegexOptions.IgnoreCase Or RegexOptions.Multiline)

		DirectionsContentTextBlock.Text = regex.Replace(directions.ToString(), String.Empty)
		DirectionsGrid.Visibility = Visibility.Visible

		Dim routePath As RoutePath = args.Result.Result.RoutePath

		Dim line As New Polyline()
		line.Paths.Add(New PointCollection())

		For Each location As ESRI.ArcGIS.Client.Bing.RouteService.Location In routePath.Points
			line.Paths(0).Add(TryCast(mercator.FromGeographic(New MapPoint(location.Longitude, location.Latitude)), MapPoint))
		Next location

		Dim graphic As New Graphic() With {.Geometry = line, .Symbol = TryCast(LayoutRoot.Resources("RoutePathSymbol"), Symbol)}
		routeResultsGraphicsLayer.Graphics.Add(graphic)
	End Sub

	Private Sub AddStopPoint(ByVal mapPoint As MapPoint)
		Dim graphic As New Graphic() With {.Geometry = mapPoint, .Symbol = TryCast(LayoutRoot.Resources("ResultStopSymbol"), Symbol)}
		graphic.Attributes.Add("StopNumber", waypointGraphicsLayer.Graphics.Count + 1)
		waypointGraphicsLayer.Graphics.Add(graphic)
	End Sub

	Private Sub ClearRouteButton_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
		waypointGraphicsLayer.ClearGraphics()
		routeResultsGraphicsLayer.ClearGraphics()
		DirectionsContentTextBlock.Text = ""
		DirectionsGrid.Visibility = System.Windows.Visibility.Collapsed
	End Sub
End Class
