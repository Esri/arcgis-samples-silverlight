using System.Windows.Controls;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client;
using System;

namespace ArcGISSilverlightSDK
{
    public partial class TimeMapService : UserControl
    {
        public TimeMapService()
        {
            InitializeComponent();
        }

        private void ArcGISDynamicMapServiceLayer_Initialized(object sender, System.EventArgs e)
        {
            TimeExtent extent = new TimeExtent(MyTimeSlider.MinimumValue, MyTimeSlider.MaximumValue);
            MyTimeSlider.Intervals = TimeSlider.CreateTimeStopsByTimeInterval(extent, TimeSpan.FromDays(500));

            MyTimeSlider.Value = new TimeExtent(MyTimeSlider.MinimumValue, MyTimeSlider.MinimumValue.AddYears(10));
        }
    }
}
