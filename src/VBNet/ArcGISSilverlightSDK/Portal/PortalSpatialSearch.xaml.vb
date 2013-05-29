'INSTANT VB NOTE: This code snippet uses implicit typing. You will need to set 'Option Infer On' in the VB file or set 'Option Infer' at the project level.

Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Portal
Imports System.Linq

Partial Public Class PortalSpatialSearch
    Inherits UserControl
    Private arcgisPortal As ArcGISPortal
    Private mercator As New ESRI.ArcGIS.Client.Projection.WebMercator()

    Private webmapGraphicsLayer As GraphicsLayer

    Public Sub New()
        InitializeComponent()

        ' Search public web maps on www.arcgis.com
        arcgisPortal = New ArcGISPortal() With {.Url = "http://www.arcgis.com/sharing/rest"}
        webmapGraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
    End Sub

    Private Sub FindWebMapsButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        webmapGraphicsLayer.Graphics.Clear()

        ' Search envelope must be in geographic (WGS84).  Convert the current map extent from Web Mercator
        ' to geographic. 
        Dim geom As ESRI.ArcGIS.Client.Geometry.Geometry = mercator.ToGeographic(MyMap.Extent)

        Dim parameters As SpatialSearchParameters = New SpatialSearchParameters With {.Limit = If(String.IsNullOrEmpty(resultLimit.Text) = True, 15, Convert.ToInt32(resultLimit.Text)), .SearchExtent = geom.Extent, .QueryString = String.Format("{0} And type:Web Map", searchText.Text)}

        ' Set the ItemsSource for the Listbox to an IEnumerable of ArcGISPortalItems.  
        ' Bindings setup in the Listbox item template in XAML will enable the discovery and 
        ' display individual result items.  
        ' For each web map returned, add the center (point) of the web map extent as a graphic.
        ' Add the ArcGISPortalItem instance as an attribute.  This will be used to select graphics
        ' in the map.  
        arcgisPortal.SearchItemsAsync(parameters, Sub(result, err)
                                                               If err Is Nothing Then
                                                                   WebMapsListBox.ItemsSource = result.Results
                                                                   For Each item In result.Results
                                                                       Dim graphic As New Graphic()
                                                                       graphic.Attributes.Add("PortalItem", item)
                                                                       Dim extentCenter As MapPoint = item.Extent.GetCenter()
                                                                       graphic.Geometry = New MapPoint(extentCenter.X, extentCenter.Y, New SpatialReference(4326))
                                                                       webmapGraphicsLayer.Graphics.Add(graphic)
                                                                   Next item
                                                               End If
                                                           End Sub)
		End Sub

    ' Click on a graphic in the map and select it.  Scroll to the web map item in the Listbox. 
    Private Sub GraphicsLayer_MouseLeftButtonUp(ByVal sender As Object, ByVal e As GraphicMouseButtonEventArgs)
        webmapGraphicsLayer.ClearSelection()
        Dim graphic As Graphic = e.Graphic
        graphic.Selected = True
        Dim portalitem As ArcGISPortalItem = TryCast(graphic.Attributes("PortalItem"), ArcGISPortalItem)
        WebMapsListBox.SelectedItem = portalitem
        WebMapsListBox.ScrollIntoView(portalitem)
    End Sub

    ' Click on a web map item in the Listbox, zoom to and select the respective graphic in the map. 
    Private Sub Image_MouseLeftButtonUp(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        webmapGraphicsLayer.ClearSelection()
        Dim portalItem As ArcGISPortalItem = TryCast((TryCast(sender, Image)).DataContext, ArcGISPortalItem)
        If portalItem.Extent IsNot Nothing Then
            MyMap.ZoomTo(mercator.FromGeographic(portalItem.Extent))
        End If

        ' Use LINQ to select the graphic where the portal item attribute equals the portal item instance
        ' in the Listbox.
        Dim queryPortalItemGraphics = From g In webmapGraphicsLayer.Graphics
                                      Where g.Attributes("PortalItem") Is portalItem
                                      Select g

        ' Get the first graphic in the IEnumerable of graphics and set it selected.
        For Each graphic In queryPortalItemGraphics
            graphic.Selected = True
            Return
        Next graphic
    End Sub

    ' Listen for when the FullExtent property changes for the graphics layer that stores web map extent center points.
    ' When FullExtent changes (e.g. new search results are returned) zoom to an extent that contains all the results.  
    Private Sub GraphicsLayer_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        If (e.PropertyName = "FullExtent") AndAlso ((TryCast(sender, GraphicsLayer)).FullExtent IsNot Nothing) Then
            MyMap.ZoomTo((TryCast(sender, GraphicsLayer)).FullExtent)
        End If
    End Sub
End Class