Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.WebMap
Imports System.Net
Imports System
Imports System.Json
Imports System.Windows
Imports ESRI.ArcGIS.Client

Partial Public Class LoadSecureWebMap
  Inherits UserControl
  Public Sub New()
    InitializeComponent()
  End Sub

  Private Sub LoadWebMapButton_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
    IdentityManager.Current.GenerateCredentialAsync("https://www.arcgis.com/sharing",
                                                    UsernameTextBox.Text, PasswordTextBox.Password,
                                                    Sub(credential, ex)
                                                      If ex Is Nothing Then
                                                        Dim webMap As New Document()
                                                        webMap.Token = credential.Token
                                                        AddHandler webMap.GetMapCompleted, AddressOf webMap_GetMapCompleted
                                                        webMap.GetMapAsync(WebMapTextBox.Text)
                                                      End If
                                                    End Sub, Nothing)
  End Sub
  Private Sub webMap_GetMapCompleted(ByVal sender As Object, ByVal e As GetMapCompletedEventArgs)
    If Not e.Error Is Nothing Then
      MessageBox.Show(String.Format("Unable to load webmap. {0}", e.Error.Message))
    Else
      LayoutRoot.Children.Insert(0, e.Map)
    End If
  End Sub
End Class

