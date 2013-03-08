Imports Microsoft.VisualBasic
Imports System.Windows
Imports System.Windows.Controls


  Partial Public Class ElementLayer
    Inherits UserControl
    Public Sub New()
      InitializeComponent()
    End Sub

    Private Sub RedlandsButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      MessageBox.Show("You found Redlands")
    End Sub
  End Class

