Imports System.Json
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Bing

Partial Public Class BingImagery
	Inherits UserControl
	Public Sub New()
		InitializeComponent()
	End Sub

	Private Sub RadioButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
		Dim tileLayer As ESRI.ArcGIS.Client.Bing.TileLayer = TryCast(MyMap.Layers("BingLayer"), TileLayer)
		Dim layerTypeTag As String = CStr((CType(sender, RadioButton)).Tag)
		Dim newLayerType As TileLayer.LayerType = CType(System.Enum.Parse(GetType(TileLayer.LayerType), layerTypeTag, True), TileLayer.LayerType)
		tileLayer.LayerStyle = newLayerType
	End Sub

	Private Sub BingKeyTextBox_TextChanged(ByVal sender As Object, ByVal e As TextChangedEventArgs)
		If (TryCast(sender, TextBox)).Text.Length >= 64 Then
			LoadMapButton.IsEnabled = True
		Else
			LoadMapButton.IsEnabled = False
		End If
	End Sub

	Private Sub LoadMapButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
		Dim webClient As New WebClient()
		Dim uri As String = String.Format("http://dev.virtualearth.net/REST/v1/Imagery/Metadata/Aerial?supressStatus=true&key={0}", BingKeyTextBox.Text)

		AddHandler webClient.OpenReadCompleted, Sub(s, a)
																							If a.Error Is Nothing Then
																								Dim jsonResponse As JsonValue = JsonObject.Load(a.Result)
																								Dim authenticationResult As String = jsonResponse("authenticationResultCode")
																								a.Result.Close()

																								If authenticationResult = "ValidCredentials" Then
																									Dim tileLayer As ESRI.ArcGIS.Client.Bing.TileLayer = New TileLayer() With
																									{
																										.ID = "BingLayer",
																										.LayerStyle = tileLayer.LayerType.Road,
																										.ServerType = ServerType.Production,
																										.Token = BingKeyTextBox.Text
																									}
																									MyMap.Layers.Add(tileLayer)

																									BingKeyGrid.Visibility = System.Windows.Visibility.Collapsed
																									LayerStyleGrid.Visibility = System.Windows.Visibility.Visible

																									InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Collapsed

																								Else
																									InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Visible
																								End If
																							Else
																								InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Visible
																							End If
																						End Sub

		webClient.OpenReadAsync(New System.Uri(uri))
	End Sub
End Class
