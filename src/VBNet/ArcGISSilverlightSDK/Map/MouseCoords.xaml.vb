Imports Microsoft.VisualBasic
Imports System
Imports System.Windows.Controls


Partial Public Class MouseCoords
	Inherits UserControl
	Public Sub New()
		InitializeComponent()
	End Sub

	Private Sub MyMap_MouseMove(ByVal sender As Object, ByVal args As System.Windows.Input.MouseEventArgs)
		If MyMap.Extent IsNot Nothing Then
			Dim screenPoint As System.Windows.Point = args.GetPosition(MyMap)
			ScreenCoordsTextBlock.Text = String.Format("Screen Coords: X = {0}, Y = {1}", screenPoint.X, screenPoint.Y)

			Dim mapPoint As ESRI.ArcGIS.Client.Geometry.MapPoint = MyMap.ScreenToMap(screenPoint)

			If MyMap.WrapAroundIsActive Then
				mapPoint = TryCast(ESRI.ArcGIS.Client.Geometry.Geometry.NormalizeCentralMeridian(mapPoint), ESRI.ArcGIS.Client.Geometry.MapPoint)
			End If
			MapCoordsTextBlock.Text = String.Format("Map Coords: X = {0}, Y = {1}", Math.Round(mapPoint.X, 4), Math.Round(mapPoint.Y, 4))
		End If
	End Sub
End Class

