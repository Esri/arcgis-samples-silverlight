Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client

Partial Public Class RasterFunctionImageService
  Inherits UserControl
  Public Sub New()
    InitializeComponent()
  End Sub

  Private Sub ArcGISImageServiceLayer_Initialized(ByVal sender As Object, ByVal e As EventArgs)
    RasterFunctionsComboBox.ItemsSource = (TryCast(sender, ArcGISImageServiceLayer)).RasterFunctionInfos
    RasterFunctionsComboBox.SelectedIndex = 0
  End Sub

  Private Sub RasterFunctionsComboBox_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
    Dim imageLayer As ArcGISImageServiceLayer = TryCast(MyMap.Layers("MyImageLayer"), ArcGISImageServiceLayer)
    Dim rasterFunction = TryCast((TryCast(sender, ComboBox)).SelectedItem, RasterFunctionInfo)
    If rasterFunction IsNot Nothing Then
      Dim renderingRule As New RenderingRule() With {.RasterFunctionName = rasterFunction.Name}
      imageLayer.RenderingRule = renderingRule
      imageLayer.Refresh()
    End If
  End Sub
End Class
