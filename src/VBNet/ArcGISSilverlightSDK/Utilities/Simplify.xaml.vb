Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Tasks


  Partial Public Class Simplify
    Inherits UserControl
    Private _unsimplifiedGraphic As New Graphic()

    Public Sub New()
      InitializeComponent()

      AddHandler MyMap.Layers.LayersInitialized, AddressOf Layers_LayersInitialized
    End Sub

    Private Sub Layers_LayersInitialized(ByVal sender As Object, ByVal args As EventArgs)
      If (TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)).Graphics.Count < 1 Then
        drawPolygon()
      End If
    End Sub

    Private Sub drawPolygon()
      Dim center As MapPoint = MyMap.Extent.GetCenter()
      Dim lat As Double = center.Y
      Dim lon As Double = center.X + 300
      Dim latOffset As Double = 300
      Dim lonOffset As Double = 300

      ' Initialize the set of points
      Dim points As New ESRI.ArcGIS.Client.Geometry.PointCollection
      Dim pointArray() As MapPoint =
          {
              New MapPoint(lon - lonOffset, lat),
              New MapPoint(lon, lat + latOffset),
              New MapPoint(lon + lonOffset, lat),
              New MapPoint(lon, lat - latOffset),
              New MapPoint(lon - lonOffset, lat),
              New MapPoint(lon - 2 * lonOffset, lat + latOffset),
              New MapPoint(lon - 3 * lonOffset, lat),
              New MapPoint(lon - 2 * lonOffset, lat - latOffset),
              New MapPoint(lon - 1.5 * lonOffset, lat + latOffset),
              New MapPoint(lon - lonOffset, lat)
          }
      For Each point As MapPoint In pointArray
        points.Add(point)
      Next

      Dim polygon As New ESRI.ArcGIS.Client.Geometry.Polygon()
      polygon.Rings.Add(points)
      polygon.SpatialReference = MyMap.SpatialReference
      _unsimplifiedGraphic.Geometry = polygon

      Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
      _unsimplifiedGraphic.Symbol = TryCast(LayoutRoot.Resources("PolygonFillSymbol"), Symbols.Symbol)
      graphicsLayer.Graphics.Add(_unsimplifiedGraphic)
    End Sub

    Private Sub QueryOnlyButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      doQuery(_unsimplifiedGraphic.Geometry)
    End Sub

    Private Sub SimplifyAndQueryButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
		Dim geometryService As New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")
      AddHandler geometryService.SimplifyCompleted, AddressOf GeometryService_SimplifyCompleted
      AddHandler geometryService.Failed, AddressOf GeometryService_Failed

      Dim graphicList As New List(Of Graphic)()
      graphicList.Add(_unsimplifiedGraphic)
      geometryService.SimplifyAsync(graphicList)
    End Sub

    Private Sub GeometryService_SimplifyCompleted(ByVal sender As Object, ByVal args As GraphicsEventArgs)
      doQuery(args.Results(0).Geometry)
    End Sub

    Private Sub doQuery(ByVal geometry As ESRI.ArcGIS.Client.Geometry.Geometry)
      Dim queryTask As New QueryTask("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/BloomfieldHillsMichigan/Parcels/MapServer/2")
      AddHandler queryTask.ExecuteCompleted, AddressOf QueryTask_ExecuteCompleted
      AddHandler queryTask.Failed, AddressOf QueryTask_Failed

      Dim query As New ESRI.ArcGIS.Client.Tasks.Query() With
          {
              .Geometry = geometry,
              .SpatialRelationship = SpatialRelationship.esriSpatialRelContains,
              .OutSpatialReference = MyMap.SpatialReference,
              .ReturnGeometry = True
          }

      query.OutFields.Add("*")
      queryTask.ExecuteAsync(query)
    End Sub

    Private Sub QueryTask_ExecuteCompleted(ByVal sender As Object, ByVal args As QueryEventArgs)
      If args.FeatureSet Is Nothing Then
        Return
      End If
      Dim featureSet As FeatureSet = args.FeatureSet
      Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
      graphicsLayer.ClearGraphics()

      If featureSet IsNot Nothing AndAlso featureSet.Features.Count > 0 Then
        For Each feature As Graphic In featureSet.Features
          Dim graphic As New ESRI.ArcGIS.Client.Graphic() With
              {
                  .Geometry = feature.Geometry,
                  .Symbol = TryCast(LayoutRoot.Resources("ParcelFillSymbol"), Symbols.Symbol)
              }
          graphicsLayer.Graphics.Add(graphic)
        Next feature
      End If
      graphicsLayer.Graphics.Add(_unsimplifiedGraphic)
    End Sub

    Private Sub GeometryService_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      MessageBox.Show("Geometry Service error: " & e.Error.Message)
    End Sub

    Private Sub QueryTask_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      MessageBox.Show("Query failed: " & e.Error.Message)
    End Sub
  End Class

