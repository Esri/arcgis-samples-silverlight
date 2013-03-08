Imports Microsoft.VisualBasic
Imports System
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Symbols

Partial Public Class GeometryJson
    Inherits UserControl
    Private _myToJsonGraphicsLayer As GraphicsLayer
    Private _myFromJsonGraphicsLayer As GraphicsLayer
    Private _myDrawObject As Draw

    Private jsonPoint As String = "{""x"":-100.609,""y"":43.729,""spatialReference"":{""wkid"":4326}}"

    Private jsonPolyline As String = "{""paths"":[[[0,51.399]," &
        ControlChars.CrLf & "[2.637,48.865]," &
        ControlChars.CrLf & "[12.568,41.706]," &
        ControlChars.CrLf & "[13.447,52.483]," &
        ControlChars.CrLf & "[21.357,52.160]," &
        ControlChars.CrLf & "[30.322,59.845]]]," &
        ControlChars.CrLf & """spatialReference"":{""wkid"":4326}}"

    Private jsonPolygon As String = "{""rings"":[[[110.039,-20.303]," &
        ControlChars.CrLf & "[132.539,-7.0137]," &
        ControlChars.CrLf & "[153.281,-13.923]," &
        ControlChars.CrLf & "[162.773,-35.174]," &
        ControlChars.CrLf & "[133.594,-43.180]," &
        ControlChars.CrLf & "[111.797,-36.032]," &
        ControlChars.CrLf & "[110.039,-20.303]]]," &
        ControlChars.CrLf & """spatialReference"":{""wkid"":4326}}"

    Private jsonEnvelope As String = "{""xmin"" : -109.55, ""ymin"" : 25.76, " &
        ControlChars.CrLf & """xmax"" : -86.39, ""ymax"" : 49.94," &
        ControlChars.CrLf & """spatialReference"" : {""wkid"" : 4326}}"

    Public Sub New()
        InitializeComponent()

        _myFromJsonGraphicsLayer = TryCast(MyMap.Layers("MyFromJsonGraphicsLayer"), GraphicsLayer)
        _myToJsonGraphicsLayer = TryCast(MyMap.Layers("MyToJsonGraphicsLayer"), GraphicsLayer)
        InJsonTextBox.Text = jsonPoint

        _myDrawObject = New Draw(MyMap) With {.LineSymbol = TryCast(LayoutRoot.Resources("DrawLineSymbol"), LineSymbol), .FillSymbol = TryCast(LayoutRoot.Resources("DrawFillSymbol"), FillSymbol)}
        AddHandler _myDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete
    End Sub

    Private Sub ResetButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearGraphicsLayers()
        InJsonTextBox.Text = ""
        OutJsonTextBox.Text = ""
    End Sub

    Private Sub PointJsonButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearGraphicsLayers()
        InJsonTextBox.Text = jsonPoint
    End Sub

    Private Sub PolylineJsonButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearGraphicsLayers()
        InJsonTextBox.Text = jsonPolyline
    End Sub

    Private Sub PolygonJsonButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearGraphicsLayers()
        InJsonTextBox.Text = jsonPolygon
    End Sub

    Private Sub EnvelopeJsonButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearGraphicsLayers()
        InJsonTextBox.Text = jsonEnvelope
    End Sub

    Private Sub ClearGraphicsLayers()
        _myToJsonGraphicsLayer.Graphics.Clear()
        _myFromJsonGraphicsLayer.Graphics.Clear()
    End Sub

    Private Sub ApplyButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearGraphicsLayers()
        OutJsonTextBox.Text = ""

        Try
            ' Convert from Geometry to ArcGIS REST geometry json
            Dim geometry As Geometry = geometry.FromJson(InJsonTextBox.Text)

            Dim graphic As New Graphic()

            If TypeOf geometry Is MapPoint Then
                graphic.Symbol = TryCast(LayoutRoot.Resources("RedMarkerSymbol"), SimpleMarkerSymbol)
            ElseIf TypeOf geometry Is Polyline Then
                graphic.Symbol = TryCast(LayoutRoot.Resources("RedLineSymbol"), SimpleLineSymbol)
            ElseIf TypeOf geometry Is Polygon Then
                graphic.Symbol = TryCast(LayoutRoot.Resources("RedFillSymbol"), SimpleFillSymbol)
            ElseIf TypeOf geometry Is Envelope Then
                graphic.Symbol = TryCast(LayoutRoot.Resources("RedFillSymbol"), SimpleFillSymbol)
            End If

            If graphic.Symbol IsNot Nothing Then
                graphic.Geometry = geometry
                _myFromJsonGraphicsLayer.Graphics.Add(graphic)
            End If
        Catch
            MessageBox.Show("Unable to convert json into geometry")
        End Try
    End Sub

    Private Sub DrawGeometryButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ClearGraphicsLayers()
        OutJsonTextBox.Text = ""

        Select Case TryCast((TryCast(sender, Button)).Tag, String)
            Case "DrawPoint"
                _myDrawObject.DrawMode = DrawMode.Point
            Case "DrawPolyline"
                _myDrawObject.DrawMode = DrawMode.Polyline
            Case "DrawPolygon"
                _myDrawObject.DrawMode = DrawMode.Polygon
            Case "DrawRectangle"
                _myDrawObject.DrawMode = DrawMode.Rectangle
            Case Else
                _myDrawObject.DrawMode = DrawMode.None
                ClearGraphicsLayers()
        End Select
        _myDrawObject.IsEnabled = (_myDrawObject.DrawMode <> DrawMode.None)
    End Sub

    Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As ESRI.ArcGIS.Client.DrawEventArgs)
        ClearGraphicsLayers()

        Dim graphic As New Graphic()
        If TypeOf args.Geometry Is MapPoint Then
            graphic.Symbol = TryCast(LayoutRoot.Resources("BlueMarkerSymbol"), SimpleMarkerSymbol)
        ElseIf TypeOf args.Geometry Is Polyline Then
            graphic.Symbol = TryCast(LayoutRoot.Resources("BlueLineSymbol"), SimpleLineSymbol)
        ElseIf TypeOf args.Geometry Is Polygon Then
            graphic.Symbol = TryCast(LayoutRoot.Resources("BlueFillSymbol"), SimpleFillSymbol)
        ElseIf TypeOf args.Geometry Is Envelope Then
            graphic.Symbol = TryCast(LayoutRoot.Resources("BlueFillSymbol"), SimpleFillSymbol)
        End If

        If graphic.Symbol IsNot Nothing Then
            graphic.Geometry = args.Geometry
            _myToJsonGraphicsLayer.Graphics.Add(graphic)
        End If

        ' Convert from Geometry to ArcGIS REST geometry json
        OutJsonTextBox.Text = args.Geometry.ToJson()
    End Sub

    Private Sub TabControl_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
        If _myFromJsonGraphicsLayer IsNot Nothing OrElse _myToJsonGraphicsLayer IsNot Nothing Then
            ClearGraphicsLayers()
            OutJsonTextBox.Text = ""
        Else
            Return
        End If

        If (TryCast((TryCast((TryCast(sender, TabControl)).SelectedItem, TabItem)).Header, String)) = "From JSON" Then
            _myDrawObject.IsEnabled = False
        End If
    End Sub
End Class