Imports System
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Symbols
Imports ESRI.ArcGIS.Client.Tasks
Imports System.Collections.Generic


Partial Public Class Intersect
  Inherits UserControl
  Private MyDrawObject As Draw
  Private parcelGraphicsLayer As GraphicsLayer
  Private intersectGraphicsLayer As GraphicsLayer
  Private geometryService As GeometryService
  Private random As Random

  Public Sub New()
    InitializeComponent()

    AddHandler MyMap.Layers.LayersInitialized, AddressOf Layers_LayersInitialized

    MyMap.MinimumResolution = Double.Epsilon

    MyDrawObject = New Draw(MyMap) With
                   {
                       .DrawMode = DrawMode.Polygon,
                       .IsEnabled = False,
                       .FillSymbol = TryCast(LayoutRoot.Resources("CyanFillSymbol"), ESRI.ArcGIS.Client.Symbols.FillSymbol)
                   }

    AddHandler MyDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete

    parcelGraphicsLayer = TryCast(MyMap.Layers("ParcelsGraphicsLayer"), GraphicsLayer)
    intersectGraphicsLayer = TryCast(MyMap.Layers("IntersectGraphicsLayer"), GraphicsLayer)

    geometryService = New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")

    AddHandler geometryService.SimplifyCompleted, AddressOf GeometryService_SimplifyCompleted
    AddHandler geometryService.IntersectCompleted, AddressOf GeometryService_IntersectCompleted
    AddHandler geometryService.Failed, AddressOf GeometryService_Failed

    random = New Random()
  End Sub

  Private Sub Layers_LayersInitialized(ByVal sender As Object, ByVal args As EventArgs)
    If parcelGraphicsLayer IsNot Nothing AndAlso parcelGraphicsLayer.Graphics.Count = 0 Then
      Dim queryTask As New QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1")
      Dim query As New Query()
      Dim contractRatio As Double = MyMap.Extent.Width / 6
      query.Geometry = New Envelope(MyMap.Extent.GetCenter().X - contractRatio,
                                    MyMap.Extent.GetCenter().Y - contractRatio,
                                    MyMap.Extent.GetCenter().X + contractRatio,
                                    MyMap.Extent.GetCenter().Y + contractRatio) With {.SpatialReference = MyMap.SpatialReference}


      query.ReturnGeometry = True
      AddHandler queryTask.ExecuteCompleted, AddressOf queryTask_ExecuteCompleted
      AddHandler queryTask.Failed, AddressOf queryTask_Failed
      queryTask.ExecuteAsync(query)
    End If
  End Sub

  Private Sub queryTask_ExecuteCompleted(ByVal sender As Object, ByVal e As QueryEventArgs)
    For Each g As Graphic In e.FeatureSet.Features
      g.Symbol = TryCast(LayoutRoot.Resources("BlueFillSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
      g.Geometry.SpatialReference = MyMap.SpatialReference
      parcelGraphicsLayer.Graphics.Add(g)
    Next g
    MyDrawObject.IsEnabled = True
  End Sub

  Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As DrawEventArgs)
    MyDrawObject.IsEnabled = False

    Dim intersectPolygon As ESRI.ArcGIS.Client.Geometry.Polygon = TryCast(args.Geometry, ESRI.ArcGIS.Client.Geometry.Polygon)
    intersectPolygon.SpatialReference = MyMap.SpatialReference

    Dim graphicList As New List(Of Graphic)()
    graphicList.Add(New Graphic() With {.Geometry = intersectPolygon})
    geometryService.SimplifyAsync(graphicList)
  End Sub

  Private Sub GeometryService_SimplifyCompleted(ByVal sender As Object, ByVal e As GraphicsEventArgs)
    doIntersect(e.Results(0).Geometry)
  End Sub

  Private Sub doIntersect(ByVal intersectGeometry As Geometry)
    geometryService.IntersectAsync(parcelGraphicsLayer.Graphics.ToList(), intersectGeometry)
  End Sub

  Private Sub GeometryService_IntersectCompleted(ByVal sender As Object, ByVal e As GraphicsEventArgs)
    intersectGraphicsLayer.ClearGraphics()

    For Each g As Graphic In e.Results
      Dim symbol As New SimpleFillSymbol() With
          {
              .Fill = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255,
                                                                                                   CByte(random.Next(0, 255)),
                                                                                                   CByte(random.Next(0, 255)),
                                                                                                   CByte(random.Next(0, 255)))),
              .BorderBrush = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black),
              .BorderThickness = 1
          }
      g.Symbol = symbol
      intersectGraphicsLayer.Graphics.Add(g)
    Next g
  End Sub

  Private Sub GeometryService_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
    MessageBox.Show("Geometry Service error: " & e.Error.ToString())
  End Sub

  Private Sub queryTask_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
    MessageBox.Show("Query error: " & e.Error.ToString())
  End Sub

  Private Sub Button_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    intersectGraphicsLayer.ClearGraphics()
    MyDrawObject.IsEnabled = True
  End Sub

End Class

