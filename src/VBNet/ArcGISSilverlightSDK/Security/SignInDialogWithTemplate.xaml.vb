Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client

Partial Public Class SignInDialogWithTemplate
  Inherits UserControl
  Public Sub New()
    InitializeComponent()

    IdentityManager.Current.ChallengeMethod = AddressOf Challenge

    AddHandler Unloaded, AddressOf SignInDialogWithTemplate_Unloaded
  End Sub

  Private Sub SignInDialogWithTemplate_Unloaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
    MySignInDialog.Visibility = System.Windows.Visibility.Collapsed
    MySignInDialog.IsActive = False
  End Sub

  Private Sub Challenge(ByVal url As String, ByVal callback As Action(Of IdentityManager.Credential, Exception), ByVal options As IdentityManager.GenerateTokenOptions)
    ' Option 1: Access to url, callback, and options passed to SignInDialog
    'SignInDialog.DoSignIn(url, callback, options);

    ' Option 2: Use Popup to contain SignInDialog
    'var popup = new Popup
    '{
    '    HorizontalOffset = 200,
    '    VerticalOffset = 200
    '};

    'SignInDialog signInDialog = new SignInDialog()
    '{
    '    Width = 300,
    '    Url = url,
    '    IsActive = true,
    '    Callback = (credential, ex) =>
    ' {
    '     callback(credential, ex);
    '     popup.IsOpen = false;
    ' }
    '};
    'popup.Child = signInDialog;
    'popup.IsOpen = true;

    ' Option 3: Use a template to define SignInDialog content             
    MySignInDialog.Url = url
    MySignInDialog.IsActive = True
    MySignInDialog.Visibility = System.Windows.Visibility.Visible


    MySignInDialog.Callback = Sub(credential, ex)
                                callback(credential, ex)
                                MySignInDialog.Visibility = System.Windows.Visibility.Collapsed
                                MySignInDialog.IsActive = False
                              End Sub

  End Sub

  Private Sub Layer_InitializationFailed(ByVal sender As Object, ByVal e As System.EventArgs)
  End Sub

  Private Sub Layer_Initialized(ByVal sender As Object, ByVal e As System.EventArgs)
    MyMap.Extent = (TryCast(sender, Layer)).FullExtent
  End Sub
End Class
