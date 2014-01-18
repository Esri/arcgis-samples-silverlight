Imports System.Threading
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Tasks
Imports System.Collections.Generic
Imports System


Partial Public Class RoutingDirectionsTaskAsync
    Inherits UserControl
    Private _routeTask As RouteTask
    Private _locator As Locator
    Private _stops As New List(Of Graphic)()
    Private _activeSegmentGraphic As Graphic
    Private _routeParams As RouteParameters
    Private _directionsFeatureSet As DirectionsFeatureSet
    Private _cts As CancellationTokenSource
    Private _routeGraphicsLayer As GraphicsLayer

    Public Sub New()
        InitializeComponent()

        _locator = New Locator("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer")

        _routeTask = New RouteTask("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route")

        _routeParams = New RouteParameters() With {.ReturnRoutes = False, .ReturnDirections = True, .DirectionsLengthUnits = esriUnits.esriMiles, .Stops = _stops, .UseTimeWindows = False}

        _routeGraphicsLayer = (TryCast(MyMap.Layers("MyRouteGraphicsLayer"), GraphicsLayer))
    End Sub

    Private Async Sub GetDirections_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Try
            If _cts IsNot Nothing Then
                _cts.Cancel()
            End If

            _cts = New CancellationTokenSource()

            DirectionsStackPanel.Children.Clear()
            _stops.Clear()
            _routeGraphicsLayer.Graphics.Clear()

            'Geocode from address
            Dim fromAddress As LocatorTaskFindResult = Await _locator.FindTaskAsync(ParseSearchText(FromTextBox.Text), _cts.Token)
            ' if no result?
            Dim fromLocation As Graphic = fromAddress.Result.Locations(0).Graphic
            fromLocation.Geometry.SpatialReference = MyMap.SpatialReference
            fromLocation.Attributes.Add("name", fromAddress.Result.Locations(0).Name)
            _stops.Add(fromLocation)

            fromLocation.Symbol = TryCast(LayoutRoot.Resources("FromSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
            _routeGraphicsLayer.Graphics.Add(fromLocation)

            'Geocode to address
            Dim toAddress As LocatorTaskFindResult = Await _locator.FindTaskAsync(ParseSearchText(ToTextBox.Text), _cts.Token)
            Dim toLocation As Graphic = toAddress.Result.Locations(0).Graphic
            toLocation.Geometry.SpatialReference = MyMap.SpatialReference
            toLocation.Attributes.Add("name", toAddress.Result.Locations(0).Name)
            _stops.Add(toLocation)

            toLocation.Symbol = TryCast(LayoutRoot.Resources("ToSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
            _routeGraphicsLayer.Graphics.Add(toLocation)

            'Get route between from and to
            _routeParams.OutSpatialReference = MyMap.SpatialReference
            Dim solveRouteResult As SolveRouteResult = Await _routeTask.SolveTaskAsync(_routeParams, _cts.Token)

            Dim routeResult As RouteResult = solveRouteResult.RouteResults(0)
            _directionsFeatureSet = routeResult.Directions

            _routeGraphicsLayer.Graphics.Add(New Graphic() With {.Geometry = _directionsFeatureSet.MergedGeometry, .Symbol = TryCast(LayoutRoot.Resources("RouteSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)})
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
        Catch ex As Exception
            If TypeOf ex Is ServiceException Then
                MessageBox.Show(String.Format("{0}: {1}", (TryCast(ex, ServiceException)).Code.ToString(), (TryCast(ex, ServiceException)).Details(0)), "Error", MessageBoxButton.OK)
                Return
            End If
        End Try
    End Sub

    Private Sub directionsSegment_MouseLeftButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
        Dim textBlock As TextBlock = TryCast(sender, TextBlock)
        Dim feature As Graphic = TryCast(textBlock.Tag, Graphic)
        MyMap.ZoomTo(Expand(feature.Geometry.Extent))
        If _activeSegmentGraphic Is Nothing Then
            _activeSegmentGraphic = New Graphic() With {.Symbol = TryCast(LayoutRoot.Resources("SegmentSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)}
            _routeGraphicsLayer.Graphics.Add(_activeSegmentGraphic)
        End If
        _activeSegmentGraphic.Geometry = feature.Geometry
    End Sub

    Private Sub StackPanel_MouseLeftButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
        If _directionsFeatureSet IsNot Nothing Then
            MyMap.ZoomTo(Expand(_directionsFeatureSet.Extent))
        End If
    End Sub

    Private Function ParseSearchText(ByVal searchText As String) As LocatorFindParameters
        Dim locatorFindParams As New LocatorFindParameters() With {.Text = searchText, .Location = MyMap.Extent.GetCenter(), .Distance = MyMap.Extent.Width \ 2, .MaxLocations = 1, .OutSpatialReference = MyMap.SpatialReference}
        Return locatorFindParams
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
        Dim hours As Integer = CInt(Math.Floor(time.TotalHours))
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

