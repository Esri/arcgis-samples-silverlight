Imports Microsoft.VisualBasic
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client


  Partial Public Class SDSRenderersXAML
    Inherits UserControl
    Public Sub New()
      InitializeComponent()
    End Sub

    Private Sub FeatureLayer_UpdateCompleted(ByVal sender As Object, ByVal e As System.EventArgs)
      MyMap.ZoomTo((TryCast(sender, FeatureLayer)).FullExtent)
    End Sub

  End Class

