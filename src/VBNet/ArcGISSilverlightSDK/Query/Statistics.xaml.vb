Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Tasks
Partial Public Class Statistics
  Inherits UserControl
  Private queryTask As QueryTask

  Public Sub New()
    InitializeComponent()
    queryTask = New QueryTask("http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/2")
    AddHandler queryTask.ExecuteCompleted, AddressOf queryTask_ExecuteCompleted
  End Sub

  Private Sub OutStatisticsDataGrid_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
    Dim query As New Query()
    With query
      .GroupByFieldsForStatistics = New List(Of String) _
        From {"SUB_REGION"}
      .OutStatistics = New List(Of OutStatistic) _
    From
    {New OutStatistic() With
    {
    .OnStatisticField = "pop2000",
    .OutStatisticFieldName = "SubRegionPopulation",
    .StatisticType = StatisticType.Sum
    },
    New OutStatistic() With
    {
    .OnStatisticField = "sub_region",
    .OutStatisticFieldName = "NumberOfStates",
    .StatisticType = StatisticType.Count
    }
    }
    End With
    queryTask.ExecuteAsync(query)
  End Sub

  Private Sub queryTask_ExecuteCompleted(ByVal sender As Object, ByVal e As QueryEventArgs)
    Dim featureSet As FeatureSet = e.FeatureSet

    If featureSet IsNot Nothing AndAlso featureSet.Features.Count > 0 Then
      OutStatisticsDataGrid.ItemsSource = featureSet.Features
    End If
  End Sub
End Class

