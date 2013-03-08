Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks

Partial Public Class OrderByFieldQuery
  Inherits UserControl
  Private parcelsGraphicsLayer As GraphicsLayer

  Public Sub New()
    InitializeComponent()

    parcelsGraphicsLayer = TryCast(MyMap.Layers("MontgomeryParcels"), GraphicsLayer)
  End Sub

  Private Sub MyMap_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
    If Not MyMap.SpatialReference Is Nothing Then
      RemoveHandler MyMap.PropertyChanged, AddressOf MyMap_PropertyChanged

      RunQuery()
    End If
  End Sub

  Private Sub GraphicsLayer_MouseLeftButtonUp(ByVal sender As Object, ByVal e As GraphicMouseButtonEventArgs)
    TryCast(sender, GraphicsLayer).ClearSelection()
    e.Graphic.Select()
  End Sub

  Private Sub Button_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    RunQuery()
  End Sub

  Private Sub RunQuery()
    parcelsGraphicsLayer.Graphics.Clear()

    Dim query As New ESRI.ArcGIS.Client.Tasks.Query() With
        {
            .ReturnGeometry = True,
            .OutSpatialReference = MyMap.SpatialReference,
            .Where = String.Format("OWNER_NAME LIKE '%{0}%'", SearchTextBox.Text),
            .OrderByFields = New List(Of OrderByField)() From {New OrderByField("OWNER_NAME", SortOrder.Ascending)}}

    query.OutFields.Add("OWNER_NAME,PARCEL_ID,ZONING,DEED_DATE")


    Dim queryTask As New QueryTask("http://sampleserver6.arcgisonline.com/arcgis/rest/services/MontgomeryQuarters/MapServer/1")
    AddHandler queryTask.ExecuteCompleted, Sub(s, a)
                                             For Each g As Graphic In a.FeatureSet.Features
                                               parcelsGraphicsLayer.Graphics.Add(g)
                                             Next g
                                           End Sub
    queryTask.ExecuteAsync(query)
  End Sub
End Class

