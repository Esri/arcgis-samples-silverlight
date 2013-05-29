Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Threading
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks


Partial Public Class ClipFeatures
    Inherits UserControl
    Private _processingTimer As DispatcherTimer
    Private MyDrawObject As Draw

    Public Sub New()
        InitializeComponent()

        _processingTimer = New System.Windows.Threading.DispatcherTimer()
        _processingTimer.Interval = New TimeSpan(0, 0, 0, 0, 800)
        AddHandler _processingTimer.Tick, AddressOf ProcessingTimer_Tick

        MyDrawObject = New Draw(MyMap) With
                       {
                           .DrawMode = DrawMode.Polyline, .IsEnabled = True,
                           .LineSymbol = TryCast(LayoutRoot.Resources("ClipLineSymbol"), ESRI.ArcGIS.Client.Symbols.LineSymbol)
                       }
        AddHandler MyDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete
    End Sub

    Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As DrawEventArgs)
        MyDrawObject.IsEnabled = False

        ProcessingTextBlock.Visibility = Visibility.Visible
        _processingTimer.Start()

        Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)

        Dim graphic As New Graphic() With
            {
                .Symbol = TryCast(LayoutRoot.Resources("ClipLineSymbol"), ESRI.ArcGIS.Client.Symbols.LineSymbol),
                .Geometry = args.Geometry
            }
        graphicsLayer.Graphics.Add(graphic)

        Dim geoprocessorTask As New Geoprocessor("http://serverapps10.esri.com/ArcGIS/rest/services/" & "SamplesNET/USA_Data_ClipTools/GPServer/ClipCounties")
        'geoprocessorTask.ProxyURL = "http://servicesbeta3.esri.com/SilverlightDemos/ProxyPage/proxy.ashx";
        geoprocessorTask.UpdateDelay = 5000
        AddHandler geoprocessorTask.JobCompleted, AddressOf GeoprocessorTask_JobCompleted

        Dim parameters As New List(Of GPParameter)()
        parameters.Add(New GPFeatureRecordSetLayer("Input_Features", args.Geometry))
        parameters.Add(New GPLinearUnit("Linear_unit", esriUnits.esriMiles, Int32.Parse(DistanceTextBox.Text)))

        geoprocessorTask.SubmitJobAsync(parameters)
    End Sub

    Private Sub GeoprocessorTask_JobCompleted(ByVal sender As Object, ByVal e As JobInfoEventArgs)
        Dim geoprocessorTask As Geoprocessor = TryCast(sender, Geoprocessor)

        AddHandler geoprocessorTask.GetResultDataCompleted,
            Sub(s1, ev1)
              Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyResultGraphicsLayer"), GraphicsLayer)

              If TypeOf ev1.Parameter Is GPFeatureRecordSetLayer Then
                  Dim gpLayer As GPFeatureRecordSetLayer = TryCast(ev1.Parameter, GPFeatureRecordSetLayer)
                  If gpLayer.FeatureSet.Features.Count = 0 Then
                      AddHandler geoprocessorTask.GetResultImageLayerCompleted,
                          Sub(s2, ev2)
                        Dim gpImageLayer As GPResultImageLayer = ev2.GPResultImageLayer
                        gpImageLayer.Opacity = 0.5
                        MyMap.Layers.Add(gpImageLayer)

                        ProcessingTextBlock.Text = "Greater than 500 features returned.  Results drawn using map service."
                        _processingTimer.Stop()
                    End Sub

                      geoprocessorTask.GetResultImageLayerAsync(e.JobInfo.JobId, "Clipped_Counties")
                      Return
                  End If

                  For Each graphic As Graphic In gpLayer.FeatureSet.Features
                      graphic.Symbol = TryCast(LayoutRoot.Resources("ClipFeaturesFillSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
                      graphicsLayer.Graphics.Add(graphic)
                  Next graphic
              End If

              ProcessingTextBlock.Visibility = Visibility.Collapsed
              _processingTimer.Stop()
          End Sub

        geoprocessorTask.GetResultDataAsync(e.JobInfo.JobId, "Clipped_Counties")
    End Sub

    Private Sub GeoprocessorTask_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
        MessageBox.Show("Geoprocessor service failed: " & e.Error.ToString)
    End Sub

    Private Sub ClearButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim gpResultImageLayers As New List(Of Layer)()

        For Each layer As Layer In MyMap.Layers
            If TypeOf layer Is GraphicsLayer Then
                TryCast(layer, GraphicsLayer).Graphics.Clear()
            ElseIf TypeOf layer Is GPResultImageLayer Then
                gpResultImageLayers.Add(layer)
            End If
        Next layer
        For i As Integer = 0 To gpResultImageLayers.Count - 1
            MyMap.Layers.Remove(gpResultImageLayers(i))
        Next i

        MyDrawObject.IsEnabled = True

        ProcessingTextBlock.Text = ""
        ProcessingTextBlock.Visibility = Visibility.Collapsed
    End Sub

    Private Sub ProcessingTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
        If ProcessingTextBlock.Text.Length > 20 OrElse (Not ProcessingTextBlock.Text.StartsWith("Processing")) Then
            ProcessingTextBlock.Text = "Processing."
        Else
            ProcessingTextBlock.Text = ProcessingTextBlock.Text & "."
        End If
    End Sub

End Class

