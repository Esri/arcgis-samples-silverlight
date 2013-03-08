Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Toolkit.DataSources
Imports System


Partial Public Class HeatMapLayerSimple
  Inherits UserControl
  Public Sub New()
    InitializeComponent()
    AddHandler MyMap.Layers.LayersInitialized, AddressOf HeatMapLayer_Initialized
  End Sub

  Private Sub HeatMapLayer_Initialized(ByVal sender As Object, ByVal e As EventArgs)
    'Add 1000 random points to the heat map layer
    'Replace this with "real" data points that you want to display
    'in the heat map.
    Dim layer As HeatMapLayer = TryCast(MyMap.Layers("RandomHeatMapLayer"), HeatMapLayer)
    Dim rand As New Random()
    For i As Integer = 0 To 999
      Dim x As Double = rand.NextDouble() * MyMap.Extent.Width - MyMap.Extent.Width / 2
      Dim y As Double = rand.NextDouble() * MyMap.Extent.Height - MyMap.Extent.Height / 2
      layer.HeatMapPoints.Add(New ESRI.ArcGIS.Client.Geometry.MapPoint(x, y))
    Next i
  End Sub

End Class

