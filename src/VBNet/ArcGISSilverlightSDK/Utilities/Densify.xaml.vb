Imports Microsoft.VisualBasic
Imports System
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Tasks


  Partial Public Class Densify
    Inherits UserControl
    Private MyDrawObject As Draw

    Public Sub New()
      InitializeComponent()
      MyDrawObject = New Draw(MyMap) With
                     {
                         .DrawMode = DrawMode.Polygon,
                         .IsEnabled = True,
                         .FillSymbol = TryCast(LayoutRoot.Resources("DefaultFillSymbol"), ESRI.ArcGIS.Client.Symbols.FillSymbol)
                     }

      AddHandler MyDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete
      AddHandler MyDrawObject.DrawBegin, AddressOf MyDrawObject_DrawBegin
    End Sub

    Private Sub MyDrawObject_DrawBegin(ByVal sender As Object, ByVal args As EventArgs)
      Dim graphicsLayerPolygon As GraphicsLayer = TryCast(MyMap.Layers("PolygonGraphicsLayer"), GraphicsLayer)
      graphicsLayerPolygon.ClearGraphics()
      Dim graphicsLayerVertices As GraphicsLayer = TryCast(MyMap.Layers("VerticesGraphicsLayer"), GraphicsLayer)
      graphicsLayerVertices.ClearGraphics()
    End Sub

    Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As DrawEventArgs)
      Dim polygon As ESRI.ArcGIS.Client.Geometry.Polygon = TryCast(args.Geometry, ESRI.ArcGIS.Client.Geometry.Polygon)
      polygon.SpatialReference = MyMap.SpatialReference
      Dim graphic As New Graphic() With
          {
              .Symbol = TryCast(LayoutRoot.Resources("DefaultFillSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol),
              .Geometry = polygon
          }

      Dim graphicsLayerPolygon As GraphicsLayer = TryCast(MyMap.Layers("PolygonGraphicsLayer"), GraphicsLayer)
      graphicsLayerPolygon.Graphics.Add(graphic)

      Dim graphicsLayerVertices As GraphicsLayer = TryCast(MyMap.Layers("VerticesGraphicsLayer"), GraphicsLayer)
      For Each point As MapPoint In polygon.Rings(0)
        Dim vertice As New Graphic() With
            {
                .Symbol = TryCast(LayoutRoot.Resources("DefaultMarkerSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol),
                .Geometry = point
            }

        graphicsLayerVertices.Graphics.Add(vertice)
      Next point
      DensifyButton.IsEnabled = True
    End Sub

    Private Sub DensifyButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      DensifyButton.IsEnabled = False

      Dim graphicsLayerPolygon As GraphicsLayer = TryCast(MyMap.Layers("PolygonGraphicsLayer"), GraphicsLayer)

		Dim geometryService As New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")
      AddHandler geometryService.DensifyCompleted, AddressOf GeometryService_DensifyCompleted
      AddHandler geometryService.Failed, AddressOf GeometryService_Failed

		Dim densityParameters As New DensifyParameters() With
			{
				.LengthUnit = LinearUnit.Meter,
				.Geodesic = True,
				.MaxSegmentLength = MyMap.Resolution * 10
			}

      geometryService.DensifyAsync(graphicsLayerPolygon.Graphics.ToList(), densityParameters)
    End Sub

    Private Sub GeometryService_DensifyCompleted(ByVal sender As Object, ByVal e As GraphicsEventArgs)
      Dim graphicsLayerVertices As GraphicsLayer = TryCast(MyMap.Layers("VerticesGraphicsLayer"), GraphicsLayer)
      For Each g As Graphic In e.Results
        Dim p As ESRI.ArcGIS.Client.Geometry.Polygon = TryCast(g.Geometry, ESRI.ArcGIS.Client.Geometry.Polygon)

        For Each pc As ESRI.ArcGIS.Client.Geometry.PointCollection In p.Rings
          For Each point As MapPoint In pc
            Dim vertice As New Graphic() With
                {
                    .Symbol = TryCast(LayoutRoot.Resources("NewMarkerSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol),
                    .Geometry = point
                }
            graphicsLayerVertices.Graphics.Add(vertice)
          Next point
        Next pc
      Next g
      DensifyButton.IsEnabled = True
    End Sub

    Private Sub GeometryService_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      MessageBox.Show("Geometry Service error: " & e.Error.ToString)
    End Sub

  End Class

