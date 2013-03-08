using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ArcGISSilverlightSDK
{
    public partial class TimeImageService : UserControl
    {
        public TimeImageService()
        {
            InitializeComponent();           
        }

        private void MyTimeSlider_Loaded(object sender, RoutedEventArgs e)
        {
            List<DateTime> DateTimeMonths = new List<DateTime>();
            for (int i = 1; i <= 12; i++)
            {
                DateTimeMonths.Add(new DateTime(2004,i,1,0,0,0,DateTimeKind.Utc));
            }

            MyTimeSlider.Intervals = DateTimeMonths;
        }
    }
}
