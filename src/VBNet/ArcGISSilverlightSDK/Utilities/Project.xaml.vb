Imports System.Windows.Controls
Imports System.Windows
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Tasks
Imports ESRI.ArcGIS.Client
Imports System.Collections.Generic
Imports ESRI.ArcGIS.Client.Symbols


Partial Public Class Project
	Inherits UserControl
	Private geometryService As GeometryService
	Private graphicsLayer As GraphicsLayer

	Public Sub New()
		InitializeComponent()

		geometryService = New GeometryService("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")
		AddHandler geometryService.ProjectCompleted, AddressOf geometryService_ProjectCompleted
		AddHandler geometryService.Failed, AddressOf geometryService_Failed

		graphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
	End Sub

	Private Sub ProjectButton_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
		Dim x As Double
		Dim y As Double
		If (Not Double.TryParse(XTextBox.Text, x)) OrElse (Not Double.TryParse(YTextBox.Text, y)) Then
			MessageBox.Show("Enter valid coordinate values.")
			Return
		End If

		Dim inputMapPoint As New MapPoint(x, y, New SpatialReference(4326))

		geometryService.ProjectAsync(New List(Of Graphic)(New Graphic() {New Graphic() With {.Geometry = inputMapPoint}}), MyMap.SpatialReference, inputMapPoint)
	End Sub

	Private Sub geometryService_ProjectCompleted(ByVal sender As Object, ByVal e As GraphicsEventArgs)
		Dim resultGraphic As Graphic = e.Results(0)

		If resultGraphic.Geometry.Extent IsNot Nothing Then
			resultGraphic.Symbol = TryCast(LayoutRoot.Resources("RoundMarkerSymbol"), SimpleMarkerSymbol)

			Dim resultMapPoint As MapPoint = TryCast(resultGraphic.Geometry, MapPoint)
			resultGraphic.Attributes.Add("Output_CoordinateX", resultMapPoint.X)
			resultGraphic.Attributes.Add("Output_CoordinateY", resultMapPoint.Y)

			Dim inputMapPoint As MapPoint = TryCast(e.UserState, MapPoint)
			resultGraphic.Attributes.Add("Input_CoordinateX", inputMapPoint.X)
			resultGraphic.Attributes.Add("Input_CoordinateY", inputMapPoint.Y)

			graphicsLayer.Graphics.Add(resultGraphic)

			MyMap.PanTo(resultGraphic.Geometry)
		Else
			MessageBox.Show("Invalid input coordinate, unable to project.")
		End If
	End Sub

	Private Sub geometryService_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
		MessageBox.Show("Geometry Service error: " & e.Error.ToString())
	End Sub

End Class

