Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Toolkit.DataSources
Imports System.Windows


Partial Public Class OpenStreetMapSimple
  Inherits UserControl
  Public Sub New()
    InitializeComponent()
  End Sub

  Private Sub RadioButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    Dim osmLayer As OpenStreetMapLayer = TryCast(MyMap.Layers("OSMLayer"), OpenStreetMapLayer)
    Dim layerTypeTag As String = CStr((CType(sender, RadioButton)).Tag)
    Dim newLayerType As OpenStreetMapLayer.MapStyle = CType(System.Enum.Parse(GetType(OpenStreetMapLayer.MapStyle), layerTypeTag, True), OpenStreetMapLayer.MapStyle)
    osmLayer.Style = newLayerType
  End Sub
End Class

