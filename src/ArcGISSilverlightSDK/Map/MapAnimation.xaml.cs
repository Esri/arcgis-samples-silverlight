using System;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISSilverlightSDK
{
    public partial class MapAnimation : UserControl
    {
        public MapAnimation()
        {
            InitializeComponent();
        }

        private void ZoomAnimation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int seconds = Convert.ToInt32(e.NewValue);
            MyMap.ZoomDuration = new TimeSpan(0, 0, seconds);
            ZoomValueLabel.Text = string.Format("Value: {0}", seconds);
        }

        private void PanAnimation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int seconds = Convert.ToInt32(e.NewValue);
            MyMap.PanDuration = new TimeSpan(0, 0, seconds);
            PanValueLabel.Text = string.Format("Value: {0}", seconds);
        }
    }
}
