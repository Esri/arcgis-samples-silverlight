'INSTANT VB NOTE: This code snippet uses implicit typing. You will need to set 'Option Infer On' in the VB file or set 'Option Infer' at the project level.

Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Linq
Imports System.Reflection
Imports System.Threading
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Printing
Imports System.Windows.Threading
Imports ESRI.ArcGIS.Client

Partial Public Class ClientPrinting
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub ActivatePrintPreview(ByVal sender As Object, ByVal e As RoutedEventArgs)
        PrintPreview.Visibility = Visibility.Visible
        MyMapPrinter.Map = MyMap ' sets the Map to print and initializes the PrintMap with a cloned map (as defined in the print style)
    End Sub

    Private Sub DesactivatePrintPreview(ByVal sender As Object, ByVal e As RoutedEventArgs)
        PrintPreview.Visibility = Visibility.Collapsed
        MyMapPrinter.Map = Nothing ' cancel the current print and frees the cloned map
    End Sub

    Private Sub OnPreviewSizeChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
        ' Chnage the preview size of the print map.
        ' Note that this size will be overwritten during the print process by the real print area of the printer (depending on print parameters: paper size, orientation,...)
        Dim previewSize = TryCast((CType(sender, ComboBox)).SelectedItem, PreviewSize)
        If previewSize IsNot Nothing AndAlso MyMapPrinter IsNot Nothing Then
            MyMapPrinter.SetPrintableArea(previewSize.Height, previewSize.Width)
        End If
    End Sub

    Private Sub OnPrint(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ' Start the print process
        MyMapPrinter.Print()
    End Sub
End Class

' Main print control that displays the map using the print template
Public Class MapPrinter
    Inherits Control
    Implements INotifyPropertyChanged
    Public Sub New()
        _isPrinting = False
        DataContext = Me ' simplify binding in print styles
    End Sub

    ' Executed when the print template changed
    Public Overrides Sub OnApplyTemplate()
        Dim extent = If(PrintMap Is Nothing, Nothing, PrintMap.Extent) ' save the current print extent that will be lost after OnApplyTemplate (since the PrintMap changes)
        MyBase.OnApplyTemplate()
        PrintMap = TryCast(GetTemplateChild("PrintMap"), ESRI.ArcGIS.Client.Map)
        PrintMap.Extent = If(extent, Map.Extent) ' restore previous print extent or init it with the current map extent
    End Sub

    ' Map to print (Dependency Property)
    Public Property Map() As ESRI.ArcGIS.Client.Map
        Get
            Return CType(GetValue(MapProperty), ESRI.ArcGIS.Client.Map)
        End Get
        Set(ByVal value As ESRI.ArcGIS.Client.Map)
            SetValue(MapProperty, value)
        End Set
    End Property

    Public Shared ReadOnly MapProperty As DependencyProperty = DependencyProperty.Register("Map", GetType(ESRI.ArcGIS.Client.Map), GetType(MapPrinter), New PropertyMetadata(Nothing, AddressOf OnMapChanged))

    Private Shared Sub OnMapChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim mapPrinter = TryCast(d, MapPrinter)
        Dim newMap = TryCast(e.NewValue, ESRI.ArcGIS.Client.Map)
        If mapPrinter IsNot Nothing Then
            If newMap IsNot Nothing AndAlso mapPrinter.PrintMap IsNot Nothing Then
                mapPrinter.PrintMap.Extent = newMap.Extent
            End If
            If newMap Is Nothing AndAlso mapPrinter.IsPrinting Then
                mapPrinter.IsCancelingPrint = True
            End If
        End If
    End Sub

    ' Title of the print document (Dependency Property)
    Public Property Title() As String
        Get
            Return CStr(GetValue(TitleProperty))
        End Get
        Set(ByVal value As String)
            SetValue(TitleProperty, value)
        End Set
    End Property

    Public Shared ReadOnly TitleProperty As DependencyProperty = DependencyProperty.Register("Title", GetType(String), GetType(MapPrinter), New PropertyMetadata("Map Document"))

    ' Flag indicating that the map must be rotated 90° 
    Public Property RotateMap() As Boolean
        Get
            Return CBool(GetValue(RotateMapProperty))
        End Get
        Set(ByVal value As Boolean)
            SetValue(RotateMapProperty, value)
        End Set
    End Property

    Public Shared ReadOnly RotateMapProperty As DependencyProperty = DependencyProperty.Register("RotateMap", GetType(Boolean), GetType(MapPrinter), New PropertyMetadata(False, AddressOf OnRotateMapChanged))

    Private Shared Sub OnRotateMapChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim mapPrinter = TryCast(d, MapPrinter)
        If mapPrinter IsNot Nothing Then
            mapPrinter.PrintMap.Rotation = (If(CBool(e.NewValue), -90, 0))
        End If
    End Sub

    Private _isPrinting As Boolean
    ' Indicates if a print task is going on.
    Public Property IsPrinting() As Boolean
        Get
            Return _isPrinting
        End Get
        Private Set(ByVal value As Boolean)
            If value <> _isPrinting Then
                _isPrinting = value
                NotifyPropertyChanged("IsPrinting")
            End If
        End Set
    End Property

    ' The print map (defined in the print template)
    Private privatePrintMap As ESRI.ArcGIS.Client.Map
    Public Property PrintMap() As ESRI.ArcGIS.Client.Map
        Get
            Return privatePrintMap
        End Get
        Private Set(ByVal value As ESRI.ArcGIS.Client.Map)
            privatePrintMap = value
        End Set
    End Property

    ' Gets the current date/time.
    Public ReadOnly Property Now() As Date
        Get
            Return Date.Now
        End Get
    End Property

    ' Start the print process (by delagating either to the Silverlight print engine or to the WPF print engine)
    Public Sub Print()
        If IsPrinting Then
            Return
        End If

        ' Create the print engine depending on silverlight/WPF
        Dim printEngine = New SilverlightPrintEngine(Me)

        ' Call the print engine doing the work
        Try
            printEngine.Print()
        Catch e As Exception
            EndPrint(e)
        End Try
    End Sub

    ' InotifyPropertyChanged implementation
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    ' Notifies the property changed.
    Protected Sub NotifyPropertyChanged(ByVal propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    ' Internal methods/properties
    Friend Property IsCancelingPrint() As Boolean

    Friend Sub BeginPrint()
        IsCancelingPrint = False
        IsPrinting = True

        NotifyPropertyChanged("Now") ' in case time is displayed
    End Sub

    Friend Sub EndPrint(ByVal [error] As Exception)
        If [error] IsNot Nothing AndAlso (Not IsCancelingPrint) Then
            MessageBox.Show(String.Format("Error during print: {0}", [error].Message))
        End If
        IsPrinting = False
        IsCancelingPrint = False
    End Sub

    Friend Sub SetPrintableArea(ByVal printableAreaHeight As Double, ByVal printableAreaWidth As Double)
        ' Recalculate layout in order to fit printable area
        Height = printableAreaHeight
        Width = printableAreaWidth

        ' Update map size
        UpdateLayout()
    End Sub
End Class

' Collection of PreviewSize (creatable in XAML)
Public Class PreviewSizes
    Inherits ObservableCollection(Of PreviewSize)
End Class

' Represents a Preview Size (creatable in XAML)
Public Class PreviewSize
    Public Property Id() As String
    Public Property Width() As Double
    Public Property Height() As Double
End Class

' Silverlight PrintEngine class: Print a map by using a SL PrintDocument
Friend Class SilverlightPrintEngine
    Private ReadOnly _mapPrinter As MapPrinter

    ' Used during print
    Private _isLoading As Boolean
    Private _isReady As Boolean
    Private _tryCount As Integer
    Private _mapLoader As MapLoader

    Public Sub New(ByVal mapPrinter As MapPrinter)
        _mapPrinter = mapPrinter
    End Sub

    Public Sub Print()
        Dim doc = New PrintDocument()

        AddHandler doc.BeginPrint, Sub(s, e) BeginPrint()
        AddHandler doc.PrintPage, AddressOf PrintPage
        AddHandler doc.EndPrint, Sub(s, e) EndPrint(If(e IsNot Nothing, e.Error, Nothing))

        doc.Print(If(String.IsNullOrEmpty(_mapPrinter.Title), "Map Print", _mapPrinter.Title), Nothing)
    End Sub

    Private Sub BeginPrint()
        _mapPrinter.BeginPrint()

        _mapLoader = New MapLoader(_mapPrinter.PrintMap)
        AddHandler _mapLoader.Loaded, AddressOf OnMapLoaderLoaded
        _isLoading = False
        _tryCount = 0
        _isReady = False
    End Sub

    Private Sub OnMapLoaderLoaded(ByVal sender As Object, ByVal e As EventArgs)
        ' All layers are loaded in the map --> ready to print (next time PrintPage will be called by SL framework)
        _isLoading = False
        _mapPrinter.UpdateLayout() ' to be sure all tiles will be displayed
    End Sub

    Private Sub EndPrint(ByVal [error] As Exception)
        _mapPrinter.EndPrint([error])
        RemoveHandler _mapLoader.Loaded, AddressOf OnMapLoaderLoaded
        _mapLoader = Nothing
    End Sub

    Private Sub PrintPage(ByVal sender As Object, ByVal e As PrintPageEventArgs)
        e.PageVisual = Nothing

        _tryCount += 1
        If _mapPrinter.IsCancelingPrint Then
            ' Print has been canceled by user
            e.HasMorePages = False ' Note that despite this setting to false, the framework will continue to call PrintPage 7 times
            Return
        End If

        If _tryCount = 1 Then
            ' Set the printable area size which is unknown before the print of the first page
            Dim extent = _mapPrinter.PrintMap.Extent
            _mapPrinter.SetPrintableArea(e.PrintableArea.Height, e.PrintableArea.Width)

            ' change the extent of the map and wait for all layers loaded (progress == 100)
            _isLoading = True
            _mapLoader.WaitForLoaded()
            _mapPrinter.PrintMap.Extent = extent
            e.HasMorePages = True ' retry later but nothing to print at this time
        Else
            _mapPrinter.UpdateLayout()
            If _isLoading AndAlso _tryCount <= 8 Then
                ' Wait for loaded layers
                e.HasMorePages = True ' retry later

                Thread.Sleep(100 + 300 * _tryCount) ' sleep to give a chance to load layers before the maximum of 7 tries
            Else
                If _isReady OrElse _tryCount > 8 Then
                    ' Print the page
                    e.HasMorePages = False
                    e.PageVisual = _mapPrinter
                Else
                    ' FeatureLayers in OnDemand mode need to be rendered once in order to be printable --> wait once more
                    e.HasMorePages = True
                    _isReady = True
                End If
            End If
        End If
    End Sub
End Class

' Helper class to know when a map is loaded and so ready to print.
' It's waiting for progress = 100 but sometimes this event never comes (e.g. with a dynamic layer when the image is in the cache)
' So there is a timer to avoid infinite wait.
Friend Class MapLoader
    Private ReadOnly _map As ESRI.ArcGIS.Client.Map
    Private ReadOnly _timer As DispatcherTimer
    Private _isProgressing As Boolean ' some progress events came up

    Public Sub New(ByVal map As ESRI.ArcGIS.Client.Map)
        _map = map
        _timer = New DispatcherTimer()
        AddHandler _timer.Tick, AddressOf OnTimerTick
        _isProgressing = False
    End Sub

    ' Waits for the map loaded.
    Public Sub WaitForLoaded()
        AddHandler _map.Progress, AddressOf OnMapProgress ' subscribe to OnProgress event
        If _timer.IsEnabled Then
            _timer.Stop()
        End If
        _timer.Interval = TimeSpan.FromSeconds(10) ' Wait 10 seconds before the first mapprogress event, after that, consider that the map was already ready
        _timer.Start()
    End Sub

    ''' Cancels the wait.
    Public Sub CancelWait()
        _timer.Stop()
        RemoveHandler _map.Progress, AddressOf OnMapProgress
    End Sub

    ' Occurs when the map is loaded.
    Public Event Loaded As EventHandler(Of EventArgs)
    Private Sub OnLoaded()
        CancelWait()
        RaiseEvent Loaded(Me, New EventArgs())
    End Sub

    ' Security timer to avoid infinite waiting (not useful with Silverlight which anyway calls PrintPage only 7 times)
    Private Sub OnTimerTick(ByVal sender As Object, ByVal e As EventArgs)
        If _isProgressing Then
            ' Progress events are coming -> wait more
            _isProgressing = False
            _timer.Interval = TimeSpan.FromSeconds(30)
        Else
            ' No progress event since last test --> stop and consider the map as loaded
            OnLoaded()
        End If
    End Sub

    Private Sub OnMapProgress(ByVal sender As Object, ByVal e As ProgressEventArgs)
        _isProgressing = True
        If e.Progress = 100 Then
            OnLoaded() ' map is ready
        End If
    End Sub
End Class

' Define an attached property allowing to initialize a map by cloning the layers of another map.
Public NotInheritable Class CloneMap
    ' Map to clone attached property
    Private Sub New()
    End Sub
    Public Shared Function GetMap(ByVal obj As DependencyObject) As String
        Return CType(obj.GetValue(MapProperty), String)
    End Function

    Public Shared Sub SetMap(ByVal obj As DependencyObject, ByVal value As String)
        obj.SetValue(MapProperty, value)
    End Sub

    Public Shared ReadOnly MapProperty As DependencyProperty = DependencyProperty.RegisterAttached("Map", GetType(ESRI.ArcGIS.Client.Map), GetType(CloneMap), New PropertyMetadata(Nothing, AddressOf OnMapChanged))

    Private Shared Sub OnMapChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim map = TryCast(d, ESRI.ArcGIS.Client.Map)
        If map Is Nothing Then
            Return
        End If
        Dim mapToClone = CType(e.NewValue, ESRI.ArcGIS.Client.Map)

        map.Layers.Clear()
        If mapToClone IsNot Nothing Then
            Clone(map, mapToClone)
        End If
    End Sub

    ' Clone a Map
    Private Shared Sub Clone(ByVal map As ESRI.ArcGIS.Client.Map, ByVal mapToClone As ESRI.ArcGIS.Client.Map)
        map.MinimumResolution = mapToClone.MinimumResolution
        map.MaximumResolution = mapToClone.MaximumResolution
        map.TimeExtent = mapToClone.TimeExtent
        map.WrapAround = mapToClone.WrapAround

        ' Clone layers
        For Each toLayer In mapToClone.Layers.Select(AddressOf CloneLayer).Where(Function(to_layer) to_layer IsNot Nothing)
            AddHandler toLayer.InitializationFailed, Sub(s, e)
                                                     End Sub ' to avoid crash if bad layer
            map.Layers.Add(toLayer) ' use index in order to keep existing layers after cloned layers
        Next toLayer
    End Sub

    ' Clone a Layer
    Private Shared Function CloneLayer(ByVal layer As Layer) As Layer
        Dim toLayer As Layer
        Dim featureLayer = TryCast(layer, FeatureLayer)

        If TypeOf layer Is GraphicsLayer AndAlso (featureLayer Is Nothing OrElse featureLayer.Url Is Nothing OrElse featureLayer.Mode <> featureLayer.QueryMode.OnDemand) Then
            ' Clone the layer and the graphics
            Dim fromLayer = TryCast(layer, GraphicsLayer)
            Dim printLayer = New GraphicsLayer With {.Renderer = fromLayer.Renderer, .Clusterer = If(fromLayer.Clusterer Is Nothing, Nothing, fromLayer.Clusterer.Clone()), .ShowLegend = fromLayer.ShowLegend, .RendererTakesPrecedence = fromLayer.RendererTakesPrecedence, .ProjectionService = fromLayer.ProjectionService}
            toLayer = printLayer

            Dim graphicCollection = New GraphicCollection()
            For Each graphic In fromLayer.Graphics
                Dim clone = New Graphic()

                For Each kvp In graphic.Attributes
                    If TypeOf kvp.Value Is DependencyObject Then
                        ' If the attribute is a dependency object --> clone it
                        Dim clonedkvp = New KeyValuePair(Of String, Object)(kvp.Key, (TryCast(kvp.Value, DependencyObject)).Clone())
                        clone.Attributes.Add(clonedkvp)
                    Else
                        clone.Attributes.Add(kvp)
                    End If
                Next kvp
                clone.Geometry = graphic.Geometry
                clone.Symbol = graphic.Symbol
                clone.Selected = graphic.Selected
                clone.TimeExtent = graphic.TimeExtent
                graphicCollection.Add(clone)
            Next graphic

            printLayer.Graphics = graphicCollection

            toLayer.ID = layer.ID
            toLayer.Opacity = layer.Opacity
            toLayer.Visible = layer.Visible
            toLayer.MaximumResolution = layer.MaximumResolution
            toLayer.MinimumResolution = layer.MinimumResolution
        Else
            ' Clone other layer types
            toLayer = layer.Clone()

            If TypeOf layer Is GroupLayerBase Then
                ' Clone sublayers (not cloned in Clone() to avoid issue with graphicslayer)
                Dim childLayers = New LayerCollection()
                For Each subLayer As Layer In (TryCast(layer, GroupLayerBase)).ChildLayers
                    Dim toSubLayer = CloneLayer(subLayer)

                    If toSubLayer IsNot Nothing Then
                        AddHandler toSubLayer.InitializationFailed, Sub(s, e)
                                                                    End Sub ' to avoid crash if bad layer

							childLayers.Add(toSubLayer)
						End If
					Next subLayer
					CType(toLayer, GroupLayerBase).ChildLayers = childLayers
				End If
			End If
			Return toLayer
		End Function
End Class

' Generic class extention for cloning recursively a dependency object
' Very limited implementation based on CLR properties
' Attached properties are not taken in care except specific case for this sample
' Is used to clone Layers and Elements of ElementLayer
Public Module CloneExtension
    ' Clones a dependency object.
    <System.Runtime.CompilerServices.Extension> _
    Public Function Clone(Of T As DependencyObject)(ByVal source As T) As T
        Dim _t As Type = source.GetType() ' can be different from typeof(T)
        Dim printclone = CType(Activator.CreateInstance(_t), T)

        ' Loop on CLR properties (except name, parent and graphics)
        For Each propertyInfo As PropertyInfo In _t.GetProperties()
            If propertyInfo.Name = "Name" OrElse propertyInfo.Name = "Parent" OrElse propertyInfo.Name = "Graphics" OrElse propertyInfo.Name = "ChildLayers" OrElse (Not propertyInfo.CanRead) OrElse propertyInfo.GetGetMethod() Is Nothing OrElse propertyInfo.GetIndexParameters().Length > 0 Then
                Continue For
            End If

            Try
                Dim value As Object = propertyInfo.GetValue(source, Nothing)
                If value IsNot Nothing Then
                    If propertyInfo.PropertyType.GetInterface("IList", True) IsNot Nothing AndAlso (Not propertyInfo.PropertyType.IsArray) Then
                        ' Collection ==> loop on items and clone them (we suppose the collection itself is already initialized!)
                        Dim count = CInt(Fix(propertyInfo.PropertyType.InvokeMember("get_Count", BindingFlags.InvokeMethod, Nothing, value, Nothing)))
                        propertyInfo.PropertyType.InvokeMember("Clear", BindingFlags.InvokeMethod, Nothing, propertyInfo.GetValue(printclone, Nothing), Nothing) ' without this line, text can be duplicated due to inlines objects added after text is set

                        For index As Integer = 0 To count - 1
                            Dim itemValue As Object = propertyInfo.PropertyType.InvokeMember("get_Item", BindingFlags.InvokeMethod, Nothing, propertyInfo.GetValue(source, Nothing), New Object() {index})
                            propertyInfo.PropertyType.InvokeMember("Add", BindingFlags.InvokeMethod, Nothing, propertyInfo.GetValue(printclone, Nothing), {CloneDependencyObject(itemValue)})
                        Next index
                    ElseIf propertyInfo.CanWrite AndAlso propertyInfo.GetSetMethod() IsNot Nothing Then
                        propertyInfo.SetValue(printclone, CloneDependencyObject(value), Nothing)
                    End If
                End If
            Catch e1 As Exception
            End Try
        Next propertyInfo

        ' Copy some useful attached properties (not done by reflection)
        If TypeOf source Is UIElement Then
            Dim attachedProperty As DependencyProperty = ESRI.ArcGIS.Client.ElementLayer.EnvelopeProperty ' needed for ElementLayer
            SetDependencyProperty(attachedProperty, source, TryCast(printclone, DependencyObject))
        End If

        Return TryCast(printclone, T)
    End Function

    Private Function CloneDependencyObject(ByVal source As Object) As Object
        Return If(TypeOf source Is DependencyObject AndAlso Not (TypeOf source Is ControlTemplate), (TryCast(source, DependencyObject)).Clone(), source)
    End Function

    Private Sub SetDependencyProperty(ByVal dp As DependencyProperty, ByVal source As DependencyObject, ByVal clone As DependencyObject)
        Dim value As Object = source.GetValue(dp)
        If value IsNot Nothing Then
            clone.SetValue(dp, CloneDependencyObject(value))
        End If
    End Sub
End Module