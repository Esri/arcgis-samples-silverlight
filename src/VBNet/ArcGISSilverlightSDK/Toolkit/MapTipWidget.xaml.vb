Imports Microsoft.VisualBasic
Imports System
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks
Imports ESRI.ArcGIS.Client.Symbols


Partial Public Class MapTipWidget
	Inherits UserControl
	Public Sub New()
		InitializeComponent()
		AddHandler MyMap.Layers.LayersInitialized, AddressOf Layers_LayersInitialized
	End Sub

	Private Sub Layers_LayersInitialized(ByVal sender As Object, ByVal args As EventArgs)
		Dim query As ESRI.ArcGIS.Client.Tasks.Query = New ESRI.ArcGIS.Client.Tasks.Query() With
		{
			.Geometry = MyMap.Extent,
			.ReturnGeometry = True,
			.OutSpatialReference = MyMap.SpatialReference
		}
		query.OutFields.Add("*")

		Dim queryTask As QueryTask = New QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5")
		AddHandler queryTask.ExecuteCompleted, AddressOf QueryTask_ExecuteCompleted
		queryTask.ExecuteAsync(query)
	End Sub

	Private Sub QueryTask_ExecuteCompleted(ByVal sender As Object, ByVal args As ESRI.ArcGIS.Client.Tasks.QueryEventArgs)
		Dim featureSet As FeatureSet = args.FeatureSet

		Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
		MyMapTip.GraphicsLayer = graphicsLayer

		If Not featureSet Is Nothing AndAlso featureSet.Features.Count > 0 Then
			For Each feature As Graphic In featureSet.Features
				feature.Symbol = TryCast(LayoutRoot.Resources("DefaultFillSymbol"), Symbol)
				graphicsLayer.Graphics.Add(feature)
			Next feature
		End If
	End Sub

End Class

