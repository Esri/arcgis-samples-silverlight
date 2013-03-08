Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Toolkit.Primitives


Partial Public Class LegendWithTemplates
    Inherits UserControl

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub Legend_Refreshed(ByVal sender As System.Object, ByVal e As ESRI.ArcGIS.Client.Toolkit.Legend.RefreshedEventArgs)
        Dim removeLayerItemVM As LayerItemViewModel = Nothing
        'If a map layer has sublayers, iterate through them.
        If e.LayerItem.LayerItems IsNot Nothing Then
            'Iterate through all the sublayer items.
            For Each layerItemVM As LayerItemViewModel In e.LayerItem.LayerItems
                'Collapse all sublayers in the legend.
                If layerItemVM.IsExpanded Then
                    layerItemVM.IsExpanded = False
                End If
                'Remove the sublayer named "states" from the legend.  The layer remains visible in the map.
                If layerItemVM.Label = "states" Then
                    removeLayerItemVM = layerItemVM
                End If
            Next layerItemVM

            If removeLayerItemVM IsNot Nothing Then
                e.LayerItem.LayerItems.Remove(removeLayerItemVM)
            End If
        Else
            'Collapse all map layers in the legend.
            e.LayerItem.IsExpanded = False
        End If

    End Sub
End Class

