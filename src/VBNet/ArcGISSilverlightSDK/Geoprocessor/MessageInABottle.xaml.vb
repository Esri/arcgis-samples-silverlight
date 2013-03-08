Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks
Imports ESRI.ArcGIS.Client.Symbols


	Partial Public Class MessageInABottle
		Inherits UserControl
		Private Shared _mercator As New ESRI.ArcGIS.Client.Projection.WebMercator()
		Private _graphicsLayer As GraphicsLayer
		Public Sub New()
			InitializeComponent()
			_graphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
		End Sub

		Private Sub MyMap_MouseClick(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Map.MouseEventArgs)

			Dim graphic As New Graphic() With {.Symbol = TryCast(LayoutRoot.Resources("StartMarkerSymbol"), Symbol), .Geometry = e.MapPoint}
			_graphicsLayer.Graphics.Add(graphic)

			Dim geoprocessorTask As New Geoprocessor("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_Currents_World/GPServer/MessageInABottle")
			AddHandler geoprocessorTask.ExecuteCompleted, AddressOf GeoprocessorTask_ExecuteCompleted
			AddHandler geoprocessorTask.Failed, AddressOf GeoprocessorTask_Failed
			geoprocessorTask.OutputSpatialReference = MyMap.SpatialReference

			Dim parameters As New List(Of GPParameter)()
			parameters.Add(New GPFeatureRecordSetLayer("Input_Point", _mercator.ToGeographic(e.MapPoint)))
			parameters.Add(New GPDouble("Days", Convert.ToDouble(DaysTextBox.Text)))

			geoprocessorTask.ExecuteAsync(parameters)
		End Sub

		Private Sub GeoprocessorTask_ExecuteCompleted(ByVal sender As Object, ByVal e As GPExecuteCompleteEventArgs)
			For Each gpParameter As GPParameter In e.Results.OutParameters
				If TypeOf gpParameter Is GPFeatureRecordSetLayer Then
					Dim gpLayer As GPFeatureRecordSetLayer = TryCast(gpParameter, GPFeatureRecordSetLayer)
					For Each graphic As Graphic In gpLayer.FeatureSet.Features
						graphic.Symbol = TryCast(LayoutRoot.Resources("PathLineSymbol"), Symbol)
						_graphicsLayer.Graphics.Add(graphic)
					Next graphic
				End If
			Next gpParameter
		End Sub

		Private Sub GeoprocessorTask_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
			MessageBox.Show("Geoprocessor service failed: " & e.Error.Message)
		End Sub
	End Class

