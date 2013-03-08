Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Media
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Toolkit.DataSources
Imports ESRI.ArcGIS.Client.WebMap

Partial Public Class WebMapKML
  Inherits UserControl
  Public Sub New()
    InitializeComponent()

    Dim webMap As New Document()
    AddHandler webMap.GetMapCompleted, AddressOf webMap_GetMapCompleted
    webMap.ProxyUrl = "http://servicesbeta3.esri.com/SilverlightDemos/ProxyPage/proxy.ashx"
    webMap.GetMapAsync("d2cb7cac8b1947c7b57ed8edd6b045bb")
  End Sub

  Private Sub webMap_GetMapCompleted(ByVal sender As Object, ByVal e As GetMapCompletedEventArgs)
    If e.Error Is Nothing Then
      For Each layer As Layer In e.Map.Layers
        If TypeOf layer Is KmlLayer Then
          AddHandler TryCast(layer, KmlLayer).Initialized, AddressOf kmllayer_Initialized
        End If
      Next layer

      e.Map.WrapAround = True
      LayoutRoot.Children.Add(e.Map)
    End If
  End Sub

  Private Sub kmllayer_Initialized(ByVal sender As Object, ByVal e As System.EventArgs)
    For Each layer As Layer In (TryCast(sender, KmlLayer)).ChildLayers
      layer.Visible = True

      Dim border As New Border() With {.Background = New SolidColorBrush(Colors.White), .BorderBrush = New SolidColorBrush(Colors.Black), .BorderThickness = New Thickness(1), .CornerRadius = New CornerRadius(5)}

      Dim stackPanelChild As New StackPanel() With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(5)}

      Dim textValue As New TextBlock()
      Dim valueBinding As New Binding(String.Format("[{0}]", "name"))
      textValue.SetBinding(TextBlock.TextProperty, valueBinding)

      stackPanelChild.Children.Add(textValue)

      border.Child = stackPanelChild
      TryCast(layer, KmlLayer).MapTip = border
    Next layer
  End Sub
End Class

