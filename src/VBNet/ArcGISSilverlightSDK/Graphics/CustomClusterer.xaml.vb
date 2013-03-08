Imports Microsoft.VisualBasic
Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Tasks
Imports System.Collections.Generic


Partial Public Class CustomClusterer

  Inherits UserControl

  Public Sub New()
    InitializeComponent()
  End Sub

  Private Sub CheckBox_Checked(ByVal sender As Object, ByVal e As RoutedEventArgs)
    If MyMap Is Nothing Then
      Return
    End If
    Dim layer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
    layer.Clusterer = New SumClusterer() With
     {
       .AggregateColumn = "POP1990",
       .SymbolScale = 0.001
     }
  End Sub

  Private Sub CheckBox_Unchecked(ByVal sender As Object, ByVal e As RoutedEventArgs)
    If MyMap Is Nothing Then
      Return
    End If
    Dim layer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
    layer.Clusterer = Nothing
  End Sub
  Private Sub MyMap_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
    If (e.PropertyName = "SpatialReference") AndAlso (Not (TryCast(sender, ESRI.ArcGIS.Client.Map)).SpatialReference Is Nothing) Then
      LoadGraphics()
      RemoveHandler MyMap.PropertyChanged, AddressOf MyMap_PropertyChanged
    End If
  End Sub

  Private Sub LoadGraphics()
    Dim queryTask As QueryTask = New QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/0")
    AddHandler queryTask.ExecuteCompleted, AddressOf queryTask_ExecuteCompleted

    Dim query As Query = New ESRI.ArcGIS.Client.Tasks.Query() With
        {.ReturnGeometry = True,
         .OutSpatialReference = MyMap.SpatialReference,
         .Where = "1=1"
        }

    query.OutFields.AddRange(New String() {"CITY_NAME", "POP1990"})

    queryTask.ExecuteAsync(query)
  End Sub

  Private Sub queryTask_ExecuteCompleted(ByVal sender As Object, ByVal args As QueryEventArgs)
    Dim featureSet As FeatureSet = args.FeatureSet

    If featureSet Is Nothing OrElse featureSet.Features.Count < 1 Then
            MessageBox.Show("No features returned from query")
      Return
    End If

    Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)

    For Each graphic As Graphic In featureSet.Features
      graphicsLayer.Graphics.Add(graphic)
    Next graphic
  End Sub
End Class

Public Class SumClusterer

  Inherits GraphicsClusterer

  Public Sub New()
    MinimumColor = Colors.Red
    MaximumColor = Colors.Yellow
    SymbolScale = 1
    MyBase.Radius = 50
  End Sub

  Private privateAggregateColumn As String
  Public Property AggregateColumn() As String
    Get
      Return privateAggregateColumn
    End Get
    Set(ByVal value As String)
      privateAggregateColumn = value
    End Set
  End Property

  Private privateSymbolScale As Double
  Public Property SymbolScale() As Double
    Get
      Return privateSymbolScale
    End Get
    Set(ByVal value As Double)
      privateSymbolScale = value
    End Set
  End Property

  Private privateMinimumColor As Color
  Public Property MinimumColor() As Color
    Get
      Return privateMinimumColor
    End Get
    Set(ByVal value As Color)
      privateMinimumColor = value
    End Set
  End Property

  Private privateMaximumColor As Color
  Public Property MaximumColor() As Color
    Get
      Return privateMaximumColor
    End Get
    Set(ByVal value As Color)
      privateMaximumColor = value
    End Set
  End Property

  Protected Overrides Function OnCreateGraphic(ByVal cluster As GraphicCollection, ByVal point As MapPoint, ByVal maxClusterCount As Integer) As Graphic
    If cluster.Count = 1 Then
      Return cluster(0)
    End If
    Dim graphic As Graphic = Nothing

    Dim sum As Double = 0

    For Each g As Graphic In cluster
      If g.Attributes.ContainsKey(AggregateColumn) Then
        Try
          sum += Convert.ToDouble(g.Attributes(AggregateColumn))
        Catch
        End Try
      End If
    Next g
    Dim size As Double = (sum + 450) / 30
    size = (Math.Log(sum * SymbolScale / 10) * 10 + 20)
    If size < 12 Then
      size = 12
    End If
    graphic = New Graphic() With
              {
                  .Symbol = New ClusterSymbol() With
                            {
                                .Size = size
                            },
                  .Geometry = point
              }
    graphic.Attributes.Add("Count", sum)
    graphic.Attributes.Add("Size", size)
    graphic.Attributes.Add("Color", InterpolateColor(size - 12, 100))
    Return graphic
  End Function

  Private Shared Function InterpolateColor(ByVal value As Double, ByVal max As Double) As Brush
    value = CInt(Fix(Math.Round(value * 255.0 / max)))
    If value > 255 Then
      value = 255
    ElseIf value < 0 Then
      value = 0
    End If
    Return New SolidColorBrush(Color.FromArgb(127, 255, CByte(value), 0))
  End Function

End Class

Friend Class ClusterSymbol

  Inherits ESRI.ArcGIS.Client.Symbols.MarkerSymbol

  Public Sub New()

    Dim template As String

    template = "<ControlTemplate " +
     "xmlns=""http://schemas.microsoft.com/client/2007"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">" +
     "  <Grid IsHitTestVisible=""False"">" +
     "    <Ellipse Fill=""{Binding Attributes[Color]}"" Width=""{Binding Attributes[Size]}"" Height=""{Binding Attributes[Size]}"" />" +
     "    <Grid HorizontalAlignment=""Center"" VerticalAlignment=""Center"">" +
     "      <TextBlock Text=""{Binding Attributes[Count]}"" FontSize=""9"" Margin=""1,1,0,0"" FontWeight=""Bold"" Foreground=""#99000000"" />" +
     "      <TextBlock Text=""{Binding Attributes[Count]}"" FontSize=""9"" Margin=""0,0,1,1"" FontWeight=""Bold"" Foreground=""White"" />" +
     "    </Grid>" +
     "  </Grid>" +
     "</ControlTemplate>"

    Me.ControlTemplate = TryCast(System.Windows.Markup.XamlReader.Load(template), ControlTemplate)
  End Sub

  Private privateSize As Double
  Public Property Size() As Double
    Get
      Return privateSize
    End Get
    Set(ByVal value As Double)
      privateSize = value
    End Set
  End Property

  Public Overrides Property OffsetX() As Double
    Get
      Return Size / 2
    End Get
    Set(ByVal value As Double)
      Throw New NotSupportedException()
    End Set
  End Property

  Public Overrides Property OffsetY() As Double
    Get
      Return Size / 2
    End Get
    Set(ByVal value As Double)
      Throw New NotSupportedException()
    End Set
  End Property

End Class

