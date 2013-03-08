Imports Microsoft.VisualBasic
Imports System.Windows.Controls
Imports System.Windows.Input


  Partial Public Class Magnify
    Inherits UserControl
    Public Sub New()
      InitializeComponent()
    End Sub

    Private Sub MyMagnifyImage_MouseLeftButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
      MyMagnifier.Enabled = Not MyMagnifier.Enabled
    End Sub
  End Class

