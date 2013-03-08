using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class StatisticsRenderOnMap : UserControl
    {
        GraphicsLayer subRegionGraphicsLayer;
        string _layerUrl = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3";

        public StatisticsRenderOnMap()
        {
            InitializeComponent();

            subRegionGraphicsLayer = MyMap.Layers["SumGraphicsLayer"] as GraphicsLayer;

            // Generate a unique value renderer on the server
            GenerateRendererTask generateRendererTask = new GenerateRendererTask();
            generateRendererTask.Url = _layerUrl;

            generateRendererTask.ExecuteCompleted += (o, e) =>
            {
                subRegionGraphicsLayer.Renderer = e.GenerateRendererResult.Renderer;
            };

            UniqueValueDefinition uniqueValueDefinition = new UniqueValueDefinition()
            {
                Fields = new List<string>() { "SUB_REGION" }
            };

            uniqueValueDefinition.ColorRamps.Add(new ColorRamp()
            {
                From = Colors.Purple,
                To = Colors.Yellow,
                Algorithm = Algorithm.LabLChAlgorithm
            });

            GenerateRendererParameters rendererParams = new GenerateRendererParameters()
            {
                ClassificationDefinition = uniqueValueDefinition,
            };

            generateRendererTask.ExecuteAsync(rendererParams);

            // When layers initialized, return all states, generate statistics for a group, 
            // union graphics based on grouped statistics
            MyMap.Layers.LayersInitialized += (a, b) =>
              {
                  // Return all features for states layer
                  QueryTask queryTask1 = new QueryTask(_layerUrl);
                  Query queryAllStates = new Query()
                  {
                      ReturnGeometry = true,
                      OutSpatialReference = MyMap.SpatialReference,
                      Where = "1=1"
                  };
                  queryAllStates.OutFields.Add("SUB_REGION");

                  queryTask1.ExecuteCompleted += (c, d) =>
                      {
                          // Return statistics for states layer. Population and total count of states grouped by sub-region.
                          QueryTask queryTask2 = new QueryTask(_layerUrl);
                          Query query = new Query()
                          {
                              GroupByFieldsForStatistics = new List<string> { "SUB_REGION" },

                              OutStatistics = new List<OutStatistic> { 
                                new OutStatistic(){
                                    OnStatisticField = "POP2000",
                                    OutStatisticFieldName = "SubRegionPopulation",
                                    StatisticType = StatisticType.Sum
                                },
                                new OutStatistic(){
                                    OnStatisticField = "SUB_REGION",
                                    OutStatisticFieldName = "NumberOfStates",
                                    StatisticType = StatisticType.Count
                                }
                             }
                          };

                          queryTask2.ExecuteCompleted += (e, f) =>
                          {
                              // foreach group (sub-region) returned from statistic results
                              foreach (Graphic regionGraphic in f.FeatureSet.Features)
                              {
                                  // Collection of graphics based on sub-region
                                  IEnumerable<Graphic> toUnion =
                                      (f.UserState as FeatureSet).Features.Where(stateGraphic =>
                                          (string)stateGraphic.Attributes["SUB_REGION"] ==
                                          (string)regionGraphic.Attributes["SUB_REGION"]);

                                  // Union graphics based on sub-region, add to graphics layer
                                  GeometryService geometryService = 
                                      new GeometryService("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Utilities/Geometry/GeometryServer");
                                  geometryService.UnionCompleted += (g, h) =>
                                  {
                                      Graphic unionedGraphic = h.UserState as Graphic;
                                      unionedGraphic.Geometry = h.Result;
                                      subRegionGraphicsLayer.Graphics.Add(unionedGraphic);
                                  };
                                  geometryService.UnionAsync(toUnion.ToList(), regionGraphic);
                              }
                          };

                          queryTask2.ExecuteAsync(query, d.FeatureSet);
                      };

                  queryTask1.ExecuteAsync(queryAllStates);
              };
        }
    }
}
