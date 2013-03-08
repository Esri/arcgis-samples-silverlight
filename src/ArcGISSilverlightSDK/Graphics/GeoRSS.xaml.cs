using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISSilverlightSDK
{
  public partial class GeoRSS : UserControl
  {
    public GeoRSS()
    {
      InitializeComponent();
    }

    private void Fetch_Click(object sender, RoutedEventArgs e)
    {
      if (FeedLocationTextBox.Text != String.Empty)
      {
        LoadRSS(FeedLocationTextBox.Text.Trim());
        DispatcherTimer UpdateTimer = new System.Windows.Threading.DispatcherTimer();
        UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 60000);
        UpdateTimer.Tick += (evtsender, args) =>
        {
          LoadRSS(FeedLocationTextBox.Text.Trim());
        };
        UpdateTimer.Start();
      }
    }

    protected void LoadRSS(string uri)
    {
      WebClient wc = new WebClient();
      wc.OpenReadCompleted += wc_OpenReadCompleted;
      Uri feedUri = new Uri(uri, UriKind.Absolute);
      wc.OpenReadAsync(feedUri);
    }

    private void wc_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
    {

      if (e.Error != null)
      {
        FeedLocationTextBox.Text = "Error in Reading Feed. Try Again later!!";
        return;
      }

      // Optional, use LINQ to query GeoRSS feed.
      //UseLinq(e.Result); return;

      using (Stream s = e.Result)
      {
        SyndicationFeed feed;
        List<SyndicationItem> feedItems = new List<SyndicationItem>();

        GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
        graphicsLayer.ClearGraphics();

        using (XmlReader reader = XmlReader.Create(s))
        {
          feed = SyndicationFeed.Load(reader);
          foreach (SyndicationItem feedItem in feed.Items)
          {
            SyndicationElementExtensionCollection ec = feedItem.ElementExtensions;

            string x = "";
            string y = "";
            string magnitude = feedItem.Title.Text;

            foreach (SyndicationElementExtension ee in ec)
            {
              XmlReader xr = ee.GetReader();
              switch (ee.OuterName)
              {
                case ("lat"):
                  {
                    y = xr.ReadElementContentAsString();
                    break;
                  }
                case ("long"):
                  {
                    x = xr.ReadElementContentAsString();
                    break;
                  }
              }
            }

            if (!string.IsNullOrEmpty(x))
            {
              Graphic graphic = new Graphic()
              {
                Geometry = new MapPoint(Convert.ToDouble(x, System.Globalization.CultureInfo.InvariantCulture),
                            Convert.ToDouble(y, System.Globalization.CultureInfo.InvariantCulture),
                                                        new SpatialReference(4326)),
                Symbol = LayoutRoot.Resources["QuakePictureSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol
              };

              graphic.Attributes.Add("MAGNITUDE", magnitude);

              graphicsLayer.Graphics.Add(graphic);
            }
          }
        }
      }
    }

    private void UseLinq(Stream s)
    {
      GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
      graphicsLayer.ClearGraphics();

      XDocument doc = XDocument.Load(s);
      XNamespace geo = "http://www.w3.org/2003/01/geo/wgs84_pos#";

      var rssGraphics = from rssgraphic in doc.Descendants("item")
                        select new RssGraphic
                        {
                          Geometry = new MapPoint(
            Convert.ToDouble(rssgraphic.Element(geo + "long").Value, System.Globalization.CultureInfo.InvariantCulture),
            Convert.ToDouble(rssgraphic.Element(geo + "lat").Value, System.Globalization.CultureInfo.InvariantCulture),
                              new SpatialReference(4326)),
                          Symbol = LayoutRoot.Resources["QuakePictureSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol,
                          RssAttributes = new Dictionary<string, object>() { { "MAGNITUDE", rssgraphic.Element("title").Value } }
                        };

      foreach (RssGraphic rssGraphic in rssGraphics)
      {
        foreach (KeyValuePair<string, object> rssAttribute in rssGraphic.RssAttributes)
        {
          rssGraphic.Attributes.Add(rssAttribute.Key, rssAttribute.Value);
        }
        graphicsLayer.Graphics.Add((Graphic)rssGraphic);
      }
    }

    internal class RssGraphic : Graphic
    {
      public Dictionary<string, object> RssAttributes { get; set; }
    }
  }
}

