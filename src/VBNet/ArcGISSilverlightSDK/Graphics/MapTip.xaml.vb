Imports Microsoft.VisualBasic
Imports System
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Tasks


    Partial Public Class GraphicsMapTip
        Inherits UserControl
        Public Sub New()
            InitializeComponent()

            AddHandler MyMap.PropertyChanged, AddressOf MyMap_PropertyChanged
        End Sub

        Private Sub MyMap_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
            If e.PropertyName = "SpatialReference" Then
                StatesGraphicsLayerLoad()
                CitiesGraphicsLayerLoad()
                RemoveHandler MyMap.PropertyChanged, AddressOf MyMap_PropertyChanged
            End If
        End Sub

        Private Sub StatesGraphicsLayerLoad()
		Dim query As New ESRI.ArcGIS.Client.Tasks.Query() With
				{
						.Geometry = New ESRI.ArcGIS.Client.Geometry.Envelope(-180, 0, 0, 90),
						.ReturnGeometry = True,
						.OutSpatialReference = MyMap.SpatialReference
				}
            query.OutFields.Add("*")

            Dim queryTask As New QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5")
            AddHandler queryTask.ExecuteCompleted, AddressOf StatesGraphicsLayerQueryTask_ExecuteCompleted
            queryTask.ExecuteAsync(query)
        End Sub

        Private Sub StatesGraphicsLayerQueryTask_ExecuteCompleted(ByVal sender As Object, ByVal queryArgs As ESRI.ArcGIS.Client.Tasks.QueryEventArgs)
            If queryArgs.FeatureSet Is Nothing Then
                Return
            End If

            Dim resultFeatureSet As FeatureSet = queryArgs.FeatureSet
            Dim graphicsLayer As ESRI.ArcGIS.Client.GraphicsLayer = TryCast(MyMap.Layers("StatesGraphicsLayer"), ESRI.ArcGIS.Client.GraphicsLayer)

            If resultFeatureSet IsNot Nothing AndAlso resultFeatureSet.Features.Count > 0 Then
                For Each graphicFeature As ESRI.ArcGIS.Client.Graphic In resultFeatureSet.Features
                    graphicFeature.Symbol = TryCast(LayoutRoot.Resources("DefaultFillSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
                    graphicsLayer.Graphics.Add(graphicFeature)
                Next graphicFeature
            End If
        End Sub

        Private Sub CitiesGraphicsLayerLoad()
		Dim query As New ESRI.ArcGIS.Client.Tasks.Query() With
				{
						.Geometry = New ESRI.ArcGIS.Client.Geometry.Envelope(-180, 0, 0, 90),
						.ReturnGeometry = True,
						.OutSpatialReference = MyMap.SpatialReference
				}
            query.OutFields.Add("*")
            query.Where = "POP1990 > 100000"
            Dim queryTask As New QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/0")
            AddHandler queryTask.ExecuteCompleted, AddressOf CitiesGraphicsLayerQueryTask_ExecuteCompleted
            queryTask.ExecuteAsync(query)
        End Sub

        Private Sub CitiesGraphicsLayerQueryTask_ExecuteCompleted(ByVal sender As Object, ByVal queryArgs As QueryEventArgs)
            If queryArgs.FeatureSet Is Nothing Then
                Return
            End If

            Dim resultFeatureSet As FeatureSet = queryArgs.FeatureSet
            Dim graphicsLayer As ESRI.ArcGIS.Client.GraphicsLayer = TryCast(MyMap.Layers("CitiesGraphicsLayer"), ESRI.ArcGIS.Client.GraphicsLayer)

            If resultFeatureSet IsNot Nothing AndAlso resultFeatureSet.Features.Count > 0 Then
                For Each graphicFeature As ESRI.ArcGIS.Client.Graphic In resultFeatureSet.Features
                    graphicFeature.Symbol = TryCast(LayoutRoot.Resources("DefaultMarkerSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
                    graphicsLayer.Graphics.Add(graphicFeature)
                Next graphicFeature
            End If
        End Sub
    End Class

