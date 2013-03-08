Imports Microsoft.VisualBasic
Imports System
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks


  Partial Public Class Lengths
    Inherits UserControl
    Private MyDrawObject As Draw

    Public Sub New()
      InitializeComponent()
      MyDrawObject = New Draw(MyMap) With
                     {
                         .DrawMode = DrawMode.Polyline,
                         .IsEnabled = True,
                         .LineSymbol = TryCast(LayoutRoot.Resources("DefaultLineSymbol"), Symbols.LineSymbol)
                     }

      AddHandler MyDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete
      AddHandler MyDrawObject.DrawBegin, AddressOf MyDrawObject_DrawBegin
    End Sub

    Private Sub MyDrawObject_DrawBegin(ByVal sender As Object, ByVal args As EventArgs)
      Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
      graphicsLayer.ClearGraphics()
    End Sub

    Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As DrawEventArgs)
      Dim polyline As ESRI.ArcGIS.Client.Geometry.Polyline = TryCast(args.Geometry, ESRI.ArcGIS.Client.Geometry.Polyline)
      polyline.SpatialReference = MyMap.SpatialReference
      Dim graphic As New Graphic() With
          {
              .Symbol = TryCast(LayoutRoot.Resources("DefaultLineSymbol"), Symbols.Symbol),
              .Geometry = polyline
          }

		Dim geometryService As New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")
      AddHandler geometryService.LengthsCompleted, AddressOf GeometryService_LengthsCompleted
      AddHandler geometryService.Failed, AddressOf GeometryService_Failed

      Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
      graphicsLayer.Graphics.Add(graphic)
    geometryService.LengthsAsync(graphicsLayer.Graphics.ToList(), LinearUnit.SurveyMile, CalculationType.Geodesic, Nothing)
    End Sub

    Private Sub GeometryService_LengthsCompleted(ByVal sender As Object, ByVal args As ESRI.ArcGIS.Client.Tasks.LengthsEventArgs)
      ResponseTextBlock.Text = String.Format("The distance of the polyline is {0} miles", Math.Round(args.Results(0), 3))
    End Sub

    Private Sub GeometryService_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      MessageBox.Show("Geometry Service error: " & e.Error.Message)
    End Sub

  End Class

