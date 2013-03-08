Imports Microsoft.VisualBasic
Imports System
Imports System.Windows
Imports System.Windows.Controls


  Partial Public Class MapAnimation
    Inherits UserControl
    Public Sub New()
      InitializeComponent()
    End Sub

    Private Sub ZoomAnimation_ValueChanged(ByVal sender As Object, ByVal e As RoutedPropertyChangedEventArgs(Of Double))
      Dim seconds As Integer = Convert.ToInt32(e.NewValue)
      MyMap.ZoomDuration = New TimeSpan(0, 0, seconds)
      ZoomValueLabel.Text = String.Format("Value: {0}", seconds)
    End Sub

    Private Sub PanAnimation_ValueChanged(ByVal sender As Object, ByVal e As RoutedPropertyChangedEventArgs(Of Double))
      Dim seconds As Integer = Convert.ToInt32(e.NewValue)
      MyMap.PanDuration = New TimeSpan(0, 0, seconds)
      PanValueLabel.Text = String.Format("Value: {0}", seconds)
    End Sub
  End Class

