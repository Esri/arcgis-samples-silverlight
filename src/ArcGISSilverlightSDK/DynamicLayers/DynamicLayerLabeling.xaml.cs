using System.Windows.Controls;
using ESRI.ArcGIS.Client;

namespace ArcGISSilverlightSDK
{
    public partial class DynamicLayerLabeling : UserControl
    {
        public DynamicLayerLabeling()
        {
            InitializeComponent();
        }

        private void cboPlacement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MyMap == null) return;

            ArcGISDynamicMapServiceLayer dynamicLayer =
                MyMap.Layers["PopulationDynamicLayer"] as ArcGISDynamicMapServiceLayer;
            LabelPlacement placment = new LabelPlacement();

            switch (cboPlacement.SelectedIndex)
            {
                case 0:
                    placment = LabelPlacement.PointLabelPlacementAboveCenter;
                    break;
                case 1:
                    placment = LabelPlacement.PointLabelPlacementAboveLeft;
                    break;
                case 2:
                    placment = LabelPlacement.PointLabelPlacementAboveRight;
                    break;
                case 3:
                    placment = LabelPlacement.PointLabelPlacementBelowCenter;
                    break;
                case 4:
                    placment = LabelPlacement.PointLabelPlacementBelowLeft;
                    break;
                case 5:
                    placment = LabelPlacement.PointLabelPlacementBelowRight;
                    break;
                case 6:
                    placment = LabelPlacement.PointLabelPlacementCenterCenter;
                    break;
                case 7:
                    placment = LabelPlacement.PointLabelPlacementCenterLeft;
                    break;
                case 8:
                    placment = LabelPlacement.PointLabelPlacementCenterRight;
                    break;
            }

            foreach (LabelClass lClass in dynamicLayer.LayerDrawingOptions[0].LabelClasses)
            {
                lClass.LabelPlacement = placment;
            }

            dynamicLayer.Refresh();
        }
    }
}
