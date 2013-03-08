Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks
Imports System.Linq
Imports System.Windows
Imports System.Collections.Generic
Imports System.Windows.Threading
Imports System
Imports System.Net
Imports System.IO
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Symbols

Partial Public Class ExtractData
  Inherits UserControl
  Private _geoprocessorTask As Geoprocessor
  Private _processingTimer As DispatcherTimer
  Private _streamedDataFile As Stream
  Private _drawObject As Draw
  Private _graphicsLayer As GraphicsLayer

  Public Sub New()
    InitializeComponent()
    _processingTimer = New System.Windows.Threading.DispatcherTimer()
    _processingTimer.Interval = New TimeSpan(0, 0, 0, 0, 800)
    AddHandler _processingTimer.Tick, AddressOf ProcessingTimer_Tick


    _geoprocessorTask = New Geoprocessor("http://sampleserver4.arcgisonline.com/ArcGIS/rest/services/HomelandSecurity/Incident_Data_Extraction/GPServer/Extract%20Data%20Task")
    AddHandler _geoprocessorTask.GetServiceInfoCompleted, AddressOf _geoprocessorTask_GetServiceInfoCompleted
    AddHandler _geoprocessorTask.Failed, AddressOf _geoprocessorTask_Failed
    _geoprocessorTask.UpdateDelay = 5000
    AddHandler _geoprocessorTask.JobCompleted, AddressOf _geoprocessorTask_JobCompleted
    AddHandler _geoprocessorTask.GetResultDataCompleted, AddressOf _geoprocessorTask_GetResultDataCompleted
    _geoprocessorTask.GetServiceInfoAsync()

    _drawObject = New Draw(MyMap) With {.LineSymbol = TryCast(LayoutRoot.Resources("CustomAnimatedRedLineSymbol"), LineSymbol), .FillSymbol = TryCast(LayoutRoot.Resources("CustomAnimatedRedFillSymbol"), FillSymbol)}
    AddHandler _drawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete

    _graphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)

  End Sub
  Private Sub DrawPolygon_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    _graphicsLayer.ClearGraphics()
    If FreehandCheckBox.IsChecked.Value Then
      _drawObject.DrawMode = DrawMode.Freehand
    Else
      _drawObject.DrawMode = DrawMode.Polygon
    End If

    _drawObject.IsEnabled = True
  End Sub

  Private Sub FreehandCheckBox_Checked(ByVal sender As Object, ByVal e As RoutedEventArgs)
    If FreehandCheckBox.IsChecked.Value Then
      _drawObject.DrawMode = DrawMode.Freehand
    Else
      _drawObject.DrawMode = DrawMode.Polygon
    End If
  End Sub
  Private Sub _geoprocessorTask_GetServiceInfoCompleted(ByVal sender As Object, ByVal e As GPServiceInfoEventArgs)

    LayersList.ItemsSource = TryCast(e.GPServiceInfo.Parameters.FirstOrDefault(Function(p) p.Name = "Layers_to_Clip").ChoiceList, Object())

    Formats.ItemsSource = TryCast(e.GPServiceInfo.Parameters.FirstOrDefault(Function(p) p.Name = "Feature_Format").ChoiceList, Object())
    If Not Formats.ItemsSource Is Nothing AndAlso Formats.Items.Count > 0 Then
      Formats.SelectedIndex = 0
    End If
  End Sub

  Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As DrawEventArgs)

    If LayersList.SelectedItem Is Nothing Then
      MessageBox.Show("Please select layer(s) to extract")
      TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer).ClearGraphics()
      Return
    End If

    Dim filterGeometry As Geometry = args.Geometry

    If TypeOf args.Geometry Is Polyline Then
      Dim freehandLine As Polyline = TryCast(args.Geometry, Polyline)
      freehandLine.Paths(0).Add(freehandLine.Paths(0)(0).Clone())
      filterGeometry = New Polygon() With {.SpatialReference = MyMap.SpatialReference}
      TryCast(filterGeometry, Polygon).Rings.Add(freehandLine.Paths(0))
    End If

    Dim graphic As New ESRI.ArcGIS.Client.Graphic() With {.Geometry = filterGeometry}
    _graphicsLayer.Graphics.Add(graphic)
    _drawObject.IsEnabled = False

    ProcessingTextBlock.Visibility = Visibility.Visible
    _processingTimer.Start()

    Dim parameters As List(Of GPParameter) = New List(Of GPParameter)()

    Dim strLayerList As List(Of GPString) = New List(Of GPString)()

    For Each itm In LayersList.SelectedItems
      strLayerList.Add(New GPString(itm.ToString(), itm.ToString()))
    Next itm

    parameters.Add(New GPMultiValue(Of GPString)("Layers_to_Clip", strLayerList))
    parameters.Add(New GPFeatureRecordSetLayer("Area_of_Interest", _graphicsLayer.Graphics(0).Geometry))

    parameters.Add(New GPString("Feature_Format", Formats.SelectedValue.ToString()))

    _geoprocessorTask.SubmitJobAsync(parameters)

  End Sub

  Private Sub _geoprocessorTask_JobCompleted(ByVal sender As Object, ByVal e As JobInfoEventArgs)
    If e.JobInfo.JobStatus <> esriJobStatus.esriJobSucceeded Then
      MessageBox.Show("Extract Data task failed to complete.")
      Return
    End If

    _geoprocessorTask.GetResultDataAsync(e.JobInfo.JobId, "Output_Zip_File")
  End Sub

  Private Sub _geoprocessorTask_GetResultDataCompleted(ByVal sender As Object, ByVal ev1 As GPParameterEventArgs)
    If TypeOf ev1.Parameter Is GPDataFile Then
      Dim ClipResultFile As GPDataFile = TryCast(ev1.Parameter, GPDataFile)

      If String.IsNullOrEmpty(ClipResultFile.Url) Then
        Return
      End If

      Dim res As MessageBoxResult = MessageBox.Show("Data file created. Would you like to download the file?", "Geoprocessing Task Success", MessageBoxButton.OKCancel)
      If res = MessageBoxResult.OK Then
        Dim webClient As WebClient = New WebClient()
        AddHandler webClient.OpenReadCompleted, Sub(s, ev)
                                                  _streamedDataFile = ev.Result
                                                  SaveFileButton.Visibility = Visibility.Visible
                                                  ProcessingTextBlock.Text = "Download completed.  Click on 'Save data file' button to save to disk."
                                                End Sub
        webClient.OpenReadAsync(New Uri((TryCast(ev1.Parameter, GPDataFile)).Url), UriKind.Absolute)
      Else
        ProcessingTextBlock.Text = ""
        ProcessingTextBlock.Visibility = Visibility.Collapsed
        SaveFileButton.Visibility = Visibility.Collapsed
      End If
    End If
    _processingTimer.Stop()
  End Sub

  Private Sub _geoprocessorTask_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
    MessageBox.Show("Extract data task failed: " & e.Error.Message)
  End Sub


  Private Sub ResetButton_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
    TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer).ClearGraphics()
    ProcessingTextBlock.Text = ""
    ProcessingTextBlock.Visibility = Visibility.Collapsed
    SaveFileButton.Visibility = Visibility.Collapsed
  End Sub

  Private Sub ProcessingTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
    If ProcessingTextBlock.Text.Length > 20 OrElse (Not ProcessingTextBlock.Text.StartsWith("Processing")) Then
      ProcessingTextBlock.Text = "Processing."
    Else
      ProcessingTextBlock.Text = ProcessingTextBlock.Text & "."
    End If
  End Sub

  Private Sub SaveFileButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)

    Dim dialog As SaveFileDialog = New SaveFileDialog()
    dialog.DefaultFileName = "Output.zip"
    dialog.Filter = "Zip file (*.zip)|*.zip"

    Dim dialogResult As Nullable(Of Boolean) = dialog.ShowDialog()

    If dialogResult <> True Then
      Return
    End If
    Try
      Using fs As Stream = CType(dialog.OpenFile(), Stream)
        _streamedDataFile.CopyTo(fs)
        fs.Flush()
        fs.Close()
        ProcessingTextBlock.Text = "Output file saved successfully!"
        SaveFileButton.Visibility = Visibility.Collapsed
        _streamedDataFile = Nothing
      End Using
    Catch ex As Exception
      MessageBox.Show("Error saving file :" & ex.Message)
    End Try
  End Sub
End Class
