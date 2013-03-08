using System.Windows.Controls;
using System.Text;
using ESRI.ArcGIS.Client.Portal;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.WebMap;
using ESRI.ArcGIS.Client;
using System.Windows;
using System;

namespace ArcGISSilverlightSDK
{
    public partial class PortalMetadata : UserControl
    {
        public PortalMetadata()
        {
            InitializeComponent();
        }

        private void LoadPortalInfo_Click(object sender, RoutedEventArgs e)
        {
            PropertiesListBox.Items.Clear();
            if (String.IsNullOrEmpty(PortalUrltextbox.Text))
                return;
            InitializePortal(PortalUrltextbox.Text);
        }

        public void InitializePortal(string PortalUrl)
        {
            ArcGISPortal arcgisPortal = new ArcGISPortal();
            arcgisPortal.InitializeAsync(PortalUrl, (p, ex) =>
            {
                if (ex == null)
                {
                    ArcGISPortalInfo portalInfo = p.ArcGISPortalInfo;
                    if (portalInfo == null)
                    {
                        MessageBox.Show("Portal Information could not be retrieved");
                        return;
                    }
                    PropertiesListBox.Items.Add(string.Format("Current Version: {0}", p.CurrentVersion));
                    PropertiesListBox.Items.Add(string.Format("Access: {0}", portalInfo.Access));
                    PropertiesListBox.Items.Add(string.Format("Host Name: {0}", portalInfo.PortalHostname));
                    PropertiesListBox.Items.Add(string.Format("Name: {0}", portalInfo.PortalName));
                    PropertiesListBox.Items.Add(string.Format("Mode: {0}", portalInfo.PortalMode));

                    ESRI.ArcGIS.Client.WebMap.BaseMap basemap = portalInfo.DefaultBaseMap;

                    PropertiesListBox.Items.Add(string.Format("Default BaseMap Title: {0}", basemap.Title));
                    PropertiesListBox.Items.Add(string.Format("WebMap Layers ({0}):", basemap.Layers.Count));

                    foreach (WebMapLayer webmapLayer in basemap.Layers)
                    {
                        PropertiesListBox.Items.Add(webmapLayer.Url);
                    }

                    portalInfo.GetFeaturedGroupsAsync((portalgroup, exp) =>
                    {
                        if (exp == null)
                        {
                            PropertiesListBox.Items.Add("Groups:");

                            ListBox listGroups = new ListBox();
                            listGroups.ItemTemplate = LayoutRoot.Resources["PortalGroupTemplate"] as DataTemplate;

                            listGroups.ItemsSource = portalgroup;
                            PropertiesListBox.Items.Add(listGroups);
                        }
                    });

                    portalInfo.SearchFeaturedItemsAsync(new SearchParameters() { Limit = 15 }, (result, err) =>
                    {
                        if (err == null)
                        {
                            FeaturedMapsList.ItemsSource = result.Results;
                        }
                    });
                }
                else
                    MessageBox.Show("Failed to initialize" + ex.Message.ToString());
            });

        }
    }
}
