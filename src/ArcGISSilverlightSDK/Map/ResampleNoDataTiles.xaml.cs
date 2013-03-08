using System;
using System.IO;
using System.Net;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client;

namespace ArcGISSilverlightSDK
{
    public partial class ResampleNoDataTiles : UserControl
    {
        public ResampleNoDataTiles()
        {
            InitializeComponent();             
        }

        public void ArcGISTiledMapServiceLayer_TileLoaded(object sender, ESRI.ArcGIS.Client.TiledLayer.TileLoadEventArgs e)
        {
            if (isNoDataTile(e.ImageStream, MyMap.SpatialReference.WKID))
            {
                ArcGISTiledMapServiceLayer layer = sender as ArcGISTiledMapServiceLayer;
                
                // Create writeable bitmap of the same size as layer tile
                WriteableBitmap bmp = new WriteableBitmap(layer.TileInfo.Width, layer.TileInfo.Height);

                // Set image source to writeable bitmap.  Writeable bitmap 
                e.ImageSource = bmp;

                // Start the resampling process
                ResampleNoDataTile(bmp, 1, e.Level, e.Row, e.Column, layer.TileInfo.Width, layer.TileInfo.Height, layer.Url);
            }
        }

        // Need to determine if tile contains no data.  Note, Silverlight does not have access to response headers.
        private bool isNoDataTile(Stream imageStream, int wkid)
        {
            if (imageStream == null) return true;

            // Bytes in no data tile for tiled map service.  Often different for different services.
            int no_data_length = 2521;
            
            // If equal, its a no data tile
            return imageStream.Length == no_data_length;
        }

        // Recursive search for tile to resample for no data fallback scenario.
        private void ResampleNoDataTile(WriteableBitmap bmp, int levelUp, int level, int row, int col, int tileWidth, int tileHeight, string layerUrl)
        {
            // When we reach the top level, return.
            if (level - levelUp < 0)
                return;

            // The following scale calculation will only work when tiles 
            // are exactly twice the resolution of next level
            var scale = (int)Math.Pow(2, levelUp);

            // If scale becomes too big, render will be poor with such a significant stretch.  
            if (tileHeight * scale > 65536 || tileWidth * scale > 65536)            
                return; 
            
            var searchLevel = level - levelUp;	// Calculate level to search.
            var searchRow = row / scale;		// Calculate row in search level.
            var searchCol = col / scale;		// Calculate column in search level.

            // Get tile url from one level up.
            string tileurl = string.Format("{0}/tile/{1}/{2}/{3}", new object[] { 
                layerUrl, searchLevel, searchRow, searchCol });

            WebClient downloader = new WebClient();
            downloader.OpenReadCompleted += (s, e) =>
            {
                // if is no data tile, resample next level up
                if (e.Error != null || e.Result == null || isNoDataTile(e.Result, MyMap.SpatialReference.WKID))
                {
                    ResampleNoDataTile(bmp, levelUp + 1, level, row, col, tileWidth, tileHeight, layerUrl);
                }
                // if has data, get image source, determine location and scale, then render
                else
                {
                    BitmapImage bmi = new BitmapImage();
                    bmi.SetSource(e.Result);
                    double x = tileWidth * (col % scale); // Calculate x pixel coordinate of section to resample.
                    double y = tileHeight * (row % scale); // Calculate y pixel coordinate of section to resample.			

                    // Set the scale and location of the resampled tile
                    bmp.Render(new Image() { Source = bmi }, new CompositeTransform() { 
                        ScaleX = scale, ScaleY = scale, TranslateX = -x, TranslateY = -y });
                    
                    // DEBUG: Scale multiplier (x is current scale) 
                    bmp.Render(new TextBlock() { Text = string.Format("Tile resampled: {0}x", scale) }, null);

                    bmp.Invalidate();
                }
            };
            downloader.OpenReadAsync(new Uri(tileurl));
        }

        // Toggle display of layer with resampled tiles vs. no data tiles
        private void ResampleNoDataTilesCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MyMap == null)
                return; 

            ArcGISTiledMapServiceLayer resampledTiledLayer = 
                MyMap.Layers["TiledLayerResampled"] as ArcGISTiledMapServiceLayer;
            ArcGISTiledMapServiceLayer showNoDataTiledLayer = 
                MyMap.Layers["TiledLayerNoData"] as ArcGISTiledMapServiceLayer;

            showNoDataTiledLayer.Visible = !showNoDataTiledLayer.Visible;
            resampledTiledLayer.Visible = !resampledTiledLayer.Visible;
        }
    }
}
