Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.WebMap
Imports System.Collections.Generic
Imports System.Collections
Imports ESRI.ArcGIS.Client.Tasks
Imports System.Windows

Partial Public Class CreateWebMapObject
  Inherits UserControl
  Private webmap As WebMap
  Private operationLayers As New List(Of WebMapLayer)()
  Private basemap As BaseMap

  Public Sub New()
    InitializeComponent()

    'Define BaseMap Layer
    basemap = New BaseMap() With {.Layers = New List(Of WebMapLayer) From {
     New WebMapLayer With {.Url = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer"}}}

    'Add a ArcGISDynamicMapService 
    operationLayers.Add(New WebMapLayer With {.Url = "http://serverapps10.esri.com/ArcGIS/rest/services/California/MapServer", .VisibleLayers = New List(Of Object) From {0, 1, 3, 6, 9}})

    'Define popup
    Dim fieldinfos As IList(Of FieldInfo) = New List(Of FieldInfo)()
    fieldinfos.Add(New FieldInfo() With {.FieldName = "STATE_NAME", .Label = "State", .Visible = True})

    Dim mediainfos As IList(Of MediaInfo) = New List(Of MediaInfo)()
    Dim infovalue As New MediaInfoValue()
    infovalue.Fields = New String() {"POP2000,POP2007"}
    mediainfos.Add(New MediaInfo() With {.Type = MediaType.PieChart, .Value = infovalue})

    Dim popup As New PopupInfo() With {.FieldInfos = fieldinfos, .MediaInfos = mediainfos, .Title = "Population Change between 2000 and 2007"}

    'Add a Feature Layer with popup
    operationLayers.Add(New WebMapLayer With {.Url = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3", .Mode = FeatureLayer.QueryMode.OnDemand, .PopupInfo = popup})

    'Perform Query to get a featureSet and add to webmap as featurecollection
    Dim qt As New QueryTask() With {.Url = "http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Earthquakes/EarthquakesFromLastSevenDays/MapServer/0"}
    AddHandler qt.ExecuteCompleted, AddressOf qt_ExecuteCompleted

    Dim query As New ESRI.ArcGIS.Client.Tasks.Query()
    query.OutFields.Add("*")
    query.Where = "magnitude > 3.5"
    query.ReturnGeometry = True
    AddHandler qt.Failed, Sub(a, b) MessageBox.Show("QueryTask failed to execute:" & b.Error.ToString())
    qt.ExecuteAsync(query)

  End Sub

  Private Sub qt_ExecuteCompleted(ByVal sender As Object, ByVal e As QueryEventArgs)
    '			#Region "Since featureset does not include layerdefinition, we would have to populate it with appropriate drawinginfo "

    Dim layerdef As New Dictionary(Of String, Object)()
    Dim defdictionary As New Dictionary(Of String, Object)() From {{"id", 0}, {"name", "Earthquakes from last 7 days"}}

    Dim renderer As New Dictionary(Of String, Object)()
    renderer.Add("type", "simple")
    renderer.Add("style", "esriSMSCircle")

    Dim color() As Integer = {255, 0, 0, 255}
    renderer.Add("color", color)
    renderer.Add("size", 4)

    Dim outlinecolor() As Integer = {0, 0, 0, 255}

    defdictionary.Add("drawingInfo", renderer)

    layerdef.Add("layerDefinition", defdictionary)
    '			#End Region

    'Add a FeatureCollection to operational layers
    Dim featureCollection As FeatureCollection = Nothing

    If e.FeatureSet.Features.Count > 0 Then
      Dim sublayer = New WebMapSubLayer()
      sublayer.FeatureSet = e.FeatureSet

      sublayer.AddCustomProperty("layerDefinition", layerdef)
      featureCollection = New FeatureCollection With {.SubLayers = New List(Of WebMapSubLayer) From {sublayer}}
    End If

    If featureCollection IsNot Nothing Then
      operationLayers.Add(New WebMapLayer With {.FeatureCollection = featureCollection})
    End If


    'Create a new webmap object and add base map and operational layers
    webmap = New WebMap() With {.BaseMap = basemap, .OperationalLayers = operationLayers}

    Dim webmapdoc As New Document()
    AddHandler webmapdoc.GetMapCompleted, Sub(a, b)
                                            If b.Error Is Nothing Then
                                              b.Map.Extent = New ESRI.ArcGIS.Client.Geometry.Envelope(-20000000, 1100000, -3900000, 11000000)
                                              LayoutRoot.Children.Add(b.Map)
                                            End If
                                          End Sub
    webmapdoc.GetMapAsync(webmap)
  End Sub
End Class
