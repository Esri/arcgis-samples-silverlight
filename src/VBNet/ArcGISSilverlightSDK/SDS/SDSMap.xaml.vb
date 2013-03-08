Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Windows.Threading
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Symbols
Imports ESRI.ArcGIS.Client.Tasks


Partial Public Class SDSMap
  Inherits UserControl
  Private Shared mercator As New ESRI.ArcGIS.Client.Projection.WebMercator()

  Public Sub New()
    InitializeComponent()

    Dim initialExtent As New ESRI.ArcGIS.Client.Geometry.Envelope(TryCast(mercator.FromGeographic(New ESRI.ArcGIS.Client.Geometry.MapPoint(-130, 20)), MapPoint), TryCast(mercator.FromGeographic(New ESRI.ArcGIS.Client.Geometry.MapPoint(-65, 55)), MapPoint))

    initialExtent.SpatialReference = New SpatialReference(102100)

    MyMap.Extent = initialExtent
  End Sub

  Private Sub PolygonGraphicsLayer_Initialized(ByVal sender As Object, ByVal e As EventArgs)
    LoadPolygonGraphics()
  End Sub

  Private Sub LoadPolygonGraphics()
    Dim queryTask As New QueryTask("http://servicesbeta5.esri.com/arcgis/rest/services/UnitedStates/FeatureServer/3")
    AddHandler queryTask.ExecuteCompleted, AddressOf queryTaskPolygon_ExecuteCompleted

    Dim query As Query = New ESRI.ArcGIS.Client.Tasks.Query()
    query.OutSpatialReference = MyMap.SpatialReference
    query.ReturnGeometry = True
    query.Where = "1=1"

    query.OutFields.AddRange(New String() {"STATE_NAME", "POP2000"})
    queryTask.ExecuteAsync(query)
  End Sub

  Private Sub queryTaskPolygon_ExecuteCompleted(ByVal sender As Object, ByVal args As QueryEventArgs)
    Dim featureSet As FeatureSet = args.FeatureSet

    If featureSet Is Nothing OrElse featureSet.Features.Count < 1 Then
      MessageBox.Show("No features returned from query")
      Return
    End If

    Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyPolygonGraphicsLayer"), GraphicsLayer)

    Dim random As New Random()

    ' Random switch between class breaks and unique value renderers
    If random.Next(0, 2) = 0 Then
      Dim classBreakRenderer As New ClassBreaksRenderer()
      classBreakRenderer.Field = "POP2000"
      Dim classCount As Integer = 6

      Dim valueList As New List(Of Double)()
      For Each graphic As Graphic In args.FeatureSet.Features
        graphicsLayer.Graphics.Add(graphic)
        valueList.Add(CInt(Fix(graphic.Attributes(classBreakRenderer.Field))))
      Next graphic

      ' LINQ
      Dim valueEnumerator As IEnumerable(Of Double) = From aValue In valueList
                                                      Order By aValue
                                                      Select aValue

      Dim increment As Integer = Convert.ToInt32(Math.Ceiling(args.FeatureSet.Features.Count \ classCount))
      Dim rgbFactor As Integer = 255 \ classCount
      Dim j As Integer = 255

      For i As Integer = increment To valueList.Count - 1 Step increment
        Dim classBreakInfo As New ClassBreakInfo()

        If i = increment Then
          classBreakInfo.MinimumValue = 0
        Else
          classBreakInfo.MinimumValue = valueEnumerator.ElementAt(i - increment)
        End If

        classBreakInfo.MaximumValue = valueEnumerator.ElementAt(i)

        Dim symbol As New SimpleFillSymbol() With
            {
                .Fill = New SolidColorBrush(Color.FromArgb(192, CByte(j), CByte(j), CByte(j))),
                .BorderBrush = New SolidColorBrush(Colors.Transparent),
                .BorderThickness = 1
            }

        classBreakInfo.Symbol = symbol
        classBreakRenderer.Classes.Add(classBreakInfo)

        j = j - rgbFactor
      Next i

      ' Set maximum value for largest class break 
      classBreakRenderer.Classes(classBreakRenderer.Classes.Count - 1).MaximumValue = valueEnumerator.ElementAt(valueList.Count - 1) + 1

      graphicsLayer.Renderer = classBreakRenderer

    Else
      Dim uniqueValueRenderer As New UniqueValueRenderer()
      uniqueValueRenderer.DefaultSymbol = TryCast(LayoutRoot.Resources("RedFillSymbol"), Symbol)
      uniqueValueRenderer.Field = "STATE_NAME"

      For Each graphic As Graphic In args.FeatureSet.Features
        graphicsLayer.Graphics.Add(graphic)
        Dim uniqueValueInfo As New UniqueValueInfo()

        Dim symbol As New SimpleFillSymbol() With
            {
                .Fill = New SolidColorBrush(Color.FromArgb(192,
                                                           CByte(random.Next(0, 255)),
                                                           CByte(random.Next(0, 255)),
                                                           CByte(random.Next(0, 255)))),
                .BorderBrush = New SolidColorBrush(Colors.Transparent),
                .BorderThickness = 1
            }

        uniqueValueInfo.Symbol = symbol
        uniqueValueInfo.Value = graphic.Attributes("STATE_NAME")
        uniqueValueRenderer.Infos.Add(uniqueValueInfo)
      Next graphic

      graphicsLayer.Renderer = uniqueValueRenderer
    End If
  End Sub

  Private Sub PointGraphicsLayer_Initialized(ByVal sender As Object, ByVal e As EventArgs)
    LoadPointGraphics(0, 1000)
  End Sub

  Private Sub LoadPointGraphics(ByVal minLimitRange As Integer, ByVal maxLimitRange As Integer)
    Dim queryTask As New QueryTask("http://servicesbeta5.esri.com/arcgis/rest/services/UnitedStates/FeatureServer/0")
    AddHandler queryTask.ExecuteCompleted, AddressOf queryTaskPoint_ExecuteCompleted
    AddHandler queryTask.Failed, AddressOf queryTask_Failed

    Dim query As Query = New ESRI.ArcGIS.Client.Tasks.Query()
    query.OutSpatialReference = MyMap.SpatialReference
    query.ReturnGeometry = True
    query.OutFields.AddRange(New String() {"POP2000", "AREANAME"})

    query.Where = String.Format("(OBJECTID >= {0}) AND (OBJECTID <= {1})",minLimitRange, maxLimitRange)

    queryTask.ExecuteAsync(query)
  End Sub

  Private Sub queryTaskPoint_ExecuteCompleted(ByVal sender As Object, ByVal args As QueryEventArgs)
    Dim featureSet As FeatureSet = args.FeatureSet

    If featureSet Is Nothing OrElse featureSet.Features.Count < 1 Then
      MessageBox.Show("No features returned from query")
      Return
    End If

    Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyPointGraphicsLayer"), GraphicsLayer)

    For Each graphic As Graphic In args.FeatureSet.Features
      graphic.Symbol = TryCast(LayoutRoot.Resources("YellowMarkerSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
      graphicsLayer.Graphics.Add(graphic)
    Next graphic

    If featureSet.Features.Count = 1000 Then
      Dim UpdateTimer As DispatcherTimer = New System.Windows.Threading.DispatcherTimer()
      UpdateTimer.Interval = New TimeSpan(0, 0, 0, 0, 250)
      AddHandler UpdateTimer.Tick, Sub(evtsender, a)
                                     LoadPointGraphics(graphicsLayer.Graphics.Count, graphicsLayer.Graphics.Count + 1000)
                                     UpdateTimer.Stop()
                                   End Sub
      UpdateTimer.Start()
    End If
  End Sub

  Private Sub queryTask_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
    MessageBox.Show("Query failed: " & e.Error.Message)
  End Sub

End Class

