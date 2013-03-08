Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data


    Partial Public Class SubLayerList
        Inherits UserControl
        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub CheckBox_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Dim tickedCheckBox As CheckBox = TryCast(sender, CheckBox)

            Dim serviceName As String = tickedCheckBox.Name
            Dim visible As Boolean = CBool(tickedCheckBox.IsChecked)

            Dim layerIndex As Integer = CInt(Fix(tickedCheckBox.Tag))

            Dim dynamicServiceLayer As ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer = TryCast(MyMap.Layers(serviceName), ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer)

            Dim visibleLayerList As List(Of Integer) = If(dynamicServiceLayer.VisibleLayers IsNot Nothing, dynamicServiceLayer.VisibleLayers.ToList(), New List(Of Integer)())

            If visible Then
                If (Not visibleLayerList.Contains(layerIndex)) Then
                    visibleLayerList.Add(layerIndex)
                End If
            Else
                If visibleLayerList.Contains(layerIndex) Then
                    visibleLayerList.Remove(layerIndex)
                End If
            End If

            dynamicServiceLayer.VisibleLayers = visibleLayerList.ToArray()

        End Sub

        Private Sub ArcGISDynamicMapServiceLayer_Initialized(ByVal sender As Object, ByVal e As EventArgs)
            Dim dynamicServiceLayer As ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer = TryCast(sender, ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer)
            If dynamicServiceLayer.VisibleLayers Is Nothing Then
                dynamicServiceLayer.VisibleLayers = GetDefaultVisibleLayers(dynamicServiceLayer)
            End If
        End Sub

        Private Function GetDefaultVisibleLayers(ByVal dynamicService As ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer) As Integer()
            Dim visibleLayerIDList As New List(Of Integer)()

            Dim layerInfoArray() As ESRI.ArcGIS.Client.LayerInfo = dynamicService.Layers

            For index As Integer = 0 To layerInfoArray.Length - 1
                If layerInfoArray(index).DefaultVisibility Then
                    visibleLayerIDList.Add(index)
                End If
            Next index
            Return visibleLayerIDList.ToArray()
        End Function
    End Class

