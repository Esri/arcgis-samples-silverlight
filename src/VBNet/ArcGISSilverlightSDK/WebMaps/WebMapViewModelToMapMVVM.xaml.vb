Imports System.ComponentModel
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Portal
Imports ESRI.ArcGIS.Client.Tasks
Imports ESRI.ArcGIS.Client.WebMap
Imports System


Partial Public Class WebMapViewModelToMapMVVM
    Inherits UserControl
    Implements INotifyPropertyChanged
    Private _webMapViewModel As WebMapViewModel

    Public Sub New()
        InitializeComponent()
        Me.DataContext = Me
        LoadWebMap()
    End Sub

    Private Async Sub LoadWebMap()
        Try
            Dim portal As New ArcGISPortal() With {.Url = "http://www.arcgis.com/sharing/rest"}
            Dim portalItem As New ArcGISPortalItem(portal) With {.Id = "00e5e70929e14055ab686df16c842ec1"}

            Dim webMap As WebMap = Await webMap.FromPortalItemTaskAsync(portalItem)

            MyWebMapViewModel = Await WebMapViewModel.LoadAsync(webMap, portalItem.ArcGISPortal)
        Catch ex As Exception
            If TypeOf ex Is ServiceException Then
                MessageBox.Show(String.Format("{0}: {1}", (TryCast(ex, ServiceException)).Code.ToString(), (TryCast(ex, ServiceException)).Details(0)), "Error", MessageBoxButton.OK)
                Return
            End If
        End Try
    End Sub

    Public Property MyWebMapViewModel() As WebMapViewModel
        Get
            Return _webMapViewModel
        End Get
        Set(ByVal value As WebMapViewModel)
            If _webMapViewModel IsNot value Then
                _webMapViewModel = value
                OnPropertyChanged("MyWebMapViewModel")
            End If
        End Set
    End Property

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Private Sub OnPropertyChanged(ByVal propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    Public Shared Function GetMapInitialExtent(ByVal obj As DependencyObject) As Envelope
        Return CType(obj.GetValue(MapInitialExtentProperty), Envelope)
    End Function

    Public Shared Sub SetMapInitialExtent(ByVal obj As DependencyObject, ByVal value As Envelope)
        obj.SetValue(MapInitialExtentProperty, value)
    End Sub

    Public Shared ReadOnly MapInitialExtentProperty As DependencyProperty = DependencyProperty.RegisterAttached("MapInitialExtent", GetType(Envelope), GetType(WebMapViewModelToMapMVVM), New PropertyMetadata(AddressOf OnMapInitialExtentChanged))

    Private Shared Sub OnMapInitialExtentChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim map = TryCast(d, ESRI.ArcGIS.Client.Map)
        Dim extent = TryCast(e.NewValue, Envelope)
        If map Is Nothing OrElse extent Is Nothing Then
            Return
        End If
        map.Extent = extent
    End Sub
End Class

