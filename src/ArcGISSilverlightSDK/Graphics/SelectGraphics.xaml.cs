using System.Windows.Controls;

namespace ArcGISSilverlightSDK
{
    public partial class SelectGraphics : UserControl
    {
		public SelectGraphics()
        {
            InitializeComponent();
        }

        private void GraphicsLayer_MouseLeftButtonDown(object sender, ESRI.ArcGIS.Client.GraphicMouseButtonEventArgs e)
		{
			e.Graphic.Selected = !e.Graphic.Selected;
		}
    }
}
