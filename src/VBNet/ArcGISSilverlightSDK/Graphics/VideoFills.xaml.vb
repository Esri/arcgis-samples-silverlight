Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks
Imports ESRI.ArcGIS.Client.Symbols


	Partial Public Class VideoFills
		Inherits UserControl
		Private _lastActiveGraphics As List(Of Graphic)

		Public Sub New()
			InitializeComponent()
			_lastActiveGraphics = New List(Of Graphic)()

			AddHandler MyMap.Layers.LayersInitialized, AddressOf Layers_LayersInitialized
		End Sub

		Private Sub Layers_LayersInitialized(ByVal sender As Object, ByVal args As EventArgs)
			Dim query As New ESRI.ArcGIS.Client.Tasks.Query() With {.OutSpatialReference = MyMap.SpatialReference, .ReturnGeometry = True}
			query.OutFields.Add("STATE_NAME")
			query.Where = "STATE_NAME IN ('Alaska', 'Hawaii', 'Washington', 'Oregon', 'Arizona', 'Nevada', 'Idaho', 'Montana', 'Utah', 'Wyoming', 'Colorado', 'New Mexico')"

			Dim myQueryTask As New QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5")
			AddHandler myQueryTask.ExecuteCompleted, AddressOf myQueryTask_ExecuteCompleted
			myQueryTask.ExecuteAsync(query)
		End Sub

		Private Sub myQueryTask_ExecuteCompleted(ByVal sender As Object, ByVal queryArgs As QueryEventArgs)
			If queryArgs.FeatureSet Is Nothing Then
				Return
			End If

			Dim resultFeatureSet As FeatureSet = queryArgs.FeatureSet
			Dim graphicsLayer As ESRI.ArcGIS.Client.GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), ESRI.ArcGIS.Client.GraphicsLayer)

			If resultFeatureSet IsNot Nothing AndAlso resultFeatureSet.Features.Count > 0 Then
				For Each graphicFeature As ESRI.ArcGIS.Client.Graphic In resultFeatureSet.Features
					graphicFeature.Symbol = TryCast(LayoutRoot.Resources("TransparentFillSymbol"), Symbol)
					graphicsLayer.Graphics.Add(graphicFeature)
				Next graphicFeature
			End If
		End Sub

		Private Sub MyGraphicsLayer_MouseEnter(ByVal sender As Object, ByVal args As GraphicMouseEventArgs)
			Dim stateName As String = Convert.ToString(args.Graphic.Attributes("STATE_NAME"))

			If _lastActiveGraphics.Count > 0 Then
				For i As Integer = 0 To _lastActiveGraphics.Count - 1
					If Convert.ToString(_lastActiveGraphics(i).Attributes("STATE_NAME")) <> stateName Then
						ClearVideoSymbol(_lastActiveGraphics(i))
					Else
						Return
					End If
				Next i
			End If

			Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)

			Dim videoGrid As Grid = TryCast(FindName("MediaGrid"), Grid)
			videoGrid.Children.Clear()

			Dim stateMediaElement As New MediaElement() With {.Source = New Uri(String.Format("http://serverapps.esri.com/media/{0}_small.wmv", stateName), UriKind.Absolute), .Stretch = Stretch.None, .AutoPlay = True, .IsMuted = True, .Opacity = 0.0, .IsHitTestVisible = False}
			AddHandler stateMediaElement.MediaEnded, AddressOf State_Media_MediaEnded
			videoGrid.Children.Add(stateMediaElement)
			Dim stateVideoFillSymbol As FillSymbol = TryCast(LayoutRoot.Resources("StateVideoFillSymbol"), FillSymbol)
			TryCast(stateVideoFillSymbol.Fill, VideoBrush).SetSource(stateMediaElement)
			args.Graphic.Symbol = stateVideoFillSymbol

			_lastActiveGraphics.Add(args.Graphic)
		End Sub

		Private Sub MyGraphicsLayer_MouseLeave(ByVal sender As Object, ByVal args As GraphicMouseEventArgs)
			ClearVideoSymbol(args.Graphic)
		End Sub

		Private Sub ClearVideoSymbol(ByVal graphic As Graphic)
			Dim videoGrid As Grid = TryCast(FindName("MediaGrid"), Grid)
			If videoGrid.Children IsNot Nothing AndAlso videoGrid.Children.Count > 0 Then
				Dim m As MediaElement = TryCast(videoGrid.Children.ElementAt(0), MediaElement)
				If m IsNot Nothing Then
					RemoveHandler m.MediaEnded, AddressOf State_Media_MediaEnded
					m.Stop()
				End If
				videoGrid.Children.Clear()
			End If

			graphic.Symbol = TryCast(LayoutRoot.Resources("TransparentFillSymbol"), Symbol)

			_lastActiveGraphics.Remove(graphic)
		End Sub

		Private Sub State_Media_MediaEnded(ByVal sender As Object, ByVal e As RoutedEventArgs)
			' Repeat play of the video
			Dim media As MediaElement = TryCast(sender, MediaElement)
			media.Position = TimeSpan.FromSeconds(0)
			media.Play()
		End Sub
	End Class

