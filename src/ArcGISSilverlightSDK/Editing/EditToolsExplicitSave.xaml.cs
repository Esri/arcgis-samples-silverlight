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
using ESRI.ArcGIS.Client;

namespace ArcGISSilverlightSDK
{
    public partial class EditToolsExplicitSave : UserControl
    {
        public EditToolsExplicitSave()
        {
            InitializeComponent();
        }

        private void CancelEditsButton_Click(object sender, RoutedEventArgs e)
        {
            Editor editor = LayoutRoot.Resources["MyEditor"] as Editor;
            foreach (GraphicsLayer graphicsLayer in editor.GraphicsLayers)
            {
                if (graphicsLayer is FeatureLayer)
                {
                    FeatureLayer featureLayer = graphicsLayer as FeatureLayer;
                    if (featureLayer.HasEdits)
                        featureLayer.Update();
                }
            }
        }
    }
}
