Imports Microsoft.VisualBasic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Tasks
Imports ESRI.ArcGIS.Client.Symbols


  Partial Public Class BufferQuery
    Inherits UserControl
    Private _geometryService As GeometryService
    Private _queryTask As QueryTask
    Private _pointAndBufferGraphicsLayer As GraphicsLayer
    Private _resultsGraphicsLayer As GraphicsLayer

    Public Sub New()
      InitializeComponent()

		_geometryService = New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")
      AddHandler _geometryService.BufferCompleted, AddressOf GeometryService_BufferCompleted
      AddHandler _geometryService.Failed, AddressOf GeometryService_Failed

      _queryTask = New QueryTask("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/BloomfieldHillsMichigan/Parcels/MapServer/2")
      AddHandler _queryTask.ExecuteCompleted, AddressOf QueryTask_ExecuteCompleted
      AddHandler _queryTask.Failed, AddressOf QueryTask_Failed

      _pointAndBufferGraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
      _resultsGraphicsLayer = TryCast(MyMap.Layers("MyResultsGraphicsLayer"), GraphicsLayer)
    End Sub

    Private Sub MyMap_MouseClick(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Map.MouseEventArgs)
      _geometryService.CancelAsync()
      _queryTask.CancelAsync()

      Dim clickGraphic As New Graphic()
      clickGraphic.Symbol = TryCast(LayoutRoot.Resources("DefaultMarkerSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
      clickGraphic.Geometry = e.MapPoint
      ' Input spatial reference for buffer operation defined by first feature of input geometry array
      clickGraphic.Geometry.SpatialReference = MyMap.SpatialReference

      _pointAndBufferGraphicsLayer.Graphics.Clear()
      _resultsGraphicsLayer.Graphics.Clear()

      clickGraphic.SetZIndex(2)
      _pointAndBufferGraphicsLayer.Graphics.Add(clickGraphic)

      ' If buffer spatial reference is GCS and unit is linear, geometry service will do geodesic buffering
      Dim bufferParams As New ESRI.ArcGIS.Client.Tasks.BufferParameters() With
          {
              .BufferSpatialReference = New SpatialReference(4326),
              .OutSpatialReference = MyMap.SpatialReference,
              .Unit = LinearUnit.Meter
          }
      bufferParams.Distances.Add(100)
      bufferParams.Features.Add(clickGraphic)

      _geometryService.BufferAsync(bufferParams)
    End Sub

    Private Sub GeometryService_BufferCompleted(ByVal sender As Object, ByVal args As GraphicsEventArgs)
      Dim bufferGraphic As New Graphic()
      bufferGraphic.Geometry = args.Results(0).Geometry
      bufferGraphic.Symbol = TryCast(LayoutRoot.Resources("BufferSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
      bufferGraphic.SetZIndex(1)

      _pointAndBufferGraphicsLayer.Graphics.Add(bufferGraphic)

      Dim query As New ESRI.ArcGIS.Client.Tasks.Query()
      query.ReturnGeometry = True
      query.OutSpatialReference = MyMap.SpatialReference
      query.Geometry = bufferGraphic.Geometry
      query.OutFields.Add("OWNERNME1")
      _queryTask.ExecuteAsync(query)
    End Sub

    Private Sub QueryTask_ExecuteCompleted(ByVal sender As Object, ByVal args As QueryEventArgs)
      If args.FeatureSet.Features.Count < 1 Then
        MessageBox.Show("No features found")
        Return
      End If

      For Each selectedGraphic As Graphic In args.FeatureSet.Features
        selectedGraphic.Symbol = TryCast(LayoutRoot.Resources("ParcelSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
        _resultsGraphicsLayer.Graphics.Add(selectedGraphic)
      Next selectedGraphic
    End Sub

    Private Sub GeometryService_Failed(ByVal sender As Object, ByVal args As TaskFailedEventArgs)
      MessageBox.Show("Geometry service failed: " + args.Error.Message)
    End Sub

    Private Sub QueryTask_Failed(ByVal sender As Object, ByVal args As TaskFailedEventArgs)
      MessageBox.Show("Query failed: " + args.Error.Message)
    End Sub

  End Class

