Imports Microsoft.VisualBasic
Imports System
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks
Imports ESRI.ArcGIS.Client.Geometry
Imports System.Collections.Generic


  Partial Public Class TrimExtend
    Inherits UserControl
    Private MyDrawObject As Draw
    Private polylineLayer As GraphicsLayer
    Private resultsLayer As GraphicsLayer
    Private geometryService As GeometryService

    Public Sub New()
      InitializeComponent()

      polylineLayer = TryCast(MyMap.Layers("MyPolylineGraphicsLayer"), GraphicsLayer)
      resultsLayer = TryCast(MyMap.Layers("MyResultsGraphicsLayer"), GraphicsLayer)
      AddHandler MyMap.Layers.LayersInitialized, AddressOf Layers_LayersInitialized

      MyDrawObject = New Draw(MyMap) With
                     {
                         .DrawMode = DrawMode.Polyline,
                         .IsEnabled = True,
                         .LineSymbol = TryCast(LayoutRoot.Resources("DrawLineSymbol"), Symbols.LineSymbol)
                     }

      AddHandler MyDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete
    End Sub

    Private Sub Layers_LayersInitialized(ByVal sender As Object, ByVal args As EventArgs)
      For Each g As Graphic In polylineLayer.Graphics
        g.Geometry.SpatialReference = MyMap.SpatialReference
      Next g
    End Sub

    Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As DrawEventArgs)
      MyDrawObject.IsEnabled = False

      resultsLayer.ClearGraphics()

      Dim polyline As Polyline = TryCast(args.Geometry, Polyline)
      polyline.SpatialReference = MyMap.SpatialReference

		geometryService = New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")
      AddHandler geometryService.TrimExtendCompleted, AddressOf GeometryService_TrimExtendCompleted
      AddHandler geometryService.Failed, AddressOf GeometryService_Failed

      Dim polylineList As New List(Of Polyline)()
      For Each g As Graphic In polylineLayer.Graphics
        polylineList.Add(TryCast(g.Geometry, Polyline))
      Next g

      geometryService.TrimExtendAsync(polylineList, polyline, CurveExtension.DefaultCurveExtension)
    End Sub

    Private Sub GeometryService_TrimExtendCompleted(ByVal sender As Object, ByVal e As GraphicsEventArgs)
      For Each g As Graphic In e.Results
        g.Symbol = TryCast(LayoutRoot.Resources("ResultsLineSymbol"), Symbols.Symbol)
        resultsLayer.Graphics.Add(g)
      Next g

      MyDrawObject.IsEnabled = True
    End Sub

    Private Sub GeometryService_Failed(ByVal sender As Object, ByVal args As TaskFailedEventArgs)
      MessageBox.Show("Geometry Service error: " & args.Error.Message)
    End Sub

  End Class

