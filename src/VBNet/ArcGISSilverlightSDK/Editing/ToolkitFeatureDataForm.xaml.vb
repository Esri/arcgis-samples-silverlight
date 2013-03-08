Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry


Partial Public Class ToolkitFeatureDataForm
  Inherits UserControl
  Private Shared _mercator As New ESRI.ArcGIS.Client.Projection.WebMercator()

  Public Sub New()
    InitializeComponent()

    Dim initialExtent As New ESRI.ArcGIS.Client.Geometry.Envelope(TryCast(_mercator.FromGeographic(New ESRI.ArcGIS.Client.Geometry.MapPoint(-122.4545596, 37.783443296)), MapPoint), TryCast(_mercator.FromGeographic(New ESRI.ArcGIS.Client.Geometry.MapPoint(-122.4449924, 37.786447331)), MapPoint))

    initialExtent.SpatialReference = New SpatialReference(102100)

    MyMap.Extent = initialExtent
  End Sub

  Private Sub FeatureLayer_MouseLeftButtonUp(ByVal sender As Object, ByVal args As GraphicMouseButtonEventArgs)
    Dim featureLayer As FeatureLayer = TryCast(sender, FeatureLayer)

    For Each g As Graphic In featureLayer.Graphics
      If (g.Selected) Then
        g.UnSelect()
      End If
    Next

    args.Graphic.Select()
    MyFeatureDataForm.GraphicSource = args.Graphic

    FeatureDataFormBorder.Visibility = Visibility.Visible
  End Sub
End Class

