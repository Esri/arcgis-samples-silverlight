Imports System.Collections.Generic
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Toolkit
Imports System.Windows
Imports System.Windows.Controls



Partial Public Class InfoWindowSimple
  Inherits UserControl

  Public Sub New()
    InitializeComponent()
  End Sub

  Private Sub MyMap_MouseClick(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.Map.MouseEventArgs)
    Dim featureLayer As ESRI.ArcGIS.Client.FeatureLayer = TryCast(MyMap.Layers("MyFeatureLayer"), FeatureLayer)
    Dim screenPnt As System.Windows.Point = MyMap.MapToScreen(e.MapPoint)

    ' Account for difference between Map and application origin
    Dim generalTransform As System.Windows.Media.GeneralTransform = MyMap.TransformToVisual(Application.Current.RootVisual)
    Dim transformScreenPnt As System.Windows.Point = generalTransform.Transform(screenPnt)

    Dim selected As IEnumerable(Of Graphic) = featureLayer.FindGraphicsInHostCoordinates(transformScreenPnt)

    For Each g As Graphic In selected

      MyInfoWindow.Anchor = e.MapPoint
      MyInfoWindow.IsOpen = True
      'Since a ContentTemplate is defined, Content will define the DataContext for the ContentTemplate
      MyInfoWindow.Content = g.Attributes
      Return
    Next g

    Dim window As New InfoWindow() With
        {
            .Anchor = e.MapPoint,
            .Map = MyMap,
            .IsOpen = True,
            .Placement = InfoWindow.PlacementMode.Auto,
            .ContentTemplate = TryCast(LayoutRoot.Resources("LocationInfoWindowTemplate"), System.Windows.DataTemplate),
            .Content = e.MapPoint
        }

    'Since a ContentTemplate is defined, Content will define the DataContext for the ContentTemplate
    LayoutRoot.Children.Add(window)
  End Sub

  Private Sub MyInfoWindow_MouseLeftButtonUp(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
    MyInfoWindow.IsOpen = False
  End Sub

End Class

