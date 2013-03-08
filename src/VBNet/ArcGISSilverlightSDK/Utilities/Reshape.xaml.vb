Imports Microsoft.VisualBasic
Imports System
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks
Imports System.Collections.Generic
Imports System.Windows.Media


  Partial Public Class Reshape
    Inherits UserControl
    Private MyDrawObject As Draw
    Private parcelGraphicsLayer As GraphicsLayer
    Private selectedGraphic As Graphic

    Public Sub New()
      InitializeComponent()

      AddHandler MyMap.Layers.LayersInitialized, AddressOf Layers_LayersInitialized

      MyMap.MinimumResolution = Double.Epsilon

      MyDrawObject = New Draw(MyMap) With
                     {
                         .DrawMode = DrawMode.Point,
                         .LineSymbol = TryCast(LayoutRoot.Resources("RedLineSymbol"), Symbols.LineSymbol),
                         .IsEnabled = False
                     }
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
      MyDrawObject.IsEnabled = False

      If MyDrawObject.DrawMode = DrawMode.Point Then
        Dim point As ESRI.ArcGIS.Client.Geometry.MapPoint = TryCast(args.Geometry, ESRI.ArcGIS.Client.Geometry.MapPoint)
        point.SpatialReference = MyMap.SpatialReference
        Dim screenPnt As System.Windows.Point = MyMap.MapToScreen(point)

        ' Account for difference between Map and application origin
        Dim generalTransform As GeneralTransform = MyMap.TransformToVisual(Application.Current.RootVisual)
        Dim transformScreenPnt As System.Windows.Point = generalTransform.Transform(screenPnt)

        Dim selected As IEnumerable(Of Graphic) = parcelGraphicsLayer.FindGraphicsInHostCoordinates(transformScreenPnt)

        If selected.ToArray().Length <= 0 Then
          MyDrawObject.IsEnabled = True
          Return
        End If

        selectedGraphic = TryCast(selected.ToList()(0), Graphic)

        selectedGraphic.Select()

        MyDrawObject.DrawMode = DrawMode.Polyline
        MyDrawObject.IsEnabled = True

        InfoTextBlock.Text = TryCast(LayoutRoot.Resources("EndText"), String)
      Else
        Dim polyline As ESRI.ArcGIS.Client.Geometry.Polyline = TryCast(args.Geometry, ESRI.ArcGIS.Client.Geometry.Polyline)
        polyline.SpatialReference = MyMap.SpatialReference

			Dim geometryService As New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")

        AddHandler geometryService.ReshapeCompleted, AddressOf GeometryService_ReshapeCompleted
        AddHandler geometryService.Failed, AddressOf GeometryService_Failed

        geometryService.ReshapeAsync(selectedGraphic.Geometry, polyline)
      End If
    End Sub

    Private Sub GeometryService_ReshapeCompleted(ByVal sender As Object, ByVal e As GeometryEventArgs)
      parcelGraphicsLayer.Graphics.Remove(selectedGraphic)

      Dim graphic As New Graphic() With
          {
              .Symbol = TryCast(LayoutRoot.Resources("BlueFillSymbol"), Symbols.Symbol),
              .Geometry = e.Result
          }

      parcelGraphicsLayer.Graphics.Add(graphic)

      MyDrawObject.DrawMode = DrawMode.Point
      MyDrawObject.IsEnabled = True
      InfoTextBlock.Text = TryCast(LayoutRoot.Resources("StartText"), String)
    End Sub

    Private Sub GeometryService_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      MessageBox.Show("Geometry Service error: " & e.Error.Message)
    End Sub

  End Class


