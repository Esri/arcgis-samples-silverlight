Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Toolkit

Partial Public Class SignInDialogSimple
  Inherits UserControl
  Public Sub New()
    InitializeComponent()

    IdentityManager.Current.ChallengeMethod = AddressOf SignInDialog.DoSignIn
  End Sub

  Private Sub Layer_InitializationFailed(ByVal sender As Object, ByVal e As System.EventArgs)
  End Sub

  Private Sub Layer_Initialized(ByVal sender As Object, ByVal e As System.EventArgs)
    MyMap.Extent = (TryCast(sender, Layer)).FullExtent
  End Sub
End Class
