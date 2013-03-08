Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Tasks
Imports System.Collections.Generic
Imports System.Windows
Imports ESRI.ArcGIS.Client
Imports System.Linq
Imports System.Collections.ObjectModel
Imports System


Partial Public Class BatchGeocoding
  Inherits UserControl
  Private _locatorTask As Locator
  Private batchaddresses As New ObservableCollection(Of IDictionary(Of String, String))()
  Private geocodedResults As GraphicsLayer

  Public Sub New()
    InitializeComponent()

    _locatorTask = New Locator("http://serverapps101.esri.com/arcgis/rest/services/USA_Geocode/GeocodeServer")
    AddHandler _locatorTask.AddressesToLocationsCompleted, AddressOf _locatorTask_AddressesToLocationsCompleted
    AddHandler _locatorTask.Failed, AddressOf LocatorTask_Failed

    geocodedResults = TryCast(MyMap.Layers("LocationGraphicsLayer"), GraphicsLayer)


    'List of addresses to geocode
    batchaddresses.Add(New Dictionary(Of String, String) From {{"Street", "4409 Redwood Dr"}, {"Zip", "92501"}})
    batchaddresses.Add(New Dictionary(Of String, String) From {{"Street", "3758 Cedar St"}, {"Zip", "92501"}})
    batchaddresses.Add(New Dictionary(Of String, String) From {{"Street", "3486 Orange St"}, {"Zip", "92501"}})
    batchaddresses.Add(New Dictionary(Of String, String) From {{"Street", "2999 4th St"}, {"Zip", "92507"}})
    batchaddresses.Add(New Dictionary(Of String, String) From {{"Street", "3685 10th St"}, {"Zip", "92501"}})
    batchaddresses.Add(New Dictionary(Of String, String) From {{"Street", "4462 Main St"}, {"Zip", "92501"}})
    batchaddresses.Add(New Dictionary(Of String, String) From {{"Street", "3964 Ramona Dr"}, {"Zip", "92506"}})
    batchaddresses.Add(New Dictionary(Of String, String) From {{"Street", "2965 13th St"}, {"Zip", "92507"}})
    batchaddresses.Add(New Dictionary(Of String, String) From {{"Street", "2245 11th St"}, {"Zip", "92507"}})
    batchaddresses.Add(New Dictionary(Of String, String) From {{"Street", "3887 Park Ave"}, {"Zip", "92507"}})

    AddressListbox.ItemsSource = batchaddresses
  End Sub

  Private Sub BatchGeocodeButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    If batchaddresses.Count > 0 Then
      _locatorTask.AddressesToLocationsAsync(batchaddresses.ToList(), MyMap.SpatialReference)
    End If
  End Sub

  Private Sub _locatorTask_AddressesToLocationsCompleted(ByVal sender As Object, ByVal e As AddressesToLocationsEventArgs)
    If e.Result IsNot Nothing AndAlso e.Result.AddressCandidates IsNot Nothing AndAlso e.Result.AddressCandidates.Count > 0 Then
      geocodedResults.Graphics.Clear()
      For Each location As AddressCandidate In e.Result.AddressCandidates
        Dim graphic As New Graphic() With {.Geometry = location.Location}
        graphic.Attributes.Add("X", location.Attributes("X"))
        graphic.Attributes.Add("Y", location.Attributes("Y"))
        graphic.Attributes.Add("Match_addr", location.Attributes("Match_addr"))
        graphic.Attributes.Add("Score", location.Attributes("Score"))
        geocodedResults.Graphics.Add(graphic)
      Next location
    End If

  End Sub

  Private Sub LocatorTask_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
    MessageBox.Show("Locator service failed: " & e.Error.ToString())
  End Sub

  Private Sub addtolist_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    If String.IsNullOrEmpty(StreetTextBox.Text) OrElse String.IsNullOrEmpty(ZipTextBox.Text) Then
      MessageBox.Show("Value is required for all inputs")
      Return
    End If

    Dim number As Integer
    Dim result As Boolean = Int32.TryParse(ZipTextBox.Text, number)

    If Not result Then
      MessageBox.Show("Incorrect Zip format")
      Return
    End If
    batchaddresses.Add(New Dictionary(Of String, String) From {{"Street", StreetTextBox.Text}, {"Zip", ZipTextBox.Text}})
  End Sub

  Private Sub ResetList_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    batchaddresses.Clear()
    geocodedResults.ClearGraphics()
  End Sub

  Private Sub GraphicsLayer_MouseLeftButtonDown(ByVal sender As Object, ByVal e As GraphicMouseButtonEventArgs)
    e.Graphic.Selected = Not e.Graphic.Selected
  End Sub

End Class
