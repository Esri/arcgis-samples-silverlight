Imports Microsoft.VisualBasic
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports System.Windows
Partial Public Class DynamicAndTile

	Inherits UserControl

	Public Sub New()
		InitializeComponent()
	End Sub
	Private Sub Layer_InitializationFailed(ByVal sender As Object, ByVal e As System.EventArgs)
		Dim layer As Layer = TryCast(sender, Layer)
		If layer.InitializationFailure IsNot Nothing Then
			MessageBox.Show(layer.ID & ":" & layer.InitializationFailure.ToString())
		End If
	End Sub
End Class


