Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Symbols



Partial Public Class DynamicLayer
  Inherits UserControl
  Public Sub New()
    InitializeComponent()
  End Sub

  Private Sub ApplyRangeValueClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
    Dim newClassBreaksRenderer As New ClassBreaksRenderer()
    newClassBreaksRenderer.Field = "POP00_SQMI"

    newClassBreaksRenderer.Classes.Add(New ClassBreakInfo() With
     {
     .MinimumValue = 0,
     .MaximumValue = 12,
     .Symbol = New SimpleFillSymbol() With
     {.Fill = New SolidColorBrush(Color.FromArgb(255, 0, 255, 0))}
    })

    newClassBreaksRenderer.Classes.Add(New ClassBreakInfo() With
     {
     .MaximumValue = 31.3,
     .Symbol = New SimpleFillSymbol() With
     {.Fill = New SolidColorBrush(Color.FromArgb(255, 100, 255, 100))}
     })

    newClassBreaksRenderer.Classes.Add(New ClassBreakInfo() With
     {
     .MaximumValue = 59.7,
     .Symbol = New SimpleFillSymbol() With
     {.Fill = New SolidColorBrush(Color.FromArgb(255, 0, 255, 200))}
     })

    newClassBreaksRenderer.Classes.Add(New ClassBreakInfo() With
     {
     .MaximumValue = 146.2,
     .Symbol = New SimpleFillSymbol() With
     {.Fill = New SolidColorBrush(Color.FromArgb(255, 0, 255, 255))}
     })

    newClassBreaksRenderer.Classes.Add(New ClassBreakInfo() With
     {
     .MaximumValue = 57173,
     .Symbol = New SimpleFillSymbol() With
     {.Fill = New SolidColorBrush(Color.FromArgb(255, 0, 0, 255))}
     })

    Dim layerDrawOptions As New LayerDrawingOptions()
    layerDrawOptions.LayerID = 3
    layerDrawOptions.Renderer = newClassBreaksRenderer

    TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer).LayerDrawingOptions = New LayerDrawingOptionsCollection() From {layerDrawOptions}
    TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer).VisibleLayers = New Integer() {3}
    ' Changing VisibleLayers will refresh the layer, otherwise an explicit call to Refresh is needed.
    '(MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).Refresh();
  End Sub

  Private Sub ApplyUniqueValueClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
    Dim newUniqueValueRenderer As New UniqueValueRenderer() With
     {
     .DefaultSymbol = New SimpleFillSymbol() With {.Fill = New SolidColorBrush(Colors.Gray)},
     .Field = "SUB_REGION"
     }

    newUniqueValueRenderer.Infos.Add(New UniqueValueInfo() With
    {
     .Value = "Pacific",
     .Symbol = New SimpleFillSymbol() With
     {
     .Fill = New SolidColorBrush(Colors.Purple),
     .BorderBrush = New SolidColorBrush(Colors.Transparent)
     }
     })

    newUniqueValueRenderer.Infos.Add(New UniqueValueInfo() With
    {
     .Value = "W N Cen",
     .Symbol = New SimpleFillSymbol() With
     {
     .Fill = New SolidColorBrush(Colors.Green),
     .BorderBrush = New SolidColorBrush(Colors.Transparent)
     }
    })

    newUniqueValueRenderer.Infos.Add(New UniqueValueInfo() With
    {
     .Value = "W S Cen",
     .Symbol = New SimpleFillSymbol() With
     {
     .Fill = New SolidColorBrush(Colors.Cyan),
     .BorderBrush = New SolidColorBrush(Colors.Transparent)
     }
    })

    newUniqueValueRenderer.Infos.Add(New UniqueValueInfo() With
    {
     .Value = "E N Cen",
     .Symbol = New SimpleFillSymbol() With
     {
     .Fill = New SolidColorBrush(Colors.Yellow),
     .BorderBrush = New SolidColorBrush(Colors.Transparent)
     }
    })

    newUniqueValueRenderer.Infos.Add(New UniqueValueInfo() With
    {
     .Value = "Mtn",
     .Symbol = New SimpleFillSymbol() With
     {
     .Fill = New SolidColorBrush(Colors.Blue),
     .BorderBrush = New SolidColorBrush(Colors.Transparent)
     }
    })

    newUniqueValueRenderer.Infos.Add(New UniqueValueInfo() With
    {
     .Value = "N Eng",
     .Symbol = New SimpleFillSymbol() With
     {
     .Fill = New SolidColorBrush(Colors.Red),
     .BorderBrush = New SolidColorBrush(Colors.Transparent)
     }
    })

    newUniqueValueRenderer.Infos.Add(New UniqueValueInfo() With
    {
     .Value = "E S Cen",
     .Symbol = New SimpleFillSymbol() With
     {
     .Fill = New SolidColorBrush(Colors.Brown),
     .BorderBrush = New SolidColorBrush(Colors.Transparent)
     }
    })

    newUniqueValueRenderer.Infos.Add(New UniqueValueInfo() With
    {
    .Value = "Mid Atl",
    .Symbol = New SimpleFillSymbol() With
     {
     .Fill = New SolidColorBrush(Colors.Magenta),
     .BorderBrush = New SolidColorBrush(Colors.Transparent)
     }
    })

    newUniqueValueRenderer.Infos.Add(New UniqueValueInfo() With
    {
     .Value = "S Atl",
     .Symbol = New SimpleFillSymbol() With
     {
     .Fill = New SolidColorBrush(Colors.Orange),
     .BorderBrush = New SolidColorBrush(Colors.Transparent)
     }
    })

    Dim layerDrawOptions As New LayerDrawingOptions()
    layerDrawOptions.LayerID = 2
    layerDrawOptions.Renderer = newUniqueValueRenderer

    TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer).LayerDrawingOptions = New LayerDrawingOptionsCollection() From {layerDrawOptions}
    TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer).VisibleLayers = New Integer() {2}
    ' Changing VisibleLayers will refresh the layer, otherwise an explicit call to Refresh is needed.
    '(MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).Refresh();

  End Sub

  Private Sub ChangeLayerOrderClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
    TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer).LayerDrawingOptions = Nothing

    Dim myDynamicLayerInfos As DynamicLayerInfoCollection = (TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer)).DynamicLayerInfos
    If myDynamicLayerInfos Is Nothing Then
      myDynamicLayerInfos = (TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer)).CreateDynamicLayerInfosFromLayerInfos()
    End If

    Dim aDynamicLayerInfo = myDynamicLayerInfos(0)
    myDynamicLayerInfos.RemoveAt(0)
    myDynamicLayerInfos.Add(aDynamicLayerInfo)

    TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer).DynamicLayerInfos = myDynamicLayerInfos
    TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer).VisibleLayers = Nothing
    ' Changing VisibleLayers will refresh the layer, otherwise an explicit call to Refresh is needed.
    '(MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).Refresh();
  End Sub

  Private Sub AddLayerClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
    TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer).LayerDrawingOptions = Nothing

    Dim myDynamicLayerInfos As DynamicLayerInfoCollection = (TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer)).DynamicLayerInfos
    If myDynamicLayerInfos Is Nothing Then
      myDynamicLayerInfos = (TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer)).CreateDynamicLayerInfosFromLayerInfos()
    End If

    Dim dli As New DynamicLayerInfo() With
     {
     .ID = 4,
     .Source = New LayerDataSource() With
     {
     .DataSource = New TableDataSource() With
     {
     .WorkspaceID = "MyDatabaseWorkspaceIDSSR2",
     .DataSourceName = "ss6.gdb.Lakes"
     }
     }
    }

    Dim layerDrawOptions As New LayerDrawingOptions()
    layerDrawOptions.LayerID = 4
    layerDrawOptions.Renderer = New SimpleRenderer() With
        {
        .Symbol = New SimpleFillSymbol() With
         {
          .Fill = New SolidColorBrush(Color.FromArgb(CInt(255), CInt(0), CInt(0), CInt(255)))
         }
        }

    TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer).LayerDrawingOptions = New LayerDrawingOptionsCollection() From {layerDrawOptions}

    myDynamicLayerInfos.Insert(0, dli)
    TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer).DynamicLayerInfos = myDynamicLayerInfos
    TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer).VisibleLayers = New Integer() {3, 4}
    ' Changing VisibleLayers will refresh the layer, otherwise an explicit call to Refresh is needed.
    '(MyMap.Layers["USA"] as ArcGISDynamicMapServiceLayer).Refresh();
  End Sub
End Class

