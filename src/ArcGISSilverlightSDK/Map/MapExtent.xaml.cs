using System.Windows.Controls;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISSilverlightSDK
{
    public partial class MapExtent : UserControl
    {
        public MapExtent()
        {
            InitializeComponent();
        }

        private void MyMap_ExtentChanged(object sender, ESRI.ArcGIS.Client.ExtentEventArgs e)
        {            
            Envelope newExtent = null;

            if (MyMap.WrapAroundIsActive)
            {
                Geometry normalizedExtent = Geometry.NormalizeCentralMeridian(e.NewExtent);
                if (normalizedExtent is Polygon)
                {
                    newExtent = new Envelope();

                    foreach (MapPoint p in (normalizedExtent as Polygon).Rings[0])
                    {
                        if (p.X < newExtent.XMin || double.IsNaN(newExtent.XMin))
                            newExtent.XMin = p.X;
                        if (p.Y < newExtent.YMin || double.IsNaN(newExtent.YMin))
                            newExtent.YMin = p.Y;
                    }

                    foreach (MapPoint p in (normalizedExtent as Polygon).Rings[1])
                    {
                        if (p.X > newExtent.XMax || double.IsNaN(newExtent.XMax))
                            newExtent.XMax = p.X;
                        if (p.Y > newExtent.YMax || double.IsNaN(newExtent.YMax))
                            newExtent.YMax = p.Y;
                    }
                }
                else if (normalizedExtent is Envelope)
                    newExtent = normalizedExtent as Envelope;
            } else 
                newExtent = e.NewExtent;
            
            MinXNormalized.Text = newExtent.XMin.ToString("0.000");
            MinYNormalized.Text = newExtent.YMin.ToString("0.000");
            MaxXNormalized.Text = newExtent.XMax.ToString("0.000");
            MaxYNormalized.Text = newExtent.YMax.ToString("0.000");
        }
    }
}