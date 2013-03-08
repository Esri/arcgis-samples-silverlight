Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports System.Collections.Generic

Partial Public Class AttributeOnlyEditing
  Inherits UserControl
  Public Sub New()
    InitializeComponent()
  End Sub

  Private Sub Editor_EditCompleted(ByVal sender As Object, ByVal e As Editor.EditEventArgs)
    Dim editor = TryCast(sender, Editor)
    If e.Action = editor.EditAction.Select Then
      For Each edit In e.Edits
        If edit.Graphic IsNot Nothing AndAlso edit.Graphic.Selected Then
          Dim layer = TryCast(edit.Layer, FeatureLayer)
          If layer IsNot Nothing AndAlso layer.IsGeometryUpdateAllowed(edit.Graphic) Then
            ' edit geometry
            If editor.EditVertices.CanExecute(Nothing) Then
              editor.EditVertices.Execute(Nothing)
            End If
          End If

          ' edit attribute
          MyFeatureDataGrid.SelectedItem = edit.Graphic
          Exit For
        End If
      Next edit
    ElseIf e.Action = editor.EditAction.EditVertices Then
      ' should never happen since feature service AllowGeometryUpdates=False
      If editor.Select.CanExecute("new") Then
        editor.Select.Execute("new")
      End If
    End If
  End Sub

  Private Sub FeatureLayer_Initialized(ByVal sender As Object, ByVal e As EventArgs)
    Dim editor = TryCast(LayoutRoot.Resources("MyEditor"), Editor)
    If editor.Select.CanExecute("new") Then
      editor.Select.Execute("new")
    End If
  End Sub

  Private Sub FeatureLayer_EndSaveEdits(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Tasks.EndEditEventArgs)
    If e.Success Then
      TryCast(MyMap.Layers("PoolPermitDynamicLayer"), ArcGISDynamicMapServiceLayer).Refresh()
    End If
  End Sub
End Class
