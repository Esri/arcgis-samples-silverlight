Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports System.Collections.Generic
Imports System


Partial Public Class WebRequestFiltering
    Inherits UserControl
    Public Sub New()
        ' Custom parameters for any url, permanent set for the lifetime of the app 
        'WebRequest.RegisterPrefix(
        '    "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer",
        '    new MyHttpRequestCreator(
        '        new Dictionary<string, string>() { 
        '            {"myParameterName", "0"} 
        '        }));

        InitializeComponent()

        Dim layer = (TryCast(MyMap.Layers(1), ArcGISDynamicMapServiceLayer))

        ' Set value for initial request for dynamic map service layer info 
        SetCustomParameters(layer, New Dictionary(Of String, String) From {{"myParameterName", "1"}})
    End Sub

    Private Sub ArcGISDynamicMapServiceLayer_Initialized(ByVal sender As Object, ByVal e As EventArgs)
        Dim layer = (TryCast(sender, ArcGISDynamicMapServiceLayer))

        ' Set value for subsequent dynamic map service layer requests  (eg. export)
        SetCustomParameters(layer, New Dictionary(Of String, String) From {{"myParameterName", "2"}})
    End Sub

    Private Sub Button_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ' Change parameter value for subsequent requests to dynamic map service layer
        Dim layer = (TryCast(MyMap.Layers(1), ArcGISDynamicMapServiceLayer))
        SetCustomParameters(layer, New Dictionary(Of String, String) From {{"myParameterName", "3"}})
        layer.Refresh()
    End Sub

    Public Shared Function GetCustomParameters(ByVal obj As DependencyObject) As IEnumerable(Of KeyValuePair(Of String, String))
        Return CType(obj.GetValue(CustomParametersProperty), IEnumerable(Of KeyValuePair(Of String, String)))
    End Function

    Public Shared Sub SetCustomParameters(ByVal obj As DependencyObject, ByVal value As IEnumerable(Of KeyValuePair(Of String, String)))
        obj.SetValue(CustomParametersProperty, value)
    End Sub

    Private Shared ReadOnly WebRequestCreatorProperty As DependencyProperty = DependencyProperty.RegisterAttached("WebRequestCreator", GetType(MyHttpRequestCreator), GetType(WebRequestFiltering), Nothing)

    Public Shared ReadOnly CustomParametersProperty As DependencyProperty = DependencyProperty.RegisterAttached("CustomParameters", GetType(IEnumerable(Of KeyValuePair(Of String, String))), GetType(WebRequestFiltering), New PropertyMetadata(Nothing, AddressOf OnCustomParametersPropertyChanged))

    Private Shared Sub OnCustomParametersPropertyChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        If TypeOf d Is ArcGISDynamicMapServiceLayer Then
            Dim newValue = TryCast(e.NewValue, IEnumerable(Of KeyValuePair(Of String, String)))
            Dim creator = TryCast((TryCast(d, ArcGISDynamicMapServiceLayer)).GetValue(WebRequestCreatorProperty), MyHttpRequestCreator)
            If creator Is Nothing Then
                creator = New MyHttpRequestCreator(newValue)
                ' Register prefix for a url with a custom http request creator
                WebRequest.RegisterPrefix((TryCast(d, ArcGISDynamicMapServiceLayer)).Url, creator)
                TryCast(d, ArcGISDynamicMapServiceLayer).SetValue(WebRequestCreatorProperty, creator)
            Else
                creator.SetCallback(newValue)
            End If
        End If
    End Sub
End Class

Public Class MyHttpRequestCreator
    Implements IWebRequestCreate
    Public Sub New(ByVal parameters As IEnumerable(Of KeyValuePair(Of String, String)))
        SetCallback(parameters)
    End Sub

    Public Sub SetCallback(ByVal parameters As IEnumerable(Of KeyValuePair(Of String, String)))
        If parameters Is Nothing Then
            Callback = Nothing
        Else
            Callback = Function(uri)
                           Dim url As String = uri.OriginalString
                           If url.Contains("?") Then
                               If Not url.EndsWith("&") Then
                                   url &= "&"
                               End If
                           Else
                               url &= "?"
                           End If
                           For Each p In parameters
                               url &= String.Format("{0}={1}&", p.Key, uri.EscapeDataString(p.Value))
                           Next p

                           Return New Uri(url, If(uri.IsAbsoluteUri, UriKind.Absolute, UriKind.Relative))
                       End Function
        End If
    End Sub

    Public Function Create(ByVal uri As Uri) As WebRequest Implements IWebRequestCreate.Create
        Return System.Net.Browser.WebRequestCreator.BrowserHttp.Create(If(Callback Is Nothing, uri, Callback(uri)))
    End Function

    Public Property Callback() As Func(Of Uri, Uri)
End Class

