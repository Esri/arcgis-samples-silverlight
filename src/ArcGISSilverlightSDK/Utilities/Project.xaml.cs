using System.Windows.Controls;
using System.Windows;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Symbols;

namespace ArcGISSilverlightSDK
{
    public partial class Project : UserControl
    {
        GeometryService geometryService;
        GraphicsLayer graphicsLayer;

        public Project()
        {
            InitializeComponent();

            geometryService = new GeometryService("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
            geometryService.ProjectCompleted += geometryService_ProjectCompleted;
            geometryService.Failed += geometryService_Failed;

            graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
        }

        private void ProjectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            double x;
            double y;
            if (!double.TryParse(XTextBox.Text, out x) || !double.TryParse(YTextBox.Text, out y))
            {
                MessageBox.Show("Enter valid coordinate values.");
                return;
            }

            MapPoint inputMapPoint = new MapPoint(x, y, new SpatialReference(4326));

            geometryService.ProjectAsync(new List<Graphic>() { new Graphic() { Geometry = inputMapPoint } }, MyMap.SpatialReference, inputMapPoint);
        }

        void geometryService_ProjectCompleted(object sender, GraphicsEventArgs e)
        {
            Graphic resultGraphic = e.Results[0];

            if (resultGraphic.Geometry.Extent != null)
            {
                resultGraphic.Symbol = LayoutRoot.Resources["RoundMarkerSymbol"] as SimpleMarkerSymbol;

                MapPoint resultMapPoint = resultGraphic.Geometry as MapPoint;
                resultGraphic.Attributes.Add("Output_CoordinateX", resultMapPoint.X);
                resultGraphic.Attributes.Add("Output_CoordinateY", resultMapPoint.Y);

                MapPoint inputMapPoint = e.UserState as MapPoint;
                resultGraphic.Attributes.Add("Input_CoordinateX", inputMapPoint.X); 
                resultGraphic.Attributes.Add("Input_CoordinateY", inputMapPoint.Y);

                graphicsLayer.Graphics.Add(resultGraphic);

                MyMap.PanTo(resultGraphic.Geometry);
            }
            else
            {
                MessageBox.Show("Invalid input coordinate, unable to project.");
            }

        }

        void geometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
        }
    }
}
