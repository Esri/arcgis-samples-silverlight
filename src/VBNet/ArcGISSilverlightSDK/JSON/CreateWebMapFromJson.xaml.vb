Imports Microsoft.VisualBasic
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.WebMap
Imports ESRI.ArcGIS.Client.Geometry


Partial Public Class CreateWebMapFromJson
  Inherits UserControl

  Private webMapDocument As Document
  Private mercator As New ESRI.ArcGIS.Client.Projection.WebMercator()

    Public Sub New()
        InitializeComponent()
        CreateWebMapJson()
    End Sub

    Private Sub CreateWebMapJson()

        Dim jsonInput As String = "{" & ControlChars.CrLf &
          " ""baseMap"": {" & ControlChars.CrLf &
          " ""baseMapLayers"": [" & ControlChars.CrLf &
          " {" & ControlChars.CrLf &
          " ""opacity"": 1," & ControlChars.CrLf &
          " ""url"": ""http://services.arcgisonline.com/ArcGIS/rest/services/World_Terrain_Base/MapServer""," & ControlChars.CrLf &
          " ""visibility"": true" & ControlChars.CrLf & " }," & ControlChars.CrLf &
          " {" & ControlChars.CrLf & "  ""isReference"": true," & ControlChars.CrLf & " ""opacity"": 1," & ControlChars.CrLf &
          " ""url"": ""http://services.arcgisonline.com/ArcGIS/rest/services/Reference/World_Reference_Overlay/MapServer""," & ControlChars.CrLf &
          " ""visibility"": true" & ControlChars.CrLf & " }" & ControlChars.CrLf & " ]," & ControlChars.CrLf &
          "  ""title"": ""World_Terrain_Base""" & ControlChars.CrLf & " }," & ControlChars.CrLf & " ""operationalLayers"": [" & ControlChars.CrLf &
          "   {" & ControlChars.CrLf & " ""itemId"": ""204d94c9b1374de9a21574c9efa31164""," & ControlChars.CrLf & " ""opacity"": 0.75," & ControlChars.CrLf &
          "     ""title"": ""Soil Survey Map""," & ControlChars.CrLf & " ""url"": ""http://services.arcgisonline.com/ArcGIS/rest/services/Specialty/Soil_Survey_Map/MapServer""," & ControlChars.CrLf &
          "  ""visibility"": true" & ControlChars.CrLf & " }" & ControlChars.CrLf & " ]," & ControlChars.CrLf & "  ""version"": ""1.1""}"

        JsonTextBox.Text = jsonInput

    End Sub


    Private Sub Button_Load(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim webMapDocument As New Document()
        AddHandler webMapDocument.GetMapCompleted, AddressOf webMapDocument_GetMapCompleted
        webMapDocument.GetMapFromJsonAsync(JsonTextBox.Text)
    End Sub

    Private Sub webMapDocument_GetMapCompleted(ByVal sender As Object, ByVal e As GetMapCompletedEventArgs)
        If e.Error Is Nothing Then
            e.Map.Extent = TryCast(mercator.FromGeographic(New Envelope(-139.4916, 20.7191, -52.392, 59.5199)), Envelope)
            MyMapGrid.Children.Add(e.Map)
        End If
    End Sub

    Private Sub Button_ClearMap(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        MyMapGrid.Children.Clear()
        MyMapGrid.UpdateLayout()
    End Sub
End Class

