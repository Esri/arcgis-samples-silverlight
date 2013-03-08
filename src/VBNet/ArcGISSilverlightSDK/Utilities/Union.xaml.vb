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


  Partial Public Class Union
    Inherits UserControl
    Private MyDrawObject As Draw
    Private parcelGraphicsLayer As GraphicsLayer
    Private selectedGraphics As New List(Of Graphic)()

    Public Sub New()
      InitializeComponent()

      AddHandler MyMap.Layers.LayersInitialized, AddressOf Layers_LayersInitialized

      MyMap.MinimumResolution = Double.Epsilon

      MyDrawObject = New Draw(MyMap) With {.DrawMode = DrawMode.Point, .IsEnabled = False}
      AddHandler MyDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete

      parcelGraphicsLayer = TryCast(MyMap.Layers("ParcelsGraphicsLayer"), GraphicsLayer)
    End Sub

    Private Sub Layers_LayersInitialized(ByVal sender As Object, ByVal args As EventArgs)
      If parcelGraphicsLayer IsNot Nothing AndAlso parcelGraphicsLayer.Graphics.Count = 0 Then
        Dim queryTask As New QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1")
        Dim query As New Query()
        query.Geometry = MyMap.Extent
        query.ReturnGeometry = True
        AddHandler queryTask.ExecuteCompleted, AddressOf queryTask_ExecuteCompleted
        AddHandler queryTask.Failed, AddressOf queryTask_Failed
        queryTask.ExecuteAsync(query)
      End If
    End Sub

    Private Sub queryTask_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      MessageBox.Show("Query error: " & e.Error.Message)
    End Sub

    Private Sub queryTask_ExecuteCompleted(ByVal sender As Object, ByVal e As QueryEventArgs)
      For Each g As Graphic In e.FeatureSet.Features
        g.Symbol = TryCast(LayoutRoot.Resources("BlueFillSymbol"), Symbols.Symbol)
        g.Geometry.SpatialReference = MyMap.SpatialReference
        parcelGraphicsLayer.Graphics.Add(g)
      Next g
      MyDrawObject.IsEnabled = True
    End Sub

    Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As DrawEventArgs)
      Dim point As ESRI.ArcGIS.Client.Geometry.MapPoint = TryCast(args.Geometry, ESRI.ArcGIS.Client.Geometry.MapPoint)
      point.SpatialReference = MyMap.SpatialReference
      Dim screenPnt As System.Windows.Point = MyMap.MapToScreen(point)

      ' Account for difference between Map and application origin
      Dim generalTransform As GeneralTransform = MyMap.TransformToVisual(Application.Current.RootVisual)
      Dim transformScreenPnt As System.Windows.Point = generalTransform.Transform(screenPnt)

      Dim selected As IEnumerable(Of Graphic) = parcelGraphicsLayer.FindGraphicsInHostCoordinates(transformScreenPnt)

      For Each g As Graphic In selected
        If g.Selected Then
          g.UnSelect()
          selectedGraphics.Remove(g)
        Else
          g.Select()
          selectedGraphics.Add(g)
        End If
      Next g

      If selectedGraphics.Count > 1 Then
        UnionButton.IsEnabled = True
      Else
        UnionButton.IsEnabled = False
      End If
    End Sub

    Private Sub UnionButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      UnionButton.IsEnabled = False
      MyDrawObject.IsEnabled = False

		Dim geometryService As New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")
      AddHandler geometryService.UnionCompleted, AddressOf GeometryService_UnionCompleted
      AddHandler geometryService.Failed, AddressOf GeometryService_Failed

      geometryService.UnionAsync(selectedGraphics)
    End Sub

    Private Sub GeometryService_UnionCompleted(ByVal sender As Object, ByVal e As GeometryEventArgs)
      For Each g As Graphic In selectedGraphics
        parcelGraphicsLayer.Graphics.Remove(g)
      Next g
      selectedGraphics.Clear()

      parcelGraphicsLayer.Graphics.Add(New Graphic() With
                                       {
                                           .Geometry = e.Result,
                                           .Symbol = TryCast(LayoutRoot.Resources("BlueFillSymbol"), Symbols.Symbol)
                                       })

      MyDrawObject.IsEnabled = True
    End Sub

    Private Sub GeometryService_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      MessageBox.Show("Geometry Service error: " & e.Error.Message)
    End Sub

  End Class

