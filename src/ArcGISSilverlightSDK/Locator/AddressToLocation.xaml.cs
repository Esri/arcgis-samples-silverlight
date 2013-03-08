using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;


namespace ArcGISSilverlightSDK
{
    public partial class AddressToLocation : UserControl
    {
        Locator _locatorTask;
        GraphicsLayer _candidateGraphicsLayer;
        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
            new ESRI.ArcGIS.Client.Projection.WebMercator();

        public AddressToLocation()
        {
            InitializeComponent();

            ESRI.ArcGIS.Client.Geometry.Envelope initialExtent =
                    new ESRI.ArcGIS.Client.Geometry.Envelope(
                _mercator.FromGeographic(
                    new ESRI.ArcGIS.Client.Geometry.MapPoint(-122.554, 37.615)) as MapPoint,
                _mercator.FromGeographic(
                    new ESRI.ArcGIS.Client.Geometry.MapPoint(-122.245, 37.884)) as MapPoint);

            initialExtent.SpatialReference = new SpatialReference(102100);

            MyMap.Extent = initialExtent;

            _candidateGraphicsLayer = MyMap.Layers["CandidateGraphicsLayer"] as GraphicsLayer;
        }

        private void FindAddressButton_Click(object sender, RoutedEventArgs e)
        {
            _locatorTask = new Locator("http://tasks.arcgisonline.com/ArcGIS/rest/services/Locators/TA_Streets_US_10/GeocodeServer");
            _locatorTask.AddressToLocationsCompleted += LocatorTask_AddressToLocationsCompleted;
            _locatorTask.Failed += LocatorTask_Failed;

            AddressToLocationsParameters addressParams = new AddressToLocationsParameters()
            {
                OutSpatialReference = MyMap.SpatialReference
            };

            Dictionary<string, string> address = addressParams.Address;

            if (!string.IsNullOrEmpty(InputAddress.Text))
                address.Add("Street", InputAddress.Text);
            if (!string.IsNullOrEmpty(City.Text))
                address.Add("City", City.Text);
            if (!string.IsNullOrEmpty(State.Text))
                address.Add("State", State.Text);
            if (!string.IsNullOrEmpty(Zip.Text))
                address.Add("ZIP", Zip.Text);

            _locatorTask.AddressToLocationsAsync(addressParams);
        }

        private void LocatorTask_AddressToLocationsCompleted(object sender, ESRI.ArcGIS.Client.Tasks.AddressToLocationsEventArgs args)
        {
            _candidateGraphicsLayer.ClearGraphics();
            CandidateListBox.Items.Clear();

            List<AddressCandidate> returnedCandidates = args.Results;

            foreach (AddressCandidate candidate in returnedCandidates)
            {
                if (candidate.Score >= 80)
                {
                    CandidateListBox.Items.Add(candidate.Address);

                    Graphic graphic = new Graphic()
                    {
                        Symbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol,
                        Geometry = candidate.Location
                    };

                    graphic.Attributes.Add("Address", candidate.Address);

                    string latlon = String.Format("{0}, {1}", candidate.Location.X, candidate.Location.Y);
                    graphic.Attributes.Add("LatLon", latlon);

                    if (candidate.Location.SpatialReference == null)
                    {
                        candidate.Location.SpatialReference = new SpatialReference(4326);
                    }

                    if (!candidate.Location.SpatialReference.Equals(MyMap.SpatialReference))
                    {
                        if (MyMap.SpatialReference.Equals(new SpatialReference(102100)) && candidate.Location.SpatialReference.Equals(new SpatialReference(4326)))
                            graphic.Geometry = _mercator.FromGeographic(graphic.Geometry);
                        else if (MyMap.SpatialReference.Equals(new SpatialReference(4326)) && candidate.Location.SpatialReference.Equals(new SpatialReference(102100)))
                            graphic.Geometry = _mercator.ToGeographic(graphic.Geometry);
                        else if (MyMap.SpatialReference != new SpatialReference(4326))
                        {
                            GeometryService geometryService =
                                new GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");

                            geometryService.ProjectCompleted += (s, a) =>
                            {
                                graphic.Geometry = a.Results[0].Geometry;
                            };

                            geometryService.Failed += (s, a) =>
                            {
                                MessageBox.Show("Projection error: " + a.Error.Message);
                            };

                            geometryService.ProjectAsync(new List<Graphic> { graphic }, MyMap.SpatialReference);
                        }
                    }

                    _candidateGraphicsLayer.Graphics.Add(graphic);
                }
            }

            if (_candidateGraphicsLayer.Graphics.Count > 0)
            {
                CandidatePanelGrid.Visibility = Visibility.Visible;
                CandidateListBox.SelectedIndex = 0;
            }
        }

        void _candidateListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = (sender as ListBox).SelectedIndex;
            if (index >= 0)
            {
                MapPoint candidatePoint = _candidateGraphicsLayer.Graphics[index].Geometry as MapPoint;
                double displaySize = MyMap.MinimumResolution * 30; 

                ESRI.ArcGIS.Client.Geometry.Envelope displayExtent = new ESRI.ArcGIS.Client.Geometry.Envelope(
                    candidatePoint.X - (displaySize / 2),
                    candidatePoint.Y - (displaySize / 2),
                    candidatePoint.X + (displaySize / 2),
                    candidatePoint.Y + (displaySize / 2));

                MyMap.ZoomTo(displayExtent);
            }
        }

        private void LocatorTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Locator service failed: " + e.Error);
        }
    }
}
