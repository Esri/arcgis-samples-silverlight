Imports Microsoft.VisualBasic
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.WebMap
Imports System
Imports ESRI.ArcGIS.Client

Partial Public Class LoadWebMapDynamically
	Inherits UserControl
	Public Sub New()
		InitializeComponent()
	End Sub

	Private Sub LoadWebMapButton_Click(ByVal sender As Object, ByVal evt As System.Windows.RoutedEventArgs)

		If Not String.IsNullOrEmpty(WebMapTextBox.Text) Then
			Dim webMap As New Document()
			AddHandler webMap.GetMapCompleted, Sub(s, e)
																					 MyMap.Extent = e.Map.Extent

																					 Dim layerCollection As New LayerCollection()
																					 For Each layer As Layer In e.Map.Layers
																						 layerCollection.Add(layer)
																					 Next layer

																					 e.Map.Layers.Clear()
																					 MyMap.Layers = layerCollection
																					 WebMapPropertiesTextBox.DataContext = e.ItemInfo
																				 End Sub

			webMap.GetMapAsync(WebMapTextBox.Text)
		End If
	End Sub
End Class
