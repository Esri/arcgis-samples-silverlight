Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks
Imports ESRI.ArcGIS.Client.Geometry


    Partial Public Class LocationToAddress
        Inherits UserControl
        Private Shared _mercator As New ESRI.ArcGIS.Client.Projection.WebMercator()
        Private _locationGraphicsLayer As GraphicsLayer

        Public Sub New()
            InitializeComponent()

            Dim initialExtent As New ESRI.ArcGIS.Client.Geometry.Envelope(TryCast(_mercator.FromGeographic(New ESRI.ArcGIS.Client.Geometry.MapPoint(-117.387, 33.97)), MapPoint), TryCast(_mercator.FromGeographic(New ESRI.ArcGIS.Client.Geometry.MapPoint(-117.355, 33.988)), MapPoint))

            initialExtent.SpatialReference = New SpatialReference(102100)

            MyMap.Extent = initialExtent

            _locationGraphicsLayer = TryCast(MyMap.Layers("LocationGraphicsLayer"), GraphicsLayer)
        End Sub

        Private Sub MyMap_MouseClick(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Map.MouseEventArgs)
        Dim locatorTask As New Locator("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer")
            AddHandler locatorTask.LocationToAddressCompleted, AddressOf LocatorTask_LocationToAddressCompleted
            AddHandler locatorTask.Failed, AddressOf LocatorTask_Failed

            ' Tolerance (distance) specified in meters
            Dim tolerance As Double = 30
            locatorTask.LocationToAddressAsync(e.MapPoint, tolerance, e.MapPoint)
        End Sub

        Private Sub LocatorTask_LocationToAddressCompleted(ByVal sender As Object, ByVal args As AddressEventArgs)
            Dim address As Address = args.Address
            Dim attributes As Dictionary(Of String, Object) = address.Attributes

            Dim graphic As New Graphic() With {.Symbol = TryCast(LayoutRoot.Resources("DefaultMarkerSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol), .Geometry = TryCast(args.UserState, MapPoint)}

            Dim latlon As String = String.Format("{0}, {1}", address.Location.X, address.Location.Y)
        Dim address1 As String = attributes("Address").ToString()
        Dim address2 As String = String.Format("{0}, {1} {2}", attributes("City"), attributes("Region"), attributes("Postal"))

            graphic.Attributes.Add("LatLon", latlon)
            graphic.Attributes.Add("Address1", address1)
            graphic.Attributes.Add("Address2", address2)

            _locationGraphicsLayer.Graphics.Add(graphic)
        End Sub

        Private Sub LocatorTask_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
            MessageBox.Show("Unable to determine an address. Try selecting a location closer to a street.")
        End Sub

    Private Sub GraphicsLayer_MouseEnter(sender As Object, e As GraphicMouseEventArgs)
        e.Graphic.Select()
    End Sub

    Private Sub GraphicsLayer_MouseLeave(sender As Object, e As GraphicMouseEventArgs)
        e.Graphic.UnSelect()
    End Sub
End Class

