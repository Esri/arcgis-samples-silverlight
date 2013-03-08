Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry


    Partial Public Class ToolkitFeatureDataGrid
        Inherits UserControl
        Private _lastGraphic As Graphic
        Private Shared _mercator As New ESRI.ArcGIS.Client.Projection.WebMercator()

        Public Sub New()
            InitializeComponent()

            Dim initialExtent As New ESRI.ArcGIS.Client.Geometry.Envelope(TryCast(_mercator.FromGeographic(New ESRI.ArcGIS.Client.Geometry.MapPoint(-122.4545596, 37.783443296)), MapPoint), TryCast(_mercator.FromGeographic(New ESRI.ArcGIS.Client.Geometry.MapPoint(-122.4449924, 37.786447331)), MapPoint))

            initialExtent.SpatialReference = New SpatialReference(102100)

            MyMap.Extent = initialExtent
        End Sub

        Private Sub FeatureLayer_MouseLeftButtonUp(ByVal sender As Object, ByVal e As GraphicMouseButtonEventArgs)
            If _lastGraphic IsNot Nothing Then
                _lastGraphic.UnSelect()
            End If

            e.Graphic.Select()
            If e.Graphic.Selected Then
                MyDataGrid.ScrollIntoView(e.Graphic, Nothing)
            End If

            _lastGraphic = e.Graphic
        End Sub
    End Class

