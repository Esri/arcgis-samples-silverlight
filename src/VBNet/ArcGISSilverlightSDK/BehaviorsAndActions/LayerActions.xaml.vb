Imports Microsoft.VisualBasic
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Actions
Imports ESRI.ArcGIS.Client


  Partial Public Class LayerActions
    Inherits UserControl
    Public Sub New()
      InitializeComponent()
        End Sub

        Private Sub EventTrigger_PreviewInvoke(ByVal sender As Object, ByVal e As System.Windows.Interactivity.PreviewInvokeEventArgs)
            Dim featureLayer As FeatureLayer = TryCast(MyMap.Layers("MyFeatureLayer"), FeatureLayer)
            If featureLayer.Where Is Nothing Then
                featureLayer.Where = "POP2000 > 1000"
            Else
                featureLayer.Where = Nothing
            End If
        End Sub

  End Class

