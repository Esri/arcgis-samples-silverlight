Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks


  Partial Public Class QueryWithoutMap
    Inherits UserControl
    Public Sub New()
      InitializeComponent()
    End Sub

    Private Sub QueryButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      Dim queryTask As New QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5")
      AddHandler queryTask.ExecuteCompleted, AddressOf QueryTask_ExecuteCompleted
      AddHandler queryTask.Failed, AddressOf QueryTask_Failed

      Dim query As New ESRI.ArcGIS.Client.Tasks.Query()
      query.Text = StateNameTextBox.Text

      query.OutFields.Add("*")
      queryTask.ExecuteAsync(query)
    End Sub

    Private Sub QueryTask_ExecuteCompleted(ByVal sender As Object, ByVal args As ESRI.ArcGIS.Client.Tasks.QueryEventArgs)
      Dim featureSet As FeatureSet = args.FeatureSet

      If featureSet IsNot Nothing AndAlso featureSet.Features.Count > 0 Then
        QueryDetailsDataGrid.ItemsSource = featureSet.Features
      Else
        MessageBox.Show("No features returned from query")
      End If
    End Sub

    Private Sub QueryTask_Failed(ByVal sender As Object, ByVal args As TaskFailedEventArgs)
      MessageBox.Show("Query execute error: " & args.Error.Message)
    End Sub
  End Class


