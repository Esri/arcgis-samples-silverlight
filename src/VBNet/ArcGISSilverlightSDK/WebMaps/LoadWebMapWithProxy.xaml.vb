Imports Microsoft.VisualBasic
Imports System
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.WebMap
Imports ESRI.ArcGIS.Client


	Partial Public Class LoadWebMapWithProxy
		Inherits UserControl
		Public Sub New()
			InitializeComponent()

			Dim webMap As New Document()
    webMap.ProxyUrl = "http://servicesbeta3.esri.com/SilverlightDemos/ProxyPage/proxy.ashx"
			AddHandler webMap.GetMapCompleted, AddressOf webMap_GetMapCompleted

			webMap.GetMapAsync("07cbed6b51474885b420cd5ed4c3e082")
		End Sub

		Private Sub webMap_GetMapCompleted(ByVal sender As Object, ByVal e As GetMapCompletedEventArgs)
			If e.Error Is Nothing Then
				LayoutRoot.Children.Add(e.Map)
			End If
		End Sub
	End Class

