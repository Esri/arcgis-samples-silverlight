Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.WebMap

Partial Public Class WebMapFeatureServicePopups
	Inherits UserControl
	Private mapTipsElements As New Dictionary(Of String, FrameworkElement)()
	Private lastPoint As MapPoint = Nothing

	Public Sub New()
		InitializeComponent()
		Dim webMap As New Document()
		AddHandler webMap.GetMapCompleted, AddressOf webMap_GetMapCompleted

    webMap.GetMapAsync("921e9016d2a5423da8bd08eb306e4e0e")
	End Sub

	Private Sub webMap_GetMapCompleted(ByVal sender As Object, ByVal e As GetMapCompletedEventArgs)
		If e.Error Is Nothing Then
			MyMap.Extent = e.Map.Extent
			Dim i As Integer = 0

			Dim layerCollection As New LayerCollection()
			For Each layer As Layer In e.Map.Layers
				layer.ID = i.ToString()
				If TypeOf layer Is FeatureLayer Then
					mapTipsElements.Add(layer.ID, (TryCast(layer, FeatureLayer)).MapTip)
				End If
				layerCollection.Add(layer)
				i += 1
			Next layer

			e.Map.Layers.Clear()
			MyMap.Layers = layerCollection
		End If
	End Sub

	Private Sub MapTipRadioButton_Checked(ByVal sender As Object, ByVal e As RoutedEventArgs)
		If MyMap IsNot Nothing Then
			MyInfoWindow.IsOpen = False
			For Each layer As Layer In MyMap.Layers
				If TypeOf layer Is FeatureLayer Then
					RemoveHandler TryCast(layer, FeatureLayer).MouseLeftButtonUp, AddressOf WebMapFeatureServicePopups_MouseLeftButtonUp
				End If
				If mapTipsElements.ContainsKey(layer.ID) Then
					TryCast(layer, FeatureLayer).MapTip = mapTipsElements(layer.ID)
				End If
			Next layer
		End If
	End Sub

	Private Sub InfoWindowRadioButton_Unchecked(ByVal sender As Object, ByVal e As RoutedEventArgs)
		For Each layer As Layer In MyMap.Layers
			If TypeOf layer Is FeatureLayer Then
				TryCast(layer, FeatureLayer).MapTip = Nothing
				AddHandler TryCast(layer, FeatureLayer).MouseLeftButtonUp, AddressOf WebMapFeatureServicePopups_MouseLeftButtonUp
			End If
		Next layer
	End Sub

	Private Sub WebMapFeatureServicePopups_MouseLeftButtonUp(ByVal sender As Object, ByVal e As GraphicMouseButtonEventArgs)
		Dim flayer As FeatureLayer = TryCast(sender, FeatureLayer)
		Dim clickPoint As MapPoint = MyMap.ScreenToMap(e.GetPosition(MyMap))

		If clickPoint IsNot lastPoint Then
			If flayer.GetValue(Document.PopupTemplateProperty) IsNot Nothing Then
				Dim dt As DataTemplate = TryCast(flayer.GetValue(Document.PopupTemplateProperty), DataTemplate)

				MyInfoWindow.Anchor = clickPoint
				MyInfoWindow.ContentTemplate = dt
				MyInfoWindow.Content = e.Graphic.Attributes
				MyInfoWindow.IsOpen = True
				lastPoint = clickPoint
			End If
		End If
	End Sub
End Class
