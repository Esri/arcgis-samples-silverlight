using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit;

namespace ArcGISSilverlightSDK
{
    public partial class TemporalRendererPoints : UserControl
    {
        public TemporalRendererPoints()
        {
            InitializeComponent();
        }

        private void MyTimeSlider_Loaded(object sender, RoutedEventArgs e)
        {
            MyTimeSlider.MinimumValue = DateTime.Now.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
            MyTimeSlider.MaximumValue = DateTime.Now.ToUniversalTime();
            MyTimeSlider.Value = new TimeExtent(MyTimeSlider.MinimumValue, MyTimeSlider.MinimumValue.AddHours(2));
            MyTimeSlider.Intervals = TimeSlider.CreateTimeStopsByTimeInterval(
                new TimeExtent(MyTimeSlider.MinimumValue, MyTimeSlider.MaximumValue), new TimeSpan(0, 2, 0, 0));
        }
    }
}
