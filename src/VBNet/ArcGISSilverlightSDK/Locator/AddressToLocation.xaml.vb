Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Tasks

Partial Public Class AddressToLocation
    Inherits UserControl
    Private _locatorTask As Locator
    Private _candidateGraphicsLayer As GraphicsLayer
    Private Shared _mercator As New ESRI.ArcGIS.Client.Projection.WebMercator()

    Public Sub New()
        InitializeComponent()

        Dim initialExtent As New ESRI.ArcGIS.Client.Geometry.Envelope(TryCast(_mercator.FromGeographic(New ESRI.ArcGIS.Client.Geometry.MapPoint(-122.554, 37.615)), MapPoint), TryCast(_mercator.FromGeographic(New ESRI.ArcGIS.Client.Geometry.MapPoint(-122.245, 37.884)), MapPoint))

        initialExtent.SpatialReference = New SpatialReference(102100)

        MyMap.Extent = initialExtent

        _candidateGraphicsLayer = TryCast(MyMap.Layers("CandidateGraphicsLayer"), GraphicsLayer)
    End Sub

    Private Sub FindAddressButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        _locatorTask = New Locator("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer")
        AddHandler _locatorTask.AddressToLocationsCompleted, AddressOf LocatorTask_AddressToLocationsCompleted
        AddHandler _locatorTask.Failed, AddressOf LocatorTask_Failed

        Dim addressParams As New AddressToLocationsParameters() With {.OutSpatialReference = MyMap.SpatialReference}

        Dim address As Dictionary(Of String, String) = addressParams.Address

        If Not String.IsNullOrEmpty(InputAddress.Text) Then
            address.Add("Address", InputAddress.Text)
        End If
        If Not String.IsNullOrEmpty(City.Text) Then
            address.Add("City", City.Text)
        End If
        If Not String.IsNullOrEmpty(State.Text) Then
            address.Add("Region", State.Text)
        End If
        If Not String.IsNullOrEmpty(Zip.Text) Then
            address.Add("Postal", Zip.Text)
        End If

        _locatorTask.AddressToLocationsAsync(addressParams)
    End Sub

    Private Sub LocatorTask_AddressToLocationsCompleted(ByVal sender As Object, ByVal args As ESRI.ArcGIS.Client.Tasks.AddressToLocationsEventArgs)
        _candidateGraphicsLayer.Graphics.Clear()
        CandidateListBox.Items.Clear()

        Dim returnedCandidates As List(Of AddressCandidate) = args.Results

        For Each candidate As AddressCandidate In returnedCandidates
            If candidate.Score >= 80 Then
                CandidateListBox.Items.Add(candidate.Address)

                Dim graphic As New Graphic() With {.Symbol = TryCast(LayoutRoot.Resources("DefaultMarkerSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol), .Geometry = candidate.Location}

                graphic.Attributes.Add("Address", candidate.Address)

                Dim latlon As String = String.Format("{0}, {1}", candidate.Location.X, candidate.Location.Y)
                graphic.Attributes.Add("LatLon", latlon)

                If candidate.Location.SpatialReference Is Nothing Then
                    candidate.Location.SpatialReference = New SpatialReference(4326)
                End If

                If Not candidate.Location.SpatialReference.Equals(MyMap.SpatialReference) Then
                    If MyMap.SpatialReference.Equals(New SpatialReference(102100)) AndAlso candidate.Location.SpatialReference.Equals(New SpatialReference(4326)) Then
                        graphic.Geometry = _mercator.FromGeographic(graphic.Geometry)
                    ElseIf MyMap.SpatialReference.Equals(New SpatialReference(4326)) AndAlso candidate.Location.SpatialReference.Equals(New SpatialReference(102100)) Then
                        graphic.Geometry = _mercator.ToGeographic(graphic.Geometry)
                    ElseIf Not MyMap.SpatialReference.Equals(New SpatialReference(4326)) Then
                        Dim geometryService As New GeometryService("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")

                        AddHandler geometryService.ProjectCompleted, Sub(s, a) graphic.Geometry = a.Results(0).Geometry

                        AddHandler geometryService.Failed, Sub(s, a) MessageBox.Show("Projection error: " & a.Error.Message)

                        geometryService.ProjectAsync(New List(Of Graphic) From {graphic}, MyMap.SpatialReference)
                    End If
                End If

                _candidateGraphicsLayer.Graphics.Add(graphic)
            End If
        Next candidate

        If _candidateGraphicsLayer.Graphics.Count > 0 Then
            CandidatePanelGrid.Visibility = Visibility.Visible
            CandidateListBox.SelectedIndex = 0
        End If
    End Sub

    Private Sub _candidateListBox_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
        Dim index As Integer = (TryCast(sender, ListBox)).SelectedIndex
        If index >= 0 Then
            Dim candidatePoint As MapPoint = TryCast(_candidateGraphicsLayer.Graphics(index).Geometry, MapPoint)
            Dim displaySize As Double = MyMap.MinimumResolution * 30

            Dim displayExtent As New ESRI.ArcGIS.Client.Geometry.Envelope(candidatePoint.X - (displaySize / 2), candidatePoint.Y - (displaySize / 2), candidatePoint.X + (displaySize / 2), candidatePoint.Y + (displaySize / 2))

            MyMap.ZoomTo(displayExtent)
        End If
    End Sub

    Private Sub LocatorTask_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
        MessageBox.Show("Locator service failed: " & e.Error.ToString())
    End Sub
End Class

