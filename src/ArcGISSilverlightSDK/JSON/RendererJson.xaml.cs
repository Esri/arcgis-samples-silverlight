using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISSilverlightSDK
{
    public partial class RendererJson : UserControl
    {
        public RendererJson()
        {
            InitializeComponent();
            CreateRendererJson();
        }

        private void Button_Load(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                TextBox textbox = LayoutRoot.FindName((sender as Button).Tag.ToString()) as TextBox;
                IRenderer irenderer = Renderer.FromJson(textbox.Text);
                (MyMap.Layers["MyFeatureLayerStates"] as FeatureLayer).Renderer = irenderer;
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GraphicsLayer creation failed", MessageBoxButton.OK);
            }
        }

        private void CreateRendererJson()
        {
          string jsonSimple = @"
{
  ""type"": ""simple"",
  ""symbol"": {
    ""type"": ""esriSFS"",
    ""style"": ""esriSFSSolid"",
    ""color"": [
      100,
      205,
      250,
      100
    ],
    ""outline"": {
      ""type"": ""esriSLS"",
      ""style"": ""esriSLSSolid"",
      ""color"": [
        110,
        110,
        110,
        255
      ],
      ""width"": 1.0
    }
  },
  ""label"": """",
  ""description"": """"
}
";

          JsonTextBoxSimple.Text = jsonSimple;

          string jsonClassBreaks = @"
{
 ""type"": ""classBreaks"",
 ""field"": ""POP1999"",
 ""minValue"": 293782,
 ""classBreakInfos"": [
  {
   ""classMinValue"": 293782,
   ""classMaxValue"": 2926324,
   ""label"": ""293782 - 2926324"",
   ""description"": """",
   ""symbol"": {
    ""type"": ""esriSFS"",
    ""style"": ""esriSFSSolid"",
    ""outline"": {
     ""type"": ""esriSLS"",
     ""style"": ""esriSLSSolid"",
     ""color"": [
      0,
      0,
      0,
      255
     ],
     ""width"": 1
    },
    ""color"": [
     0,
     255,
     0,
     255
    ]
   }
  },
  {
   ""classMinValue"": 2926325,
   ""classMaxValue"": 7078515,
   ""label"": ""2926325 - 7078515"",
   ""description"": """",
   ""symbol"": {
    ""type"": ""esriSFS"",
    ""style"": ""esriSFSSolid"",
    ""outline"": {
     ""type"": ""esriSLS"",
     ""style"": ""esriSLSSolid"",
     ""color"": [
      0,
      0,
      0,
      255
     ],
     ""width"": 1
    },
    ""color"": [
     0,
     255,
     170,
     255
    ]
   }
  },
  {
   ""classMinValue"": 7078516,
   ""classMaxValue"": 15982378,
   ""label"": ""7078516 - 15982378"",
   ""description"": """",
   ""symbol"": {
    ""type"": ""esriSFS"",
    ""style"": ""esriSFSSolid"",
    ""outline"": {
     ""type"": ""esriSLS"",
     ""style"": ""esriSLSSolid"",
     ""color"": [
      0,
      0,
      0,
      255
     ],
     ""width"": 1
    },
    ""color"": [
     0,
     170,
     255,
     255
    ]
   }
  },
  {
   ""classMinValue"": 15982379,
   ""classMaxValue"": 38871648,
   ""label"": ""15982379 - 38871648"",
   ""description"": """",
   ""symbol"": {
    ""type"": ""esriSFS"",
    ""style"": ""esriSFSSolid"",
    ""outline"": {
     ""type"": ""esriSLS"",
     ""style"": ""esriSLSSolid"",
     ""color"": [
      0,
      0,
      0,
      255
     ],
     ""width"": 1
    },
    ""color"": [
     0,
     0,
     255,
     255
    ]
   }
  }
 ]
}
";

          JsonTextBoxClassBreaks.Text = jsonClassBreaks;

          string jsonUniqueValue = @"
{
 ""type"": ""uniqueValue"",
 ""field1"": ""SUB_REGION"",
 ""field2"": """",
 ""field3"": """",
 ""fieldDelimiter"": "","",
 ""defaultSymbol"": {
  ""type"": ""esriSFS"",
  ""style"": ""esriSFSSolid"",
  ""color"": [
   115,
   76,
   0,
   255
  ],
  ""outline"": {
   ""type"": ""esriSLS"",
   ""style"": ""esriSLSSolid"",
   ""color"": [
    110,
    110,
    110,
    255
   ],
   ""width"": 1
  }
 },
 ""defaultLabel"": """",
 ""uniqueValueInfos"": [
  {
   ""value"": ""E N Cen"",
   ""count"": 5,
   ""label"": ""E N Cen"",
   ""description"": """",
   ""symbol"": {
    ""type"": ""esriSFS"",
    ""style"": ""esriSFSSolid"",
    ""outline"": {
     ""type"": ""esriSLS"",
     ""style"": ""esriSLSSolid"",
     ""color"": [
      110,
      110,
      110,
      255
     ],
     ""width"": 1
    },
    ""color"": [
     115,
     77,
     0,
     255
    ]
   }
  },
  {
   ""value"": ""E S Cen"",
   ""count"": 4,
   ""label"": ""E S Cen"",
   ""description"": """",
   ""symbol"": {
    ""type"": ""esriSFS"",
    ""style"": ""esriSFSSolid"",
    ""outline"": {
     ""type"": ""esriSLS"",
     ""style"": ""esriSLSSolid"",
     ""color"": [
      110,
      110,
      110,
      255
     ],
     ""width"": 1
    },
    ""color"": [
     92,
     130,
     3,
     255
    ]
   }
  },
  {
   ""value"": ""Mid Atl"",
   ""count"": 3,
   ""label"": ""Mid Atl"",
   ""description"": """",
   ""symbol"": {
    ""type"": ""esriSFS"",
    ""style"": ""esriSFSSolid"",
    ""outline"": {
     ""type"": ""esriSLS"",
     ""style"": ""esriSLSSolid"",
     ""color"": [
      110,
      110,
      110,
      255
     ],
     ""width"": 1
    },
    ""color"": [
     14,
     148,
     4,
     255
    ]
   }
  },
  {
   ""value"": ""Mtn"",
   ""count"": 8,
   ""label"": ""Mtn"",
   ""description"": """",
   ""symbol"": {
    ""type"": ""esriSFS"",
    ""style"": ""esriSFSSolid"",
    ""outline"": {
     ""type"": ""esriSLS"",
     ""style"": ""esriSLSSolid"",
     ""color"": [
      110,
      110,
      110,
      255
     ],
     ""width"": 1
    },
    ""color"": [
     7,
     166,
     97,
     255
    ]
   }
  },
  {
   ""value"": ""N Eng"",
   ""count"": 6,
   ""label"": ""N Eng"",
   ""description"": """",
   ""symbol"": {
    ""type"": ""esriSFS"",
    ""style"": ""esriSFSSolid"",
    ""outline"": {
     ""type"": ""esriSLS"",
     ""style"": ""esriSLSSolid"",
     ""color"": [
      110,
      110,
      110,
      255
     ],
     ""width"": 1
    },
    ""color"": [
     9,
     149,
     184,
     255
    ]
   }
  },
  {
   ""value"": ""Pacific"",
   ""count"": 5,
   ""label"": ""Pacific"",
   ""description"": """",
   ""symbol"": {
    ""type"": ""esriSFS"",
    ""style"": ""esriSFSSolid"",
    ""outline"": {
     ""type"": ""esriSLS"",
     ""style"": ""esriSLSSolid"",
     ""color"": [
      110,
      110,
      110,
      255
     ],
     ""width"": 1
    },
    ""color"": [
     14,
     45,
     201,
     255
    ]
   }
  },
  {
   ""value"": ""S Atl"",
   ""count"": 9,
   ""label"": ""S Atl"",
   ""description"": """",
   ""symbol"": {
    ""type"": ""esriSFS"",
    ""style"": ""esriSFSSolid"",
    ""outline"": {
     ""type"": ""esriSLS"",
     ""style"": ""esriSLSSolid"",
     ""color"": [
      110,
      110,
      110,
      255
     ],
     ""width"": 1
    },
    ""color"": [
     112,
     18,
     219,
     255
    ]
   }
  },
  {
   ""value"": ""W N Cen"",
   ""count"": 7,
   ""label"": ""W N Cen"",
   ""description"": """",
   ""symbol"": {
    ""type"": ""esriSFS"",
    ""style"": ""esriSFSSolid"",
    ""outline"": {
     ""type"": ""esriSLS"",
     ""style"": ""esriSLSSolid"",
     ""color"": [
      110,
      110,
      110,
      255
     ],
     ""width"": 1
    },
    ""color"": [
     237,
     21,
     216,
     255
    ]
   }
  },
  {
   ""value"": ""W S Cen"",
   ""count"": 4,
   ""label"": ""W S Cen"",
   ""description"": """",
   ""symbol"": {
    ""type"": ""esriSFS"",
    ""style"": ""esriSFSSolid"",
    ""outline"": {
     ""type"": ""esriSLS"",
     ""style"": ""esriSLSSolid"",
     ""color"": [
      110,
      110,
      110,
      255
     ],
     ""width"": 1
    },
    ""color"": [
     255,
     25,
     87,
     255
    ]
   }
  }
 ]
}

";

          JsonTextBoxUniqueValue.Text = jsonUniqueValue;

        }

        private void FeatureLayer_Initialized(object sender, EventArgs e)
        {
            FeatureLayer featureLayer = sender as FeatureLayer;
            JsonTextBoxCurrent.Text = (featureLayer.Renderer as IJsonSerializable).ToJson();
            featureLayer.MaxAllowableOffset = MyMap.Resolution * 4;
        }

        private void Button_ResetMap(object sender, RoutedEventArgs e)
        {
            (MyMap.Layers["MyFeatureLayerStates"] as FeatureLayer).Renderer = 
                (MyMap.Layers["MyFeatureLayerStates"] as FeatureLayer).LayerInfo.Renderer;
        }

        private void FeatureLayer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Renderer")
            {
                FeatureLayer featureLayer = sender as FeatureLayer;
                JsonTextBoxCurrent.Text = (featureLayer.Renderer as IJsonSerializable).ToJson();
            }
        }
    }
}
