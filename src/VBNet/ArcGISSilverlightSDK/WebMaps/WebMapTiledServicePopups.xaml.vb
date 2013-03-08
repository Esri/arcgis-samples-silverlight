Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Symbols
Imports ESRI.ArcGIS.Client.Tasks
Imports ESRI.ArcGIS.Client.WebMap

Partial Public Class WebMapTiledServicePopups
	Inherits UserControl

	Dim glayer As GraphicsLayer
	Public Sub New()
		InitializeComponent()

		Dim webMap As New Document()
		AddHandler webMap.GetMapCompleted, AddressOf webMap_GetMapCompleted

        webMap.GetMapAsync("0e8aa0cc8dcb47d3b9a46e3c6c7c0f8f")
	End Sub

	Private Sub webMap_GetMapCompleted(ByVal sender As Object, ByVal e As GetMapCompletedEventArgs)
		If e.Error Is Nothing Then
			MyMap.Extent = e.Map.Extent

			Dim layerCollection As New LayerCollection()
			For Each layer As Layer In e.Map.Layers
				layerCollection.Add(layer)
			Next layer

			Dim selectedGraphics As New GraphicsLayer() With {.RendererTakesPrecedence = False, .ID = "MySelectionGraphicsLayer"}
			layerCollection.Add(selectedGraphics)

			e.Map.Layers.Clear()
			MyMap.Layers = layerCollection
			glayer = TryCast(MyMap.Layers("MySelectionGraphicsLayer"), GraphicsLayer)
		End If
	End Sub

	Private Sub MyMap_MouseClick(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Map.MouseEventArgs)

		glayer.Graphics.Clear()

		MyInfoWindow.IsOpen = False

        Dim mapScale As Double = MyMap.Scale

		Dim alayer As ArcGISTiledMapServiceLayer = Nothing
		Dim dt As DataTemplate = Nothing
		Dim layid As Integer = 0

		For Each layer As Layer In MyMap.Layers
			If layer.GetValue(Document.PopupTemplatesProperty) IsNot Nothing Then
				alayer = TryCast(layer, ArcGISTiledMapServiceLayer)
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
			query.Geometry = e.MapPoint
			query.OutSpatialReference = MyMap.SpatialReference
			query.OutFields.Add("*")
			query.ReturnGeometry = True

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

			Dim symbolColor As New SolidColorBrush(Colors.Cyan)

			If TypeOf g.Geometry Is Polygon OrElse TypeOf g.Geometry Is Envelope Then
				g.Symbol = New SimpleFillSymbol() With {.BorderBrush = symbolColor, .BorderThickness = 2}
			ElseIf TypeOf g.Geometry Is Polyline Then
				g.Symbol = New SimpleLineSymbol() With {.Color = symbolColor}
			Else ' Point
				g.Symbol = New SimpleMarkerSymbol() With {.Color = symbolColor, .Size = 12}
			End If
			glayer.Graphics.Add(g)
		Else
			MyInfoWindow.Content = Nothing
			MyInfoWindow.IsOpen = False
		End If
	End Sub
End Class
