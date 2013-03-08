Imports Microsoft.VisualBasic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks


  Partial Public Class ConvexHull
    Inherits UserControl
    Private MyDrawObject As Draw
    Private outputGraphicsLayer As GraphicsLayer
    Private inputGraphicsLayer As GraphicsLayer

    Public Sub New()
      InitializeComponent()

      MyDrawObject = New Draw(MyMap) With {.DrawMode = DrawMode.Point, .IsEnabled = True}
      AddHandler MyDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete

      outputGraphicsLayer = TryCast(MyMap.Layers("ConvexHullGraphicsLayer"), GraphicsLayer)
      inputGraphicsLayer = TryCast(MyMap.Layers("InputGraphicsLayer"), GraphicsLayer)
    End Sub

    Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As DrawEventArgs)
      outputGraphicsLayer.ClearGraphics()

      Dim point As ESRI.ArcGIS.Client.Geometry.MapPoint = TryCast(args.Geometry, ESRI.ArcGIS.Client.Geometry.MapPoint)
      point.SpatialReference = MyMap.SpatialReference
      Dim graphic As New Graphic() With
          {
              .Symbol = TryCast(LayoutRoot.Resources("DefaultMarkerSymbol"), Symbols.Symbol),
              .Geometry = point
          }

      inputGraphicsLayer.Graphics.Add(graphic)

      If inputGraphicsLayer.Graphics.Count >= 3 Then
        ConvexButton.IsEnabled = True
      End If
    End Sub

    Private Sub ConvexButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      ConvexButton.IsEnabled = False
      outputGraphicsLayer.ClearGraphics()

		Dim geometryService As New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")
      AddHandler geometryService.ConvexHullCompleted, AddressOf GeometryService_ConvexHullCompleted
      AddHandler geometryService.Failed, AddressOf GeometryService_Failed

      geometryService.ConvexHullAsync(inputGraphicsLayer.ToList())
    End Sub

    Private Sub GeometryService_ConvexHullCompleted(ByVal sender As Object, ByVal e As GeometryEventArgs)
      Dim graphic As New Graphic() With
          {
              .Symbol = TryCast(LayoutRoot.Resources("DefaultFillSymbol"), Symbols.Symbol),
              .Geometry = e.Result
          }
      outputGraphicsLayer.Graphics.Add(graphic)

      ConvexButton.IsEnabled = True
    End Sub

    Private Sub GeometryService_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      MessageBox.Show("Geometry Service error: " & e.Error.Message)
    End Sub

  End Class

