Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Tasks
Imports ESRI.ArcGIS.Client.WebMap

Partial Public Class WebMapDynamicServicePopups
	Inherits UserControl
	Public Sub New()
		InitializeComponent()
		Dim webMap As New Document()
		AddHandler webMap.GetMapCompleted, AddressOf webMap_GetMapCompleted

		webMap.GetMapAsync("fd7fb514579f4422ab2698f47a7d4a46")
	End Sub

	Private Sub webMap_GetMapCompleted(ByVal sender As Object, ByVal e As GetMapCompletedEventArgs)
		If e.Error Is Nothing Then
			MyMap.Extent = e.Map.Extent

			Dim layerCollection As New LayerCollection()
			For Each layer As Layer In e.Map.Layers
				layerCollection.Add(layer)
			Next layer

			e.Map.Layers.Clear()
			MyMap.Layers = layerCollection
		End If
	End Sub

	Private Sub MyMap_MouseClick(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Map.MouseEventArgs)
		MyInfoWindow.IsOpen = False

        Dim mapScale As Double = MyMap.Scale

		Dim alayer As ArcGISDynamicMapServiceLayer = Nothing
		Dim dt As DataTemplate = Nothing
		Dim layid As Integer = 0

		For Each layer As Layer In MyMap.Layers
			If layer.GetValue(Document.PopupTemplatesProperty) IsNot Nothing Then
				alayer = TryCast(layer, ArcGISDynamicMapServiceLayer)
				Dim idict As IDictionary(Of Integer, DataTemplate) = TryCast(layer.GetValue(Document.PopupTemplatesProperty), IDictionary(Of Integer, DataTemplate))
				For Each linfo As LayerInfo In alayer.Layers
					If ((mapScale > linfo.MaxScale AndAlso mapScale < linfo.MinScale) OrElse (linfo.MaxScale = 0.0 AndAlso linfo.MinScale = 0.0) OrElse (mapScale > linfo.MaxScale AndAlso linfo.MinScale = 0.0)) AndAlso idict.ContainsKey(linfo.ID) Then ' id present in dictionary
						layid = linfo.ID
						dt = idict(linfo.ID)
						Exit For
					End If
				Next linfo
			End If
		Next layer

		If dt IsNot Nothing Then
			Dim qt As New QueryTask(String.Format("{0}/{1}", alayer.Url, layid))
			AddHandler qt.ExecuteCompleted, AddressOf qt_ExecuteComplete

			Dim query As New ESRI.ArcGIS.Client.Tasks.Query()
			Dim contractRatio As Double = MyMap.Extent.Width / 20
			Dim inputEnvelope As New Envelope(e.MapPoint.X - contractRatio, e.MapPoint.Y - contractRatio, e.MapPoint.X + contractRatio, e.MapPoint.Y + contractRatio)
			inputEnvelope.SpatialReference = MyMap.SpatialReference
			query.Geometry = inputEnvelope
			query.OutSpatialReference = MyMap.SpatialReference
			query.OutFields.Add("*")

			qt.ExecuteAsync(query)

			MyInfoWindow.Anchor = e.MapPoint
			MyInfoWindow.ContentTemplate = dt
		End If
	End Sub

	Private Sub qt_ExecuteComplete(ByVal sender As Object, ByVal qe As QueryEventArgs)

		If qe.FeatureSet.Features.Count > 0 Then
			Dim g As Graphic = qe.FeatureSet.Features(0)
			MyInfoWindow.Content = g.Attributes
			MyInfoWindow.IsOpen = True
		Else
			MyInfoWindow.Content = Nothing
			MyInfoWindow.IsOpen = False
		End If
	End Sub
End Class
