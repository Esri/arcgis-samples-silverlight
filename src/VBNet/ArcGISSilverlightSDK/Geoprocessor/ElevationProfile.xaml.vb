Imports System.Threading
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.DataVisualization.Charting
Imports System.Windows.Input
Imports System.Windows.Shapes
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Symbols
Imports ESRI.ArcGIS.Client.Tasks
Imports System.Collections.Generic
Imports System


Partial Public Class ElevationProfile
    Inherits UserControl
    Private _myDrawObject As Draw
    Private _lineGraphicLayer As GraphicsLayer
    Private _pointGraphicLayer As GraphicsLayer
    Private _cts As CancellationTokenSource

    Public Sub New()
        InitializeComponent()

        _lineGraphicLayer = TryCast(MyMap.Layers("LineGraphicsLayer"), GraphicsLayer)
        _lineGraphicLayer.Graphics.Add(New Graphic() With {.Geometry = Nothing})

        _pointGraphicLayer = TryCast(MyMap.Layers("PointGraphicsLayer"), GraphicsLayer)
        _pointGraphicLayer.Graphics.Add(New Graphic() With {.Geometry = New MapPoint(Double.NaN, Double.NaN)})

        _myDrawObject = New Draw(MyMap) With {.LineSymbol = TryCast((TryCast(LayoutRoot.Resources("LineRenderer"), SimpleRenderer)).Symbol, LineSymbol)}
        AddHandler _myDrawObject.DrawComplete, AddressOf _myDrawObject_DrawComplete
    End Sub

    Private Sub Tool_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ChartContainer.Visibility = System.Windows.Visibility.Collapsed
        _lineGraphicLayer.Graphics(0).Geometry = Nothing
        TryCast(_pointGraphicLayer.Graphics(0).Geometry, MapPoint).X = Double.NaN
        TryCast(_pointGraphicLayer.Graphics(0).Geometry, MapPoint).Y = Double.NaN

        Select Case TryCast((TryCast(sender, Button)).Tag, String)
            Case "DrawPolyline"
                _myDrawObject.DrawMode = DrawMode.Polyline
                _myDrawObject.IsEnabled = True
            Case "DrawFreehand"
                _myDrawObject.DrawMode = DrawMode.Freehand
                _myDrawObject.IsEnabled = True
            Case Else
                _myDrawObject.DrawMode = DrawMode.None
        End Select
    End Sub

    Private Async Sub _myDrawObject_DrawComplete(ByVal sender As Object, ByVal e As DrawEventArgs)
        If e.Geometry Is Nothing Then
            ChartContainer.Visibility = System.Windows.Visibility.Collapsed
            Return
        End If

        Try
            _myDrawObject.IsEnabled = False

            Me.Cursor = Cursors.Wait

            _lineGraphicLayer.Graphics(0).Geometry = e.Geometry

            If _cts IsNot Nothing Then
                _cts.Cancel()
            End If

            _cts = New CancellationTokenSource()

            Dim geoprocessorTask As New Geoprocessor("http://elevation.arcgis.com/arcgis/rest/services/Tools/ElevationSync/GPServer/Profile")

            Dim parameters As New List(Of GPParameter)()
            parameters.Add(New GPFeatureRecordSetLayer("InputLineFeatures", e.Geometry))
            parameters.Add(New GPString("returnM", "true"))
            parameters.Add(New GPString("returnZ", "true"))

            Dim results As GPExecuteResults = Await geoprocessorTask.ExecuteTaskAsync(parameters, _cts.Token)

            If results Is Nothing OrElse results.OutParameters.Count = 0 OrElse (TryCast(results.OutParameters(0), GPFeatureRecordSetLayer)).FeatureSet.Features.Count = 0 Then
                MessageBox.Show("Fail to get elevation data. Draw another line")
                Return
            End If

            Dim elevationLine As ESRI.ArcGIS.Client.Geometry.Polyline = TryCast((TryCast(results.OutParameters(0), GPFeatureRecordSetLayer)).FeatureSet.Features(0).Geometry, ESRI.ArcGIS.Client.Geometry.Polyline)

            For Each p As MapPoint In elevationLine.Paths(0)
                p.M = Math.Round(p.M / 1000, 2)
                p.Z = Math.Round(p.Z, 2)
            Next p

            Dim lastPoint As MapPoint = elevationLine.Paths(0)(elevationLine.Paths(0).Count - 1)

            lblDistance.Text = String.Format("Total Distance {0} Kilometers", lastPoint.M.ToString())

            TryCast(ElevationChart.Series(0), LineSeries).ItemsSource = elevationLine.Paths(0)

            ChartContainer.Visibility = System.Windows.Visibility.Visible
        Catch ex As Exception
            If TypeOf ex Is ServiceException Then
                MessageBox.Show(String.Format("{0}: {1}", (TryCast(ex, ServiceException)).Code.ToString(), (TryCast(ex, ServiceException)).Details(0)), "Error", MessageBoxButton.OK)
                Return
            End If
        Finally
            Me.Cursor = Cursors.Arrow
        End Try
    End Sub

    Private Sub ElevationChart_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
        If Not (TypeOf (CType((CType(e, System.Windows.RoutedEventArgs)).OriginalSource, System.Windows.FrameworkElement)) Is Ellipse) Then
            TryCast(_pointGraphicLayer.Graphics(0).Geometry, MapPoint).X = Double.NaN
            TryCast(_pointGraphicLayer.Graphics(0).Geometry, MapPoint).Y = Double.NaN
        Else
            Dim chartPoint As Ellipse = TryCast((CType((CType(e, System.Windows.RoutedEventArgs)).OriginalSource, System.Windows.FrameworkElement)), Ellipse)
            Dim mapPoint As MapPoint = TryCast(chartPoint.DataContext, MapPoint)

            TryCast(_pointGraphicLayer.Graphics(0).Geometry, MapPoint).X = mapPoint.X
            TryCast(_pointGraphicLayer.Graphics(0).Geometry, MapPoint).Y = mapPoint.Y
        End If
    End Sub
End Class

