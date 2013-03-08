Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.WebMap

Partial Public Class WebMapCSV
  Inherits UserControl
  Public Sub New()
    InitializeComponent()

    Dim webMap As New Document()
    AddHandler webMap.GetMapCompleted, AddressOf webMap_GetMapCompleted

    webMap.GetMapAsync("e64c82296b5a48acb0a7f18e3f556607")
  End Sub

  Private Sub webMap_GetMapCompleted(ByVal sender As Object, ByVal e As GetMapCompletedEventArgs)
    If e.Error Is Nothing Then
      LayoutRoot.Children.Add(e.Map)
    End If
  End Sub
End Class

