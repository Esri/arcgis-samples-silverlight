Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Symbols
Imports ESRI.ArcGIS.Client.Tasks
Imports ESRI.ArcGIS.Client.Geometry
Imports System.Threading

Partial Public Class DriveTimes
    Inherits UserControl
    Private _geoprocessorTask As Geoprocessor
    Private jobid As String
    Private graphicsLayer As GraphicsLayer
    Private bufferSymbols As List(Of FillSymbol)
    Private inputPoint As MapPoint

    Public Sub New()
        InitializeComponent()

        graphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
        bufferSymbols = New List(Of FillSymbol)(New FillSymbol() {TryCast(LayoutRoot.Resources("FillSymbol1"), FillSymbol), TryCast(LayoutRoot.Resources("FillSymbol2"), FillSymbol), TryCast(LayoutRoot.Resources("FillSymbol3"), FillSymbol)})

        _geoprocessorTask = New Geoprocessor("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/GPServer/Generate%20Service%20Areas")
        AddHandler _geoprocessorTask.JobCompleted, AddressOf GeoprocessorTask_JobCompleted
        AddHandler _geoprocessorTask.StatusUpdated, AddressOf GeoprocessorTask_StatusUpdated
        AddHandler _geoprocessorTask.GetResultDataCompleted, AddressOf GeoprocessorTask_GetResultDataCompleted
        AddHandler _geoprocessorTask.Failed, AddressOf GeoprocessorTask_Failed
    End Sub

    Private Sub MyMap_MouseClick(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Map.MouseEventArgs)
        If jobid IsNot Nothing Then
            inputPoint = e.MapPoint
            _geoprocessorTask.CancelJobAsync(jobid)
        Else
            inputPoint = Nothing
            SubmitJob(e.MapPoint)
        End If
    End Sub

    Private Sub SubmitJob(ByVal mp As MapPoint)
        graphicsLayer.Graphics.Clear()

        Dim graphic As New Graphic() With {.Symbol = TryCast(LayoutRoot.Resources("DefaultMarkerSymbol"), Symbol), .Geometry = mp}
        graphic.SetZIndex(1)
        graphicsLayer.Graphics.Add(graphic)

        Dim parameters As New List(Of GPParameter)()
        parameters.Add(New GPFeatureRecordSetLayer("Facilities", New FeatureSet(graphicsLayer.Graphics)))
        parameters.Add(New GPString("Break_Values", "1 2 3"))
        parameters.Add(New GPString("Break_Units", "Minutes"))

        _geoprocessorTask.SubmitJobAsync(parameters)
    End Sub

    Private Sub GeoprocessorTask_StatusUpdated(ByVal sender As Object, ByVal e As JobInfoEventArgs)
        jobid = If(e.JobInfo.JobStatus = esriJobStatus.esriJobCancelled OrElse e.JobInfo.JobStatus = esriJobStatus.esriJobDeleted OrElse e.JobInfo.JobStatus = esriJobStatus.esriJobFailed, Nothing, e.JobInfo.JobId)
        If e.JobInfo.JobStatus = esriJobStatus.esriJobCancelling AndAlso inputPoint IsNot Nothing Then
            SubmitJob(inputPoint)
            inputPoint = Nothing
        End If
    End Sub

    Private Sub GeoprocessorTask_JobCompleted(ByVal sender As Object, ByVal e As JobInfoEventArgs)
        jobid = Nothing
        If e.JobInfo.JobStatus = esriJobStatus.esriJobSucceeded Then
            _geoprocessorTask.GetResultDataAsync(e.JobInfo.JobId, "ServiceAreas")
        End If
    End Sub
    Private Sub GeoprocessorTask_GetResultDataCompleted(ByVal sender As Object, ByVal e As GPParameterEventArgs)
        If TypeOf e.Parameter Is GPFeatureRecordSetLayer Then
            Dim gpLayer As GPFeatureRecordSetLayer = TryCast(e.Parameter, GPFeatureRecordSetLayer)

            Dim count As Integer = 0
            For Each graphic As Graphic In gpLayer.FeatureSet.Features
                graphic.Symbol = bufferSymbols(count)
                count += 1
                graphicsLayer.Graphics.Add(graphic)
            Next graphic
        End If
    End Sub

    Private Sub GeoprocessorTask_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
        MessageBox.Show("Geoprocessing service failed: " & e.Error.ToString())
    End Sub
End Class
