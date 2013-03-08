Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Symbols


Partial Public Class GeodesicOperations
  Inherits UserControl
  Private MyDrawObject As Draw
  Private featureGraphicsLayer As GraphicsLayer
  Private verticesGraphicsLayer As GraphicsLayer

  Private Shared _mercator As New ESRI.ArcGIS.Client.Projection.WebMercator()

  Public Sub New()
    InitializeComponent()

    featureGraphicsLayer = TryCast(MyMap.Layers("GeodesicResultGraphicsLayer"), GraphicsLayer)
    verticesGraphicsLayer = TryCast(MyMap.Layers("VerticesGraphicsLayer"), GraphicsLayer)

    MyDrawObject = New Draw(MyMap) With {.LineSymbol = TryCast(LayoutRoot.Resources("DrawLineSymbol"), LineSymbol), .FillSymbol = TryCast(LayoutRoot.Resources("DrawFillSymbol"), FillSymbol)}

    AddHandler MyDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete
  End Sub

  Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As ESRI.ArcGIS.Client.DrawEventArgs)
    Dim geometryType As Type = args.Geometry.GetType()
    Dim wgs84Geometry As Geometry = _mercator.ToGeographic(args.Geometry)
    Dim mercatorDensifiedGeometry As Geometry = Nothing
    Dim densifiedGeometry As Geometry = Nothing
    Dim originalVerticeCount As Integer = 0
    Dim densifiedVerticeCount As Integer = 0

    If geometryType Is GetType(Polygon) Then
      ' Values returned in meters
      If RadioButtonGeodesic.IsChecked.Value Then
        TextBlockLength.Text = (ESRI.ArcGIS.Client.Geometry.Geodesic.Length(TryCast(wgs84Geometry, Polygon)) * 0.000621371192).ToString("#0.000") & " mi"
        TextBlockArea.Text = (Math.Abs(ESRI.ArcGIS.Client.Geometry.Geodesic.Area(TryCast(wgs84Geometry, Polygon))) * 0.000000386102159).ToString("#0.000") & " sq mi"
      Else
        TextBlockLength.Text = (ESRI.ArcGIS.Client.Geometry.Euclidian.Length(TryCast(args.Geometry, Polygon)) * 0.000621371192).ToString("#0.000") & " mi"
        TextBlockArea.Text = (Math.Abs(ESRI.ArcGIS.Client.Geometry.Euclidian.Area(TryCast(args.Geometry, Polygon))) * 0.000000386102159).ToString("#0.000") & " sq mi"
      End If

      For Each ring As PointCollection In (TryCast(args.Geometry, Polygon)).Rings
        For Each mp As MapPoint In ring
          originalVerticeCount += 1
          verticesGraphicsLayer.Graphics.Add(New ESRI.ArcGIS.Client.Graphic() With {.Geometry = mp, .Symbol = TryCast(LayoutRoot.Resources("OriginalMarkerSymbol"), Symbol)})
        Next mp
      Next ring

      If RadioButtonGeodesic.IsChecked.Value Then
        densifiedGeometry = ESRI.ArcGIS.Client.Geometry.Geodesic.Densify(wgs84Geometry, MyMap.Resolution * 10)
        mercatorDensifiedGeometry = _mercator.FromGeographic(densifiedGeometry)
      Else
        mercatorDensifiedGeometry = ESRI.ArcGIS.Client.Geometry.Euclidian.Densify(args.Geometry, MyMap.Resolution * 10)
      End If

      For Each ring As PointCollection In (TryCast(mercatorDensifiedGeometry, Polygon)).Rings
        For Each mp As MapPoint In ring
          densifiedVerticeCount += 1
          verticesGraphicsLayer.Graphics.Add(New ESRI.ArcGIS.Client.Graphic() With {.Geometry = mp, .Symbol = TryCast(LayoutRoot.Resources("NewMarkerSymbol"), Symbol)})
        Next mp
      Next ring
    Else ' Polyline
      ' Value returned in meters
      If RadioButtonGeodesic.IsChecked.Value Then
        TextBlockLength.Text = (ESRI.ArcGIS.Client.Geometry.Geodesic.Length(TryCast(wgs84Geometry, Polyline)) * 0.000621371192).ToString("#0.000") & " mi"
      Else
        TextBlockLength.Text = (ESRI.ArcGIS.Client.Geometry.Euclidian.Length(TryCast(args.Geometry, Polyline)) * 0.000621371192).ToString("#0.000") & " mi"
      End If
      TextBlockArea.Text = "NA"

      For Each path As PointCollection In (TryCast(args.Geometry, Polyline)).Paths
        For Each mp As MapPoint In path
          originalVerticeCount += 1
          verticesGraphicsLayer.Graphics.Add(New ESRI.ArcGIS.Client.Graphic() With {.Geometry = mp, .Symbol = TryCast(LayoutRoot.Resources("OriginalMarkerSymbol"), Symbol)})
        Next mp
      Next path

      If RadioButtonGeodesic.IsChecked.Value Then
        densifiedGeometry = ESRI.ArcGIS.Client.Geometry.Geodesic.Densify(wgs84Geometry, MyMap.Resolution * 10)
        mercatorDensifiedGeometry = _mercator.FromGeographic(densifiedGeometry)
      Else
        mercatorDensifiedGeometry = ESRI.ArcGIS.Client.Geometry.Euclidian.Densify(args.Geometry, MyMap.Resolution * 10)
      End If

      For Each path As PointCollection In (TryCast(mercatorDensifiedGeometry, Polyline)).Paths
        For Each mp As MapPoint In path
          densifiedVerticeCount += 1
          verticesGraphicsLayer.Graphics.Add(New ESRI.ArcGIS.Client.Graphic() With {.Geometry = mp, .Symbol = TryCast(LayoutRoot.Resources("NewMarkerSymbol"), Symbol)})
        Next mp
      Next path
    End If

    featureGraphicsLayer.Graphics.Add(New ESRI.ArcGIS.Client.Graphic() With {
                                      .Geometry = mercatorDensifiedGeometry,
                                      .Symbol = If(geometryType Is GetType(Polygon), TryCast(LayoutRoot.Resources("ResultFillSymbol"), Symbol), TryCast(LayoutRoot.Resources("ResultLineSymbol"), Symbol))})

    TextBlockVerticesBefore.Text = originalVerticeCount.ToString()
    TextBlockVerticesAfter.Text = densifiedVerticeCount.ToString()
  End Sub

  Private Sub Button_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    Dim button As Button = TryCast(sender, Button)
    If (TryCast(button.Tag, String)) = "Polygon" Then
      MyDrawObject.DrawMode = DrawMode.Polygon
      MyDrawObject.IsEnabled = True
    ElseIf (TryCast(button.Tag, String)) = "Polyline" Then
      MyDrawObject.DrawMode = DrawMode.Polyline
      MyDrawObject.IsEnabled = True
    Else
      MyDrawObject.IsEnabled = False
      featureGraphicsLayer.ClearGraphics()
      verticesGraphicsLayer.ClearGraphics()
    End If
  End Sub
End Class

