Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Tasks


Partial Public Class AreasAndLengths
  Inherits UserControl
  Private MyDrawObject As Draw

  Public Sub New()
    InitializeComponent()
    Dim initialExtent As New ESRI.ArcGIS.Client.Geometry.Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(New ESRI.ArcGIS.Client.Geometry.MapPoint(-130, 20)),
                                                                  ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(New ESRI.ArcGIS.Client.Geometry.MapPoint(-65, 55)))

    initialExtent.SpatialReference = New SpatialReference(102100)

    MyMap.Extent = initialExtent

    MyDrawObject = New Draw(MyMap) With
                   {
                       .DrawMode = DrawMode.Polygon,
                       .IsEnabled = True,
                       .FillSymbol = TryCast(LayoutRoot.Resources("DefaultFillSymbol"), Symbols.FillSymbol)
                   }

    AddHandler MyDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete
    AddHandler MyDrawObject.DrawBegin, AddressOf MyDrawObject_DrawBegin

    calculationTypeCombo.ItemsSource = System.Enum.GetValues(GetType(CalculationType))
    calculationTypeCombo.SelectedIndex = 0
  End Sub

  Private Sub MyDrawObject_DrawBegin(ByVal sender As Object, ByVal args As EventArgs)
    Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
    graphicsLayer.ClearGraphics()
  End Sub

  Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As DrawEventArgs)
    Dim polygon As ESRI.ArcGIS.Client.Geometry.Polygon = TryCast(args.Geometry, ESRI.ArcGIS.Client.Geometry.Polygon)
    polygon.SpatialReference = MyMap.SpatialReference

    Dim graphic As New Graphic() With
        {
            .Symbol = TryCast(LayoutRoot.Resources("DefaultFillSymbol"), Symbols.Symbol),
            .Geometry = polygon
        }

    Dim geometryService As New GeometryService("http://serverapps101.esri.com/arcgis/rest/services/Geometry/GeometryServer")
    AddHandler geometryService.AreasAndLengthsCompleted, AddressOf GeometryService_AreasAndLengthsCompleted
    AddHandler geometryService.Failed, AddressOf GeometryService_Failed

    Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
    graphicsLayer.Graphics.Add(graphic)

    Dim graphicList As New List(Of Graphic)()
    graphicList.Add(graphic)

    'Since there are multiple overloads for AreasAndLengthsAsync, make sure to use appropriate signature with correct parameter types.
    geometryService.AreasAndLengthsAsync(graphicList, Nothing, Nothing, CType(calculationTypeCombo.SelectedValue, CalculationType))

    ' GeometryService.AreasAndLengths returns distances and areas in the units of the spatial reference.
    ' The units in the map view's projection is decimal degrees. 
    ' Use the Project method to convert graphic points to a projection that uses a measured unit (e.g. meters).
    ' If the map units are in measured units, the call to Project is unnecessary. 
    ' Important: Use a projection appropriate for your area of interest.
  End Sub

  Private Sub GeometryService_AreasAndLengthsCompleted(ByVal sender As Object, ByVal args As AreasAndLengthsEventArgs)
    ' convert results from meters into miles and sq meters into sq miles for our display
    Dim miles As Double = args.Results.Lengths(0) * 0.0006213700922
    Dim sqmi As Double = Math.Abs(args.Results.Areas(0)) * 0.0000003861003
    ResponseTextBlock.Text = String.Format("Polygon area: {0} sq. miles" & Constants.vbLf &
                                           "Polygon perimeter: {1} miles.", Math.Round(sqmi, 3), Math.Round(miles, 3))
  End Sub

  Private Sub GeometryService_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
    MessageBox.Show("Geometry Service error: " & e.Error.Message)
  End Sub

End Class

