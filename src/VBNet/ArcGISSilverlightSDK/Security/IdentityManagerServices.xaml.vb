Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client

Partial Public Class IdentityManagerServices
  Inherits UserControl

  Public Sub New()
    InitializeComponent()
    ' Activate identity manager
    IdentityManager.Current.ChallengeMethod = AddressOf Challenge

  End Sub


  Private challengeAttemptsPerUrl As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)()

  Private Sub Challenge(ByVal url As String, ByVal callback As Action(Of IdentityManager.Credential, Exception), ByVal options As IdentityManager.GenerateTokenOptions)

    LoginGrid.Visibility = System.Windows.Visibility.Visible

    TitleTextBlock.Text = String.Format("Login to access:" + Environment.NewLine + " {0}", url)

    If (Not challengeAttemptsPerUrl.ContainsKey(url)) Then
      challengeAttemptsPerUrl.Add(url, 0)
    End If

    Dim handleClick As RoutedEventHandler = Nothing
    handleClick = Sub(s, e)
                    IdentityManager.Current.GenerateCredentialAsync(url, UserTextBox.Text, PasswordTextBox.Text,
                                                                    Sub(credential, ex)
                                                                      challengeAttemptsPerUrl(url) += 1
                                                                      If ex Is Nothing OrElse challengeAttemptsPerUrl(url) = 3 Then
                                                                        RemoveHandler LoginLoadLayerButton.Click, handleClick
                                                                        callback(credential, ex)
                                                                      End If
                                                                    End Sub _
                                                                    , options)
                  End Sub
    AddHandler LoginLoadLayerButton.Click, handleClick

    Dim handleEnterKeyDown As System.Windows.Input.KeyEventHandler = Nothing
    handleEnterKeyDown = Sub(s, e)
                           If e.Key = System.Windows.Input.Key.Enter Then
                             RemoveHandler PasswordTextBox.KeyDown, handleEnterKeyDown
                             handleClick(Nothing, Nothing)
                           End If
                         End Sub
    AddHandler PasswordTextBox.KeyDown, handleEnterKeyDown

  End Sub

  Private Sub Layer_Initialized(sender As System.Object, e As System.EventArgs)
    If MyMap.Layers(MyMap.Layers.Count - 1) Is (TryCast(sender, Layer)) Then
      LoginGrid.Visibility = System.Windows.Visibility.Collapsed
      ShadowGrid.Visibility = System.Windows.Visibility.Collapsed
      LegendBorder.Visibility = System.Windows.Visibility.Visible
    End If
  End Sub
  Private Sub Layer_InitializationFailed(sender As System.Object, e As System.EventArgs)

  End Sub
End Class
