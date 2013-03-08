Imports Microsoft.VisualBasic
Imports System.Windows.Controls
Imports System.Windows.Input
Imports ESRI.ArcGIS.Client


  Partial Public Class FeatureDataGrid
    Inherits UserControl
    Public Sub New()
      InitializeComponent()
    End Sub

    Private Sub FeatureLayer_MouseLeftButtonDown(ByVal sender As Object, ByVal args As GraphicMouseButtonEventArgs)
      args.Graphic.Selected = Not args.Graphic.Selected
      If args.Graphic.Selected Then
        MyDataGrid.ScrollIntoView(args.Graphic, Nothing)
      End If
    End Sub
  End Class

