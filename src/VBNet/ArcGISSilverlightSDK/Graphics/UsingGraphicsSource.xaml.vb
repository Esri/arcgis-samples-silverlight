Imports System
Imports System.Collections.ObjectModel
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Windows.Threading
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Symbols

Partial Public Class UsingGraphicsSource
	Inherits UserControl
	Public Sub New()
		InitializeComponent()
	End Sub
End Class

Public Class Customers
	Inherits ObservableCollection(Of Graphic)
	Private random As Random

	Private Shared mercator As New ESRI.ArcGIS.Client.Projection.WebMercator()

	Public Sub New()
		Dim timer As New DispatcherTimer() With {.Interval = TimeSpan.FromSeconds(4)}
		AddHandler timer.Tick, AddressOf timer_Tick
		timer.Start()
	End Sub

	Private Sub timer_Tick(ByVal sender As Object, ByVal e As EventArgs)
		ClearItems()

		random = New Random()

		For i As Integer = 0 To 9
			Dim g As New Graphic() With {.Geometry = mercator.FromGeographic(New MapPoint(random.Next(-180, 180), random.Next(-90, 90)))}

			g.Symbol = New SimpleMarkerSymbol() With {.Color = New SolidColorBrush(Color.FromArgb(255, CByte(random.Next(0, 255)), CByte(random.Next(0, 255)), CByte(random.Next(0, 255)))), .Size = 24}

			Add(g)
		Next i
	End Sub
End Class
