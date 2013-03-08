Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Toolkit.Primitives
Imports ESRI.ArcGIS.Client.Toolkit

Partial Public Class GroupLayers
  Inherits UserControl
  Public Sub New()
    InitializeComponent()
  End Sub

  Private Sub Legend_Refreshed(ByVal sender As Object, ByVal e As Legend.RefreshedEventArgs)
    If e.LayerItem.LayerItems IsNot Nothing Then
      For Each layerItemVM As LayerItemViewModel In e.LayerItem.LayerItems
        If layerItemVM.IsExpanded Then
          layerItemVM.IsExpanded = False
        End If
      Next layerItemVM
    End If
  End Sub
End Class

