using System;
using System.Collections.Generic;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using System.Windows.Data;

namespace ArcGISSilverlightSDK
{
    public partial class TimeFeatureLayer : UserControl
    {
        public TimeFeatureLayer()
        {
            InitializeComponent();
        }

        private void FeatureLayer_Initialized(object sender, EventArgs e)
        {
            List<DateTime> intervals = new List<DateTime>();
            DateTime dt = MyTimeSlider.MinimumValue;
            while (dt < MyTimeSlider.MaximumValue)
            {
                intervals.Add(dt);
                dt = dt.AddYears(2);
            }
            intervals.Add(MyTimeSlider.MaximumValue);
            MyTimeSlider.Intervals = intervals;
        }
    }
}
