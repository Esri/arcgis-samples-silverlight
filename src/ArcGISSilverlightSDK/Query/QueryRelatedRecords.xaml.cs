using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class QueryRelatedRecords : UserControl
    {
        GraphicsLayer graphicsLayer;
        QueryTask queryTask;

        public QueryRelatedRecords()
        {
            InitializeComponent();

            graphicsLayer = MyMap.Layers["GraphicsWellsLayer"] as GraphicsLayer;

            queryTask = new QueryTask("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Petroleum/KSPetro/MapServer/0");
            queryTask.ExecuteCompleted += QueryTask_ExecuteCompleted;
            queryTask.ExecuteRelationshipQueryCompleted += QueryTask_ExecuteRelationshipQueryCompleted;
            queryTask.Failed += QueryTask_Failed;
        }

        void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {
            graphicsLayer.Graphics.Clear();
            SelectedWellsTreeView.ItemsSource = null;
            RelatedRowsDataGrid.ItemsSource = null;
            
            Query query = new Query()
            {
                Geometry = Expand(MyMap.Extent, e.MapPoint, 0.01),
                ReturnGeometry = true,
                OutSpatialReference = MyMap.SpatialReference                 
            };
            query.OutFields.Add("*");

            queryTask.ExecuteAsync(query);
        }

        private Envelope Expand(Envelope mapExtent, MapPoint point, double pct)
        {
            return new Envelope(
                point.X - mapExtent.Width * (pct / 2), point.Y - mapExtent.Height * (pct / 2),
                point.X + mapExtent.Width * (pct / 2), point.Y + mapExtent.Height * (pct / 2))
                {
                    SpatialReference = mapExtent.SpatialReference
                };
        }

        void QueryTask_ExecuteCompleted(object sender, ESRI.ArcGIS.Client.Tasks.QueryEventArgs args)
        {
            FeatureSet featureSet = args.FeatureSet;            
            if (featureSet != null && featureSet.Features.Count > 0)
            {
                SelectedWellsTreeView.Tag = featureSet.ObjectIdFieldName;
                SelectedWellsTreeView.ItemsSource = featureSet.Features;
                foreach (Graphic g in featureSet.Features)
                {
                    g.Symbol = LayoutRoot.Resources["SelectMarkerSymbol"] as MarkerSymbol;
                    graphicsLayer.Graphics.Add(g);
                }
                ResultsDisplay.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                ResultsDisplay.Visibility = System.Windows.Visibility.Collapsed;
                MessageBox.Show("No wells found here, please try another location.");                
            }        
        }

        private void QueryTask_Failed(object sender, TaskFailedEventArgs args)
        {
            MessageBox.Show("Query execute error: " + args.Error);
        }

        private void SelectedWellsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.OldValue != null)
            {
                Graphic g = e.OldValue as Graphic;
                g.UnSelect();
                g.SetZIndex(0);
            }
            
            if (e.NewValue != null)
            {
                Graphic g = e.NewValue as Graphic;
                g.Select();
                g.SetZIndex(1);
                
                //Relationship query
                RelationshipParameter relationshipParameters = new RelationshipParameter()
                {
                    ObjectIds = new int[] {Convert.ToInt32(g.Attributes[SelectedWellsTreeView.Tag as string])},
                    OutFields = new string[] { "OBJECTID, API_NUMBER, ELEVATION, FORMATION, TOP" },
                    RelationshipId = 3,
                    OutSpatialReference = MyMap.SpatialReference
                };
                
                queryTask.ExecuteRelationshipQueryAsync(relationshipParameters);
            }
        }

        void QueryTask_ExecuteRelationshipQueryCompleted(object sender, RelationshipEventArgs e)
        {
            RelationshipResult pr = e.Result;
            if (pr.RelatedRecordsGroup.Count == 0)
            {
                RelatedRowsDataGrid.ItemsSource = null;
            }
            else
            {
                foreach (var pair in pr.RelatedRecordsGroup)
                {
                    RelatedRowsDataGrid.ItemsSource = pair.Value;
                }
            }
        }

    }
}
