Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client


Partial Public Class DynamicLayerLabeling
    Inherits UserControl
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub cboPlacement_SelectionChanged_1(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
        If MyMap Is Nothing Then
            Return
        End If

        Dim dynamicLayer As ArcGISDynamicMapServiceLayer = TryCast(MyMap.Layers("PopulationDynamicLayer"), ArcGISDynamicMapServiceLayer)
        Dim placment As New LabelPlacement()

        Select Case cboPlacement.SelectedIndex
            Case 0
                placment = LabelPlacement.PointLabelPlacementAboveCenter
            Case 1
                placment = LabelPlacement.PointLabelPlacementAboveLeft
            Case 2
                placment = LabelPlacement.PointLabelPlacementAboveRight
            Case 3
                placment = LabelPlacement.PointLabelPlacementBelowCenter
            Case 4
                placment = LabelPlacement.PointLabelPlacementBelowLeft
            Case 5
                placment = LabelPlacement.PointLabelPlacementBelowRight
            Case 6
                placment = LabelPlacement.PointLabelPlacementCenterCenter
            Case 7
                placment = LabelPlacement.PointLabelPlacementCenterLeft
            Case 8
                placment = LabelPlacement.PointLabelPlacementCenterRight
        End Select

        For Each lClass As LabelClass In dynamicLayer.LayerDrawingOptions(0).LabelClasses
            lClass.LabelPlacement = placment
        Next lClass

        dynamicLayer.Refresh()
    End Sub
End Class

