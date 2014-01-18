Imports System.Threading
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Tasks
Imports System


Partial Public Class BufferQueryTaskAsync
    Inherits UserControl
    Private _geometryService As GeometryService
    Private _queryTask As QueryTask
    Private _pointAndBufferGraphicsLayer As GraphicsLayer
    Private _resultsGraphicsLayer As GraphicsLayer
    Private _cts As CancellationTokenSource

    Public Sub New()
        InitializeComponent()

        _geometryService = New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")

        _queryTask = New QueryTask("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/BloomfieldHillsMichigan/Parcels/MapServer/2")

        _pointAndBufferGraphicsLayer = TryCast(MyMap.Layers("MyBufferPointGraphicsLayer"), GraphicsLayer)
        _resultsGraphicsLayer = TryCast(MyMap.Layers("MyResultsGraphicsLayer"), GraphicsLayer)
    End Sub

    Private Async Sub MyMap_MouseClick(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Map.MouseEventArgs)
        Try
            If _cts IsNot Nothing Then
                _cts.Cancel()
            End If

            _cts = New CancellationTokenSource()

            Dim clickGraphic As New Graphic()
            clickGraphic.Symbol = TryCast(LayoutRoot.Resources("DefaultMarkerSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
            clickGraphic.Geometry = e.MapPoint
            clickGraphic.Geometry.SpatialReference = MyMap.SpatialReference

            _pointAndBufferGraphicsLayer.Graphics.Clear()
            _resultsGraphicsLayer.Graphics.Clear()

            clickGraphic.SetZIndex(2)
            _pointAndBufferGraphicsLayer.Graphics.Add(clickGraphic)

            Dim bufferParams As New ESRI.ArcGIS.Client.Tasks.BufferParameters() With {.BufferSpatialReference = New SpatialReference(4326), .OutSpatialReference = MyMap.SpatialReference, .Unit = LinearUnit.Meter}
            bufferParams.Distances.Add(100)
            bufferParams.Features.Add(clickGraphic)

            Dim bufferResult As BufferResult = Await _geometryService.BufferTaskAsync(bufferParams, _cts.Token)

            Dim bufferGraphic As New Graphic()
            bufferGraphic.Geometry = bufferResult.Results(0).Geometry
            bufferGraphic.Symbol = TryCast(LayoutRoot.Resources("BufferSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
            bufferGraphic.SetZIndex(1)

            _pointAndBufferGraphicsLayer.Graphics.Add(bufferGraphic)

            Dim query As New ESRI.ArcGIS.Client.Tasks.Query()
            query.ReturnGeometry = True
            query.OutSpatialReference = MyMap.SpatialReference
            query.Geometry = bufferGraphic.Geometry
            query.OutFields.Add("OWNERNME1")

            Dim queryResult As QueryResult = Await _queryTask.ExecuteTaskAsync(query, _cts.Token)

            If queryResult.FeatureSet.Features.Count < 1 Then
                MessageBox.Show("No features found")
                Return
            End If

            For Each selectedGraphic As Graphic In queryResult.FeatureSet.Features
                selectedGraphic.Symbol = TryCast(LayoutRoot.Resources("ParcelSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
                _resultsGraphicsLayer.Graphics.Add(selectedGraphic)
            Next selectedGraphic
        Catch ex As Exception
            If TypeOf ex Is ServiceException Then
                MessageBox.Show(String.Format("{0}: {1}", (TryCast(ex, ServiceException)).Code.ToString(), (TryCast(ex, ServiceException)).Details(0)), "Error", MessageBoxButton.OK)
                Return
            End If
        End Try
    End Sub
End Class

