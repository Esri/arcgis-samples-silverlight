Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Media
Imports System.Windows.Media.Effects
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Symbols
Imports ESRI.ArcGIS.Client.Tasks

Partial Public Class FeatureSetJson
  Inherits UserControl

  Private graphicsLayerFromFeatureSet As GraphicsLayer
  Private Shared _mercator As New ESRI.ArcGIS.Client.Projection.WebMercator()

  Public Sub New()
    InitializeComponent()
    CreateFeatureSetJson()
  End Sub

  Private Sub Button_Load(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
    Try
      Dim featureSet As FeatureSet = featureSet.FromJson(JsonTextBox.Text)

      graphicsLayerFromFeatureSet = New GraphicsLayer() With {.Graphics = New GraphicCollection(featureSet)}

      If (Not featureSet.SpatialReference.Equals(MyMap.SpatialReference)) Then
        If MyMap.SpatialReference.Equals(New SpatialReference(102100)) AndAlso featureSet.SpatialReference.Equals(New SpatialReference(4326)) Then
          For Each g As Graphic In graphicsLayerFromFeatureSet.Graphics
            g.Geometry = _mercator.FromGeographic(g.Geometry)
          Next g

        ElseIf MyMap.SpatialReference.Equals(New SpatialReference(4326)) AndAlso featureSet.SpatialReference.Equals(New SpatialReference(102100)) Then
          For Each g As Graphic In graphicsLayerFromFeatureSet.Graphics
            g.Geometry = _mercator.ToGeographic(g.Geometry)
          Next g

        Else
          Dim geometryService As New GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer")

          AddHandler geometryService.ProjectCompleted, AddressOf geometryService_ProjectCompleted

          AddHandler geometryService.Failed, AddressOf Geometry_Failed

          geometryService.ProjectAsync(graphicsLayerFromFeatureSet.Graphics, MyMap.SpatialReference)
        End If
      End If

      Dim simpleRenderer As New SimpleRenderer()
      Dim symbolColor As New SolidColorBrush(Colors.Blue)
      graphicsLayerFromFeatureSet.Renderer = simpleRenderer

      If featureSet.GeometryType = GeometryType.Polygon OrElse featureSet.GeometryType = GeometryType.Polygon Then
        simpleRenderer.Symbol = New SimpleFillSymbol() With {.Fill = symbolColor}
      ElseIf featureSet.GeometryType = GeometryType.Polyline Then
        simpleRenderer.Symbol = New SimpleLineSymbol() With {.Color = symbolColor}
      Else ' Point
        simpleRenderer.Symbol = New SimpleMarkerSymbol() With {.Color = symbolColor, .Size = 12}
      End If

      Dim border As New Border() With {.Background = New SolidColorBrush(Colors.White), .BorderBrush = New SolidColorBrush(Colors.Black), .BorderThickness = New Thickness(1), .CornerRadius = New CornerRadius(10), .Effect = New DropShadowEffect()}

      Dim stackPanelParent As New StackPanel() With {.Orientation = Orientation.Vertical, .Margin = New Thickness(12)}

      For Each keyvalue As KeyValuePair(Of String, String) In featureSet.FieldAliases
        Dim stackPanelChild As New StackPanel() With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 0, 0, 6)}
        Dim textValue As New TextBlock() With {.Text = keyvalue.Value & ": "}

        Dim textKey As New TextBlock()
        Dim keyBinding As New Binding(String.Format("[{0}]", keyvalue.Key))
        textKey.SetBinding(TextBlock.TextProperty, keyBinding)

        stackPanelChild.Children.Add(textValue)
        stackPanelChild.Children.Add(textKey)

        If keyvalue.Key = featureSet.DisplayFieldName Then
          textValue.FontWeight = FontWeights.Bold
          textKey.FontWeight = textValue.FontWeight
          stackPanelParent.Children.Insert(0, stackPanelChild)
        Else
          stackPanelParent.Children.Add(stackPanelChild)
        End If

      Next keyvalue

      border.Child = stackPanelParent
      graphicsLayerFromFeatureSet.MapTip = border

      MyMap.Layers.Add(graphicsLayerFromFeatureSet)
    Catch ex As Exception
      MessageBox.Show(ex.Message, "GraphicsLayer creation failed", MessageBoxButton.OK)
    End Try
  End Sub


  Private Sub geometryService_ProjectCompleted(ByVal s As Object, ByVal a As ESRI.ArcGIS.Client.Tasks.GraphicsEventArgs)
    For i As Integer = 0 To a.Results.Count - 1
      graphicsLayerFromFeatureSet.Graphics(i).Geometry = a.Results(i).Geometry
    Next i
  End Sub
  Private Sub Geometry_Failed(ByVal s As Object, ByVal a As TaskFailedEventArgs)
    MessageBox.Show("Projection error: " & a.Error.Message)
  End Sub

  Private Sub Button_ClearMap(ByVal sender As Object, ByVal e As RoutedEventArgs)
    Dim graphicsLayers As New List(Of GraphicsLayer)()

    For Each layer As Layer In MyMap.Layers
      If TypeOf layer Is GraphicsLayer Then
        graphicsLayers.Add(TryCast(layer, GraphicsLayer))
      End If
    Next layer

    For i As Integer = 0 To graphicsLayers.Count - 1
      MyMap.Layers.Remove(graphicsLayers(i))
    Next i

    OutTextBox.Text = String.Empty
  End Sub

  Private Sub CreateFeatureSetJson()
    Dim jsonInput As String = "{" & ControlChars.CrLf & "    ""displayFieldName"" : ""AREANAME""," &
      ControlChars.CrLf & "    ""geometryType"" : ""esriGeometryPoint""," &
      ControlChars.CrLf & "    ""spatialReference"" : {""wkid"" : 4326}," &
      ControlChars.CrLf & "    ""fieldAliases"" : {" & ControlChars.CrLf & "        ""ST"" : ""State Name""," &
      ControlChars.CrLf & "        ""POP2000"" : ""Population""," &
      ControlChars.CrLf & "        ""AREANAME"" : ""City Name"" " & ControlChars.CrLf & "        },     " &
      ControlChars.CrLf & "    ""features"" : [" & ControlChars.CrLf & "        {" &
      ControlChars.CrLf & "            ""attributes"" : {" & ControlChars.CrLf & "            ""ST"" : ""CA""," &
      ControlChars.CrLf & "            ""POP2000"" : 3694820," & ControlChars.CrLf & "            ""AREANAME"" : ""Los Angeles""" &
      ControlChars.CrLf & "            }," & ControlChars.CrLf & "        ""geometry"" : { ""x"" : -118.4, ""y"" : 34.1 }" &
      ControlChars.CrLf & "        }," & ControlChars.CrLf & "        {" &
      ControlChars.CrLf & "            ""attributes"" : {" & ControlChars.CrLf & "            ""ST"" : ""WA""," &
      ControlChars.CrLf & "            ""POP2000"" : 563374," & ControlChars.CrLf & "            ""AREANAME"" : ""Seattle""" &
      ControlChars.CrLf & "        }," & ControlChars.CrLf & "        ""geometry"" : { ""x"" : -122.3, ""y"" : 47.5 }" &
      ControlChars.CrLf & "        }" & ControlChars.CrLf & "    ]" & ControlChars.CrLf & "}"

    JsonTextBox.Text = jsonInput
  End Sub

  Private Sub Button_Show(ByVal sender As Object, ByVal e As RoutedEventArgs)
    OutTextBox.Text = String.Empty
    Dim featureLayer As New FeatureLayer() With {.Url = FeatureLayerUrlTextBox.Text}

    AddHandler featureLayer.InitializationFailed, AddressOf featureLayer_InitializationFailed

    AddHandler featureLayer.UpdateCompleted, AddressOf featureLayer_UpdateCompleted
    AddHandler featureLayer.UpdateFailed, AddressOf featureLayer_UpdateFailed
    MyMap.Layers.Add(featureLayer)
  End Sub

  Private Sub featureLayer_InitializationFailed(ByVal sender As Object, ByVal e As EventArgs)
    MessageBox.Show("Url must reference a feature layer in a map or feature service.", "FeatureLayer initialization failed", MessageBoxButton.OK)
  End Sub

  Private Sub featureLayer_UpdateCompleted(ByVal sender As Object, ByVal e As EventArgs)
    Dim featureSet As New FeatureSet((TryCast(sender, FeatureLayer)).Graphics)
    Dim jsonOutput As String = featureSet.ToJson()
    OutTextBox.Text = jsonOutput
  End Sub

  Private Sub featureLayer_UpdateFailed(ByVal sender As Object, ByVal e As TaskFailedEventArgs)
    MessageBox.Show(e.Error.Message, "FeatureLayer update failed", MessageBoxButton.OK)
  End Sub
End Class
