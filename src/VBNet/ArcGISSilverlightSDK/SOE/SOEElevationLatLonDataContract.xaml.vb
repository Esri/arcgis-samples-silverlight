Imports System
Imports System.Net
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Json
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Geometry


    Partial Public Class SOEElevationLatLonDataContract
        Inherits UserControl
        Private Shared _mercator As New ESRI.ArcGIS.Client.Projection.WebMercator()

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub MyMap_MouseClick(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Map.MouseEventArgs)
            Dim geographicPoint As MapPoint = TryCast(_mercator.ToGeographic(e.MapPoint), MapPoint)

            Dim SOEurl As String = "http://sampleserver4.arcgisonline.com/ArcGIS/rest/services/Elevation/ESRI_Elevation_World/MapServer/exts/ElevationsSOE/ElevationLayers/1/GetElevationAtLonLat"
            SOEurl &= String.Format(System.Globalization.CultureInfo.InvariantCulture, "?lon={0}&lat={1}&f=json", geographicPoint.X, geographicPoint.Y)

            Dim webClient As New WebClient()

            AddHandler webClient.OpenReadCompleted, Sub(s, a)
                                                        Dim serializer As New DataContractJsonSerializer(GetType(ElevationSOELatLon))
                                                        Dim elevationSOELatLon As ElevationSOELatLon = TryCast(serializer.ReadObject(a.Result), ElevationSOELatLon)
                                                        a.Result.Close()

                                                        MyInfoWindow.Anchor = e.MapPoint
                                                        MyInfoWindow.Content = String.Format("Elevation: {0} meters", elevationSOELatLon.Elevation.ToString("0"))
                                                        MyInfoWindow.IsOpen = True
                                                    End Sub

            webClient.OpenReadAsync(New Uri(SOEurl))
        End Sub

        <DataContract()>
        Public Class ElevationSOELatLon
            <DataMember(Name:="elevation")>
            Public Property Elevation() As Double
        End Class

    End Class

