Imports Microsoft.VisualBasic
Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks


Partial Public Class SDSGraphicsLayer
  Inherits UserControl
  Public Sub New()
    InitializeComponent()

    ' Wait for Map spatial reference to be set by first layer. Unnecessary if set explicitly.  
    AddHandler MyMap.Layers.LayersInitialized, AddressOf Layers_LayersInitialized
  End Sub

  Private Sub Layers_LayersInitialized(ByVal sender As Object, ByVal args As EventArgs)
    LoadGraphics()
  End Sub

  Private Sub LoadGraphics()
    Dim queryTask As New QueryTask("http://servicesbeta5.esri.com/arcgis/rest/services/World/FeatureServer/0")

    AddHandler queryTask.ExecuteCompleted, AddressOf queryTask_ExecuteCompleted
    queryTask.DisableClientCaching = True

    Dim query As Query = New ESRI.ArcGIS.Client.Tasks.Query()
    query.OutSpatialReference = MyMap.SpatialReference
    query.ReturnGeometry = True
    query.Where = "POP_RANK < 4"
    ' Note, query.Text is not supported for use with Spatial Data Services

    query.OutFields.AddRange(New String() {"CITY_NAME"})
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

