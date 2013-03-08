using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using System.Collections.Generic;

namespace ArcGISSilverlightSDK
{
  public partial class CustomClusterer : UserControl
  {
    public CustomClusterer()
    {
      InitializeComponent();
    }

    private void CheckBox_Checked(object sender, RoutedEventArgs e)
    {
      if (MyMap == null) return;
      GraphicsLayer layer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
      layer.Clusterer = new SumClusterer() { AggregateColumn = "POP1990", SymbolScale = 0.001 };
    }

    private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
      if (MyMap == null) return;
      GraphicsLayer layer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
      layer.Clusterer = null;
    }

    void MyMap_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if ((e.PropertyName == "SpatialReference") &&
          ((sender as ESRI.ArcGIS.Client.Map).SpatialReference != null))
      {
        LoadGraphics();
        MyMap.PropertyChanged -= MyMap_PropertyChanged;
      }
    }

    private void LoadGraphics()
    {
      QueryTask queryTask =
          new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/0");
      queryTask.ExecuteCompleted += queryTask_ExecuteCompleted;

      Query query = new ESRI.ArcGIS.Client.Tasks.Query()
      {
        OutSpatialReference = MyMap.SpatialReference,
        ReturnGeometry = true,
        Where = "1=1"
      };

      query.OutFields.AddRange(new string[] { "CITY_NAME", "POP1990" });

      queryTask.ExecuteAsync(query);
    }

    void queryTask_ExecuteCompleted(object sender, QueryEventArgs args)
    {
      FeatureSet featureSet = args.FeatureSet;

      if (featureSet == null || featureSet.Features.Count < 1)
      {
        MessageBox.Show("No features returned from query");
        return;
      }

      GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;

      foreach (Graphic graphic in featureSet.Features)
        graphicsLayer.Graphics.Add(graphic);
    }
  }

  public class SumClusterer : GraphicsClusterer
  {
    public SumClusterer()
    {
      MinimumColor = Colors.Red;
      MaximumColor = Colors.Yellow;
      SymbolScale = 1;
      base.Radius = 50;
    }

    public string AggregateColumn { get; set; }
    public double SymbolScale { get; set; }
    public Color MinimumColor { get; set; }
    public Color MaximumColor { get; set; }

    protected override Graphic OnCreateGraphic(GraphicCollection cluster, MapPoint point, int maxClusterCount)
    {
      if (cluster.Count == 1) return cluster[0];
      Graphic graphic = null;

      double sum = 0;

      foreach (Graphic g in cluster)
      {
        if (g.Attributes.ContainsKey(AggregateColumn))
        {
          try
          {
            sum += Convert.ToDouble(g.Attributes[AggregateColumn]);
          }
          catch { }
        }
      }
      double size = (sum + 450) / 30;
      size = (Math.Log(sum * SymbolScale / 10) * 10 + 20);
      if (size < 12) size = 12;
      graphic = new Graphic() { Symbol = new ClusterSymbol() { Size = size }, Geometry = point };
      graphic.Attributes.Add("Count", sum);
      graphic.Attributes.Add("Size", size);
      graphic.Attributes.Add("Color", InterpolateColor(size - 12, 100));
      return graphic;
    }

    private static Brush InterpolateColor(double value, double max)
    {
      value = (int)Math.Round(value * 255.0 / max);
      if (value > 255) value = 255;
      else if (value < 0) value = 0;
      return new SolidColorBrush(Color.FromArgb(127, 255, (byte)value, 0));
    }
  }

  internal class ClusterSymbol : ESRI.ArcGIS.Client.Symbols.MarkerSymbol
  {
    public ClusterSymbol()
    {
      string template = @"
            <ControlTemplate xmlns=""http://schemas.microsoft.com/client/2007"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" >
                <Grid IsHitTestVisible=""False"">
					<Ellipse
						Fill=""{Binding Attributes[Color]}"" 
						Width=""{Binding Attributes[Size]}""
						Height=""{Binding Attributes[Size]}"" />
					<Grid HorizontalAlignment=""Center"" VerticalAlignment=""Center"">
					<TextBlock 
						Text=""{Binding Attributes[Count]}"" 
						FontSize=""9"" Margin=""1,1,0,0"" FontWeight=""Bold""
						Foreground=""#99000000"" />
					<TextBlock
						Text=""{Binding Attributes[Count]}"" 
						FontSize=""9"" Margin=""0,0,1,1"" FontWeight=""Bold""
						Foreground=""White"" />
					</Grid>
				</Grid>
			</ControlTemplate>";

      this.ControlTemplate = System.Windows.Markup.XamlReader.Load(template) as ControlTemplate;
    }

    public double Size { get; set; }

    public override double OffsetX
    {
      get
      {
        return Size / 2;
      }
      set
      {
        throw new NotSupportedException();
      }
    }
    public override double OffsetY
    {
      get
      {
        return Size / 2;
      }
      set
      {
        throw new NotSupportedException();
      }
    }
  }
}
