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


  Partial Public Class Difference
    Inherits UserControl
    Private MyDrawObject As Draw
    Private geometryService As GeometryService
    Private inputGraphicsLayer As GraphicsLayer
    Private outputGraphicsLayer As GraphicsLayer

    Public Sub New()
      InitializeComponent()

		geometryService = New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")

      AddHandler geometryService.DifferenceCompleted, AddressOf GeometryService_DifferenceCompleted
      AddHandler geometryService.SimplifyCompleted, AddressOf GeometryService_SimplifyCompleted
      AddHandler geometryService.Failed, AddressOf GeometryService_Failed

      inputGraphicsLayer = TryCast(MyMap.Layers("InputGraphicsLayer"), GraphicsLayer)

      MyDrawObject = New Draw(MyMap) With
                     {
                         .DrawMode = DrawMode.Polygon,
                         .IsEnabled = True,
                         .FillSymbol = TryCast(LayoutRoot.Resources("DrawFillSymbol"), Symbols.FillSymbol)
                     }

      AddHandler MyDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete
    End Sub

    Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As DrawEventArgs)
      MyDrawObject.IsEnabled = False

      Dim polygon As ESRI.ArcGIS.Client.Geometry.Polygon = TryCast(args.Geometry, ESRI.ArcGIS.Client.Geometry.Polygon)
      polygon.SpatialReference = MyMap.SpatialReference

      geometryService.SimplifyAsync(New List(Of Graphic)(New Graphic() {New Graphic() With {.Geometry = polygon}}))
    End Sub

    Private Sub GeometryService_SimplifyCompleted(ByVal sender As Object, ByVal e As GraphicsEventArgs)
      geometryService.DifferenceAsync(inputGraphicsLayer.Graphics.ToList(), e.Results(0).Geometry)
    End Sub

    Private Sub GeometryService_DifferenceCompleted(ByVal sender As Object, ByVal e As GraphicsEventArgs)
      outputGraphicsLayer = TryCast(MyMap.Layers("OutputGraphicsLayer"), GraphicsLayer)
      outputGraphicsLayer.Graphics.Clear()
      For Each g As Graphic In e.Results
        If TypeOf g.Geometry Is Polygon Then
          g.Symbol = TryCast(LayoutRoot.Resources("DifferenceFillSymbol"), Symbols.Symbol)
        End If
        outputGraphicsLayer.Graphics.Add(g)
      Next g

      MyDrawObject.IsEnabled = True
      ResetButton.IsEnabled = True
    End Sub

    Private Sub GeometryService_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      MessageBox.Show("Geometry Service error: " & e.Error.Message)
      MyDrawObject.IsEnabled = True
    End Sub

    Private Sub ResetButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      Dim outputGraphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("OutputGraphicsLayer"), GraphicsLayer)
      outputGraphicsLayer.Graphics.Clear()

      ResetButton.IsEnabled = False
    End Sub

  End Class

