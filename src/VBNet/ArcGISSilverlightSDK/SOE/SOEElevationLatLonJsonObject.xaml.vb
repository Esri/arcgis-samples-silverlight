Imports System
Imports System.Json
Imports System.Net
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Geometry


    Partial Public Class SOEElevationLatLonJsonObject
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
                                                        Dim jsonResponse As JsonValue = JsonObject.Load(a.Result)
                                                        Dim elevation As Double = jsonResponse("elevation")
                                                        a.Result.Close()

                                                        MyInfoWindow.Anchor = e.MapPoint
                                                        MyInfoWindow.Content = String.Format("Elevation: {0} meters", elevation.ToString("0"))
                                                        MyInfoWindow.IsOpen = True
                                                    End Sub

            webClient.OpenReadAsync(New Uri(SOEurl))
        End Sub
    End Class

