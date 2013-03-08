Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.WebMap

Partial Public Class WebMapWMS
  Inherits UserControl
  Public Sub New()
    InitializeComponent()

    Dim webMap As New Document()
    AddHandler webMap.GetMapCompleted, AddressOf webMap_GetMapCompleted
    webMap.ProxyUrl = "http://servicesbeta3.esri.com/SilverlightDemos/ProxyPage/proxy.ashx"
    webMap.GetMapAsync("b3e11e1d7aac4d6a98fde6b864d3a2b7")
  End Sub

  Private Sub webMap_GetMapCompleted(ByVal sender As Object, ByVal e As GetMapCompletedEventArgs)
    If e.Error Is Nothing Then
      LayoutRoot.Children.Add(e.Map)
    End If
  End Sub
End Class

