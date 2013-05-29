Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks


    Partial Public Class Find
        Inherits UserControl
        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub ExecuteButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
            graphicsLayer.Graphics.Clear()

            Dim findTask As New FindTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer")
            AddHandler findTask.Failed, AddressOf FindTask_Failed

            Dim findParameters As New FindParameters()
            ' Layer ids to search
            findParameters.LayerIds.AddRange(New Integer() {0, 1, 2})
            ' Fields in layers to search
            findParameters.SearchFields.AddRange(New String() {"CITY_NAME", "NAME", "SYSTEM", "STATE_ABBR", "STATE_NAME"})
            ' Return features in map's spatial reference
            findParameters.SpatialReference = MyMap.SpatialReference

            ' Bind data grid to find results.  Bind to the LastResult property which returns a list
            ' of FindResult instances.  When LastResult is updated, the ItemsSource property on the 
            ' will update.  
            Dim resultFeaturesBinding As New Binding("LastResult")
            resultFeaturesBinding.Source = findTask
            FindDetailsDataGrid.SetBinding(DataGrid.ItemsSourceProperty, resultFeaturesBinding)

            findParameters.SearchText = FindText.Text
            findTask.ExecuteAsync(findParameters)

            ' Since binding to DataGrid, handling the ExecuteComplete event is not necessary.
        End Sub

        Private Sub FindDetails_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
            ' Highlight the graphic feature associated with the selected row
            Dim dataGrid As DataGrid = TryCast(sender, DataGrid)

            Dim selectedIndex As Integer = dataGrid.SelectedIndex
            If selectedIndex > -1 Then
                Dim findResult As FindResult = CType(FindDetailsDataGrid.SelectedItem, FindResult)
                Dim graphic As Graphic = findResult.Feature

                Select Case graphic.Attributes("Shape").ToString()
                    Case "Polygon"
                        graphic.Symbol = TryCast(LayoutRoot.Resources("DefaultFillSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
                    Case "Polyline"
                        graphic.Symbol = TryCast(LayoutRoot.Resources("DefaultLineSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
                    Case "Point"
                        graphic.Symbol = TryCast(LayoutRoot.Resources("DefaultMarkerSymbol"), ESRI.ArcGIS.Client.Symbols.Symbol)
                End Select

                Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)
                graphicsLayer.Graphics.Clear()
                graphicsLayer.Graphics.Add(graphic)
            End If
        End Sub

        Private Sub FindTask_Failed(ByVal sender As Object, ByVal args As TaskFailedEventArgs)
            MessageBox.Show("Find failed: " & args.Error.ToString())
        End Sub
    End Class

