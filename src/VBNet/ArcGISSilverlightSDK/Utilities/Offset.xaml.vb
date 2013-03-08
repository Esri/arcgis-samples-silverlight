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
Imports ESRI.ArcGIS.Client.Geometry


  Partial Public Class Offset
    Inherits UserControl
    Private MyDrawObject As Draw
    Private parcelGraphicsLayer As GraphicsLayer
    Private offsetGraphicsLayer As GraphicsLayer
    Private geometryService As GeometryService

    Public Sub New()
      InitializeComponent()

      AddHandler MyMap.Layers.LayersInitialized, AddressOf Layers_LayersInitialized

      MyMap.MinimumResolution = Double.Epsilon

      MyDrawObject = New Draw(MyMap) With {.DrawMode = DrawMode.Point, .IsEnabled = False}
      AddHandler MyDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete

      parcelGraphicsLayer = TryCast(MyMap.Layers("ParcelsGraphicsLayer"), GraphicsLayer)
      offsetGraphicsLayer = TryCast(MyMap.Layers("OffsetGraphicsLayer"), GraphicsLayer)
    End Sub

    Private Sub Layers_LayersInitialized(ByVal sender As Object, ByVal args As EventArgs)
      If parcelGraphicsLayer IsNot Nothing AndAlso parcelGraphicsLayer.Graphics.Count = 0 Then
        Dim queryTask As New QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1")
        Dim query As New Query()
      query.Geometry = New Envelope(-83.3188395774275, 42.614283126528512, -83.312956640689578, 42.616709132698553) With {.SpatialReference = New SpatialReference(4326)}
      query.ReturnGeometry = True
        query.OutSpatialReference = MyMap.SpatialReference
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
      MyDrawObject.IsEnabled = False
      offsetGraphicsLayer.Graphics.Clear()

      Dim point As ESRI.ArcGIS.Client.Geometry.MapPoint = TryCast(args.Geometry, ESRI.ArcGIS.Client.Geometry.MapPoint)
      point.SpatialReference = MyMap.SpatialReference
      Dim screenPnt As System.Windows.Point = MyMap.MapToScreen(point)

      ' Account for difference between Map and application origin
      Dim generalTransform As GeneralTransform = MyMap.TransformToVisual(Application.Current.RootVisual)
      Dim transformScreenPnt As System.Windows.Point = generalTransform.Transform(screenPnt)

      Dim selected As IEnumerable(Of Graphic) = parcelGraphicsLayer.FindGraphicsInHostCoordinates(transformScreenPnt)

      Dim selectedList As List(Of Graphic) = selected.ToList()
      If (selectedList.Count < 1) Then
        MyDrawObject.IsEnabled = True
        Return
      End If

		geometryService = New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")

      AddHandler geometryService.OffsetCompleted, AddressOf GeometryService_OffsetCompleted
      AddHandler geometryService.Failed, AddressOf GeometryService_Failed

      Dim offsetParameters As New OffsetParameters() With
          {
              .BevelRatio = 1.1,
              .OffsetDistance = -30,
              .OffsetHow = GeometryOffset.Bevelled,
              .OffsetUnit = LinearUnit.Meter,
              .Simplify = True
          }

      geometryService.OffsetAsync(selectedList, offsetParameters)
    End Sub

    Private Sub GeometryService_OffsetCompleted(ByVal sender As Object, ByVal e As GraphicsEventArgs)
      offsetGraphicsLayer.ClearGraphics()
      For Each g As Graphic In e.Results
        g.Symbol = TryCast(LayoutRoot.Resources("CyanFillSymbol"), Symbols.Symbol)
        offsetGraphicsLayer.Graphics.Add(g)
      Next g
      MyDrawObject.IsEnabled = True
    End Sub

    Private Sub GeometryService_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      MessageBox.Show("Geometry Service error: " & e.Error.Message)
    End Sub

  End Class

