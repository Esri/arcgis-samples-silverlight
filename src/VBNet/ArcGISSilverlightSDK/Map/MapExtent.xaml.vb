Imports Microsoft.VisualBasic
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Geometry


Partial Public Class MapExtent
	Inherits UserControl
	Public Sub New()
		InitializeComponent()
	End Sub
	Private Sub MyMap_ExtentChanged(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.ExtentEventArgs)
		Dim newExtent As Envelope = Nothing

		If MyMap.WrapAroundIsActive Then
			Dim normalizedExtent As Geometry = Geometry.NormalizeCentralMeridian(e.NewExtent)
			If TypeOf normalizedExtent Is Polygon Then
				newExtent = New Envelope()

				For Each p As MapPoint In (TryCast(normalizedExtent, Polygon)).Rings(0)
					If p.X < newExtent.XMin OrElse Double.IsNaN(newExtent.XMin) Then
						newExtent.XMin = p.X
					End If
					If p.Y < newExtent.YMin OrElse Double.IsNaN(newExtent.YMin) Then
						newExtent.YMin = p.Y
					End If
				Next p

				For Each p As MapPoint In (TryCast(normalizedExtent, Polygon)).Rings(1)
					If p.X > newExtent.XMax OrElse Double.IsNaN(newExtent.XMax) Then
						newExtent.XMax = p.X
					End If
					If p.Y > newExtent.YMax OrElse Double.IsNaN(newExtent.YMax) Then
						newExtent.YMax = p.Y
					End If
				Next p
			ElseIf TypeOf normalizedExtent Is Envelope Then
				newExtent = TryCast(normalizedExtent, Envelope)
			End If
		Else
			newExtent = e.NewExtent
		End If

		MinXNormalized.Text = newExtent.XMin.ToString("0.000")
		MinYNormalized.Text = newExtent.YMin.ToString("0.000")
		MaxXNormalized.Text = newExtent.XMax.ToString("0.000")
		MaxYNormalized.Text = newExtent.YMax.ToString("0.000")
	End Sub
End Class

