Imports System
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports System.Windows


Partial Public Class EditToolsExplicitSave
	Inherits UserControl
	Public Sub New()
		InitializeComponent()
	End Sub

	Private Sub CancelEditsButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
		Dim editor As Editor = TryCast(LayoutRoot.Resources("MyEditor"), Editor)
		For Each graphicsLayer As GraphicsLayer In editor.GraphicsLayers
			If TypeOf graphicsLayer Is FeatureLayer Then
				Dim featureLayer As FeatureLayer = TryCast(graphicsLayer, FeatureLayer)
				If featureLayer.HasEdits Then
					featureLayer.Update()
				End If

			End If
		Next graphicsLayer
	End Sub
End Class

