Imports Microsoft.VisualBasic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks


Partial Public Class SimpleClusterer
  Inherits UserControl
  Public Sub New()
    InitializeComponent()
  End Sub

  Private Sub MyMap_PropertyChanged(ByVal sender As System.Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
    If (e.PropertyName = "SpatialReference") AndAlso (Not (TryCast(sender, ESRI.ArcGIS.Client.Map)).SpatialReference Is Nothing) Then
      LoadGraphics()
      RemoveHandler MyMap.PropertyChanged, AddressOf MyMap_PropertyChanged
    End If
  End Sub

  Private Sub GraphicsLayer_Initialized(ByVal sender As Object, ByVal e As System.EventArgs)
    LoadGraphics()
  End Sub

  Private Sub LoadGraphics()
    Dim queryTask As New QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/0")
    AddHandler queryTask.ExecuteCompleted, AddressOf queryTask_ExecuteCompleted

    Dim query As Query = New ESRI.ArcGIS.Client.Tasks.Query() With
      {
        .OutSpatialReference = MyMap.SpatialReference,
        .ReturnGeometry = True,
        .Where = "1=1"
      }
    query.OutFields.Add("CITY_NAME")
    queryTask.ExecuteAsync(query)
  End Sub

  Private Sub queryTask_ExecuteCompleted(ByVal sender As Object, ByVal args As QueryEventArgs)
    Dim featureSet As FeatureSet = args.FeatureSet

    If featureSet Is Nothing OrElse featureSet.Features.Count < 1 Then
      MessageBox.Show("No features returned from query")
      Return
    End If

    Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)

    For Each graphic As Graphic In featureSet.Features
      graphic.Symbol = TryCast(LayoutRoot.Resources("YellowMarkerSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
      graphicsLayer.Graphics.Add(graphic)
    Next graphic
  End Sub



End Class

