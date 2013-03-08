Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks
Imports ESRI.ArcGIS.Client.Geometry


  Partial Public Class Distance
    Inherits UserControl
    Private MyDrawObject As Draw
    Private graphicsLayer As GraphicsLayer

    Public Sub New()
      InitializeComponent()

      graphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)

      MyDrawObject = New Draw(MyMap) With {.DrawMode = DrawMode.Polyline, .IsEnabled = True}
      AddHandler MyDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete
      AddHandler MyDrawObject.DrawBegin, AddressOf MyDrawObject_DrawBegin
    End Sub

    Private Sub MyDrawObject_DrawBegin(ByVal sender As Object, ByVal args As EventArgs)
      If graphicsLayer.Graphics.Count >= 2 Then
        graphicsLayer.ClearGraphics()
      End If
    End Sub

    Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As DrawEventArgs)
      args.Geometry.SpatialReference = MyMap.SpatialReference
      Dim graphic As New Graphic() With {.Geometry = args.Geometry}
      If TypeOf args.Geometry Is Polyline Then
        graphic.Symbol = TryCast(LayoutRoot.Resources("DefaultLineSymbol"), Symbols.Symbol)
      Else
        graphic.Symbol = TryCast(LayoutRoot.Resources("DefaultMarkerSymbol"), Symbols.Symbol)
      End If

      graphicsLayer.Graphics.Add(graphic)

      If graphicsLayer.Graphics.Count = 1 Then
        MyDrawObject.DrawMode = DrawMode.Point
      ElseIf graphicsLayer.Graphics.Count = 2 Then
        MyDrawObject.IsEnabled = False
			Dim geometryService As New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")
        AddHandler geometryService.DistanceCompleted, AddressOf GeometryService_DistanceCompleted
        AddHandler geometryService.Failed, AddressOf GeometryService_Failed

        MyDrawObject.DrawMode = DrawMode.Polyline

        Dim distanceParameters As New DistanceParameters() With {.DistanceUnit = LinearUnit.SurveyMile, .Geodesic = True}

        geometryService.DistanceAsync(graphicsLayer.Graphics(0).Geometry, graphicsLayer.Graphics(1).Geometry, distanceParameters)
        ResponseTextBlock.Text = "The distance between geometries is... "
      End If
    End Sub

    Private Sub GeometryService_DistanceCompleted(ByVal sender As Object, ByVal e As DistanceEventArgs)
      ResponseTextBlock.Text = String.Format("The distance between geometries is {0} miles", Math.Round(e.Distance, 3))
      MyDrawObject.IsEnabled = True
    End Sub

    Private Sub GeometryService_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      MessageBox.Show("Geometry Service error: " & e.Error.Message)
      MyDrawObject.IsEnabled = True
    End Sub

  End Class

