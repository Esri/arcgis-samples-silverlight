Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Tasks


Partial Public Class BufferPoint
    Inherits UserControl
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub MyMap_MouseClick(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Map.MouseEventArgs)
        Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
        graphicsLayer.Graphics.Clear()

        e.MapPoint.SpatialReference = MyMap.SpatialReference
        Dim graphic As Graphic = New ESRI.ArcGIS.Client.Graphic() With
                                 {
                                     .Geometry = e.MapPoint,
                                     .Symbol = TryCast(LayoutRoot.Resources("DefaultClickSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
                                 }

        graphic.SetZIndex(1)
        graphicsLayer.Graphics.Add(graphic)

        Dim geometryService As New GeometryService("http://serverapps101.esri.com/arcgis/rest/services/Geometry/GeometryServer")
        AddHandler geometryService.BufferCompleted, AddressOf GeometryService_BufferCompleted
        AddHandler geometryService.Failed, AddressOf GeometryService_Failed

        ' If buffer spatial reference is GCS and unit is linear, geometry service will do geodesic buffering
        Dim bufferParams As New BufferParameters() With
            {
                .Unit = If(chkGeodesic.IsChecked.HasValue AndAlso chkGeodesic.IsChecked.Value, LinearUnit.StatuteMile, CType(Nothing, LinearUnit?)),
                .BufferSpatialReference = New SpatialReference(4326),
                .OutSpatialReference = MyMap.SpatialReference
            }

        bufferParams.Features.Add(graphic)

        bufferParams.Distances.Add(5)
        bufferParams.Geodesic = If(chkGeodesic.IsChecked, True, False)
        geometryService.BufferAsync(bufferParams)
    End Sub

    Private Sub GeometryService_BufferCompleted(ByVal sender As Object, ByVal args As GraphicsEventArgs)
        Dim results As IList(Of Graphic) = args.Results
        Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)

        For Each graphic As Graphic In results
            graphic.Symbol = TryCast(LayoutRoot.Resources("DefaultBufferSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
            graphicsLayer.Graphics.Add(graphic)
        Next graphic
    End Sub

    Private Sub GeometryService_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
        MessageBox.Show("Geometry Service error: " & e.Error.ToString)
    End Sub

End Class

