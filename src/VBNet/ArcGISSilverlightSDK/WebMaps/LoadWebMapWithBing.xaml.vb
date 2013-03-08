Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.WebMap
Imports System.Json
Imports System.Net

Partial Public Class LoadWebMapWithBing
	Inherits UserControl
	Public Sub New()
		InitializeComponent()
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

																								BingKeyGrid.Visibility = System.Windows.Visibility.Collapsed
																								InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Collapsed

																								If authenticationResult = "ValidCredentials" Then
																									Dim webMap As New Document()
																									webMap.BingToken = BingKeyTextBox.Text
																									AddHandler webMap.GetMapCompleted, Sub(s1, e1)
																																											 If e1.Error Is Nothing Then
																																												 LayoutRoot.Children.Add(e1.Map)
																																											 End If
																																										 End Sub

																									webMap.GetMapAsync("75e222ec54b244a5b73aeef40ce76adc")
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
