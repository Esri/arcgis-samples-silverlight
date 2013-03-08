Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Symbols
Imports ESRI.ArcGIS.Client.Tasks


    Partial Public Class QueryRelatedRecords
        Inherits UserControl
        Private graphicsLayer As GraphicsLayer
        Private queryTask As QueryTask

        Public Sub New()
            InitializeComponent()

            graphicsLayer = TryCast(MyMap.Layers("GraphicsWellsLayer"), GraphicsLayer)

            queryTask = New QueryTask("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Petroleum/KSPetro/MapServer/0")
            AddHandler queryTask.ExecuteCompleted, AddressOf QueryTask_ExecuteCompleted
            AddHandler queryTask.ExecuteRelationshipQueryCompleted, AddressOf QueryTask_ExecuteRelationshipQueryCompleted
            AddHandler queryTask.Failed, AddressOf QueryTask_Failed
        End Sub

        Private Sub MyMap_MouseClick(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Map.MouseEventArgs)
            graphicsLayer.Graphics.Clear()
            SelectedWellsTreeView.ItemsSource = Nothing
            RelatedRowsDataGrid.ItemsSource = Nothing

            Dim query As New Query() With {.Geometry = Expand(MyMap.Extent, e.MapPoint, 0.01), .ReturnGeometry = True, .OutSpatialReference = MyMap.SpatialReference}
            query.OutFields.Add("*")

            queryTask.ExecuteAsync(query)
        End Sub

        Private Function Expand(ByVal mapExtent As Envelope, ByVal point As MapPoint, ByVal pct As Double) As Envelope
            Return New Envelope(point.X - mapExtent.Width * (pct / 2), point.Y - mapExtent.Height * (pct / 2), point.X + mapExtent.Width * (pct / 2), point.Y + mapExtent.Height * (pct / 2)) With {.SpatialReference = mapExtent.SpatialReference}
        End Function

        Private Sub QueryTask_ExecuteCompleted(ByVal sender As Object, ByVal args As ESRI.ArcGIS.Client.Tasks.QueryEventArgs)
            Dim featureSet As FeatureSet = args.FeatureSet
            If featureSet IsNot Nothing AndAlso featureSet.Features.Count > 0 Then
                SelectedWellsTreeView.Tag = featureSet.ObjectIdFieldName
                SelectedWellsTreeView.ItemsSource = featureSet.Features
                For Each g As Graphic In featureSet.Features
                    g.Symbol = TryCast(LayoutRoot.Resources("SelectMarkerSymbol"), MarkerSymbol)
                    graphicsLayer.Graphics.Add(g)
                Next g
                ResultsDisplay.Visibility = System.Windows.Visibility.Visible
            Else
                ResultsDisplay.Visibility = System.Windows.Visibility.Collapsed
                MessageBox.Show("No wells found here, please try another location.")
            End If
        End Sub

        Private Sub QueryTask_Failed(ByVal sender As Object, ByVal args As TaskFailedEventArgs)
            MessageBox.Show("Query execute error: " & args.Error.ToString())
        End Sub

        Private Sub SelectedWellsTreeView_SelectedItemChanged(ByVal sender As Object, ByVal e As RoutedPropertyChangedEventArgs(Of Object))
            If e.OldValue IsNot Nothing Then
                Dim g As Graphic = TryCast(e.OldValue, Graphic)
                g.UnSelect()
                g.SetZIndex(0)
            End If

            If e.NewValue IsNot Nothing Then
                Dim g As Graphic = TryCast(e.NewValue, Graphic)
                g.Select()
                g.SetZIndex(1)

                'Relationship query
                Dim relationshipParameters As New RelationshipParameter() With {.ObjectIds = New Integer() {Convert.ToInt32(g.Attributes(TryCast(SelectedWellsTreeView.Tag, String)))}, .OutFields = New String() {"OBJECTID, API_NUMBER, ELEVATION, FORMATION, TOP"}, .RelationshipId = 3, .OutSpatialReference = MyMap.SpatialReference}

                queryTask.ExecuteRelationshipQueryAsync(relationshipParameters)
            End If
        End Sub

        Private Sub QueryTask_ExecuteRelationshipQueryCompleted(ByVal sender As Object, ByVal e As RelationshipEventArgs)
            Dim pr As RelationshipResult = e.Result
            If pr.RelatedRecordsGroup.Count = 0 Then
                RelatedRowsDataGrid.ItemsSource = Nothing
            Else
                For Each pair In pr.RelatedRecordsGroup
                    RelatedRowsDataGrid.ItemsSource = pair.Value
                Next pair
            End If
        End Sub

    End Class

