Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Portal
Imports ESRI.ArcGIS.Client.WebMap

Partial Public Class BasemapSwitcher
    Inherits UserControl
    Private _map As ESRI.ArcGIS.Client.Map

    Public Sub New()
        InitializeComponent()
        LoadWebMap()
        LoadPortalBasemaps()
    End Sub
    Private Sub LoadWebMap()
        ' Downloads a web map from ArcGIS Online that contains a basemap and operational layer (federal land holdings)
        Dim doc As New Document()
        AddHandler doc.GetMapCompleted, Sub(s, e)
                                            _map = e.Map
                                            Grid.SetColumn(_map, 1)
                                            LayoutRoot.Children.Insert(0, _map)
                                        End Sub
        doc.GetMapAsync("3679c136c2694d0b95bb5e6c3f2b480e")
    End Sub

    Private Sub LoadPortalBasemaps()
        ' Initialize a portal to get the query to return web maps in the basemap gallery
        Dim portal As New ArcGISPortal()
        portal.InitializeAsync("http://www.arcgis.com/sharing/rest",
                               Sub(s, e)
                                   If portal.ArcGISPortalInfo IsNot Nothing Then
                                       ' Return the first 20 basemaps in the gallery
                                       Dim parameters As New SearchParameters() With {.Limit = 20}
                                       ' Use the advertised query to search the basemap gallery and return web maps as portal items
                                       portal.ArcGISPortalInfo.SearchBasemapGalleryAsync(parameters,
                                      Sub(result, err)
                                          If err Is Nothing AndAlso result IsNot Nothing Then
                                              basemapList.ItemsSource = result.Results
                                          End If
                                      End Sub)
                                   End If
                               End Sub)
    End Sub

    Private Sub BaseMapButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim item = TryCast((TryCast(sender, Button)).DataContext, ArcGISPortalItem)
        'Create a map instance from the webmap
        ' Remove basemap layers in current map control
        ' Get the basemap layers from the result map control from the call to GetMapAsync
        ' To add the basemap layer retrieved in the previous line to the current map control, remove them from 
        ' the result map control
        ' Use an index to determine where to insert basemap reference layers
        ' If basemap contains a Bing tile layer and no Bing key is available, skip adding the basemap layer.
        ' Returns json definition for the layer
        ' Reference layers go on top of other layers (e.g. labels)
        ' Basemap layers go below all other layers

        ESRI.ArcGIS.Client.WebMap.WebMap.FromPortalItemAsync(item,
                                                             Sub(webmap, err)
                                                                 Dim basemaps = webmap.BaseMap
                                                                 'Create a map instance from the webmap
                                                                 Dim doc As New Document() With
                                                                     {.BingToken = ""} 'Required if you want to load bing basemaps
                                                                 AddHandler doc.GetMapCompleted,
                                                                     Sub(s, result)
                                                                         'Remove basemap layers in current map control
                                                                         For Each layer In _map.Layers.ToList()
                                                                             If Document.GetIsBaseMap(layer) Then
                                                                                 _map.Layers.Remove(layer)
                                                                             End If
                                                                         Next layer
                                                                         'Get the basemap layers from the result map control from the call to GetMapAsync
                                                                         Dim newBaseLayers = result.Map.Layers.Where(Function(layer) Document.GetIsBaseMap(layer)).ToList()
                                                                         'To add the basemap layer retrieved in the previous line to the current map control, remove them from
                                                                         'the result map control
                                                                         result.Map.Layers.Clear()
                                                                         'Use an index to determine where to insert basemap reference layers
                                                                         Dim idx As Integer = 0
                                                                         For Each layer In newBaseLayers
                                                                             'If basemap contains a Bing tile layer and no Bing key is available, skip adding the basemap layer.
                                                                             If TypeOf layer Is ESRI.ArcGIS.Client.Bing.TileLayer AndAlso String.IsNullOrEmpty(doc.BingToken) Then
                                                                                 MessageBox.Show("Bing key not available. Bing layer cannot be used as a basemap.")
                                                                                 Exit For
                                                                             End If
                                                                             'Returns json definition for the layer
                                                                             Dim data = TryCast(layer.GetValue(Document.WebMapDataProperty), IDictionary(Of String, Object))
                                                                             'Reference layers go on top of other layers (e.g. labels)
                                                                             If data.ContainsKey("isReference") Then
                                                                                 _map.Layers.Add(layer)
                                                                             Else
                                                                                 'Basemap layers go below all other layers
                                                                                 _map.Layers.Insert(idx, layer)
                                                                                 idx += 1
                                                                             End If
                                                                         Next layer
                                                                     End Sub
                                                                 doc.GetMapAsync(webmap)
                                                             End Sub)
    End Sub
End Class