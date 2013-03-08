Imports Microsoft.VisualBasic
Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Symbols

Partial Public Class SymbolJson
	Inherits UserControl
	Private Shared _mercator As New ESRI.ArcGIS.Client.Projection.WebMercator()

	Public Sub New()
		InitializeComponent()
		Button_SMS(Nothing, Nothing)
	End Sub

	Private Sub GraphicsLayer_Initialized(ByVal sender As Object, ByVal e As EventArgs)
		Dim graphicsLayer As GraphicsLayer = TryCast(sender, GraphicsLayer)
		For Each g As Graphic In graphicsLayer.Graphics
			g.Geometry = _mercator.FromGeographic(g.Geometry)

			If TypeOf g.Geometry Is Polygon OrElse TypeOf g.Geometry Is Envelope Then
				JsonTextBoxFillCurrent.Text = (TryCast(g.Symbol, IJsonSerializable)).ToJson()
			ElseIf TypeOf g.Geometry Is Polyline Then
				JsonTextBoxLineCurrent.Text = (TryCast(g.Symbol, IJsonSerializable)).ToJson()
			Else
				JsonTextBoxMarkerCurrent.Text = (TryCast(g.Symbol, IJsonSerializable)).ToJson()
			End If

			AddHandler g.PropertyChanged, AddressOf g_PropertyChanged
		Next g
	End Sub

	Private Sub g_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
		If e.PropertyName = "Symbol" Then
			Dim g As Graphic = TryCast(sender, Graphic)
			If TypeOf g.Geometry Is Polygon OrElse TypeOf g.Geometry Is Envelope Then
				JsonTextBoxFillCurrent.Text = (TryCast(g.Symbol, IJsonSerializable)).ToJson()
			ElseIf TypeOf g.Geometry Is Polyline Then
				JsonTextBoxLineCurrent.Text = (TryCast(g.Symbol, IJsonSerializable)).ToJson()
			Else
				JsonTextBoxMarkerCurrent.Text = (TryCast(g.Symbol, IJsonSerializable)).ToJson()
			End If
		End If
	End Sub

	Private Sub Button_Load(ByVal sender As Object, ByVal e As RoutedEventArgs)
		Try
			Dim symbol As Symbol = symbol.FromJson(JsonTextBox.Text)

			Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)

			For Each g As Graphic In graphicsLayer.Graphics
				If (TypeOf g.Geometry Is Polygon OrElse TypeOf g.Geometry Is Envelope) AndAlso TypeOf symbol Is FillSymbol Then
					g.Symbol = symbol
				ElseIf TypeOf g.Geometry Is Polyline AndAlso TypeOf symbol Is LineSymbol Then
					g.Symbol = symbol
				ElseIf TypeOf g.Geometry Is MapPoint AndAlso TypeOf symbol Is MarkerSymbol Then
					g.Symbol = symbol
				End If
			Next g
		Catch ex As Exception
			MessageBox.Show(ex.Message, "Deserializing JSON failed", MessageBoxButton.OK)
		End Try
	End Sub

	Private Sub Button_SMS(ByVal sender As Object, ByVal e As RoutedEventArgs)
    Dim jsonString As String = "{" & ControlChars.CrLf & "    ""type"": ""esriSMS""," & ControlChars.CrLf & "    ""style"": ""esriSMSSquare""," & ControlChars.CrLf & "    ""color"": [76,115,0,255]," & ControlChars.CrLf & "    ""size"": 8," & ControlChars.CrLf & "    ""angle"": 0," & ControlChars.CrLf & "    ""xoffset"": 0," & ControlChars.CrLf & "    ""yoffset"": 0," & ControlChars.CrLf & "    ""outline"": " & ControlChars.CrLf & "    {" & ControlChars.CrLf & "        ""color"": [152,230,0,255]," & ControlChars.CrLf & "        ""width"": 1" & ControlChars.CrLf & "    }" & ControlChars.CrLf & "}" & ControlChars.CrLf & ""
    JsonTextBox.Text = jsonString
  End Sub

  Private Sub Button_PMS(ByVal sender As Object, ByVal e As RoutedEventArgs)
    Dim jsonString As String = "{" & ControlChars.CrLf & "	""type"" : ""esriPMS"", " & ControlChars.CrLf & "	""url"" : ""http://static.arcgis.com/images/Symbols/Basic/GreenStickpin.png"", " & ControlChars.CrLf & "	""contentType"" : ""image/png"", " & ControlChars.CrLf & "	""color"" : null, " & ControlChars.CrLf & "	""width"" : 28, " & ControlChars.CrLf & "	""height"" : 28, " & ControlChars.CrLf & "	""angle"" : 0, " & ControlChars.CrLf & "	""xoffset"" : 0, " & ControlChars.CrLf & "	""yoffset"" : 0" & ControlChars.CrLf & "}" & ControlChars.CrLf & ""
    JsonTextBox.Text = jsonString
  End Sub

  Private Sub Button_SLS(ByVal sender As Object, ByVal e As RoutedEventArgs)
    Dim jsonString As String = "{" & ControlChars.CrLf & "    ""type"": ""esriSLS""," & ControlChars.CrLf & "    ""style"": ""esriSLSDot""," & ControlChars.CrLf & "    ""color"": [115,76,0,255]," & ControlChars.CrLf & "    ""width"": 2" & ControlChars.CrLf & "}" & ControlChars.CrLf & ""
    JsonTextBox.Text = jsonString
  End Sub

  Private Sub Button_SFS(ByVal sender As Object, ByVal e As RoutedEventArgs)
    Dim jsonString As String = "{" & ControlChars.CrLf & "    ""type"": ""esriSFS""," & ControlChars.CrLf & "    ""style"": ""esriSFSSolid""," & ControlChars.CrLf & "    ""color"": [250,76,0,150]," & ControlChars.CrLf & "    ""outline"": " & ControlChars.CrLf & "    {" & ControlChars.CrLf & "        ""type"": ""esriSLS""," & ControlChars.CrLf & "        ""style"": ""esriSLSSolid""," & ControlChars.CrLf & "        ""color"": [110,110,110,255]," & ControlChars.CrLf & "        ""width"": 2" & ControlChars.CrLf & "    }" & ControlChars.CrLf & "}" & ControlChars.CrLf & ""
    JsonTextBox.Text = jsonString
  End Sub

  Private Sub Button_PFS(ByVal sender As Object, ByVal e As RoutedEventArgs)
    Dim jsonString As String = "{" & ControlChars.CrLf & "	""type"" : ""esriPFS"", " & ControlChars.CrLf & "	""url"" : ""http://static.arcgis.com/images/Symbols/Transportation/AmberBeacon.png"", " & ControlChars.CrLf & "	""contentType"" : ""image/png"", " & ControlChars.CrLf & "	""color"" : null, " & ControlChars.CrLf & "	""outline"" : " & ControlChars.CrLf & "	{" & ControlChars.CrLf & "		""type"" : ""esriSLS"", " & ControlChars.CrLf & "		""style"" : ""esriSLSSolid"", " & ControlChars.CrLf & "		""color"" : [110,110,110,255], " & ControlChars.CrLf & "		""width"" : 1" & ControlChars.CrLf & "	}, " & ControlChars.CrLf & "	""width"" : 12, " & ControlChars.CrLf & "	""height"" : 12, " & ControlChars.CrLf & "	""angle"" : 0, " & ControlChars.CrLf & "	""xoffset"" : 0, " & ControlChars.CrLf & "	""yoffset"" : 0, " & ControlChars.CrLf & "	""xscale"" : 1, " & ControlChars.CrLf & "	""yscale"" : 1" & ControlChars.CrLf & "  }" & ControlChars.CrLf & ControlChars.CrLf & ""
    JsonTextBox.Text = jsonString
  End Sub
End Class
