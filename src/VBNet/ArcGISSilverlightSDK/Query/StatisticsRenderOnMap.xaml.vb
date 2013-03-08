Imports System.Collections.Generic
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Tasks
Imports ESRI.ArcGIS.Client
Imports System.Windows.Media
Imports System.Linq

Partial Public Class StatisticsRenderOnMap
  Inherits UserControl

  Private subRegionGraphicsLayer As GraphicsLayer
  Private _layerUrl As String = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3"


  Public Sub New()

    InitializeComponent()

    subRegionGraphicsLayer = TryCast(MyMap.Layers("SumGraphicsLayer"), GraphicsLayer)

    ' Generate a unique value renderer on the server
    Dim generateRendererTask As New GenerateRendererTask()
    generateRendererTask.Url = _layerUrl

    AddHandler generateRendererTask.ExecuteCompleted, Sub(o, e) subRegionGraphicsLayer.Renderer = e.GenerateRendererResult.Renderer

    Dim uniqueValueDefinition As UniqueValueDefinition = New UniqueValueDefinition() With
                                                         {
                                                            .Fields = New List(Of String)() From {"SUB_REGION"}
                                                          }
    uniqueValueDefinition.ColorRamps.Add(New ColorRamp() With
                                         {.From = Colors.Purple,
                                          .To = Colors.Yellow,
                                          .Algorithm = Algorithm.LabLChAlgorithm
                                          })

    Dim rendererParams As GenerateRendererParameters = New GenerateRendererParameters() With
                                                       {.ClassificationDefinition = uniqueValueDefinition}

    generateRendererTask.ExecuteAsync(rendererParams)


    ' When layers initialized, return all states, generate statistics for a group, 
    ' union graphics based on grouped statistics
    AddHandler MyMap.Layers.LayersInitialized, Sub(a, b)
                                                 Dim queryTask1 As New QueryTask(_layerUrl)
                                                 Dim queryAllStates As New Query() With {.ReturnGeometry = True,
                                                                                         .OutSpatialReference = MyMap.SpatialReference,
                                                                                         .Where = "1=1"}
                                                 queryAllStates.OutFields.Add("SUB_REGION")
                                                 AddHandler queryTask1.ExecuteCompleted, Sub(c, d)
                                                                                           Dim queryTask2 As New QueryTask(_layerUrl)
                                                                                           Dim query As Query = New Query() With
                                                                                               {.GroupByFieldsForStatistics = New List(Of String) From {"SUB_REGION"},
                                                                                                .OutStatistics = New List(Of OutStatistic) From {
                                                                                                  New OutStatistic() With {.OnStatisticField = "POP2000",
                                                                                                                           .OutStatisticFieldName = "SubRegionPopulation",
                                                                                                                           .StatisticType = StatisticType.Sum
                                                                                                                          },
                                                                                                 New OutStatistic() With {.OnStatisticField = "SUB_REGION",
                                                                                                                          .StatisticType = StatisticType.Count,
                                                                                                                          .OutStatisticFieldName = "NumberOfStates"
                                                                                                                         }
                                                                                                                                                 }
                                                                                               }
                                                                                           AddHandler queryTask2.ExecuteCompleted, Sub(e, f)
                                                                                                                                     ' foreach group (sub-region) returned from statistic results
                                                                                                                                     For Each regionGraphic As Graphic In f.FeatureSet.Features
                                                                                                                                       ' Collection of graphics based on sub-region
                                                                                                                                       Dim toUnion As IEnumerable(Of Graphic) = (TryCast(f.UserState, FeatureSet)).Features.Where(Function(stateGraphic) CStr(stateGraphic.Attributes("SUB_REGION")) = CStr(regionGraphic.Attributes("SUB_REGION")))

                                                                                                                                       ' Union graphics based on sub-region, add to graphics layer
                                                                                                                                       Dim geometryService As New GeometryService("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Utilities/Geometry/GeometryServer")
                                                                                                                                       AddHandler geometryService.UnionCompleted, Sub(g, h)
                                                                                                                                                                                    Dim unionedGraphic As Graphic = TryCast(h.UserState, Graphic)
                                                                                                                                                                                    unionedGraphic.Geometry = h.Result
                                                                                                                                                                                    subRegionGraphicsLayer.Graphics.Add(unionedGraphic)
                                                                                                                                                                                  End Sub
                                                                                                                                       geometryService.UnionAsync(toUnion.ToList(), regionGraphic)
                                                                                                                                     Next regionGraphic
                                                                                                                                   End Sub

                                                                                           queryTask2.ExecuteAsync(query, d.FeatureSet)

                                                                                         End Sub
                                                 queryTask1.ExecuteAsync(queryAllStates)
                                               End Sub
  End Sub

End Class
