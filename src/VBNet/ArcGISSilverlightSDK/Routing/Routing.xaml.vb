Imports Microsoft.VisualBasic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks
Imports ESRI.ArcGIS.Client.Symbols


Partial Public Class Routing
    Inherits UserControl
    Private stopsGraphicsLayer As GraphicsLayer
    Private routeGraphicsLayer As GraphicsLayer
    Private routeTask As RouteTask

    Public Sub New()
        InitializeComponent()

        stopsGraphicsLayer = TryCast(MyMap.Layers("MyStopsGraphicsLayer"), GraphicsLayer)
        routeGraphicsLayer = TryCast(MyMap.Layers("MyRouteGraphicsLayer"), GraphicsLayer)
        routeTask = TryCast(LayoutRoot.Resources("MyRouteTask"), RouteTask)
    End Sub

    Private Sub MyMap_MouseClick(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Map.MouseEventArgs)
        Dim [stop] As New Graphic() With {.Geometry = e.MapPoint}

        stopsGraphicsLayer.Graphics.Add([stop])

        If stopsGraphicsLayer.Graphics.Count > 1 Then
            If routeTask.IsBusy Then
                routeTask.CancelAsync()
                stopsGraphicsLayer.Graphics.RemoveAt(stopsGraphicsLayer.Graphics.Count - 1)
            End If
            routeTask.SolveAsync(New RouteParameters() With {.Stops = stopsGraphicsLayer, .UseTimeWindows = False, .OutSpatialReference = MyMap.SpatialReference})
        End If
    End Sub

    Private Sub MyRouteTask_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
        Dim errorMessage As String = "Routing error: "
        errorMessage &= e.Error.Message
        For Each detail As String In (TryCast(e.Error, ServiceException)).Details
            errorMessage &= "," & detail
        Next detail

        MessageBox.Show(errorMessage)

        stopsGraphicsLayer.Graphics.RemoveAt(stopsGraphicsLayer.Graphics.Count - 1)
    End Sub

    Private Sub MyRouteTask_SolveCompleted(ByVal sender As Object, ByVal e As RouteEventArgs)
        routeGraphicsLayer.Graphics.Clear()

        Dim routeResult As RouteResult = e.RouteResults(0)

        Dim lastRoute As Graphic = routeResult.Route

        Dim totalTime As Decimal = CDec(lastRoute.Attributes("Total_TravelTime"))
        TotalTimeTextBlock.Text = String.Format("Total time: {0} minutes", totalTime.ToString("#0.000"))


        routeGraphicsLayer.Graphics.Add(lastRoute)
    End Sub
End Class

