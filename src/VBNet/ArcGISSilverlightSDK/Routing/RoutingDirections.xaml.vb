Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Tasks


Partial Public Class RoutingDirections
	Inherits UserControl
	Private _routeTask As RouteTask
	Private _locator As Locator
	Private _stops As New List(Of Graphic)()
	Private _activeSegmentGraphic As Graphic
	Private _routeParams As RouteParameters
	Private _directionsFeatureSet As DirectionsFeatureSet

	Public Sub New()
		InitializeComponent()

    _routeParams = New RouteParameters() With {.ReturnRoutes = False, .ReturnDirections = True, .DirectionsLengthUnits = esriUnits.esriMiles, .Stops = _stops, .UseTimeWindows = False}

		_routeTask = New RouteTask("http://tasks.arcgisonline.com/ArcGIS/rest/services/NetworkAnalysis/ESRI_Route_NA/NAServer/Route")
		AddHandler _routeTask.SolveCompleted, AddressOf routeTask_SolveCompleted
		AddHandler _routeTask.Failed, AddressOf task_Failed

		_locator = New Locator("http://tasks.arcgisonline.com/ArcGIS/rest/services/Locators/TA_Address_NA/GeocodeServer")
		AddHandler _locator.AddressToLocationsCompleted, AddressOf locator_AddressToLocationsCompleted
		AddHandler _locator.Failed, AddressOf task_Failed
	End Sub

	Private Sub GetDirections_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
		'Reset
		DirectionsStackPanel.Children.Clear()
		_stops.Clear()

		TryCast(MyMap.Layers("MyRouteGraphicsLayer"), GraphicsLayer).ClearGraphics()
		_locator.CancelAsync()
		_routeTask.CancelAsync()

		'Geocode from address
		_locator.AddressToLocationsAsync(ParseAddress(FromTextBox.Text), "from")

	End Sub

	Private Sub locator_AddressToLocationsCompleted(ByVal sender As Object, ByVal e As AddressToLocationsEventArgs)
		Dim graphicsLayer As GraphicsLayer = (TryCast(MyMap.Layers("MyRouteGraphicsLayer"), GraphicsLayer))
		If e.Results.Count > 0 Then
			Dim address As AddressCandidate = e.Results(0)
			Dim graphicLocation As New Graphic() With {.Geometry = address.Location}
			graphicLocation.Attributes.Add("address", address.Address)
			graphicLocation.Attributes.Add("score", address.Score)

			_stops.Add(graphicLocation)
			If CStr(e.UserState) Is "from" Then
				graphicLocation.Symbol = TryCast(LayoutRoot.Resources("FromSymbol"), Symbols.Symbol)
				'Geocode to address
				_locator.AddressToLocationsAsync(ParseAddress(ToTextBox.Text), "to")
			Else
				graphicLocation.Symbol = TryCast(LayoutRoot.Resources("ToSymbol"), Symbols.Symbol)
				'Get route between from and to
				_routeParams.OutSpatialReference = MyMap.SpatialReference
				_routeTask.SolveAsync(_routeParams)
			End If

			graphicsLayer.Graphics.Add(graphicLocation)
		End If
	End Sub

	Private Sub routeTask_SolveCompleted(ByVal sender As Object, ByVal e As RouteEventArgs)
		Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyRouteGraphicsLayer"), GraphicsLayer)
		Dim routeResult As RouteResult = e.RouteResults(0)
		_directionsFeatureSet = routeResult.Directions

    graphicsLayer.Graphics.Add(New Graphic() With {.Geometry = _directionsFeatureSet.MergedGeometry, .Symbol = RouteSymbol})
		TotalDistanceTextBlock.Text = String.Format("Total Distance: {0}", FormatDistance(_directionsFeatureSet.TotalLength, "miles"))
		TotalTimeTextBlock.Text = String.Format("Total Time: {0}", FormatTime(_directionsFeatureSet.TotalTime))
		TitleTextBlock.Text = _directionsFeatureSet.RouteName

		Dim i As Integer = 1
		For Each graphic As Graphic In _directionsFeatureSet.Features
			Dim text As New System.Text.StringBuilder()
			text.AppendFormat("{0}. {1}", i, graphic.Attributes("text"))
			If i > 1 AndAlso i < _directionsFeatureSet.Features.Count Then
				Dim distance As String = FormatDistance(Convert.ToDouble(graphic.Attributes("length")), "miles")
				Dim time As String = Nothing
				If graphic.Attributes.ContainsKey("time") Then
					time = FormatTime(Convert.ToDouble(graphic.Attributes("time")))
				End If
				If (Not String.IsNullOrEmpty(distance)) OrElse (Not String.IsNullOrEmpty(time)) Then
					text.Append(" (")
				End If
				text.Append(distance)
				If (Not String.IsNullOrEmpty(distance)) AndAlso (Not String.IsNullOrEmpty(time)) Then
					text.Append(", ")
				End If
				text.Append(time)
				If (Not String.IsNullOrEmpty(distance)) OrElse (Not String.IsNullOrEmpty(time)) Then
					text.Append(")")
				End If
			End If
			Dim textBlock As New TextBlock() With {.Text = text.ToString(), .Tag = graphic, .Margin = New Thickness(4), .Cursor = Cursors.Hand}
			AddHandler textBlock.MouseLeftButtonDown, AddressOf directionsSegment_MouseLeftButtonDown
			DirectionsStackPanel.Children.Add(textBlock)
			i += 1
		Next graphic
		MyMap.ZoomTo(Expand(_directionsFeatureSet.Extent))
	End Sub

	Private Sub task_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
		Dim errorMessage As String = "Routing error: "
		errorMessage &= e.Error.Message
		For Each detail As String In (TryCast(e.Error, ServiceException)).Details
			errorMessage &= "," & detail
		Next detail

		MessageBox.Show(errorMessage)
	End Sub

	Private Sub directionsSegment_MouseLeftButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
		Dim textBlock As TextBlock = TryCast(sender, TextBlock)
		Dim feature As Graphic = TryCast(textBlock.Tag, Graphic)
		MyMap.ZoomTo(Expand(feature.Geometry.Extent))
		If _activeSegmentGraphic Is Nothing Then
			_activeSegmentGraphic = New Graphic() With {.Symbol = TryCast(LayoutRoot.Resources("SegmentSymbol"), Symbols.Symbol)}
			Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyRouteGraphicsLayer"), GraphicsLayer)
			graphicsLayer.Graphics.Add(_activeSegmentGraphic)
		End If
		_activeSegmentGraphic.Geometry = feature.Geometry
	End Sub

	Private Sub StackPanel_MouseLeftButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
		If _directionsFeatureSet IsNot Nothing Then
			Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyRouteGraphicsLayer"), GraphicsLayer)
			MyMap.ZoomTo(Expand(_directionsFeatureSet.Extent))
		End If
	End Sub

	Private Function ParseAddress(ByVal address As String) As AddressToLocationsParameters
		Dim fromArray() As String = address.Split(New Char() {","c})
		Dim fromAddress As New AddressToLocationsParameters()
		fromAddress.OutFields.Add("Loc_name")
		fromAddress.Address.Add("Address", fromArray(0))
		fromAddress.Address.Add("City", fromArray(1))
		fromAddress.Address.Add("State", fromArray(2))
		fromAddress.Address.Add("Zip", fromArray(3))
		fromAddress.Address.Add("Country", "USA")
		Return fromAddress
	End Function

	Private Function FormatDistance(ByVal dist As Double, ByVal units As String) As String
		Dim result As String = ""
		'INSTANT VB NOTE: The local variable formatDistance was renamed since Visual Basic will not allow local variables with the same name as their enclosing function or property:
		Dim formatDistance_Renamed As Double = Math.Round(dist, 2)
		If formatDistance_Renamed <> 0 Then
			result = formatDistance_Renamed & " " & units
		End If
		Return result
	End Function

	Private Function FormatTime(ByVal minutes As Double) As String
		Dim time As TimeSpan = TimeSpan.FromMinutes(minutes)
		Dim result As String = ""
		Dim hours As Integer = CInt(Fix(Math.Floor(time.TotalHours)))
		If hours > 1 Then
			result = String.Format("{0} hours ", hours)
		ElseIf hours = 1 Then
			result = String.Format("{0} hour ", hours)
		End If
		If time.Minutes > 1 Then
			result &= String.Format("{0} minutes ", time.Minutes)
		ElseIf time.Minutes = 1 Then
			result &= String.Format("{0} minute ", time.Minutes)
		End If
		Return result
	End Function

	Private Function Expand(ByVal e As Envelope) As Envelope
		Dim factor As Double = 0.6
		Dim centerMapPoint As MapPoint = e.GetCenter()
		Return New Envelope(centerMapPoint.X - e.Width * factor, centerMapPoint.Y - e.Height * factor, centerMapPoint.X + e.Width * factor, centerMapPoint.Y + e.Height * factor)
	End Function
End Class

