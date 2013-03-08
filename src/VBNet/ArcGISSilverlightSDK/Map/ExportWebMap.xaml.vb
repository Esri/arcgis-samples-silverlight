Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Printing
Imports ESRI.ArcGIS.Client.Tasks

Partial Public Class ExportWebMap
	Inherits UserControl
	Private printTask As PrintTask

	Public Sub New()
		InitializeComponent()

    printTask = New PrintTask("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Utilities/PrintingTools/GPServer/Export%20Web%20Map%20Task")
		printTask.DisableClientCaching = True
		AddHandler printTask.ExecuteCompleted, AddressOf printTask_PrintCompleted
		AddHandler printTask.GetServiceInfoCompleted, AddressOf printTask_GetServiceInfoCompleted
		printTask.GetServiceInfoAsync()

	End Sub

	Private Sub printTask_GetServiceInfoCompleted(ByVal sender As Object, ByVal e As ServiceInfoEventArgs)
		LayoutTemplates.ItemsSource = e.ServiceInfo.LayoutTemplates
		Formats.ItemsSource = e.ServiceInfo.Formats
	End Sub

	Private Sub printTask_PrintCompleted(ByVal sender As Object, ByVal e As PrintEventArgs)
		System.Windows.Browser.HtmlPage.Window.Navigate(e.PrintResult.Url, "_blank")
	End Sub

	Private Sub ExportMap_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    If printTask Is Nothing OrElse printTask.IsBusy Then
      Return
    End If

		Dim printParameters As New PrintParameters(MyMap) With
		 {
		 .ExportOptions = New ExportOptions() With
			{
			.Dpi = 96,
			.OutputSize = New Size(MyMap.ActualWidth, MyMap.ActualHeight)
			},
		 .LayoutTemplate = If(CStr(LayoutTemplates.SelectedItem), String.Empty),
		 .Format = CStr(Formats.SelectedItem)
		}
		printTask.ExecuteAsync(printParameters)
	End Sub
End Class

