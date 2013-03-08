Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Tasks


  Partial Public Class LabelPoints
    Inherits UserControl
    Private MyDrawObject As Draw
    Private geometryService As GeometryService
    Private graphicsLayer As GraphicsLayer

    Public Sub New()
      InitializeComponent()


      MyDrawObject = New Draw(MyMap) With
                     {
                         .FillSymbol = TryCast(LayoutRoot.Resources("DefaultPolygonSymbol"), ESRI.ArcGIS.Client.Symbols.FillSymbol),
                         .DrawMode = DrawMode.Polygon, .IsEnabled = True
                     }

      AddHandler MyDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete

		geometryService = New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")
      AddHandler geometryService.SimplifyCompleted, AddressOf GeometryService_SimplifyCompleted
      AddHandler geometryService.LabelPointsCompleted, AddressOf GeometryService_LabelPointsCompleted
      AddHandler geometryService.Failed, AddressOf GeometryService_Failed

      graphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
    End Sub

    Private Sub ClearGraphicsButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      graphicsLayer.ClearGraphics()
    End Sub

    Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As ESRI.ArcGIS.Client.DrawEventArgs)
      MyDrawObject.IsEnabled = False

      Dim polygon As ESRI.ArcGIS.Client.Geometry.Polygon = TryCast(args.Geometry, ESRI.ArcGIS.Client.Geometry.Polygon)
      polygon.SpatialReference = New SpatialReference(4326)
      Dim graphic As New Graphic() With
          {
              .Symbol = TryCast(LayoutRoot.Resources("DefaultPolygonSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol),
              .Geometry = polygon
          }
      graphic.Attributes.Add("X", "Label Point Polygon")
      graphicsLayer.Graphics.Add(graphic)

      Dim graphicsList As New List(Of Graphic)()
      graphicsList.Add(graphic)

      geometryService.SimplifyAsync(graphicsList)
    End Sub

    Private Sub GeometryService_SimplifyCompleted(ByVal sender As Object, ByVal e As GraphicsEventArgs)
      geometryService.LabelPointsAsync(e.Results)
    End Sub

    Private Sub GeometryService_LabelPointsCompleted(ByVal sender As Object, ByVal args As GraphicsEventArgs)
      For Each graphic As Graphic In args.Results
        graphic.Symbol = TryCast(LayoutRoot.Resources("DefaultRasterSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
        Dim mapPoint As MapPoint = TryCast(graphic.Geometry, MapPoint)
        graphic.Attributes.Add("X", mapPoint.X)
        graphic.Attributes.Add("Y", mapPoint.Y)
        graphicsLayer.Graphics.Add(graphic)
      Next graphic

      MyDrawObject.IsEnabled = True
    End Sub

    Private Sub GeometryService_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      MessageBox.Show("Geometry Service error: " & e.Error.Message)
    End Sub

  End Class

