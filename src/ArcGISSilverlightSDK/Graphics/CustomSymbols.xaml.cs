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
    public partial class CustomSymbols : UserControl
    {
        public CustomSymbols()
        {
            InitializeComponent();
        }

        private void GraphicsLayer_MouseLeftButtonDown(object sender, ESRI.ArcGIS.Client.GraphicMouseButtonEventArgs e)
        {
            if (e.Graphic.Selected)
                e.Graphic.UnSelect();
            else
                e.Graphic.Select();
        }
    }
}
