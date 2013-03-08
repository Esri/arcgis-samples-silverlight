Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.WebMap

Partial Public Class WebMapNotesPopups
	Inherits UserControl
	Public Sub New()
		InitializeComponent()

		Dim webMap As New Document()
		AddHandler webMap.GetMapCompleted, AddressOf webMap_GetMapCompleted

		webMap.GetMapAsync("2ccf901c5b414e5c98a346edb75e3c13")
	End Sub

	Private Sub webMap_GetMapCompleted(ByVal sender As Object, ByVal e As GetMapCompletedEventArgs)
		If e.Error Is Nothing Then
			For Each layer As Layer In e.Map.Layers
				If TypeOf layer Is GraphicsLayer Then
					Dim glayer As GraphicsLayer = TryCast(layer, GraphicsLayer)
					' Modification of the default map tip style
					Dim border As Border = TryCast(glayer.MapTip, Border)
					If border IsNot Nothing Then
						border.Background = New SolidColorBrush(Color.FromArgb(200, 102, 150, 255))
						border.CornerRadius = New CornerRadius(4)
					End If
				End If
			Next layer
			LayoutRoot.Children.Add(e.Map)
		End If
	End Sub
End Class

