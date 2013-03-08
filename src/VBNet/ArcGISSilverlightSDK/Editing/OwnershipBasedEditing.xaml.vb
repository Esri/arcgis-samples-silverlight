Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Shapes
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Toolkit

Partial Public Class OwnershipBasedEditing
  Inherits UserControl
  Public Sub New()
    InitializeComponent()
    IdentityManager.Current.ChallengeMethod = AddressOf Challenge
  End Sub

  Private Sub Challenge(ByVal url As String, ByVal callback As Action(Of IdentityManager.Credential, Exception), ByVal options As IdentityManager.GenerateTokenOptions)
    SignInDialog.DoSignIn(url, Sub(credential, err)
                                 If err Is Nothing Then
                                   ToolBorder.Visibility = System.Windows.Visibility.Visible
                                   LoggedInGrid.Visibility = System.Windows.Visibility.Visible
                                   LoggedInUserTextBlock.Text = credential.UserName
                                 End If
                                 callback(credential, err)
                               End Sub, options)
				End Sub

  Private Sub FeatureLayer_InitializationFailed(ByVal sender As Object, ByVal e As EventArgs)
  End Sub

  Private Sub SignOut_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    SignOut()
  End Sub

  Private Sub SignOut()
    Dim l = TryCast(MyMap.Layers("SaveTheBayMarineLayer"), FeatureLayer)
    Dim credential = IdentityManager.Current.FindCredential(l.Url, LoggedInUserTextBlock.Text)
    If credential Is Nothing Then
      Return
    End If
    ToolBorder.Visibility = System.Windows.Visibility.Collapsed
    LoggedInGrid.Visibility = System.Windows.Visibility.Collapsed
    IdentityManager.Current.RemoveCredential(credential)
    MyMap.Layers.Remove(l)
    l = New FeatureLayer() With {.ID = "SaveTheBayMarineLayer", .DisplayName = "Save the Bay - Marine Layer", .Url = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/SaveTheBay/FeatureServer/0", .Mode = FeatureLayer.QueryMode.OnDemand}
    l.OutFields.Add("*")
    AddHandler l.MouseLeftButtonDown, AddressOf FeatureLayer_MouseLeftButtonDown
    AddHandler l.InitializationFailed, AddressOf FeatureLayer_InitializationFailed
    MyMap.Layers.Add(l)
  End Sub

  Private Sub FeatureLayer_MouseLeftButtonDown(ByVal sender As Object, ByVal e As GraphicMouseButtonEventArgs)
    If e.Graphic IsNot Nothing AndAlso (Not e.Graphic.Selected) AndAlso (TryCast(sender, FeatureLayer)).IsUpdateAllowed(e.Graphic) Then
      Dim editor As Editor = TryCast(LayoutRoot.Resources("MyEditor"), Editor)
      If (TryCast(sender, FeatureLayer)).IsUpdateAllowed(e.Graphic) Then
        If editor.EditVertices.CanExecute(Nothing) Then
          editor.EditVertices.Execute(Nothing)
        End If
      Else
        If editor.CancelActive.CanExecute(Nothing) Then
          editor.CancelActive.Execute(Nothing)
        End If
      End If
    End If
    TryCast(sender, FeatureLayer).ClearSelection()
    e.Graphic.Select()
    MyDataGrid.ScrollIntoView(e.Graphic, Nothing)

  End Sub
End Class
