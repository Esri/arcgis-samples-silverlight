Imports System
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Tasks

Partial Public Class WorldGeocoding
    Inherits UserControl
    Private _locatorTask As Locator
    Private SearchOriginGraphicsLayer As GraphicsLayer
    Private FindResultLocationsGraphicsLayer As GraphicsLayer

    Public Sub New()
        InitializeComponent()

        ' Initialize Locator with ArcGIS Online World Geocoding Service.  See http://geocode.arcgis.com for details and doc.
        _locatorTask = New Locator("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer")
        AddHandler _locatorTask.FindCompleted, Sub(s, a)
                                                   ' When a Find operation is initiated, get the find results and add to a graphics layer for display in the map
                                                   Dim locatorFindResult As LocatorFindResult = a.Result
                                                   For Each location As Location In locatorFindResult.Locations
                                                       FindResultLocationsGraphicsLayer.Graphics.Add(location.Graphic)
                                                   Next location
                                               End Sub
        AddHandler _locatorTask.Failed, Sub(s, e) MessageBox.Show("Locator service failed: " & e.Error.ToString())

        FindResultLocationsGraphicsLayer = TryCast(MyMap.Layers("FindResultLocationsGraphicsLayer"), GraphicsLayer)
        SearchOriginGraphicsLayer = TryCast(MyMap.Layers("SearchOriginGraphicsLayer"), GraphicsLayer)

    End Sub

    Private Sub FindButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        FindResultLocationsGraphicsLayer.Graphics.Clear()
        SearchOriginGraphicsLayer.Graphics.Clear()

        ' If locator already processing a request, cancel it.  Note, the request is not cancelled on the server.   
        If _locatorTask.IsBusy Then
            _locatorTask.CancelAsync()
        End If

        ' If search text is empty, return
        If String.IsNullOrEmpty(SearchTextBox.Text) Then
            Return
        End If

        ' Search will return results based on a start location.  In this sample, the location is the center of the map.  
        ' Add a graphic when a search is initiated to determine what location was used to search from and rank results.
        SearchOriginGraphicsLayer.Graphics.Add(New Graphic() With {.Geometry = MyMap.Extent.GetCenter()})

        ' In this sample, the center of the map is used as the location from which results will be ranked and distance calculated. 
        ' The distance from the location is optional.  Specifies the radius of an area around a point location which is used to boost
        ' the rank of geocoding candidates so that candidates closest to the location are returned first. The distance value is in meters. 
        Dim locatorFindParams As New LocatorFindParameters() With {.Text = SearchTextBox.Text, .Location = MyMap.Extent.GetCenter(), .Distance = MyMap.Extent.Width \ 2, .MaxLocations = 5, .OutSpatialReference = MyMap.SpatialReference}
        locatorFindParams.OutFields.AddRange(New String() {"PlaceName", "City", "Region", "Country", "Score", "Distance", "Type"})

        _locatorTask.FindAsync(locatorFindParams)
    End Sub

    Private Sub GraphicsLayer_MouseEnter(sender As Object, e As GraphicMouseEventArgs)
        e.Graphic.Select()
    End Sub

    Private Sub GraphicsLayer_MouseLeave(sender As Object, e As GraphicMouseEventArgs)
        e.Graphic.UnSelect()
    End Sub
End Class