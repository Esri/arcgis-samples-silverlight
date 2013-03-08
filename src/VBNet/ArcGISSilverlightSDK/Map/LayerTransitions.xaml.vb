Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports ESRI.ArcGIS.Client

Partial Public Class LayerTransitions
    Inherits UserControl
    Private progress As Integer = 0
    Private fromLayer As Layer
    Private toLayer As Layer
    Private pendingLayer As Layer
    Private animatingLayer As Layer

    Public Sub New()
        InitializeComponent()
        AddHandler MyMap.Progress, AddressOf MyMap_Progress
    End Sub

    Private Sub RadioButton_Checked(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim radioButton As RadioButton = TryCast(sender, RadioButton)
        If MyMap IsNot Nothing Then
            Dim [from] As Layer = Nothing
            For Each layer As Layer In MyMap.Layers
                If layer.Visible AndAlso layer.Opacity = 1 Then
                    [from] = layer
                    Exit For
                End If
            Next layer

            Dim [to] As Layer = MyMap.Layers(radioButton.Content.ToString())

            Fade([from], [to])
        End If
    End Sub

    Private Sub Fade(ByVal [from] As Layer, ByVal [to] As Layer)
        ' If in process of animating
        If animatingLayer IsNot Nothing Then
            pendingLayer = [to]
            Return
        End If
        pendingLayer = Nothing

        [to].Opacity = 0
        [to].Visible = True
        Dispatcher.BeginInvoke(Sub()
                                   If progress >= 97 Then
                                       StartFade([from], [to])
                                   Else 'Wait for layer to load before fading to it
                                       Dim handler As EventHandler(Of ProgressEventArgs) = Nothing
                                       handler = Sub(s, e)
                                                     If e.Progress >= 97 Then
                                                         RemoveHandler MyMap.Progress, handler
                                                         StartFade([from], [to])
                                                     End If
                                                 End Sub
                                       AddHandler MyMap.Progress, handler
                                   End If
                               End Sub)
    End Sub

    Private Sub StartFade(ByVal [from] As Layer, ByVal [to] As Layer)
        fromLayer = [from]
        toLayer = [to]

        ' If fromLayer is below toLayer, layer to animate is toLayer. 
        ' If fromLayer is above toLayer, layer to animate is fromLayer. The toLayer opacity 
        ' should be set to completely opaque. 
        If MyMap.Layers.IndexOf(fromLayer) < MyMap.Layers.IndexOf(toLayer) Then
            animatingLayer = toLayer
        Else
            animatingLayer = fromLayer
            toLayer.Opacity = 1
        End If

        ' Listen for when a frame is rendered
        AddHandler CompositionTarget.Rendering, AddressOf CompositionTarget_Rendering
    End Sub

    Private Sub CompositionTarget_Rendering(ByVal sender As Object, ByVal e As EventArgs)
        ' Change the opacity of the fromLayer and toLayer
        Dim opacity As Double = -1
        If animatingLayer Is fromLayer Then
            opacity = Math.Max(0, fromLayer.Opacity - 0.05)
            fromLayer.Opacity = opacity
        Else
            opacity = Math.Min(1, toLayer.Opacity + 0.05)
            toLayer.Opacity = opacity
        End If

        ' When transition complete, set reset properties and unhook handler 
        If opacity = 1 OrElse opacity = 0 Then
            fromLayer.Opacity = 0
            fromLayer.Visible = False
            animatingLayer = Nothing
            RemoveHandler CompositionTarget.Rendering, AddressOf CompositionTarget_Rendering
            ' If layer pending animation, start fading
            If pendingLayer IsNot Nothing Then
                Fade(toLayer, pendingLayer)
            End If
            pendingLayer = Nothing
        End If
    End Sub

    ' Track overall progress of loading map content 
    Private Sub MyMap_Progress(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.ProgressEventArgs)
        progress = e.Progress
    End Sub
End Class