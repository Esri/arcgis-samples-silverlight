Imports Microsoft.VisualBasic
Imports System
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports System.Windows
Imports System.Linq
Imports System.Collections.Generic

Partial Public Class FeatureLayerFromJson
	Inherits UserControl
	Public Sub New()
		InitializeComponent()
		CreateFeatureLayerFromJson()
	End Sub
	Private Sub Button_Load(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
		Try
			Dim featureLayer As FeatureLayer = featureLayer.FromJson(JsonTextBox.Text)
			featureLayer.RendererTakesPrecedence = False
			MyMap.Layers.Add(featureLayer)
		Catch ex As Exception
			MessageBox.Show(ex.Message, "FeatureLayer creation failed", MessageBoxButton.OK)
		End Try
	End Sub

	Private Sub Button_ClearMap(ByVal sender As Object, ByVal e As RoutedEventArgs)
		Dim featureLayers As New List(Of FeatureLayer)()

		For Each layer As Layer In MyMap.Layers
			If TypeOf layer Is FeatureLayer Then
				featureLayers.Add(TryCast(layer, FeatureLayer))
			End If
		Next layer

		For i As Integer = 0 To featureLayers.Count - 1
			MyMap.Layers.Remove(featureLayers(i))
		Next i
	End Sub
	Private Sub CreateFeatureLayerFromJson()
    Dim jsonInput As String = "{" & ControlChars.CrLf & """layerDefinition"": {" &
    ControlChars.CrLf & """name"": ""My Map Notes - Points""," &
    ControlChars.CrLf & """type"": ""Feature Layer""," & ControlChars.CrLf & """displayField"": ""TITLE""," &
    ControlChars.CrLf & """visibilityField"": ""VISIBLE""," &
    ControlChars.CrLf & """geometryType"": ""esriGeometryPoint""," &
    ControlChars.CrLf & """drawingInfo"": {""renderer"": {" & ControlChars.CrLf & """type"": ""uniqueValue""," &
    ControlChars.CrLf & " ""field1"": ""TYPEID""," &
    ControlChars.CrLf & """uniqueValueInfos"": [" & ControlChars.CrLf & "{" &
    ControlChars.CrLf & """value"": ""0""," & ControlChars.CrLf & """label"": ""Stickpin""," &
    ControlChars.CrLf & " ""description"": """"," & ControlChars.CrLf & """symbol"": {" &
    ControlChars.CrLf & " ""type"": ""esriPMS""," &
    ControlChars.CrLf & """url"": ""http://static.arcgis.com/images/Symbols/Basic/GreenStickpin.png""," &
    ControlChars.CrLf & """contentType"": ""image/png""," &
    ControlChars.CrLf & " ""width"": 24," &
    ControlChars.CrLf & " ""height"": 24," &
    ControlChars.CrLf & " ""xoffset"": 0," &
    ControlChars.CrLf & " ""yoffset"": 12" &
    ControlChars.CrLf & "}" & ControlChars.CrLf & "}," &
    ControlChars.CrLf & "{" & ControlChars.CrLf & """value"": ""1""," &
    ControlChars.CrLf & """label"": ""Pushpin""," &
    ControlChars.CrLf & """symbol"": {" &
    ControlChars.CrLf & " ""type"": ""esriPMS""," &
    ControlChars.CrLf & " ""url"": ""http://static.arcgis.com/images/Symbols/Basic/GreenShinyPin.png""," &
    ControlChars.CrLf & " ""contentType"": ""image/png""," & ControlChars.CrLf & " ""width"": 24," &
    ControlChars.CrLf & " ""height"": 24," &
    ControlChars.CrLf & " ""xoffset"": 2," &
    ControlChars.CrLf & " ""yoffset"": 8" &
    ControlChars.CrLf & "}" & ControlChars.CrLf & "}," &
    ControlChars.CrLf & "{" & ControlChars.CrLf & """value"": ""2""," &
    ControlChars.CrLf & """label"": ""Cross""," &
    ControlChars.CrLf & """symbol"": {" &
    ControlChars.CrLf & " ""type"": ""esriSMS""," &
    ControlChars.CrLf & " ""style"": ""esriSMSCross""," &
    ControlChars.CrLf & " ""color"": [" &
    ControlChars.CrLf & "155," &
    ControlChars.CrLf & "187," &
    ControlChars.CrLf & "89," &
    ControlChars.CrLf & "128" &
    ControlChars.CrLf & " ]," &
    ControlChars.CrLf & " ""size"": 18," &
    ControlChars.CrLf & " ""outline"": {" &
    ControlChars.CrLf & """type"": ""esriSLS""," &
    ControlChars.CrLf & """style"": ""esriSLSSolid""," &
    ControlChars.CrLf & """color"": [" & ControlChars.CrLf & "115," & ControlChars.CrLf & "140," & ControlChars.CrLf & "61," & ControlChars.CrLf & "255" & ControlChars.CrLf & "]," &
    ControlChars.CrLf & """width"": 1" & ControlChars.CrLf & " }" & ControlChars.CrLf & "}" &
    ControlChars.CrLf & "}" & ControlChars.CrLf & " ]" & ControlChars.CrLf & "  }}," &
    ControlChars.CrLf & "  ""hasAttachments"": false," &
    ControlChars.CrLf & "  ""objectIdField"": ""OBJECTID""," &
    ControlChars.CrLf & "  ""typeIdField"": ""TYPEID""," &
    ControlChars.CrLf & "  ""fields"": [" &
    ControlChars.CrLf & " {" &
    ControlChars.CrLf & """name"": ""OBJECTID""," &
    ControlChars.CrLf & """type"": ""esriFieldTypeOID""," &
    ControlChars.CrLf & """alias"": ""OBJECTID""," &
    ControlChars.CrLf & """editable"": false" &
    ControlChars.CrLf & " }," & ControlChars.CrLf & " {" &
    ControlChars.CrLf & """name"": ""TITLE""," &
    ControlChars.CrLf & """type"": ""esriFieldTypeString""," &
    ControlChars.CrLf & """alias"": ""Title""," &
    ControlChars.CrLf & """editable"": true," & ControlChars.CrLf & """length"": 50" & ControlChars.CrLf & " }," &
    ControlChars.CrLf & " {" & ControlChars.CrLf & """name"": ""VISIBLE""," &
    ControlChars.CrLf & """type"": ""esriFieldTypeInteger""," & ControlChars.CrLf & """alias"": ""Visible""," &
    ControlChars.CrLf & """editable"": true" & ControlChars.CrLf & " }," &
    ControlChars.CrLf & " {" & ControlChars.CrLf & """name"": ""DESCRIPTION""," &
    ControlChars.CrLf & """type"": ""esriFieldTypeString""," & ControlChars.CrLf & """alias"": ""Description""," &
    ControlChars.CrLf & """editable"": true," & ControlChars.CrLf & """length"": 1073741822" & ControlChars.CrLf & " }," &
    ControlChars.CrLf & " {" & ControlChars.CrLf & """name"": ""IMAGE_URL""," & ControlChars.CrLf & """type"": ""esriFieldTypeString""," &
    ControlChars.CrLf & """alias"": ""Image URL""," &
    ControlChars.CrLf & """editable"": true," & ControlChars.CrLf & """length"": 255" & ControlChars.CrLf & " }," &
    ControlChars.CrLf & " {" & ControlChars.CrLf & """name"": ""IMAGE_LINK_URL""," &
    ControlChars.CrLf & """type"": ""esriFieldTypeString""," & ControlChars.CrLf & """alias"": ""Image Link URL""," &
    ControlChars.CrLf & """editable"": true," & ControlChars.CrLf & """length"": 255" & ControlChars.CrLf & " }," &
    ControlChars.CrLf & " {" & ControlChars.CrLf & """name"": ""DATE""," & ControlChars.CrLf & """type"": ""esriFieldTypeDate""," &
    ControlChars.CrLf & """alias"": ""DATE""," & ControlChars.CrLf & """editable"": true," & ControlChars.CrLf & """length"": 36" & ControlChars.CrLf & " }," &
    ControlChars.CrLf & " {" & ControlChars.CrLf & """name"": ""TYPEID""," & ControlChars.CrLf & """type"": ""esriFieldTypeInteger""," &
    ControlChars.CrLf & """alias"": ""Type ID""," & ControlChars.CrLf & """editable"": true" & ControlChars.CrLf & " }" & ControlChars.CrLf & "  ]," &
    ControlChars.CrLf & "  ""types"": [" & ControlChars.CrLf & " {" & ControlChars.CrLf & """id"": 0," & ControlChars.CrLf & """name"": ""Stickpin""," &
    ControlChars.CrLf & """domains"": {}," & ControlChars.CrLf & """templates"": [{" & ControlChars.CrLf & """name"": ""Stickpin""," &
    ControlChars.CrLf & """description"": """"," & ControlChars.CrLf & """drawingTool"": ""esriFeatureEditToolPoint""," &
    ControlChars.CrLf & """prototype"": {""attributes"": {" & ControlChars.CrLf & " ""TYPEID"": 0," &
    ControlChars.CrLf & " ""VISIBLE"": 1," & ControlChars.CrLf & " ""TITLE"": ""Point""" & ControlChars.CrLf & "}}" &
    ControlChars.CrLf & "}]" & ControlChars.CrLf & " }," & ControlChars.CrLf & " {" & ControlChars.CrLf & """id"": 1," &
    ControlChars.CrLf & """name"": ""Pushpin""," & ControlChars.CrLf & """domains"": {}," & ControlChars.CrLf & """templates"": [{" &
    ControlChars.CrLf & """name"": ""Pushpin""," & ControlChars.CrLf & """description"": """"," &
    ControlChars.CrLf & """drawingTool"": ""esriFeatureEditToolPoint""," & ControlChars.CrLf & """prototype"": {""attributes"": {" &
    ControlChars.CrLf & " ""TYPEID"": 1," & ControlChars.CrLf & " ""VISIBLE"": 1," & ControlChars.CrLf & " ""TITLE"": ""Point""" &
    ControlChars.CrLf & "}}" & ControlChars.CrLf & "}]" & ControlChars.CrLf & " }," & ControlChars.CrLf & " {" & ControlChars.CrLf & """id"": 2," &
    ControlChars.CrLf & """name"": ""Cross""," & ControlChars.CrLf & """domains"": {}," & ControlChars.CrLf & """templates"": [{" &
    ControlChars.CrLf & """name"": ""Cross""," & ControlChars.CrLf & """description"": """"," &
    ControlChars.CrLf & """drawingTool"": ""esriFeatureEditToolPoint""," & ControlChars.CrLf & """prototype"": {""attributes"": {" &
    ControlChars.CrLf & " ""TYPEID"": 2," & ControlChars.CrLf & " ""VISIBLE"": 1," & ControlChars.CrLf & " ""TITLE"": ""Point""" &
    ControlChars.CrLf & "}}" & ControlChars.CrLf & "}]" & ControlChars.CrLf & " }" & ControlChars.CrLf & "  ]," &
    ControlChars.CrLf & "  ""templates"": []," & ControlChars.CrLf & "  ""capabilities"": ""Query,Editing""" &
    ControlChars.CrLf & "}," & ControlChars.CrLf & """featureSet"": {" & ControlChars.CrLf & "  ""geometryType"": ""esriGeometryPoint""," &
    ControlChars.CrLf & "  ""features"": [" & ControlChars.CrLf & " {" & ControlChars.CrLf & """geometry"": {" &
    ControlChars.CrLf & """x"": -1.359478966190899E7," & ControlChars.CrLf & """y"": 6040655.3174108695," &
    ControlChars.CrLf & """spatialReference"": {""wkid"": 102100}" & ControlChars.CrLf & "}," &
    ControlChars.CrLf & """attributes"": {" & ControlChars.CrLf & """TYPEID"": 0," & ControlChars.CrLf & """VISIBLE"": 1," &
    ControlChars.CrLf & """TITLE"": ""Start""," & ControlChars.CrLf & """DATE"": 1299195109191," & ControlChars.CrLf & """OBJECTID"": 0," &
    ControlChars.CrLf & """DESCRIPTION"": ""Start our balloon trip!""" & ControlChars.CrLf & "}," & ControlChars.CrLf & """symbol"": {" &
    ControlChars.CrLf & """angle"": 0," & ControlChars.CrLf & """xoffset"": 12," & ControlChars.CrLf & """yoffset"": 12," &
    ControlChars.CrLf & """type"": ""esriPMS""," &
    ControlChars.CrLf & """url"": ""http://static.arcgis.com/images/Symbols/Basic/GreenFlag.png""," &
    ControlChars.CrLf & """width"": 24," & ControlChars.CrLf & """height"": 24" & ControlChars.CrLf & "}" &
    ControlChars.CrLf & " }," & ControlChars.CrLf & " {" & ControlChars.CrLf & """geometry"": {" & ControlChars.CrLf & """x"": -1.0356305647523504E7," & ControlChars.CrLf & """y"": 4059407.5442596273," &
    ControlChars.CrLf & """spatialReference"": {""wkid"": 102100}" & ControlChars.CrLf & "}," &
    ControlChars.CrLf & """attributes"": {" & ControlChars.CrLf & """TYPEID"": 0," &
    ControlChars.CrLf & """VISIBLE"": 1," & ControlChars.CrLf & """TITLE"": ""End""," &
    ControlChars.CrLf & """DATE"": 1299195283958," & ControlChars.CrLf & """OBJECTID"": 1," &
    ControlChars.CrLf & """DESCRIPTION"": ""Finally reached our destination.""" & ControlChars.CrLf & "}," & ControlChars.CrLf & """symbol"": {" &
    ControlChars.CrLf & """angle"": 0," & ControlChars.CrLf & """xoffset"": 12," &
    ControlChars.CrLf & """yoffset"": 12," & ControlChars.CrLf & """type"": ""esriPMS""," &
    ControlChars.CrLf & """url"": ""http://static.arcgis.com/images/Symbols/Basic/RedFlag.png""," &
    ControlChars.CrLf & """width"": 24," & ControlChars.CrLf & """height"": 24" & ControlChars.CrLf & "}" &
    ControlChars.CrLf & " }," & ControlChars.CrLf & " {" & ControlChars.CrLf & """geometry"": {" &
    ControlChars.CrLf & """x"": -1.3531196670179205E7," & ControlChars.CrLf & """y"": 5908575.52289088," &
    ControlChars.CrLf & """spatialReference"": {""wkid"": 102100}" & ControlChars.CrLf & "}," &
    ControlChars.CrLf & """attributes"": {" & ControlChars.CrLf & """TYPEID"": 1," & ControlChars.CrLf & """VISIBLE"": 1," &
    ControlChars.CrLf & """TITLE"": ""Mt St. Helens""," & ControlChars.CrLf & """DATE"": 1299195143998," & ControlChars.CrLf & """OBJECTID"": 2," &
    ControlChars.CrLf & """DESCRIPTION"": ""Fantastic View!""" & ControlChars.CrLf & "}," & ControlChars.CrLf & """symbol"": {" &
    ControlChars.CrLf & """angle"": 0," & ControlChars.CrLf & """xoffset"": 0," & ControlChars.CrLf & """yoffset"": 12," & ControlChars.CrLf & """type"": ""esriPMS""," &
    ControlChars.CrLf & """url"": ""http://static.arcgis.com/images/Symbols/Basic/BrownStickpin.png""," & ControlChars.CrLf & """width"": 24," &
    ControlChars.CrLf & """height"": 24" & ControlChars.CrLf & "}" & ControlChars.CrLf & " }," & ControlChars.CrLf & " {" & ControlChars.CrLf & """geometry"": {" &
    ControlChars.CrLf & """x"": -1.2406043613821708E7," & ControlChars.CrLf & """y"": 5497650.058829881," &
    ControlChars.CrLf & """spatialReference"": {""wkid"": 102100}" & ControlChars.CrLf & "}," & ControlChars.CrLf & """attributes"": {" &
    ControlChars.CrLf & """TYPEID"": 1," & ControlChars.CrLf & """VISIBLE"": 1," & ControlChars.CrLf & """TITLE"": ""Yellowstone Park""," &
    ControlChars.CrLf & """DATE"": 1299195222797," & ControlChars.CrLf & """OBJECTID"": 3," & ControlChars.CrLf & """DESCRIPTION"": ""Watching herds of buffalo stroll across the moonlit meadows""" &
    ControlChars.CrLf & "}," & ControlChars.CrLf & """symbol"": {" &
    ControlChars.CrLf & """angle"": 0," & ControlChars.CrLf & """xoffset"": 0," & ControlChars.CrLf & """yoffset"": 12," & ControlChars.CrLf & """type"": ""esriPMS""," &
    ControlChars.CrLf & """url"": ""http://static.arcgis.com/images/Symbols/Basic/BrownStickpin.png""," & ControlChars.CrLf & """width"": 24," &
    ControlChars.CrLf & """height"": 24" & ControlChars.CrLf & "}" & ControlChars.CrLf & " }," & ControlChars.CrLf & " {" & ControlChars.CrLf & """geometry"": {" & ControlChars.CrLf & """x"": -1.1231970859361714E7," & ControlChars.CrLf & """y"": 4299117.455318636," &
    ControlChars.CrLf & """spatialReference"": {""wkid"": 102100}" & ControlChars.CrLf & "}," & ControlChars.CrLf & """attributes"": {" &
    ControlChars.CrLf & """TYPEID"": 1," & ControlChars.CrLf & """VISIBLE"": 1," & ControlChars.CrLf & """TITLE"": ""Dust bowl""," &
    ControlChars.CrLf & """DATE"": 1299195352415," & ControlChars.CrLf & """OBJECTID"": 4," &
    ControlChars.CrLf & """DESCRIPTION"": ""Wide open plains, brisk breezes.Hope we don't see any tornados.  """ & ControlChars.CrLf & "}," & ControlChars.CrLf & """symbol"": {" &
    ControlChars.CrLf & """angle"": 0," & ControlChars.CrLf & """xoffset"": 0," & ControlChars.CrLf & """yoffset"": 12," & ControlChars.CrLf & """type"": ""esriPMS""," &
    ControlChars.CrLf & """url"": ""http://static.arcgis.com/images/Symbols/Basic/BrownStickpin.png""," & ControlChars.CrLf & """width"": 24," &
    ControlChars.CrLf & """height"": 24" & ControlChars.CrLf & "}" & ControlChars.CrLf & " }" & ControlChars.CrLf & "  ]" & ControlChars.CrLf & "}," &
    ControlChars.CrLf & """nextObjectId"": 5" & ControlChars.CrLf & "}"


    JsonTextBox.Text = jsonInput
		
	End Sub
End Class
