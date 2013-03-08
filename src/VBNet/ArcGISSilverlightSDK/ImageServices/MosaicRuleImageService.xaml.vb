Imports Microsoft.VisualBasic
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports System
Imports System.Windows


  Partial Public Class MosaicRuleImageService
    Inherits UserControl
    Public Sub New()
      InitializeComponent()
    End Sub

    Private Sub MyMap_MouseClick(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Map.MouseEventArgs)
      Try
        Dim imageLayer As ArcGISImageServiceLayer = TryCast(MyMap.Layers("ImageServiceLayer"), ArcGISImageServiceLayer)
        Dim mosaicRule As New MosaicRule()
        mosaicRule.MosaicMethod = "esriMosaicViewpoint"
        mosaicRule.Viewpoint = e.MapPoint
        imageLayer.MosaicRule = mosaicRule
        imageLayer.Refresh()
      Catch ex As Exception
        MessageBox.Show(ex.Message)
      End Try
    End Sub
  End Class

