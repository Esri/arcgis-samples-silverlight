Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Symbols
Imports ESRI.ArcGIS.Client.Tasks
Imports System.Threading
Imports System.Collections.Generic
Imports System


Partial Public Class IntersectTaskAsync
    Inherits UserControl
    Private _myDrawObject As Draw
    Private _intersectGraphicsLayer As GraphicsLayer
    Private _geometryService As GeometryService
    Private _cts As CancellationTokenSource

    Public Sub New()
        InitializeComponent()

        _myDrawObject = New Draw(MyMap) With {.DrawMode = DrawMode.Polygon, .IsEnabled = False, .FillSymbol = TryCast(LayoutRoot.Resources("CyanFillSymbol"), ESRI.ArcGIS.Client.Symbols.FillSymbol)}
        AddHandler _myDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete
        _myDrawObject.IsEnabled = True

        _intersectGraphicsLayer = TryCast(MyMap.Layers("IntersectGraphicsLayer"), GraphicsLayer)

        _geometryService = New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")
    End Sub

    Private Async Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As DrawEventArgs)
        Try
            _myDrawObject.IsEnabled = False

            If _cts IsNot Nothing Then
                _cts.Cancel()
            End If

            _cts = New CancellationTokenSource()

            Dim queryTask As New QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1")
            Dim query As New Query() With {.Geometry = args.Geometry, .ReturnGeometry = True, .OutSpatialReference = MyMap.SpatialReference}

            Dim parcelsToIntersectResult As QueryResult = Await queryTask.ExecuteTaskAsync(query, _cts.Token)

            Dim graphicList As New List(Of Graphic)()
            graphicList.Add(New Graphic() With {.Geometry = args.Geometry})
            Dim simplifiedIntersectGeometryResult As SimplifyResult = Await _geometryService.SimplifyTaskAsync(graphicList, _cts.Token)

            

            Dim intersectedParcelsResult As IntersectResult = Await _geometryService.IntersectTaskAsync(parcelsToIntersectResult.FeatureSet.Features, simplifiedIntersectGeometryResult.Results(0).Geometry, _cts.Token)

            Dim random As New Random()
            For Each g As Graphic In intersectedParcelsResult.Results
                Dim symbol As New SimpleFillSymbol() With {.Fill = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, CByte(random.Next(0, 255)), CByte(random.Next(0, 255)), CByte(random.Next(0, 255)))), .BorderBrush = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black), .BorderThickness = 1}
                g.Symbol = symbol
                _intersectGraphicsLayer.Graphics.Add(g)
            Next g
        Catch ex As Exception
            If TypeOf ex Is ServiceException Then
                MessageBox.Show(String.Format("{0}: {1}", (TryCast(ex, ServiceException)).Code.ToString(), (TryCast(ex, ServiceException)).Details(0)), "Error", MessageBoxButton.OK)
                Return
            End If
        End Try
    End Sub

    Private Sub Button_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        _intersectGraphicsLayer.Graphics.Clear()
        _myDrawObject.IsEnabled = True
    End Sub
End Class

