Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Runtime.Serialization.Json
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports System.Json

Partial Public Class GenerateFeatures
    Inherits UserControl

    Private _openFileDialog As OpenFileDialog

    Public Sub New()
        InitializeComponent()
        _openFileDialog = New OpenFileDialog()
        _openFileDialog.Filter = "zip Files | *.zip"
    End Sub

    Private Sub BrowseButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ' Call the ShowDialog method to show the dialog box.
        Dim userClickedOK? As Boolean = _openFileDialog.ShowDialog()

        ' Process input if the user clicked OK.
        If userClickedOK = True Then
            ' Open the selected file to read.
            Dim file As System.IO.FileInfo = _openFileDialog.File

            'Uri to the ArcGIS Portal API generate operation.
            'Reference documentation available here: http://www.arcgis.com/apidocs/rest/generate.html
            Dim address As New Uri("http://www.arcgis.com/sharing/rest/content/features/generate")

            'Get the file contents for the local file
            Dim fs As FileStream = file.OpenRead()

            'Create ArcGISWebClient.StreamContent instance 
            Dim strmContent As New ArcGISWebClient.StreamContent() With
                {.Name = "file",
                 .Filename = file.Name,
                 .Stream = fs,
                 .ContentType = "application/zip"
                }

            'Create a list of stream content to POST
            Dim filestream As IList(Of ArcGISWebClient.StreamContent) = New List(Of ArcGISWebClient.StreamContent)()
            filestream.Add(strmContent)

            'Create dictionary to store parameter to POST 
            Dim postParameters As New Dictionary(Of String, String)()

            ' Calculate the current map resolution.
            Dim resolution As Double = MyMap.Extent.Width / MyMap.Width

            'A class created to store publish parameters for the generate operation
            Dim param As New GenerateFeaturesParams() With
                {
                    .name = file.Name.Substring(0, file.Name.Length - 4),
                    .maxRecordCount = 1000,
                    .generalize = False,
                    .reducePrecision = True,
                    .targetSR = MyMap.SpatialReference}


            'Must specify the output type (json) the file type (shapefile) and the publish parameters
            postParameters.Add("f", "json")
            postParameters.Add("filetype", "shapefile")
            postParameters.Add("publishParameters", SerializeToJsonString(param))

            'Url to the generate operation, part of the ArcGIS Portal REST API (http://www.arcgis.com/apidocs/rest/generate.html)
            Dim postURL As String = "http://www.arcgis.com/sharing/rest/content/features/generate"

            ' Use ArcGISWebClient POST shapefile to the ArcGIS Portal generate operation.  The generate operation requires a file to be passed
            ' in a multi-part post request.
            Dim agsWebClient As New ArcGISWebClient()
            AddHandler agsWebClient.PostMultipartCompleted, Sub(a, b)
                                                                If b.Error Is Nothing Then
                                                                    Try
                                                                        ' Use the the generic JsonValue to handle dynamic json content.
                                                                        ' In this case, generate always returns a "featureCollection" object which contains
                                                                        ' a "layers" array with one feature layer.  
                                                                        Dim featureCollection As JsonValue = JsonValue.Load(b.Result)
                                                                        Dim layer As String = featureCollection("featureCollection")("layers")(0).ToString()

                                                                        Dim featureLayer As FeatureLayer = featureLayer.FromJson(layer)

                                                                        If featureLayer IsNot Nothing Then
                                                                            ' Add the feature layer to the map and zoom to it
                                                                            MyMap.Layers.Add(featureLayer)
                                                                            MyMap.ZoomTo(featureLayer.FullExtent.Expand(1.25))
                                                                        End If
                                                                    Catch ex As Exception
                                                                        MessageBox.Show(ex.Message, "FeatureLayer creation failed", MessageBoxButton.OK)
                                                                    End Try
                                                                End If
                                                            End Sub
            agsWebClient.PostMultipartAsync(New Uri(postURL), postParameters, filestream, Nothing)

        End If
    End Sub

    'See MSDN for more info: http://msdn.microsoft.com/en-us/library/bb412179(VS.100).aspx
    Public Shared Function SerializeToJsonString(ByVal objectToSerialize As Object) As String
        Using ms As New MemoryStream()
            Dim serializer As New DataContractJsonSerializer(objectToSerialize.GetType())
            serializer.WriteObject(ms, objectToSerialize)
            ms.Position = 0
            Using reader As New StreamReader(ms)
                Return reader.ReadToEnd()
            End Using
        End Using
    End Function

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
End Class

' Parameter class for Generate Features.
Partial Public Class GenerateFeaturesParams
    Public Property name() As String
    Public Property targetSR() As SpatialReference
    Public Property maxRecordCount() As Integer
    Public Property enforceInputFileSizeLimit() As Boolean
    Public Property enforceOutputJsonSizeLimit() As Boolean
    Public Property generalize() As Boolean
    Public Property reducePrecision() As Boolean
    Public Property maxAllowableOffset() As Double
End Class