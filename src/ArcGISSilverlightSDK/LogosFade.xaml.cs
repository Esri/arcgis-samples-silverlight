using System.Windows;
using System.Windows.Controls;
using System;
using System.Windows.Threading;

namespace ArcGISSilverlightSDK
{
    public partial class LogosFade : UserControl
    {
        public LogosFade()
        {
            InitializeComponent();
        }

        //private void UserControl_Loaded(object sender, RoutedEventArgs e)
        //{
        //    DispatcherTimer myDispatcherTimer = new DispatcherTimer();
        //    myDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 7000); // 7000 Milliseconds 
        //    myDispatcherTimer.Tick += Each_Tick;
        //    myDispatcherTimer.Start();
        //}

        //public void Each_Tick(object sender, EventArgs ea)
        //{
        //    (sender as DispatcherTimer).Stop();
        //    slStoryboard.Begin();
        //}

    }
}
