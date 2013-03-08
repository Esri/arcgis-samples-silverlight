Imports Microsoft.VisualBasic
Imports System
Imports System.Windows
Imports System.Windows.Controls


  Partial Public Class MediaLayer
    Inherits UserControl
    Public Sub New()
      InitializeComponent()
    End Sub

    Private Sub Media_MediaEnded(ByVal sender As Object, ByVal args As RoutedEventArgs)
      ' Repeat play of the video
      Dim media As MediaElement = TryCast(sender, MediaElement)
      media.Position = TimeSpan.FromSeconds(0)
      media.Play()
    End Sub
  End Class

