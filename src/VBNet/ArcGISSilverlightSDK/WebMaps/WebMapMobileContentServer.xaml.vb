Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.WebMap

Partial Public Class WebMapMobileContentServer
	Inherits UserControl
	Public Sub New()
		InitializeComponent()
		Dim webMap As New Document()
		AddHandler webMap.GetMapCompleted, AddressOf webMap_GetMapCompleted
		webMap.ServerBaseUrl = "http://arcgismobile.esri.com/arcgis/mobile/content"

		webMap.GetMapAsync("00ab0becb052428485a8d25e62afb86d")
	End Sub

	Private Sub webMap_GetMapCompleted(ByVal sender As Object, ByVal e As GetMapCompletedEventArgs)
		If e.Error Is Nothing Then
			LayoutRoot.Children.Add(e.Map)
		End If
	End Sub
End Class
