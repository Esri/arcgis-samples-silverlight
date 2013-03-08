Imports Microsoft.VisualBasic
Imports System
Imports System.IO
Imports System.Net
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports ESRI.ArcGIS.Client

Partial Public Class ResampleNoDataTiles
    Inherits UserControl
    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub ArcGISTiledMapServiceLayer_TileLoaded(ByVal sender As Object, ByVal e As ESRI.ArcGIS.Client.TiledLayer.TileLoadEventArgs)
        If isNoDataTile(e.ImageStream, MyMap.SpatialReference.WKID) Then
            Dim layer As ArcGISTiledMapServiceLayer = TryCast(sender, ArcGISTiledMapServiceLayer)

            ' Create writeable bitmap of the same size as layer tile
            Dim bmp As New WriteableBitmap(layer.TileInfo.Width, layer.TileInfo.Height)

            ' Set image source to writeable bitmap.  Writeable bitmap 
            e.ImageSource = bmp

            ' Start the resampling process
            ResampleNoDataTile(bmp, 1, e.Level, e.Row, e.Column, layer.TileInfo.Width, layer.TileInfo.Height, layer.Url)
        End If
    End Sub

    ' Need to determine if tile contains no data.  Note, Silverlight does not have access to response headers.
    Private Function isNoDataTile(ByVal imageStream As Stream, ByVal wkid As Integer) As Boolean
        If imageStream Is Nothing Then
            Return True
        End If

        ' Bytes in no data tile for tiled map service.  Often different for different services.
        Dim no_data_length As Integer = 2521

        ' If equal, its a no data tile
        Return imageStream.Length = no_data_length
    End Function

    ' Recursive search for tile to resample for no data fallback scenario.
    Private Sub ResampleNoDataTile(ByVal bmp As WriteableBitmap, ByVal levelUp As Integer, ByVal level As Integer, ByVal row As Integer, ByVal col As Integer, ByVal tileWidth As Integer, ByVal tileHeight As Integer, ByVal layerUrl As String)
        ' When we reach the top level, return.
        If level - levelUp < 0 Then
            Return
        End If

        ' The following scale calculation will only work when tiles 
        ' are exactly twice the resolution of next level
        Dim scale = CInt(Fix(Math.Pow(2, levelUp)))

        ' If scale becomes too big, render will be poor with such a significant stretch.  
        If tileHeight * scale > 65536 OrElse tileWidth * scale > 65536 Then
            Return
        End If

        Dim searchLevel = level - levelUp ' Calculate level to search.
        Dim searchRow = Math.Floor(row / scale) ' Calculate row in search level.
        Dim searchCol = Math.Floor(col / scale) ' Calculate column in search level.

        ' Get tile url from one level up.
        Dim tileurl As String = String.Format("{0}/tile/{1}/{2}/{3}", New Object() {layerUrl, searchLevel, searchRow, searchCol})

        Dim downloader As New WebClient()
        AddHandler downloader.OpenReadCompleted, Sub(s, e)
                                                     ' if is no data tile, resample next level up
                                                     If e.Error IsNot Nothing OrElse e.Result Is Nothing OrElse isNoDataTile(e.Result, MyMap.SpatialReference.WKID) Then
                                                         ResampleNoDataTile(bmp, levelUp + 1, level, row, col, tileWidth, tileHeight, layerUrl)
                                                         ' if has data, get image source, determine location and scale, then render
                                                     Else
                                                         Dim bmi As New BitmapImage()
                                                         bmi.SetSource(e.Result)
                                                         Dim x As Double = tileWidth * (col Mod scale) ' Calculate x pixel coordinate of section to resample.
                                                         Dim y As Double = tileHeight * (row Mod scale) ' Calculate y pixel coordinate of section to resample.

                                                         ' Set the scale and location of the resampled tile
                                                         bmp.Render(New Image() With {.Source = bmi}, New CompositeTransform() With {.ScaleX = scale, .ScaleY = scale, .TranslateX = -x, .TranslateY = -y})

                                                         ' DEBUG: Scale multiplier (x is current scale) 
                                                         bmp.Render(New TextBlock() With {.Text = String.Format("Tile resampled: {0}x", scale)}, Nothing)

                                                         bmp.Invalidate()
                                                     End If
                                                 End Sub
        downloader.OpenReadAsync(New Uri(tileurl))
    End Sub

    ' Toggle display of layer with resampled tiles vs. no data tiles
    Private Sub ResampleNoDataTilesCheckBox_Checked(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        If MyMap Is Nothing Then
            Return
        End If

        Dim resampledTiledLayer As ArcGISTiledMapServiceLayer = TryCast(MyMap.Layers("TiledLayerResampled"), ArcGISTiledMapServiceLayer)
        Dim showNoDataTiledLayer As ArcGISTiledMapServiceLayer = TryCast(MyMap.Layers("TiledLayerNoData"), ArcGISTiledMapServiceLayer)

        showNoDataTiledLayer.Visible = Not showNoDataTiledLayer.Visible
        resampledTiledLayer.Visible = Not resampledTiledLayer.Visible
    End Sub
End Class