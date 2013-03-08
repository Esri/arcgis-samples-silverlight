Imports Microsoft.VisualBasic
Imports System
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.WebMap


	Partial Public Class LoadWebMap
		Inherits UserControl
		Public Sub New()
			InitializeComponent()

			Dim webMap As New Document()
			AddHandler webMap.GetMapCompleted, AddressOf webMap_GetMapCompleted

			webMap.GetMapAsync("00e5e70929e14055ab686df16c842ec1")
		End Sub

		Private Sub webMap_GetMapCompleted(ByVal sender As Object, ByVal e As GetMapCompletedEventArgs)
			If e.Error Is Nothing Then
				LayoutRoot.Children.Add(e.Map)
			End If
		End Sub
	End Class

