Imports Microsoft.VisualBasic
Imports System
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks


  Partial Public Class Cut
    Inherits UserControl
    Private MyDrawObject As Draw
    Private parcelGraphicsLayer As GraphicsLayer

    Public Sub New()
      InitializeComponent()

      AddHandler MyMap.Layers.LayersInitialized, AddressOf Layers_LayersInitialized

      MyMap.MinimumResolution = Double.Epsilon

      MyDrawObject = New Draw(MyMap) With
                     {
                         .DrawMode = DrawMode.Polyline,
                         .IsEnabled = False,
                         .LineSymbol = TryCast(LayoutRoot.Resources("RedLineSymbol"), Symbols.LineSymbol)
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

      Dim polyline As ESRI.ArcGIS.Client.Geometry.Polyline = TryCast(args.Geometry, ESRI.ArcGIS.Client.Geometry.Polyline)
      polyline.SpatialReference = MyMap.SpatialReference

		Dim geometryService As New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")

      AddHandler geometryService.CutCompleted, AddressOf GeometryService_CutCompleted
      AddHandler geometryService.Failed, AddressOf GeometryService_Failed

      geometryService.CutAsync(parcelGraphicsLayer.Graphics.ToList(), polyline)
    End Sub

    Private Sub GeometryService_CutCompleted(ByVal sender As Object, ByVal e As CutEventArgs)
      parcelGraphicsLayer.Graphics.Clear()

      For Each g As Graphic In e.Results
        g.Symbol = TryCast(LayoutRoot.Resources("BlueFillSymbol"), Symbols.Symbol)
        parcelGraphicsLayer.Graphics.Add(g)
      Next g
      MyDrawObject.IsEnabled = True
    End Sub

    Private Sub GeometryService_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      MessageBox.Show("Geometry Service error: " & e.Error.Message)
    End Sub

  End Class

