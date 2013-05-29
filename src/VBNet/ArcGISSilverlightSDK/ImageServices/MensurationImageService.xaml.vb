Imports Microsoft.VisualBasic
Imports System
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Symbols
Imports ESRI.ArcGIS.Client.Tasks

Partial Public Class MensurationImageService
  Inherits UserControl
  Private draw As Draw
  Private drawGraphicsLayer As GraphicsLayer
  Private mensurationTask As MensurationTask
  Private mensurationOper As MensurationOperation
  Private clickCount As Integer = -1

  Public Sub New()
    InitializeComponent()

    draw = New Draw(MyMap)
    AddHandler draw.DrawComplete, AddressOf drawtool_DrawComplete
    AddHandler draw.VertexAdded, AddressOf drawtool_VertexAdded
    drawGraphicsLayer = TryCast(MyMap.Layers("DrawGraphicsLayer"), GraphicsLayer)

    mensurationTask = New MensurationTask((TryCast(MyMap.Layers("TiledImageServiceLayer"), ArcGISTiledMapServiceLayer)).Url)

    AddHandler mensurationTask.AreaAndPerimeterCompleted, AddressOf mt_TaskCompleted
    AddHandler mensurationTask.CentroidCompleted, AddressOf mt_TaskCompleted
    AddHandler mensurationTask.DistanceAndAngleCompleted, AddressOf mt_TaskCompleted
    AddHandler mensurationTask.HeightFromBaseAndTopCompleted, AddressOf mt_TaskCompleted
    AddHandler mensurationTask.HeightFromBaseAndTopShadowCompleted, AddressOf mt_TaskCompleted
    AddHandler mensurationTask.HeightFromTopAndTopShadowCompleted, AddressOf mt_TaskCompleted
    AddHandler mensurationTask.PointCompleted, AddressOf mt_TaskCompleted
    AddHandler mensurationTask.Failed, AddressOf mt_TaskFailed
  End Sub

  Private Sub mt_TaskCompleted(ByVal sender As Object, ByVal e As TaskEventArgs)
    Dim sb As New StringBuilder()

    If TypeOf e Is MensurationPointEventArgs Then
      Dim result As MensurationPointResult = (TryCast(e, MensurationPointEventArgs)).Result

      If result IsNot Nothing AndAlso result.Point IsNot Nothing Then
        sb.Append(result.Point)
        sb.Append(vbLf)
      End If
    ElseIf TypeOf e Is MensurationHeightEventArgs Then
      Dim result As MensurationHeightResult = (TryCast(e, MensurationHeightEventArgs)).Result

      If result IsNot Nothing Then
        If result.Height IsNot Nothing Then
          sb.Append("Height" & vbLf)
          sb.AppendFormat("Value:" & vbTab & vbTab & "{0}" & vbLf, result.Height.Value)
          sb.AppendFormat("Display Value:" & vbTab & "{0}" & vbLf, result.Height.DisplayValue)
          sb.AppendFormat("Uncertainty:" & vbTab & "{0}" & vbLf, result.Height.Uncertainty)
          sb.AppendFormat("Unit:" & vbTab & vbTab & "{0}" & vbLf, result.Height.LinearUnit)
          sb.Append(vbLf)
        End If
      End If
    ElseIf TypeOf e Is MensurationLengthEventArgs Then
      Dim result As MensurationLengthResult = (TryCast(e, MensurationLengthEventArgs)).Result

      If result IsNot Nothing Then
        If result.Distance IsNot Nothing Then
          sb.Append("Distance" & vbLf)
          sb.AppendFormat("Value:" & vbTab & vbTab & "{0}" & vbLf, result.Distance.Value)
          sb.AppendFormat("Display Value:" & vbTab & "{0}" & vbLf, result.Distance.DisplayValue)
          sb.AppendFormat("Uncertainty:" & vbTab & "{0}" & vbLf, result.Distance.Uncertainty)
          sb.AppendFormat("Unit:" & vbTab & vbTab & "{0}" & vbLf, result.Distance.LinearUnit)
          sb.Append(vbLf)
        End If
        If result.AzimuthAngle IsNot Nothing Then
          sb.Append("Azimuth Angle" & vbLf)
          sb.AppendFormat("Value:" & vbTab & vbTab & "{0}" & vbLf, result.AzimuthAngle.Value)
          sb.AppendFormat("Display Value:" & vbTab & "{0}" & vbLf, result.AzimuthAngle.DisplayValue)
          sb.AppendFormat("Uncertainty:" & vbTab & "{0}" & vbLf, result.AzimuthAngle.Uncertainty)
          sb.AppendFormat("Unit:" & vbTab & vbTab & "{0}" & vbLf, result.AzimuthAngle.AngularUnit)
          sb.Append(vbLf)
        End If
        If result.ElevationAngle IsNot Nothing Then
          sb.Append("Elevation Angle" & vbLf)
          sb.AppendFormat("Value:" & vbTab & vbTab & "{0}" & vbLf, result.ElevationAngle.Value)
          sb.AppendFormat("Display Value:" & vbTab & "{0}" & vbLf, result.ElevationAngle.DisplayValue)
          sb.AppendFormat("Uncertainty:" & vbTab & "{0}" & vbLf, result.ElevationAngle.Uncertainty)
          sb.AppendFormat("Unit:" & vbTab & vbTab & "{0}" & vbLf, result.ElevationAngle.AngularUnit)
          sb.Append(vbLf)
        End If
      End If
    ElseIf TypeOf e Is MensurationAreaEventArgs Then
      Dim result As MensurationAreaResult = (TryCast(e, MensurationAreaEventArgs)).Result

      If result IsNot Nothing Then
        If result.Area IsNot Nothing Then
          sb.Append("Area" & vbLf)
          sb.AppendFormat("Value:" & vbTab & vbTab & "{0}" & vbLf, result.Area.Value)
          sb.AppendFormat("Display Value:" & vbTab & "{0}" & vbLf, result.Area.DisplayValue)
          sb.AppendFormat("Uncertainty:" & vbTab & "{0}" & vbLf, result.Area.Uncertainty)
          sb.AppendFormat("Unit:" & vbTab & vbTab & "{0}" & vbLf, result.Area.AreaUnit)
          sb.Append(vbLf)
        End If
        If result.Perimeter IsNot Nothing Then
          sb.Append("Perimeter" & vbLf)
          sb.AppendFormat("Value:" & vbTab & vbTab & "{0}" & vbLf, result.Perimeter.Value)
          sb.AppendFormat("Display Value:" & vbTab & "{0}" & vbLf, result.Perimeter.DisplayValue)
          sb.AppendFormat("Uncertainty:" & vbTab & "{0}" & vbLf, result.Perimeter.Uncertainty)
          sb.AppendFormat("Unit:" & vbTab & vbTab & "{0}" & vbLf, result.Perimeter.LinearUnit)
          sb.Append(vbLf)
        End If
      End If
    End If

    MessageBox.Show(sb.ToString())
  End Sub

  Private Sub mt_TaskFailed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
    MessageBox.Show(String.Format(e.Error.Message))
  End Sub

  Private Sub drawtool_VertexAdded(ByVal sender As Object, ByVal e As VertexAddedEventArgs)
    If clickCount > 0 Then
      clickCount -= 1
      If clickCount = 0 Then
        draw.CompleteDraw()
        clickCount = -1
      End If
    End If
  End Sub

  Private Sub ClearMeasureToolClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
    drawGraphicsLayer.Graphics.Clear()
    draw.IsEnabled = False
  End Sub

  Private Sub drawtool_DrawComplete(ByVal sender As Object, ByVal e As DrawEventArgs)
    e.Geometry.SpatialReference = MyMap.SpatialReference
    Dim graphic As New Graphic() With {.Geometry = e.Geometry}

    If TypeOf e.Geometry Is MapPoint Then
      graphic.Symbol = TryCast(LayoutRoot.Resources("DrawPointSymbol"), SimpleMarkerSymbol)
    ElseIf TypeOf e.Geometry Is Polyline Then
      graphic.Symbol = TryCast(LayoutRoot.Resources("DrawPolylineSymbol"), SimpleLineSymbol)
    ElseIf TypeOf e.Geometry Is Polygon OrElse TypeOf e.Geometry Is Envelope Then
      graphic.Symbol = TryCast(LayoutRoot.Resources("DrawPolygonSymbol"), SimpleFillSymbol)
    End If

    drawGraphicsLayer.Graphics.Add(graphic)
    draw.IsEnabled = False

    Dim fromGeometry As Geometry = Nothing
    Dim toGeometry As Geometry = Nothing
    Select Case mensurationOper
      Case mensurationOper.DistanceAndAngle, mensurationOper.HeightFromBaseAndTop, mensurationOper.HeightFromBaseAndTopShadow, mensurationOper.HeightFromTopAndTopShadow
        fromGeometry = (TryCast(e.Geometry, Polyline)).Paths(0)(0)
        fromGeometry.SpatialReference = MyMap.SpatialReference
        toGeometry = (TryCast(e.Geometry, Polyline)).Paths(0)(1)
        toGeometry.SpatialReference = MyMap.SpatialReference
      Case mensurationOper.AreaAndPerimeter, mensurationOper.Centroid, mensurationOper.Point
        fromGeometry = e.Geometry
    End Select

    Dim LinearUnit? As esriUnits = Nothing
    Select Case comboLinearUnit.SelectedIndex
      Case 0
        LinearUnit = esriUnits.esriUnknownUnits
      Case 1
        LinearUnit = esriUnits.esriInches
      Case 2
        LinearUnit = esriUnits.esriPoints
      Case 3
        LinearUnit = esriUnits.esriFeet
      Case 4
        LinearUnit = esriUnits.esriYards
      Case 5
        LinearUnit = esriUnits.esriMiles
      Case 6
        LinearUnit = esriUnits.esriNauticalMiles
      Case 7
        LinearUnit = esriUnits.esriMillimeters
      Case 8
        LinearUnit = esriUnits.esriCentimeters
      Case 9
        LinearUnit = esriUnits.esriMeters
      Case 10
        LinearUnit = esriUnits.esriKilometers
      Case 11
        LinearUnit = esriUnits.esriDecimalDegrees
      Case 12
        LinearUnit = esriUnits.esriDecimeters
    End Select

    Dim AngularUnit As DirectionUnit = DirectionUnit.Default
    Select Case comboAngularUnit.SelectedIndex
      Case 0
        AngularUnit = DirectionUnit.Default
      Case 1
        AngularUnit = DirectionUnit.Radians
      Case 2
        AngularUnit = DirectionUnit.DecimalDegrees
      Case 3
        AngularUnit = DirectionUnit.DegreesMinutesSeconds
      Case 4
        AngularUnit = DirectionUnit.Gradians
      Case 5
        AngularUnit = DirectionUnit.Gons
    End Select

    Dim AreaUnits As AreaUnit = AreaUnit.Default
    Select Case comboAreaUnit.SelectedIndex
      Case 0
        AreaUnits = AreaUnit.Default
      Case 1
        AreaUnits = AreaUnit.SquareInches
      Case 2
        AreaUnits = AreaUnit.SquareFeet
      Case 3
        AreaUnits = AreaUnit.SquareYards
      Case 4
        AreaUnits = AreaUnit.Acres
      Case 5
        AreaUnits = AreaUnit.SquareMiles
      Case 6
        AreaUnits = AreaUnit.SquareMillimeters
      Case 7
        AreaUnits = AreaUnit.SquareCentimeters
      Case 8
        AreaUnits = AreaUnit.SquareDecimeters
      Case 9
        AreaUnits = AreaUnit.SquareMeters
      Case 10
        AreaUnits = AreaUnit.Ares
      Case 11
        AreaUnits = AreaUnit.Hectares
      Case 12
        AreaUnits = AreaUnit.SquareKilometers
    End Select

    If Not mensurationTask.IsBusy Then
      Select Case mensurationOper
        Case mensurationOper.AreaAndPerimeter
          Dim p1 As New MensurationAreaParameter()
          p1.LinearUnit = LinearUnit
          p1.AreaUnits = AreaUnits
          mensurationTask.AreaAndPerimeterAsync(TryCast(fromGeometry, Polygon), p1)
        Case mensurationOper.Centroid
          Dim p3 As New MensurationPointParameter()
          mensurationTask.CentroidAsync(TryCast(fromGeometry, Polygon), p3)
        Case mensurationOper.DistanceAndAngle
          Dim p5 As New MensurationLengthParameter()
          p5.LinearUnit = LinearUnit
          p5.AngularUnit = AngularUnit
          mensurationTask.DistanceAndAngleAsync(TryCast(fromGeometry, MapPoint), TryCast(toGeometry, MapPoint), p5)
        Case mensurationOper.Point
          Dim p7 As New MensurationPointParameter()
          mensurationTask.PointAsync(TryCast(fromGeometry, MapPoint), p7)
        Case mensurationOper.HeightFromBaseAndTop
          Dim p9 As New MensurationHeightParameter()
          p9.LinearUnit = LinearUnit
          mensurationTask.HeightFromBaseAndTopAsync(TryCast(fromGeometry, MapPoint), TryCast(toGeometry, MapPoint), p9)
        Case mensurationOper.HeightFromBaseAndTopShadow
          Dim p10 As New MensurationHeightParameter()
          p10.LinearUnit = LinearUnit
          mensurationTask.HeightFromBaseAndTopShadowAsync(TryCast(fromGeometry, MapPoint), TryCast(toGeometry, MapPoint), p10)
        Case mensurationOper.HeightFromTopAndTopShadow
          Dim p11 As New MensurationHeightParameter()
          p11.LinearUnit = LinearUnit
          mensurationTask.HeightFromTopAndTopShadowAsync(TryCast(fromGeometry, MapPoint), TryCast(toGeometry, MapPoint), p11)
      End Select
    End If
  End Sub

  Private Sub ActivateMeasureToolClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
    drawGraphicsLayer.Graphics.Clear()
    draw.IsEnabled = True

    Dim btn As Button = TryCast(sender, Button)
    Select Case CStr(btn.Tag)
      Case "AaP"
        draw.DrawMode = DrawMode.Polygon
        mensurationOper = mensurationOper.AreaAndPerimeter
      Case "Cen"
        draw.DrawMode = DrawMode.Polygon
        mensurationOper = mensurationOper.Centroid
      Case "DaA"
        draw.DrawMode = DrawMode.Polyline
        clickCount = 2
        mensurationOper = mensurationOper.DistanceAndAngle
      Case "HFBaT"
        draw.DrawMode = DrawMode.Polyline
        clickCount = 2
        mensurationOper = mensurationOper.HeightFromBaseAndTop
      Case "HFBaTS"
        draw.DrawMode = DrawMode.Polyline
        clickCount = 2
        mensurationOper = mensurationOper.HeightFromBaseAndTopShadow
      Case "HFTaTS"
        draw.DrawMode = DrawMode.Polyline
        clickCount = 2
        mensurationOper = mensurationOper.HeightFromTopAndTopShadow
      Case "Pnt"
        draw.DrawMode = DrawMode.Point
        mensurationOper = mensurationOper.Point
    End Select
  End Sub

  Friend Enum MensurationOperation
    AreaAndPerimeter
    Centroid
    DistanceAndAngle
    HeightFromBaseAndTop
    HeightFromBaseAndTopShadow
    HeightFromTopAndTopShadow
    Point
  End Enum
End Class
