Imports System.Windows.Controls


Partial Public Class WebTiledLayers
    Inherits UserControl
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub cboLayers_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
        If cboLayers Is Nothing Then
            Return
        End If

        Dim webTiledLayer As ESRI.ArcGIS.Client.Toolkit.DataSources.WebTiledLayer = TryCast(MyMap.Layers("MyWebTiledLayer"), ESRI.ArcGIS.Client.Toolkit.DataSources.WebTiledLayer)

        Select Case cboLayers.SelectedIndex
            'Esri National Geographic
            Case 0
                webTiledLayer.TemplateUrl = "http://{subDomain}.arcgisonline.com/ArcGIS/rest/services/NatGeo_World_Map/MapServer/tile/{level}/{row}/{col}"
                webTiledLayer.SubDomains = New String() {"server", "services"}
                webTiledLayer.CopyrightText = "National Geographic, Esri, DeLorme, NAVTEQ, UNEP-WCMC, USGS, NASA, ESA, METI, NRCAN, GEBCO, NOAA, iPC"
                'MapQuest
            Case 1
                webTiledLayer.TemplateUrl = "http://mtile01.mqcdn.com/tiles/1.0.0/vx/map/{level}/{col}/{row}.jpg"
                webTiledLayer.CopyrightText = "Map Quest"
                'OpenCycleMap
            Case 2
                webTiledLayer.TemplateUrl = "http://{subDomain}.tile.opencyclemap.org/cycle/{level}/{col}/{row}.png"
                webTiledLayer.SubDomains = New String() {"a", "b", "c"}
                webTiledLayer.CopyrightText = "Open Cycle Map"
                'Cloudmade Midnight Commander
            Case 3
                webTiledLayer.TemplateUrl = "http://{subDomain}.tile.cloudmade.com/1a1b06b230af4efdbb989ea99e9841af/999/256/{level}/{col}/{row}.png"
                webTiledLayer.SubDomains = New String() {"a", "b", "c"}
                webTiledLayer.CopyrightText = "Cloudmade Midnight Commander"
                'Cloudmade Pale Dawn
            Case 4
                webTiledLayer.TemplateUrl = "http://{subDomain}.tile.cloudmade.com/1a1b06b230af4efdbb989ea99e9841af/998/256/{level}/{col}/{row}.png"
                webTiledLayer.SubDomains = New String() {"a", "b", "c"}
                webTiledLayer.CopyrightText = "Cloudmade Pale Dawn"
                'MapBox Dark
            Case 5
                webTiledLayer.TemplateUrl = "http://{subDomain}.tiles.mapbox.com/v3/examples.map-cnkhv76j/{level}/{col}/{row}.png"
                webTiledLayer.SubDomains = New String() {"a", "b", "c", "d"}
                webTiledLayer.CopyrightText = "Mapbox Dark"
                'Mapbox Streets
            Case 6
                webTiledLayer.TemplateUrl = "http://{subDomain}.tiles.mapbox.com/v3/examples.map-vyofok3q/{level}/{col}/{row}.png"
                webTiledLayer.SubDomains = New String() {"a", "b", "c", "d"}
                webTiledLayer.CopyrightText = "Mapbox Streets"
                'Mapbox Terrain
            Case 7
                webTiledLayer.TemplateUrl = "http://{subDomain}.tiles.mapbox.com/v3/mapbox.mapbox-warden/{level}/{col}/{row}.png"
                webTiledLayer.SubDomains = New String() {"a", "b", "c", "d"}
                webTiledLayer.CopyrightText = "Mapbox Terrain"
                'Apple's OpenStreetMap
            Case 8
                webTiledLayer.TemplateUrl = "http://gsp2.apple.com/tile?api=1&style=slideshow&layers=default&lang=en_GB&z={level}&x={col}&y={row}&v=9"
                webTiledLayer.CopyrightText = "Apple's rendering of OSM data."
                'Stamen Terrain
            Case 9
                webTiledLayer.TemplateUrl = "http://{subDomain}.tile.stamen.com/terrain/{level}/{col}/{row}.jpg"
                webTiledLayer.SubDomains = New String() {"a", "b", "c", "d"}
                webTiledLayer.CopyrightText = "Stamen Terrain"
                'Stamen Watercolor
            Case 10
                webTiledLayer.TemplateUrl = "http://{subDomain}.tile.stamen.com/watercolor/{level}/{col}/{row}.jpg"
                webTiledLayer.SubDomains = New String() {"a", "b", "c", "d"}
                webTiledLayer.CopyrightText = "Stamen Watercolor"
                'Stamen Toner
            Case 11
                webTiledLayer.TemplateUrl = "http://{subDomain}.tile.stamen.com/toner/{level}/{col}/{row}.png"
                webTiledLayer.SubDomains = New String() {"a", "b", "c", "d"}
                webTiledLayer.CopyrightText = "Stamen Toner"
        End Select

        webTiledLayer.Refresh()
    End Sub
End Class

