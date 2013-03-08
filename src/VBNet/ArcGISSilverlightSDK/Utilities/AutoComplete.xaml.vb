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
Imports System.Windows.Shapes
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks


  Partial Public Class AutoComplete
    Inherits UserControl
    Private MyDrawObject As Draw

    Public Sub New()
      InitializeComponent()

      MyMap.MinimumResolution = Double.Epsilon

      Dim queryTask As New QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1")
      Dim query As New Query()
      query.Geometry = MyMap.Extent
      query.ReturnGeometry = True
      AddHandler queryTask.ExecuteCompleted, AddressOf queryTask_ExecuteCompleted
      AddHandler queryTask.Failed, AddressOf queryTask_Failed
      queryTask.ExecuteAsync(query)

      MyDrawObject = New Draw(MyMap) With
                     {
                         .DrawMode = DrawMode.Polyline,
                         .IsEnabled = True,
                         .LineSymbol = TryCast(LayoutRoot.Resources("RedLineSymbol"), Symbols.LineSymbol)
                     }

      AddHandler MyDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete
    End Sub

    Private Sub queryTask_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      MessageBox.Show("Query error: " & e.Error.Message)
    End Sub

    Private Sub queryTask_ExecuteCompleted(ByVal sender As Object, ByVal e As QueryEventArgs)
      Dim parcelGraphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("ParcelsGraphicsLayer"), GraphicsLayer)
      For Each g As Graphic In e.FeatureSet.Features
        g.Symbol = TryCast(LayoutRoot.Resources("BlueFillSymbol"), Symbols.Symbol)
        parcelGraphicsLayer.Graphics.Add(g)
      Next g
    End Sub

    Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As DrawEventArgs)
      Dim polyline As ESRI.ArcGIS.Client.Geometry.Polyline = TryCast(args.Geometry, ESRI.ArcGIS.Client.Geometry.Polyline)
      polyline.SpatialReference = MyMap.SpatialReference

      Dim polylineGraphic As New Graphic() With {.Geometry = polyline}
      Dim polylineList As New List(Of Graphic)()
      polylineList.Add(polylineGraphic)

		Dim geometryService As New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")
      AddHandler geometryService.AutoCompleteCompleted, AddressOf GeometryService_AutoCompleteCompleted
      AddHandler geometryService.Failed, AddressOf GeometryService_Failed

      Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("ParcelsGraphicsLayer"), GraphicsLayer)
      Dim polygonList As New List(Of Graphic)()
      For Each g As Graphic In graphicsLayer.Graphics
        g.Geometry.SpatialReference = MyMap.SpatialReference
        polygonList.Add(g)
      Next g

      geometryService.AutoCompleteAsync(polygonList, polylineList)
    End Sub

    Private Sub GeometryService_AutoCompleteCompleted(ByVal sender As Object, ByVal e As GraphicsEventArgs)
      Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("CompletedGraphicsLayer"), GraphicsLayer)
      graphicsLayer.ClearGraphics()

      For Each g As Graphic In e.Results
        g.Symbol = TryCast(LayoutRoot.Resources("RedFillSymbol"), Symbols.Symbol)
        graphicsLayer.Graphics.Add(g)
      Next g

      If e.Results.Count > 0 Then
        TryCast(MyMap.Layers("ConnectDotsGraphicsLayer"), GraphicsLayer).ClearGraphics()
      End If

    End Sub

    Private Sub GeometryService_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      MessageBox.Show("Geometry Service error: " & e.Error.Message)
    End Sub

  End Class

