Imports System
Imports System.Windows.Controls
Imports System.Windows.Input
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry

Partial Public Class EditToolsGeometry
	Inherits UserControl
	Private Shared _mercator As New ESRI.ArcGIS.Client.Projection.WebMercator()

	Private editGeometry As EditGeometry
	Private actionCount As Integer = 0

	Private selectedPointGraphic As Graphic

	Public Sub New()
		InitializeComponent()

		editGeometry = TryCast(Me.LayoutRoot.Resources("MyEditGeometry"), EditGeometry)
	End Sub

	Private Sub GraphicsLayer_MouseLeftButtonDown(ByVal sender As Object, ByVal e As GraphicMouseButtonEventArgs)
		e.Handled = True

		If TypeOf e.Graphic.Geometry Is MapPoint Then
			e.Graphic.Selected = True
			selectedPointGraphic = e.Graphic
		Else
			editGeometry.StartEdit(e.Graphic)
		End If
	End Sub

	Private Sub StopEdit_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
		editGeometry.StopEdit()
	End Sub

	Private Sub CancelEdit_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
		editGeometry.CancelEdit()
	End Sub

	Private Sub UndoLastEdit_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
		editGeometry.UndoLastEdit()
	End Sub

	Private Sub RedoLastEdit_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
		editGeometry.RedoLastEdit()
	End Sub

	Private Sub EditGeometry_GeometryEdit(ByVal sender As Object, ByVal e As EditGeometry.GeometryEditEventArgs)
		If ActionTextBox.Text <> String.Empty Then
			ActionTextBox.Select(0, 0)
			ActionTextBox.SelectedText = String.Format("{0}:{1}{2}", actionCount, e.Action, Environment.NewLine)
		Else
			ActionTextBox.Text = String.Format("{0}:{1}", actionCount, e.Action)
		End If
		actionCount += 1
	End Sub

	Private Sub GraphicsLayer_Initialized(ByVal sender As Object, ByVal e As EventArgs)
		Dim graphicsLayer As GraphicsLayer = TryCast(sender, GraphicsLayer)
		For Each g As Graphic In graphicsLayer.Graphics
			g.Geometry = _mercator.FromGeographic(g.Geometry)
		Next g
	End Sub

	Private Sub MyMap_MouseClick(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Map.MouseEventArgs)
		editGeometry.StopEdit()
	End Sub

	Private Sub MyMap_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
		If selectedPointGraphic IsNot Nothing Then
			selectedPointGraphic.Geometry = MyMap.ScreenToMap(e.GetPosition(MyMap))
		End If
	End Sub

	Private Sub MyMap_MouseLeftButtonUp(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
		If selectedPointGraphic IsNot Nothing Then
			selectedPointGraphic.Selected = False
			selectedPointGraphic = Nothing
		End If
	End Sub
End Class
