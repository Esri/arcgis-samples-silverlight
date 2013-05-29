Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports ESRI.ArcGIS.Client.Tasks
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry


  Partial Public Class Generalize
    Inherits UserControl
    Private originalGraphicsLayer As GraphicsLayer

    Public Sub New()
      InitializeComponent()

      AddHandler MyMap.Layers.LayersInitialized, AddressOf Layers_LayersInitialized

      originalGraphicsLayer = TryCast(MyMap.Layers("OriginalLineGraphicsLayer"), GraphicsLayer)
    End Sub

    Private Sub Layers_LayersInitialized(ByVal sender As Object, ByVal args As EventArgs)
      If originalGraphicsLayer IsNot Nothing AndAlso originalGraphicsLayer.Graphics.Count = 0 Then
        Dim queryTask As New QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/1")
        Dim query As New Query()
        query.ReturnGeometry = True
        query.OutSpatialReference = MyMap.SpatialReference
        query.Where = "NAME = 'Mississippi'"

        AddHandler queryTask.ExecuteCompleted, AddressOf queryTask_ExecuteCompleted
        AddHandler queryTask.Failed, AddressOf queryTask_Failed
        queryTask.ExecuteAsync(query, query.OutSpatialReference)
      End If
    End Sub

    Private Sub queryTask_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      MessageBox.Show("Query error: " & e.Error.Message)
    End Sub

    Private Sub queryTask_ExecuteCompleted(ByVal sender As Object, ByVal e As QueryEventArgs)
      Dim originalGraphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("OriginalLineGraphicsLayer"), GraphicsLayer)
      For Each g As Graphic In e.FeatureSet.Features
        g.Symbol = TryCast(LayoutRoot.Resources("DefaultLineSymbol"), Symbols.Symbol)
        g.Geometry.SpatialReference = TryCast(e.UserState, SpatialReference)
        originalGraphicsLayer.Graphics.Add(g)

        For Each pc As ESRI.ArcGIS.Client.Geometry.PointCollection In (TryCast(g.Geometry, Polyline)).Paths
          For Each point As MapPoint In pc
            Dim vertice As New Graphic() With
                {
                    .Symbol = TryCast(LayoutRoot.Resources("DefaultMarkerSymbol"), Symbols.Symbol),
                    .Geometry = point
                }
            originalGraphicsLayer.Graphics.Add(vertice)
          Next point
        Next pc
      Next g
      GeneralizeButton.IsEnabled = True
    End Sub

    Private Sub GeneralizeButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      GeneralizeButton.IsEnabled = False
      SliderStackPanel.Visibility = Visibility.Collapsed

      Dim originalGraphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("OriginalLineGraphicsLayer"), GraphicsLayer)

		Dim geometryService As New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")
      AddHandler geometryService.GeneralizeCompleted, AddressOf GeometryService_GeneralizeCompleted
      AddHandler geometryService.Failed, AddressOf GeometryService_Failed

        Dim generalizeParameters As New GeneralizeParameters() With
            {
                .DeviationUnit = LinearUnit.SurveyMile,
                .MaxDeviation = 10
            }

      geometryService.GeneralizeAsync(New List(Of Graphic)(New Graphic() {originalGraphicsLayer.Graphics(0)}), generalizeParameters)
    End Sub

    Private Sub GeometryService_GeneralizeCompleted(ByVal sender As Object, ByVal e As GraphicsEventArgs)
      Dim generalizedGraphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("GeneralizedLineGraphicsLayer"), GraphicsLayer)
      generalizedGraphicsLayer.Graphics.Clear()

      For Each g As Graphic In e.Results
        g.Symbol = TryCast(LayoutRoot.Resources("NewLineSymbol"), Symbols.Symbol)
        generalizedGraphicsLayer.Graphics.Add(g)

        Dim p As ESRI.ArcGIS.Client.Geometry.Polyline = TryCast(g.Geometry, ESRI.ArcGIS.Client.Geometry.Polyline)

        For Each pc As ESRI.ArcGIS.Client.Geometry.PointCollection In p.Paths
          For Each point As MapPoint In pc
            Dim vertice As New Graphic() With
                {
                    .Symbol = TryCast(LayoutRoot.Resources("NewMarkerSymbol"), Symbols.Symbol),
                    .Geometry = point
                }
            generalizedGraphicsLayer.Graphics.Add(vertice)
          Next point
        Next pc
      Next g
      generalizedGraphicsLayer.Opacity = 0.75
      SliderStackPanel.Visibility = Visibility.Visible
      GeneralizeButton.IsEnabled = True
    End Sub

    Private Sub GeometryService_Failed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
      MessageBox.Show("Geometry Service error: " & e.Error.Message)
    End Sub

    Private Sub GeneralizeLayerOpacity_ValueChanged(ByVal sender As Object, ByVal e As RoutedPropertyChangedEventArgs(Of Double))
      If MyMap IsNot Nothing Then
        MyMap.Layers("GeneralizedLineGraphicsLayer").Opacity = e.NewValue
      End If
    End Sub

  End Class

