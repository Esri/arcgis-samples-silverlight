Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Tasks
Imports ESRI.ArcGIS.Client.Symbols
Imports ESRI.ArcGIS.Client.Toolkit
Imports ESRI.ArcGIS.Client.Toolkit.Primitives

Partial Public Class ViewShed
  Inherits UserControl
  Private _geoprocessorTask As Geoprocessor
  Private _displayViewshedInfo As Boolean
  Private resultLayer As ArcGISDynamicMapServiceLayer = Nothing
  Private graphicsLayer As GraphicsLayer = Nothing

  Public Sub New()
    InitializeComponent()
    _geoprocessorTask = New Geoprocessor("http://serverapps101.esri.com/arcgis/rest/services/ProbabilisticViewshedModel/GPServer/ProbabilisticViewshedModel")
    AddHandler _geoprocessorTask.JobCompleted, AddressOf GeoprocessorTask_JobCompleted
    AddHandler _geoprocessorTask.Failed, AddressOf GeoprocessorTask_Failed
    graphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
  End Sub

  Private Sub MyMap_MouseClick(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Map.MouseEventArgs)
    Dim mapPoint As MapPoint = e.MapPoint
    If _displayViewshedInfo AndAlso resultLayer IsNot Nothing Then

      Dim identifyParams As ESRI.ArcGIS.Client.Tasks.IdentifyParameters = New IdentifyParameters() With {.Geometry = mapPoint, .MapExtent = MyMap.Extent, .Width = CInt(Fix(MyMap.ActualWidth)), .Height = CInt(Fix(MyMap.ActualHeight)), .LayerOption = LayerOption.visible, .SpatialReference = MyMap.SpatialReference}

      Dim identifyTask As New IdentifyTask(resultLayer.Url)
      AddHandler identifyTask.ExecuteCompleted, Sub(s, ie)
                                                  If ie.IdentifyResults.Count > 0 Then
                                                    For Each identifyresult In ie.IdentifyResults
                                                      If identifyresult.LayerId = 1 Then
                                                        Dim g As Graphic = identifyresult.Feature
                                                        MyInfoWindow.Anchor = e.MapPoint
                                                        MyInfoWindow.Content = g.Attributes
                                                        MyInfoWindow.IsOpen = True
                                                        Exit For
                                                      End If
                                                    Next identifyresult
                                                  End If
                                                End Sub
      identifyTask.ExecuteAsync(identifyParams)
    Else
      _geoprocessorTask.CancelAsync()

      graphicsLayer.ClearGraphics()

      mapPoint.SpatialReference = New SpatialReference(102100)

      Dim graphic As New Graphic() With {.Symbol = TryCast(LayoutRoot.Resources("StartMarkerSymbol"), Symbol), .Geometry = mapPoint}
      graphicsLayer.Graphics.Add(graphic)

      MyMap.Cursor = System.Windows.Input.Cursors.Wait

      Dim parameters As New List(Of GPParameter)()
      parameters.Add(New GPFeatureRecordSetLayer("Input_Features", mapPoint))
      parameters.Add(New GPString("Height", HeightTextBox.Text))
      parameters.Add(New GPLinearUnit("Distance", esriUnits.esriMiles, Convert.ToDouble(MilesTextBox.Text)))


      _geoprocessorTask.OutputSpatialReference = New SpatialReference(102100)
      _geoprocessorTask.SubmitJobAsync(parameters)
    End If

  End Sub

  Private Sub GeoprocessorTask_JobCompleted(ByVal sender As Object, ByVal e As JobInfoEventArgs)
    MyMap.Cursor = System.Windows.Input.Cursors.Hand
    If e.JobInfo.JobStatus = esriJobStatus.esriJobSucceeded Then
      Dim geoprocessorTask As Geoprocessor = TryCast(sender, Geoprocessor)

      System.Threading.Thread.Sleep(2000)

      resultLayer = geoprocessorTask.GetResultMapServiceLayer(e.JobInfo.JobId)
      AddHandler resultLayer.InitializationFailed, AddressOf resultLayer_InitializationFailed
      resultLayer.DisplayName = e.JobInfo.JobId
      If resultLayer IsNot Nothing Then
        _displayViewshedInfo = True
        MyMap.Layers.Add(resultLayer)
      End If
    Else
      MessageBox.Show("Geoprocessor service failed")
      _displayViewshedInfo = False
    End If
  End Sub

  Private Sub resultLayer_InitializationFailed(ByVal sender As Object, ByVal e As EventArgs)
  End Sub
  Private Sub GeoprocessorTask_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
    MyMap.Cursor = System.Windows.Input.Cursors.Hand
    _displayViewshedInfo = False
    MessageBox.Show("Geoprocessor service failed: " & e.Error.Message())
  End Sub

  Private Sub RemoveLayers_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    'remove all previous results
    graphicsLayer.ClearGraphics()

    Dim idx As Integer = MyMap.Layers.Count - 1
    Do While idx > 1
      MyMap.Layers.RemoveAt(idx)
      idx -= 1
    Loop
    _displayViewshedInfo = False
    MyInfoWindow.IsOpen = False
  End Sub

End Class
