Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Toolkit.DataSources

Partial Public Class OSMTileServers
    Inherits UserControl
    Private osmLayer As OpenStreetMapLayer
    Public Sub New()
        InitializeComponent()
        osmLayer = TryCast(MyMap.Layers("OSMLayer"), OpenStreetMapLayer)
    End Sub
    Private Sub RadioButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim layerTypeTag As String = CStr((CType(sender, RadioButton)).Tag)
        Dim newTileServers As OpenStreetMapLayer.TileServerList

        Select Case layerTypeTag
            Case "MapQuest"
                'Collection is replaced
                newTileServers = New OpenStreetMapLayer.TileServerList()
                newTileServers.Add("http://otile1.mqcdn.com/tiles/1.0.0/osm")
                newTileServers.Add("http://otile2.mqcdn.com/tiles/1.0.0/osm")
                newTileServers.Add("http://otile3.mqcdn.com/tiles/1.0.0/osm")
                osmLayer.TileServers = newTileServers
            Case "Cloudmade"
                'Collection is replaced
                newTileServers = New OpenStreetMapLayer.TileServerList()
                newTileServers.Add("http://a.tile.cloudmade.com/fd093e52f0965d46bb1c6c6281022199/3/256")
                newTileServers.Add("http://b.tile.cloudmade.com/fd093e52f0965d46bb1c6c6281022199/3/256")
                newTileServers.Add("http://c.tile.cloudmade.com/fd093e52f0965d46bb1c6c6281022199/3/256")
                osmLayer.TileServers = newTileServers
            Case "CycleMap"
                'same collection is pre-populated and hence needs to be refreshed.
                osmLayer.TileServers.Clear()
                osmLayer.TileServers.Add("http://a.tile.opencyclemap.org/cycle")
                osmLayer.TileServers.Add("http://b.tile.opencyclemap.org/cycle")
                osmLayer.TileServers.Add("http://c.tile.opencyclemap.org/cycle")
                osmLayer.Refresh()
        End Select
    End Sub
End Class