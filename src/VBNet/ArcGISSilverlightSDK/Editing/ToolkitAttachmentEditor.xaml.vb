Imports System
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry


Partial Public Class ToolkitAttachmentEditor
  Inherits UserControl
  Private Shared _mercator As New ESRI.ArcGIS.Client.Projection.WebMercator()

  Public Sub New()
    InitializeComponent()

    Dim initialExtent As New ESRI.ArcGIS.Client.Geometry.Envelope(TryCast(_mercator.FromGeographic(New ESRI.ArcGIS.Client.Geometry.MapPoint(-122.4306073721, 37.7666097907)), MapPoint), TryCast(_mercator.FromGeographic(New ESRI.ArcGIS.Client.Geometry.MapPoint(-122.4230971868, 37.77197420877)), MapPoint))

    initialExtent.SpatialReference = New SpatialReference(102100)

    MyMap.Extent = initialExtent
  End Sub

  Private Sub FeatureLayer_MouseLeftButtonUp(ByVal sender As Object, ByVal e As GraphicMouseButtonEventArgs)
    Dim featureLayer As FeatureLayer = TryCast(sender, FeatureLayer)


    For Each g As Graphic In featureLayer.Graphics
      If (g.Selected) Then
        g.UnSelect()
      End If
    Next

    e.Graphic.Select()
    MyAttachmentEditor.GraphicSource = e.Graphic
  End Sub

  Private Sub MyAttachmentEditor_UploadFailed(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Toolkit.AttachmentEditor.UploadFailedEventArgs)
    MessageBox.Show(e.Result.Message)
  End Sub
End Class

