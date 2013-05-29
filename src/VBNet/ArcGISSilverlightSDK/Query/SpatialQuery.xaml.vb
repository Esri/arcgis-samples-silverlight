Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Input
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Symbols
Imports ESRI.ArcGIS.Client.Tasks
Imports ESRI.ArcGIS.Client.ValueConverters

Partial Public Class SpatialQuery
  Inherits UserControl
  Private MyDrawObject As Draw
  Private selectionGraphicslayer As GraphicsLayer

  Public Sub New()
    InitializeComponent()

    selectionGraphicslayer = TryCast(MyMap.Layers("MySelectionGraphicsLayer"), GraphicsLayer)

    MyDrawObject = New Draw(MyMap) With {.LineSymbol = TryCast(LayoutRoot.Resources("DefaultLineSymbol"), SimpleLineSymbol), .FillSymbol = TryCast(LayoutRoot.Resources("DefaultFillSymbol"), FillSymbol)}
    AddHandler MyDrawObject.DrawComplete, AddressOf MyDrawSurface_DrawComplete
  End Sub

  Private Sub UnSelectTools()
    For Each element As UIElement In MyStackPanel.Children
      If TypeOf element Is Button Then
        VisualStateManager.GoToState((TryCast(element, Button)), "UnSelected", False)
      End If
    Next element
  End Sub

  Private Sub Tool_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    UnSelectTools()

    VisualStateManager.GoToState(TryCast(sender, Button), "Selected", False)

    Select Case TryCast((TryCast(sender, Button)).Tag, String)
      Case "DrawPoint"
        MyDrawObject.DrawMode = DrawMode.Point
      Case "DrawPolyline"
        MyDrawObject.DrawMode = DrawMode.Polyline
      Case "DrawPolygon"
        MyDrawObject.DrawMode = DrawMode.Polygon
      Case "DrawRectangle"
        MyDrawObject.DrawMode = DrawMode.Rectangle
      Case "DrawFreehand"
        MyDrawObject.DrawMode = DrawMode.Freehand
      Case "DrawCircle"
        MyDrawObject.DrawMode = DrawMode.Circle
      Case "DrawEllipse"
        MyDrawObject.DrawMode = DrawMode.Ellipse
      Case Else
        MyDrawObject.DrawMode = DrawMode.None
        selectionGraphicslayer.Graphics.Clear()
        QueryDetailsDataGrid.ItemsSource = Nothing
        ResultsDisplay.Visibility = Visibility.Collapsed
    End Select
    MyDrawObject.IsEnabled = (MyDrawObject.DrawMode <> DrawMode.None)
  End Sub

  Private Sub MyDrawSurface_DrawComplete(ByVal sender As Object, ByVal args As ESRI.ArcGIS.Client.DrawEventArgs)
    ResultsDisplay.Visibility = Visibility.Collapsed
    MyDrawObject.IsEnabled = False
    selectionGraphicslayer.Graphics.Clear()

    Dim queryTask As New QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5")
    AddHandler queryTask.ExecuteCompleted, AddressOf QueryTask_ExecuteCompleted
    AddHandler queryTask.Failed, AddressOf QueryTask_Failed

    ' Bind data grid to query results
    Dim resultFeaturesBinding As New Binding("LastResult.Features")
    resultFeaturesBinding.Source = queryTask
    QueryDetailsDataGrid.SetBinding(DataGrid.ItemsSourceProperty, resultFeaturesBinding)
    Dim query As Query = New ESRI.ArcGIS.Client.Tasks.Query()

    ' Specify fields to return from query
    query.OutFields.AddRange(New String() {"STATE_NAME", "SUB_REGION", "STATE_FIPS", "STATE_ABBR", "POP2000", "POP2007"})
    query.Geometry = args.Geometry

    ' Return geometry with result features
    query.ReturnGeometry = True
    query.OutSpatialReference = MyMap.SpatialReference

    queryTask.ExecuteAsync(query)
  End Sub

  Private Sub QueryTask_ExecuteCompleted(ByVal sender As Object, ByVal args As ESRI.ArcGIS.Client.Tasks.QueryEventArgs)
    Dim featureSet As FeatureSet = args.FeatureSet

    If featureSet Is Nothing OrElse featureSet.Features.Count < 1 Then
      MessageBox.Show("No features returned from query")
      Return
    End If

    If featureSet IsNot Nothing AndAlso featureSet.Features.Count > 0 Then
      For Each feature As Graphic In featureSet.Features
        feature.Symbol = TryCast(LayoutRoot.Resources("ResultsFillSymbol"), FillSymbol)
        selectionGraphicslayer.Graphics.Insert(0, feature)
      Next feature
      ResultsDisplay.Visibility = Visibility.Visible
    End If
    MyDrawObject.IsEnabled = True
  End Sub

  Private Sub QueryTask_Failed(ByVal sender As Object, ByVal args As TaskFailedEventArgs)
    MessageBox.Show("Query failed: " & args.Error.ToString())
    MyDrawObject.IsEnabled = True
  End Sub

  Private Sub GraphicsLayer_MouseEnter(ByVal sender As Object, ByVal args As GraphicMouseEventArgs)
    QueryDetailsDataGrid.Focus()
    QueryDetailsDataGrid.SelectedItem = args.Graphic
    QueryDetailsDataGrid.CurrentColumn = QueryDetailsDataGrid.Columns(0)
    QueryDetailsDataGrid.ScrollIntoView(QueryDetailsDataGrid.SelectedItem, QueryDetailsDataGrid.Columns(0))
  End Sub

  Private Sub GraphicsLayer_MouseLeave(ByVal sender As Object, ByVal args As GraphicMouseEventArgs)
    QueryDetailsDataGrid.Focus()
    QueryDetailsDataGrid.SelectedItem = Nothing
  End Sub

  Private Sub QueryDetailsDataGrid_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
    For Each g As Graphic In e.AddedItems
      g.Select()
    Next g

    For Each g As Graphic In e.RemovedItems
      g.UnSelect()
    Next g
  End Sub

  Private Sub QueryDetailsDataGrid_LoadingRow(ByVal sender As Object, ByVal e As DataGridRowEventArgs)
    AddHandler e.Row.MouseEnter, AddressOf Row_MouseEnter
    AddHandler e.Row.MouseLeave, AddressOf Row_MouseLeave
  End Sub

  Private Sub Row_MouseEnter(ByVal sender As Object, ByVal e As MouseEventArgs)
    TryCast((CType(sender, System.Windows.FrameworkElement)).DataContext, Graphic).Select()
  End Sub

  Private Sub Row_MouseLeave(ByVal sender As Object, ByVal e As MouseEventArgs)
    Dim row As DataGridRow = TryCast(sender, DataGridRow)
    Dim g As Graphic = TryCast((CType(sender, System.Windows.FrameworkElement)).DataContext, Graphic)

    If Not QueryDetailsDataGrid.SelectedItems.Contains(g) Then
      g.UnSelect()
    End If
  End Sub
End Class
