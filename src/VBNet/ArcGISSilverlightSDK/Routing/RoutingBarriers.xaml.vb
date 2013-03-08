Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks


  Partial Public Class RoutingBarriers
    Inherits UserControl
    Private _routeTask As RouteTask
    Private _stops As New List(Of Graphic)()
    Private _barriers As New List(Of Graphic)()
    Private _routeParams As New RouteParameters()
    Private stopsLayer As GraphicsLayer = Nothing
    Private barriersLayer As GraphicsLayer = Nothing


    Public Sub New()
      InitializeComponent()

		_routeTask = New RouteTask("http://tasks.arcgisonline.com/ArcGIS/rest/services/NetworkAnalysis/ESRI_Route_NA/NAServer/Route")
      AddHandler _routeTask.SolveCompleted, AddressOf routeTask_SolveCompleted
      AddHandler _routeTask.Failed, AddressOf routeTask_Failed

      _routeParams.Stops = _stops
      _routeParams.Barriers = _barriers
      _routeParams.UseTimeWindows = False

      barriersLayer = TryCast(MyMap.Layers("MyBarriersGraphicsLayer"), GraphicsLayer)
      stopsLayer = TryCast(MyMap.Layers("MyStopsGraphicsLayer"), GraphicsLayer)
    End Sub

    Private Sub MyMap_MouseClick(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Map.MouseEventArgs)

      If StopsRadioButton.IsChecked.Value Then
        Dim [stop] As New Graphic() With {.Geometry = e.MapPoint, .Symbol = TryCast(LayoutRoot.Resources("StopSymbol"), Symbols.Symbol)}
        [stop].Attributes.Add("StopNumber", stopsLayer.Graphics.Count + 1)
        stopsLayer.Graphics.Add([stop])
        _stops.Add([stop])
      ElseIf BarriersRadioButton.IsChecked.Value Then
        Dim barrier As New Graphic() With {.Geometry = e.MapPoint, .Symbol = TryCast(LayoutRoot.Resources("BarrierSymbol"), Symbols.Symbol)}
        barriersLayer.Graphics.Add(barrier)
        _barriers.Add(barrier)
      End If
      If _stops.Count > 1 Then
        If _routeTask.IsBusy Then
          _routeTask.CancelAsync()
			End If
			_routeParams.OutSpatialReference = MyMap.SpatialReference
        _routeTask.SolveAsync(_routeParams)
      End If
    End Sub

    Private Sub routeTask_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      Dim errorMessage As String = "Routing error: "
      errorMessage &= e.Error.Message
      For Each detail As String In (TryCast(e.Error, ServiceException)).Details
        errorMessage &= "," & detail
      Next detail

      MessageBox.Show(errorMessage)

      If _stops.Count > 10 Then
        stopsLayer.Graphics.RemoveAt(stopsLayer.Graphics.Count - 1)
        _stops.RemoveAt(_stops.Count - 1)
      End If

    End Sub

    Private Sub routeTask_SolveCompleted(ByVal sender As Object, ByVal e As RouteEventArgs)
      Dim routeLayer As GraphicsLayer = TryCast(MyMap.Layers("MyRouteGraphicsLayer"), GraphicsLayer)
      Dim routeResult As RouteResult = e.RouteResults(0)
      routeResult.Route.Symbol = TryCast(LayoutRoot.Resources("RouteSymbol"), Symbols.Symbol)

      routeLayer.Graphics.Clear()
      Dim lastRoute As Graphic = routeResult.Route

      routeLayer.Graphics.Add(lastRoute)
    End Sub

    Private Sub Button_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      _stops.Clear()
      _barriers.Clear()

      For Each layer As Layer In MyMap.Layers
        If TypeOf layer Is GraphicsLayer Then
          TryCast(layer, GraphicsLayer).ClearGraphics()
        End If
      Next layer
    End Sub
  End Class

