using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class OrderByFieldQuery : UserControl
    {
        GraphicsLayer parcelsGraphicsLayer;

        public OrderByFieldQuery()
        {
            InitializeComponent();

            parcelsGraphicsLayer = MyMap.Layers["MontgomeryParcels"] as GraphicsLayer;
        }

        private void MyMap_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (MyMap.SpatialReference != null)
            {
                MyMap.PropertyChanged -= MyMap_PropertyChanged;

                RunQuery();
            }
        }

        private void GraphicsLayer_MouseLeftButtonUp(object sender, GraphicMouseButtonEventArgs e)
        {
            (sender as GraphicsLayer).ClearSelection();
            e.Graphic.Select();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RunQuery();
        }

        private void RunQuery()
        {
            parcelsGraphicsLayer.Graphics.Clear();

            ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query()
            {
                ReturnGeometry = true,
                OutSpatialReference = MyMap.SpatialReference,
                Where = string.Format("OWNER_NAME LIKE '%{0}%'", SearchTextBox.Text),
                OrderByFields = new List<OrderByField>() { new OrderByField("OWNER_NAME", SortOrder.Ascending) }
            };

            query.OutFields.Add("OWNER_NAME,PARCEL_ID,ZONING,DEED_DATE");

            QueryTask queryTask = new QueryTask("http://sampleserver6.arcgisonline.com/arcgis/rest/services/MontgomeryQuarters/MapServer/1");
            queryTask.ExecuteCompleted += (s, a) =>
            {
                foreach (Graphic g in a.FeatureSet.Features)
                    parcelsGraphicsLayer.Graphics.Add(g);
            };
            queryTask.ExecuteAsync(query);
        }
    }
}
