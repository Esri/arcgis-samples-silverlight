Imports System.Threading
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks
Imports System


Partial Public Class CustomParameters
    Inherits UserControl
    Private _stopsGraphicsLayer As GraphicsLayer
    Private _routeGraphicsLayer As GraphicsLayer
    Private _routeTask As RouteTask
    Private _routeParams As RouteParameters
    Private _cts As CancellationTokenSource

    Public Sub New()
        InitializeComponent()

        _stopsGraphicsLayer = TryCast(MyMap.Layers("MyStopsGraphicsLayer"), GraphicsLayer)
        _routeGraphicsLayer = TryCast(MyMap.Layers("MyRouteGraphicsLayer"), GraphicsLayer)

        _routeTask = New RouteTask("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route")

        _routeParams = New RouteParameters() With {.Stops = _stopsGraphicsLayer, .UseTimeWindows = False, .OutSpatialReference = MyMap.SpatialReference}

        _routeTask.CustomParameters.Add("myParameterName", "0")
    End Sub

    Private Async Sub MyMap_MouseClick(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Map.MouseEventArgs)
        Try
            If _cts IsNot Nothing Then
                _cts.Cancel()
            End If

            _cts = New CancellationTokenSource()

            Dim [stop] As New Graphic() With {.Geometry = e.MapPoint}

            _stopsGraphicsLayer.Graphics.Add([stop])

            If _stopsGraphicsLayer.Graphics.Count > 1 Then
                If _routeTask.IsBusy Then
                    _cts.Cancel()
                    _stopsGraphicsLayer.Graphics.RemoveAt(_stopsGraphicsLayer.Graphics.Count - 1)
                End If

                Dim result As SolveRouteResult = Await _routeTask.SolveTaskAsync(_routeParams, _cts.Token)

                _routeGraphicsLayer.Graphics.Clear()
                _routeGraphicsLayer.Graphics.Add(result.RouteResults(0).Route)
            End If
        Catch ex As Exception
            _stopsGraphicsLayer.Graphics.RemoveAt(_stopsGraphicsLayer.Graphics.Count - 1)

            If TypeOf ex Is ServiceException Then
                MessageBox.Show(String.Format("{0}: {1}", (TryCast(ex, ServiceException)).Code.ToString(), (TryCast(ex, ServiceException)).Details(0)), "Error", MessageBoxButton.OK)
                Return
            End If
        End Try
    End Sub
End Class

