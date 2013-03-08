Imports System
Imports System.Collections.Generic
Imports System.Json
Imports System.Linq
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Json
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client

Partial Public Class ArcGISWebClientSimple
    Inherits UserControl
    Private _serverUri As Uri
    Private _webclient As ArcGISWebClient

    Public Sub New()
        InitializeComponent()

        _webclient = New ArcGISWebClient()
        AddHandler _webclient.OpenReadCompleted, AddressOf webclient_OpenReadCompleted
        AddHandler _webclient.DownloadStringCompleted, AddressOf webclient_DownloadStringCompleted
    End Sub

    ' Get a list of services for an ArcGIS Server site
    Private Sub Button_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        _serverUri = New Uri(MySvrTextBox.Text)

        ' Add the parameter f=json to return a response in json format
        Dim parameters As IDictionary(Of String, String) = New Dictionary(Of String, String)()
        parameters.Add("f", "json")

        ' If another request is in progress, cancel it
        If _webclient.IsBusy Then
            _webclient.CancelAsync()
        End If

        _webclient.OpenReadAsync(_serverUri, parameters)
    End Sub

    ' Show a list of map services in the Listbox
    Private Sub webclient_OpenReadCompleted(ByVal sender As Object, ByVal e As ArcGISWebClient.OpenReadCompletedEventArgs)
        Try
            If e.Error IsNot Nothing Then
                Throw New Exception(e.Error.Message)
            End If

            ' Deserialize response using classes defined by a data contract, included in this class definition below
            Dim serializer As New DataContractJsonSerializer(GetType(MySvcs))
            Dim mysvcs As MySvcs = TryCast(serializer.ReadObject(e.Result), MySvcs)

            If mysvcs.Services.Count = 0 Then
                Throw New Exception("No services returned")
            End If

            ' Use LINQ to return all map services
            Dim mapSvcs = From s In mysvcs.Services
                          Where s.Type = "MapServer"
                          Select s

            ' If map services are returned, show the Listbox with items as map services
            If mapSvcs.Count() > 0 Then
                MySvcTreeView.ItemsSource = mapSvcs
                MySvcTreeView.Visibility = System.Windows.Visibility.Visible
                NoMapServicesTextBlock.Visibility = System.Windows.Visibility.Collapsed
            Else
                MySvcTreeView.Visibility = System.Windows.Visibility.Collapsed
                NoMapServicesTextBlock.Visibility = System.Windows.Visibility.Visible
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        Finally
            If e.Result IsNot Nothing Then
                e.Result.Close()
            End If
        End Try
    End Sub

    ' Stores site list of map services
    <DataContract>
    Public Class MySvcs
        <DataMember(Name:="services")>
        Public Property Services() As IList(Of MySvc)
    End Class

    ' Defines service item properties in a site list
    <DataContract>
    Public Class MySvc
        <DataMember(Name:="name")>
        Public Property Name() As String

        <DataMember(Name:="type")>
        Public Property Type() As String
    End Class

    ' When item (map service) in Listbox is selected, construct service url
    Private Sub MySvcTreeView_SelectedItemChanged(ByVal sender As Object, ByVal e As System.Windows.RoutedPropertyChangedEventArgs(Of Object))
        If e.NewValue IsNot Nothing Then
            If _webclient.IsBusy Then
                _webclient.CancelAsync()
            End If

            ' Clear layers and set Extent to null (reset spatial reference)
            MyMap.Layers.Clear()
            MyMap.Extent = Nothing

            ' Get the service item selected
            Dim svc As MySvc = TryCast(e.NewValue, MySvc)

            ' Construct the url to the map service
            Dim svcUrl As String = String.Format("{0}/{1}/{2}", _serverUri, svc.Name, svc.Type)

            Dim svcParameters As IDictionary(Of String, String) = New Dictionary(Of String, String)()
            svcParameters.Add("f", "json")

            ' Pass the map service url as an user object for the handler
            _webclient.DownloadStringAsync(New Uri(svcUrl), svcParameters, ArcGISWebClient.HttpMethods.Auto, svcUrl)
        End If
    End Sub

    ' When map service item selected in Listbox, choose appropriate type and add to the map
    Private Sub webclient_DownloadStringCompleted(ByVal sender As Object, ByVal e As ArcGISWebClient.DownloadStringCompletedEventArgs)
        Try
            If e.Error IsNot Nothing Then
                Throw New Exception(e.Error.Message)
            End If

            ' Get the service url from the user object
            Dim svcUrl As String = TryCast(e.UserState, String)

            ' Abstract JsonValue holds json response
            Dim serviceInfo As JsonValue = JsonObject.Parse(e.Result)
            ' Use "singleFusedMapCache" to determine if a tiled or dynamic layer should be added to the map
            Dim isTiledMapService As Boolean = Boolean.Parse(serviceInfo("singleFusedMapCache").ToString())

            Dim lyr As Layer = Nothing

            If isTiledMapService Then
                lyr = New ArcGISTiledMapServiceLayer() With {.Url = svcUrl}
            Else
                lyr = New ArcGISDynamicMapServiceLayer() With {.Url = svcUrl}
            End If

            If lyr IsNot Nothing Then
                AddHandler lyr.InitializationFailed, Sub(a, b) Throw New Exception(lyr.InitializationFailure.Message)
                MyMap.Layers.Add(lyr)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub
End Class