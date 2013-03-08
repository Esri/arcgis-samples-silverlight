Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks

Partial Public Class GenerateRenderer
  Inherits UserControl
  Private generateRendererTask As GenerateRendererTask

  Public Sub New()
    InitializeComponent()

    generateRendererTask = New GenerateRendererTask()
    generateRendererTask.Url = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/2"
    AddHandler generateRendererTask.ExecuteCompleted, AddressOf generateRendererTask_ExecuteCompleted
    AddHandler generateRendererTask.Failed, AddressOf generateRendererTask_Failed
  End Sub

  Private Sub GenerateRangeValueClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
    Dim classBreaksDefinition As New ClassBreaksDefinition() With
      {
        .ClassificationField = "SQMI",
        .ClassificationMethod = ClassificationMethod.StandardDeviation,
        .BreakCount = 5,
        .StandardDeviationInterval = ESRI.ArcGIS.Client.Tasks.StandardDeviationInterval.OneQuarter
      }
    classBreaksDefinition.ColorRamps.Add(New ColorRamp() With
       {
         .From = Colors.Blue,
         .To = Colors.Red,
         .Algorithm = Algorithm.HSVAlgorithm
       })

    Dim rendererParams As New GenerateRendererParameters() With
      {
        .ClassificationDefinition = classBreaksDefinition,
        .Where = "STATE_NAME NOT IN ('Alaska', 'Hawaii')"
      }

    generateRendererTask.ExecuteAsync(rendererParams, rendererParams.Where)
  End Sub


  Private Sub GenerateUniqueValueClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
    Dim uniqueValueDefinition As New UniqueValueDefinition() With
      {
        .Fields = New List(Of String)() From {"STATE_NAME"}
      }
    uniqueValueDefinition.ColorRamps.Add(New ColorRamp() With
                                        {
                                          .From = Colors.Blue,
                                          .To = Colors.Red,
                                          .Algorithm = Algorithm.CIELabAlgorithm
                                        })

    Dim rendererParams As New GenerateRendererParameters() With
      {
        .ClassificationDefinition = uniqueValueDefinition,
       .Where = "STATE_NAME NOT IN ('Alaska', 'Hawaii')"
      }

    generateRendererTask.ExecuteAsync(rendererParams, rendererParams.Where)
  End Sub

  Private Sub generateRendererTask_ExecuteCompleted(ByVal sender As Object, ByVal e As GenerateRendererResultEventArgs)
    Dim rendererResult As GenerateRendererResult = e.GenerateRendererResult

    Dim options As LayerDrawingOptionsCollection = If((TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer)).LayerDrawingOptions IsNot Nothing, (TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer)).LayerDrawingOptions, New LayerDrawingOptionsCollection())

    Dim layerDrawingOptionsParcels As LayerDrawingOptions = Nothing

    For Each drawOption As LayerDrawingOptions In options
      If drawOption.LayerID = 2 Then
        layerDrawingOptionsParcels = drawOption
        drawOption.Renderer = rendererResult.Renderer
      End If
    Next drawOption

    If e.UserState IsNot Nothing Then
      Dim layerDefinition As New LayerDefinition() With {.LayerID = 2, .Definition = TryCast(e.UserState, String)}

      TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer).LayerDefinitions = New System.Collections.ObjectModel.ObservableCollection(Of LayerDefinition)() From {layerDefinition}
    Else
      TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer).LayerDefinitions = Nothing
    End If

    If layerDrawingOptionsParcels Is Nothing Then
      options.Add(New LayerDrawingOptions() With {.LayerID = 2, .Renderer = rendererResult.Renderer})
    End If

    TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer).LayerDrawingOptions = options
    TryCast(MyMap.Layers("USA"), ArcGISDynamicMapServiceLayer).Refresh()
  End Sub

  Private Sub generateRendererTask_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
    MessageBox.Show("Error: " & e.Error.Message)
  End Sub
End Class
