Imports Microsoft.VisualBasic
Imports System.Text
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client


    Partial Public Class ShowMapProperties
        Inherits UserControl
        Public Sub New()
            InitializeComponent()

            AddHandler MyMap.Layers.LayersInitialized, AddressOf Layers_LayersInitialized
        End Sub

        Private Sub Layers_LayersInitialized(ByVal sender As Object, ByVal args As System.EventArgs)
            Dim sb As New StringBuilder()

            sb.AppendLine(String.Format("Spatial Reference: {0}", If(MyMap.SpatialReference.WKT IsNot Nothing, MyMap.SpatialReference.WKT, MyMap.SpatialReference.WKID.ToString())))
            sb.AppendLine(String.Format("Minimum Resolution: {0}", MyMap.MinimumResolution))
            sb.AppendLine(String.Format("Maximum Resolution: {0}", MyMap.MaximumResolution))
            sb.AppendLine(String.Format("Width (pixels): {0}", MyMap.ActualWidth))
            sb.AppendLine(String.Format("Height (pixels): {0}", MyMap.ActualHeight))
            sb.AppendLine()
            sb.AppendLine(String.Format("---Map Layers ({0})---", MyMap.Layers.Count))
            sb.AppendLine()

            For Each layer As Layer In MyMap.Layers
                sb.AppendLine(String.Format("ID: {0}", layer.ID))
                sb.AppendLine(String.Format("Type: {0}", layer.GetType().ToString()))
            sb.AppendLine(String.Format("Visibility : {0}", layer.Visible))
                sb.AppendLine(String.Format("Opacity : {0}", layer.Opacity))
                If TypeOf layer Is ArcGISDynamicMapServiceLayer Then
                    Dim dynLayer As ArcGISDynamicMapServiceLayer = TryCast(layer, ArcGISDynamicMapServiceLayer)

				sb.AppendLine(String.Format(vbTab & "---Layers ({0})---", MyMap.Layers.Count))
				For Each layerinfo As LayerInfo In dynLayer.Layers
					sb.AppendLine(String.Format(vbTab & "ID: {0}", layerinfo.ID))
					sb.AppendLine(String.Format(vbTab & "Name: {0}", layerinfo.Name))
					sb.AppendLine(String.Format(vbTab & "Default Visibility: {0}", layerinfo.DefaultVisibility))

					sb.AppendLine(String.Format(vbTab & "Minimum Scale: {0}", layerinfo.MinScale))
					sb.AppendLine(String.Format(vbTab & "Maximum Scale: {0}", layerinfo.MaxScale))
					If layerinfo.SubLayerIds IsNot Nothing Then
						sb.AppendLine(String.Format(vbTab & "SubLayer IDs:{0}", layerinfo.SubLayerIds.ToString()))
					End If
					sb.AppendLine()
				Next layerinfo
                End If
                If TypeOf layer Is ArcGISTiledMapServiceLayer Then
                    Dim tiledLayer As ArcGISTiledMapServiceLayer = TryCast(layer, ArcGISTiledMapServiceLayer)
                    Dim ti As TileInfo = tiledLayer.TileInfo
                    sb.AppendLine("Levels and Resolution :")
                    For i As Integer = 0 To ti.Lods.Length - 1
                        sb.Append(String.Format("Level: {0}" & vbTab & " " & vbTab & " Resolution: {1}" & vbCr, i, ti.Lods(i).Resolution))
                    Next i
                End If
                sb.AppendLine()
            Next layer

            PropertiesTextBlock.Text = sb.ToString()
        End Sub
    End Class

