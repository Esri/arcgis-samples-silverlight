Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls

Partial Public Class FeatureLayerSelection
  Inherits UserControl
  Private Shared mercator As ESRI.ArcGIS.Client.Projection.WebMercator = New ESRI.ArcGIS.Client.Projection.WebMercator()

  Public Sub New()
    InitializeComponent()

    Dim geometry1 As ESRI.ArcGIS.Client.Geometry.Geometry = mercator.FromGeographic(New ESRI.ArcGIS.Client.Geometry.MapPoint(-117.190346717, 34.0514888762))
    Dim mapPoint1 As ESRI.ArcGIS.Client.Geometry.MapPoint = TryCast(geometry1, ESRI.ArcGIS.Client.Geometry.MapPoint)

    Dim geometry2 As ESRI.ArcGIS.Client.Geometry.Geometry = mercator.FromGeographic(New ESRI.ArcGIS.Client.Geometry.MapPoint(-117.160305976, 34.072946548))
    Dim mapPoint2 As ESRI.ArcGIS.Client.Geometry.MapPoint = TryCast(geometry2, ESRI.ArcGIS.Client.Geometry.MapPoint)

    Dim initialExtent As ESRI.ArcGIS.Client.Geometry.Envelope = New ESRI.ArcGIS.Client.Geometry.Envelope(mapPoint1, mapPoint2)
    initialExtent.SpatialReference = New ESRI.ArcGIS.Client.Geometry.SpatialReference(102100)

    MyMap.Extent = initialExtent

  End Sub

End Class

