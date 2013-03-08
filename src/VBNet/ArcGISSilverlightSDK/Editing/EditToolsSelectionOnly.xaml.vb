Imports System
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client

Partial Public Class EditToolsSelectionOnly
  Inherits UserControl
  Private _featureDataFormOpen As Boolean = False

  Public Sub New()
    InitializeComponent()
  End Sub

  Private Sub FeatureLayer_Initialized(ByVal sender As Object, ByVal e As EventArgs)
    Dim editor = TryCast(LayoutRoot.Resources("MyEditor"), Editor)
    If editor.Select.CanExecute("new") Then
      editor.Select.Execute("new")
    End If
  End Sub

  Private Sub Editor_EditCompleted(ByVal sender As Object, ByVal e As Editor.EditEventArgs)
    Dim editor = TryCast(sender, Editor)
    If e.Action = Editor.EditAction.Select Then
      For Each edit In e.Edits
        If edit.Graphic IsNot Nothing AndAlso edit.Graphic.Selected Then
          Dim layer = TryCast(edit.Layer, FeatureLayer)
          If layer IsNot Nothing AndAlso layer.IsGeometryUpdateAllowed(edit.Graphic) Then
            If editor.EditVertices.CanExecute(edit.Graphic) Then
              editor.EditVertices.Execute(edit.Graphic)
            End If

            FeatureDataFormBorder.Visibility = System.Windows.Visibility.Visible
            _featureDataFormOpen = True

            Dim layerDefinition As New LayerDefinition() With {.LayerID = 2, .Definition = String.Format("{0} <> {1}", layer.LayerInfo.ObjectIdField, edit.Graphic.Attributes(layer.LayerInfo.ObjectIdField).ToString())}

            TryCast(MyMap.Layers("WildFireDynamic"), ArcGISDynamicMapServiceLayer).LayerDefinitions = New System.Collections.ObjectModel.ObservableCollection(Of LayerDefinition)() From {layerDefinition}

            TryCast(MyMap.Layers("WildFireDynamic"), ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer).Refresh()
          End If

          MyFeatureDataForm.GraphicSource = edit.Graphic
          Exit For
        End If
      Next edit
    ElseIf e.Action = Editor.EditAction.ClearSelection Then
      FeatureDataFormBorder.Visibility = System.Windows.Visibility.Collapsed
      MyFeatureDataForm.GraphicSource = Nothing
      TryCast(MyMap.Layers("WildFirePolygons"), FeatureLayer).ClearSelection()
      TryCast(MyMap.Layers("WildFireDynamic"), ArcGISDynamicMapServiceLayer).LayerDefinitions = Nothing
      TryCast(MyMap.Layers("WildFireDynamic"), ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer).Refresh()
    End If
  End Sub

  Private Sub ResetEditableSelection()
    If Not _featureDataFormOpen Then
      MyFeatureDataForm.GraphicSource = Nothing
      TryCast(MyMap.Layers("WildFireDynamic"), ArcGISDynamicMapServiceLayer).LayerDefinitions = Nothing
      TryCast(MyMap.Layers("WildFirePolygons"), FeatureLayer).ClearSelection()
    End If

    TryCast(MyMap.Layers("WildFireDynamic"), ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer).Refresh()
  End Sub

  Private Sub FeatureLayer_EndSaveEdits(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Tasks.EndEditEventArgs)
    ResetEditableSelection()
  End Sub

  Private Sub FeatureLayer_SaveEditsFailed(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Tasks.TaskFailedEventArgs)
    ResetEditableSelection()
  End Sub

  Private Sub MyFeatureDataForm_EditEnded(ByVal sender As Object, ByVal e As EventArgs)
    FeatureDataFormBorder.Visibility = System.Windows.Visibility.Collapsed
    _featureDataFormOpen = False
  End Sub
End Class
