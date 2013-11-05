Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Symbols
Imports System.Windows

Partial Public Class DrawGraphics
    Inherits UserControl
    Private MyDrawObject As Draw
    Private _activeSymbol As Symbol = Nothing
    Private graphicsLayer As GraphicsLayer
    Private activeGraphic As Graphic
    Private editor As Editor

    Public Sub New()
        InitializeComponent()

        graphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)

        MyDrawObject = New Draw(MyMap) With {.LineSymbol = TryCast(LayoutRoot.Resources("DrawLineSymbol"), LineSymbol), .FillSymbol = TryCast(LayoutRoot.Resources("DrawFillSymbol"), FillSymbol)}
        AddHandler MyDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete
        editor = TryCast(LayoutRoot.Resources("MyEditor"), Editor)

    End Sub

    Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As ESRI.ArcGIS.Client.DrawEventArgs)
        Dim graphic As New ESRI.ArcGIS.Client.Graphic() With {.Geometry = args.Geometry, .Symbol = _activeSymbol}
        graphicsLayer.Graphics.Add(graphic)
    End Sub

    Private Sub GraphicsLayer_MouseLeftButtonUp(ByVal sender As Object, ByVal e As GraphicMouseButtonEventArgs)
        If EnableEditVerticesScaleRotate.IsChecked.Value Then
            If e.Graphic IsNot Nothing AndAlso Not (TypeOf e.Graphic.Geometry Is ESRI.ArcGIS.Client.Geometry.MapPoint) Then
                MyDrawObject.DrawMode = DrawMode.None
                UnSelectTools()
                editor.EditVertices.Execute(e.Graphic)
                activeGraphic = e.Graphic
            End If
        End If
    End Sub
    Private Sub UnSelectTools()
        For Each element As UIElement In MyStackPanel.Children
            If TypeOf element Is Button Then
                VisualStateManager.GoToState((TryCast(element, Button)), "UnSelected", False)
            End If
        Next element
    End Sub
    Private Sub Tool_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        UnSelectTools()
        For Each element As UIElement In MyStackPanel.Children
            If TypeOf element Is Button Then
                VisualStateManager.GoToState((TryCast(element, Button)), "UnSelected", False)
            End If
        Next element

        VisualStateManager.GoToState(TryCast(sender, Button), "Selected", False)

        If activeGraphic IsNot Nothing Then
            If (editor.CancelActive.CanExecute(Nothing)) Then
                editor.CancelActive.Execute(Nothing)
            End If
            activeGraphic = Nothing
        End If

        Select Case TryCast((TryCast(sender, Button)).Tag, String)
            Case "DrawPoint"
                MyDrawObject.DrawMode = DrawMode.Point
                _activeSymbol = TryCast(LayoutRoot.Resources("DefaultMarkerSymbol"), Symbol)
            Case "DrawPolyline"
                MyDrawObject.DrawMode = DrawMode.Polyline
                _activeSymbol = TryCast(LayoutRoot.Resources("DefaultLineSymbol"), Symbol)
            Case "DrawlineSegment"
                MyDrawObject.DrawMode = DrawMode.LineSegment
                _activeSymbol = TryCast(LayoutRoot.Resources("DefaultLineSymbol"), Symbol)
            Case "DrawPolygon"
                MyDrawObject.DrawMode = DrawMode.Polygon
                _activeSymbol = TryCast(LayoutRoot.Resources("DefaultFillSymbol"), Symbol)
            Case "DrawRectangle"
                MyDrawObject.DrawMode = DrawMode.Rectangle
                _activeSymbol = TryCast(LayoutRoot.Resources("DefaultFillSymbol"), Symbol)
            Case "DrawFreehand"
                MyDrawObject.DrawMode = DrawMode.Freehand
                _activeSymbol = TryCast(LayoutRoot.Resources("DefaultLineSymbol"), Symbol)
            Case "DrawArrow"
                MyDrawObject.DrawMode = DrawMode.Arrow
                _activeSymbol = TryCast(LayoutRoot.Resources("DefaultFillSymbol"), Symbol)
            Case "DrawTriangle"
                MyDrawObject.DrawMode = DrawMode.Triangle
                _activeSymbol = TryCast(LayoutRoot.Resources("DefaultFillSymbol"), Symbol)
            Case "DrawCircle"
                MyDrawObject.DrawMode = DrawMode.Circle
                _activeSymbol = TryCast(LayoutRoot.Resources("DefaultFillSymbol"), Symbol)
            Case "DrawEllipse"
                MyDrawObject.DrawMode = DrawMode.Ellipse
                _activeSymbol = TryCast(LayoutRoot.Resources("DefaultFillSymbol"), Symbol)
            Case Else
                MyDrawObject.DrawMode = DrawMode.None
                graphicsLayer.Graphics.Clear()
        End Select
        MyDrawObject.IsEnabled = (MyDrawObject.DrawMode <> DrawMode.None)
    End Sub
End Class
