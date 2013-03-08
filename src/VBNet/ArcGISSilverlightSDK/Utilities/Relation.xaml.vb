Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Tasks


  Partial Public Class Relation
    Inherits UserControl
    Private MyDrawObject As Draw
    Private polygonLayer As GraphicsLayer
    Private pointLayer As GraphicsLayer
    Private geometryService As GeometryService

    Public Sub New()
      InitializeComponent()

      polygonLayer = TryCast(MyMap.Layers("MyPolygonGraphicsLayer"), GraphicsLayer)
      AddHandler MyMap.Layers.LayersInitialized, AddressOf Layers_LayersInitialized
      pointLayer = TryCast(MyMap.Layers("MyPointGraphicsLayer"), GraphicsLayer)

      MyDrawObject = New Draw(MyMap) With {.DrawMode = DrawMode.Point, .IsEnabled = True}
      AddHandler MyDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete
    End Sub

    Private Sub Layers_LayersInitialized(ByVal sender As Object, ByVal args As EventArgs)
      For i As Integer = 0 To polygonLayer.Graphics.Count - 1
        Dim graphic As Graphic = polygonLayer.Graphics(i)
        graphic.Geometry.SpatialReference = MyMap.SpatialReference
        If (Not graphic.Attributes.ContainsKey("Name")) Then
          graphic.Attributes.Add("Name", String.Format("Polygon_{0}", i))
          graphic.Attributes.Add("Relation", Nothing)
        End If
      Next i
    End Sub

    Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As DrawEventArgs)
      MyDrawObject.IsEnabled = False

      Dim name As String = String.Format("Point_{0}", pointLayer.Graphics.Count)

      Dim mapPoint As MapPoint = TryCast(args.Geometry, MapPoint)

      Dim graphic As New Graphic() With
          {
              .Symbol = TryCast(LayoutRoot.Resources("DefaultPointMarkerSymbol"), Symbols.Symbol),
              .Geometry = mapPoint
          }

      graphic.Attributes.Add("Name", name)
      graphic.Attributes.Add("Relation", Nothing)

      pointLayer.Graphics.Add(graphic)

      MyDrawObject.IsEnabled = True
    End Sub

    Private Sub ExecuteRelationButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      MyDrawObject.IsEnabled = False
      ExecuteRelationButton.Visibility = Visibility.Collapsed

		geometryService = New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")
      AddHandler geometryService.RelationCompleted, AddressOf GeometryService_RelationCompleted
      AddHandler geometryService.SimplifyCompleted, AddressOf GeometryService_SimplifyCompleted
      AddHandler geometryService.Failed, AddressOf GeometryService_Failed

      If pointLayer.Graphics.Count < 1 Then
        MessageBox.Show("No points available")
        MyDrawObject.IsEnabled = True
        ExecuteRelationButton.Visibility = Visibility.Visible
        Return
      End If

      For Each graphic As Graphic In pointLayer.Graphics
        graphic.Attributes("Relation") = Nothing
      Next graphic

      For Each graphic As Graphic In polygonLayer.Graphics
        graphic.Attributes("Relation") = Nothing
      Next graphic

      ' Call simplify operation to correct orientation of rings in a polygon (clockwise = ring, counterclockwise = hole)
      geometryService.SimplifyAsync(polygonLayer.Graphics)
    End Sub

    Private Sub GeometryService_SimplifyCompleted(ByVal sender As Object, ByVal e As GraphicsEventArgs)
      geometryService.RelationAsync(pointLayer.Graphics, e.Results, GeometryRelation.esriGeometryRelationWithin, Nothing)
    End Sub

    Private Sub GeometryService_RelationCompleted(ByVal sender As Object, ByVal args As RelationEventArgs)
      Dim pointLayer As GraphicsLayer = TryCast(MyMap.Layers("MyPointGraphicsLayer"), GraphicsLayer)
      Dim polygonLayer As GraphicsLayer = TryCast(MyMap.Layers("MyPolygonGraphicsLayer"), GraphicsLayer)

      Dim results As List(Of GeometryRelationPair) = args.Results
      For Each pair As GeometryRelationPair In results
        If pointLayer.Graphics(pair.Graphic1Index).Attributes("Relation") Is Nothing Then
          pointLayer.Graphics(pair.Graphic1Index).Attributes("Relation") = String.Format("Within Polygon {0}", pair.Graphic2Index)
        Else
          pointLayer.Graphics(pair.Graphic1Index).Attributes("Relation") &= "," & pair.Graphic2Index.ToString()
        End If

        If polygonLayer.Graphics(pair.Graphic2Index).Attributes("Relation") Is Nothing Then
          polygonLayer.Graphics(pair.Graphic2Index).Attributes("Relation") = String.Format("Contains Point {0}", pair.Graphic1Index)
        Else
          polygonLayer.Graphics(pair.Graphic2Index).Attributes("Relation") &= "," & pair.Graphic1Index.ToString()
        End If
      Next pair

      ExecuteRelationButton.Visibility = Visibility.Visible
    End Sub


    Private Sub GeometryService_Failed(ByVal sender As Object, ByVal args As TaskFailedEventArgs)
      MessageBox.Show("Geometry Service error: " & args.Error.Message)
    End Sub
  End Class

