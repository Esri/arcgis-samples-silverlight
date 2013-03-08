using System;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISSilverlightSDK
{
    public partial class MediaLayer : UserControl
    {
        public MediaLayer()
        {
            InitializeComponent();
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs args)
        {
            // Repeat play of the video
            MediaElement media = sender as MediaElement;
            media.Position = TimeSpan.FromSeconds(0);
            media.Play();
        }
    }
}
