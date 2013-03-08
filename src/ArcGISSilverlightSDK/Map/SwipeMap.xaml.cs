using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;

namespace ArcGISSilverlightSDK
{
    public partial class SwipeMap : UserControl
    {
        bool isMouseCaptured;
        double mouseHorizontalPosition;

        public SwipeMap()
        {
            InitializeComponent();
            AboveMap.Layers.LayersInitialized += (a, b) =>
              {
                  slider.Height = (rootCanvas).ActualHeight;
                  slider.SetValue(Canvas.LeftProperty, (rootCanvas).ActualWidth - slider.ActualWidth);
              };
        }

        public void Handle_MouseDown(object sender, MouseEventArgs args)
        {
            Rectangle item = sender as Rectangle;
            mouseHorizontalPosition = args.GetPosition(null).X;

            BelowMap.Extent = AboveMap.Extent;
            BelowMap.Visibility = Visibility.Visible;

            isMouseCaptured = true;
            item.CaptureMouse();
        }

        public void Handle_MouseMove(object sender, MouseEventArgs args)
        {
            Rectangle item = sender as Rectangle;

            if (isMouseCaptured)
            {
                // Calculate the current position of the object.        
                double deltaH = args.GetPosition(null).X - mouseHorizontalPosition;
                double newLeft = deltaH + (double)item.GetValue(Canvas.LeftProperty);

                //if slider is pulled beyond start of map, default it back to start.
                if (newLeft < 0.0)
                {
                    item.ReleaseMouseCapture();
                    newLeft = 0.0;
                    isMouseCaptured = false;
                }

                //if slider is pulled beyond screen, default it back to end.
                if (newLeft > rootCanvas.ActualWidth)
                {
                    item.ReleaseMouseCapture();
                    newLeft = rootCanvas.ActualWidth - slider.ActualWidth;
                    isMouseCaptured = false;
                }

                item.SetValue(Canvas.LeftProperty, newLeft);

                // Update position global variables.
                mouseHorizontalPosition = args.GetPosition(null).X;


                Point mouse = args.GetPosition(this.AboveMap);
                // You can modify StartPoint and Endpoint to change the slope of the swipe
                // as well as where it starts and ends. Using a range from 0 to 1 allows
                // the mouse position (X,Y) to be used relative to the grid's ActualWidth or
                // ActualHeight to keep the cursor synchronized with the edge of the mask.
                LinearGradientBrush mask = new LinearGradientBrush();
                mask.StartPoint = new Point(0, 1);
                mask.EndPoint = new Point(1, 1);

                GradientStop transparentStop = new GradientStop();
                transparentStop.Color = Colors.Black;
                transparentStop.Offset = (mouse.X / this.LayoutRoot.ActualWidth);
                mask.GradientStops.Add(transparentStop);

                // The color property must be set, but the OpacityMask ignores color details.
                GradientStop visibleStop = new GradientStop();
                visibleStop.Color = Colors.Transparent;
                visibleStop.Offset = (mouse.X / this.LayoutRoot.ActualWidth);
                mask.GradientStops.Add(visibleStop);

                // Apply the OpacityMask to the map.
                this.AboveMap.OpacityMask = mask;
            }
        }

        public void Handle_MouseUp(object sender, MouseEventArgs args)
        {
            Rectangle item = sender as Rectangle;
            isMouseCaptured = false;
            item.ReleaseMouseCapture();
            mouseHorizontalPosition = -1;
        }

        // Synchronize both maps by matching the extents only when below map is visible
        private void AboveMap_ExtentChanging(object sender, ExtentEventArgs e)
        {
            if (BelowMap.Visibility == System.Windows.Visibility.Visible)
                this.BelowMap.Extent = this.AboveMap.Extent;
        }

        private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            slider.Height = (sender as Canvas).ActualHeight;
            slider.SetValue(Canvas.LeftProperty, (sender as Canvas).ActualWidth - slider.ActualWidth);
        }
    }
}