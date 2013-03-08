Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports ESRI.ArcGIS.Client.Toolkit
Imports ESRI.ArcGIS.Client.Geometry


    Partial Public Class ToolkitTemplatePicker
        Inherits UserControl
        Private Shared _mercator As New ESRI.ArcGIS.Client.Projection.WebMercator()

        Public Sub New()
            InitializeComponent()

            Dim initialExtent As New ESRI.ArcGIS.Client.Geometry.Envelope(TryCast(_mercator.FromGeographic(New ESRI.ArcGIS.Client.Geometry.MapPoint(-117.6690936441, 34.19871558256)), MapPoint), TryCast(_mercator.FromGeographic(New ESRI.ArcGIS.Client.Geometry.MapPoint(-117.411944901, 34.37896002836)), MapPoint))

            initialExtent.SpatialReference = New SpatialReference(102100)

            MyMap.Extent = initialExtent
        End Sub

        Private Sub MyTemplatePicker_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Dim myLayerIDs() As String = {"Points of Interest", "Evacuation Perimeter"}
            MyTemplatePicker.LayerIDs = myLayerIDs
        End Sub
    End Class

