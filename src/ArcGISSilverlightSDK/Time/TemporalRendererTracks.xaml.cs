using System;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit;

namespace ArcGISSilverlightSDK
{
    public partial class TemporalRendererTracks : UserControl
    {
        public TemporalRendererTracks()
        {
            InitializeComponent();
        }

        private void FeatureLayer_Initialized(object sender, EventArgs e)
        {
            TimeExtent extent = new TimeExtent(MyTimeSlider.MinimumValue, MyTimeSlider.MaximumValue);
            MyTimeSlider.Intervals = TimeSlider.CreateTimeStopsByTimeInterval(extent, new TimeSpan(0, 6, 0, 0, 0));
        }
    }
}
