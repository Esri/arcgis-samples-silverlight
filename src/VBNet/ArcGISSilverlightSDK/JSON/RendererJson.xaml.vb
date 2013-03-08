Imports Microsoft.VisualBasic
Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client

Partial Public Class RendererJson
	Inherits UserControl
	Public Sub New()
		InitializeComponent()
		CreateRendererJson()
	End Sub

	Private Sub Button_Load(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
		Try
			Dim textbox As TextBox = TryCast(LayoutRoot.FindName((TryCast(sender, Button)).Tag.ToString()), TextBox)
			Dim irenderer As IRenderer = Renderer.FromJson(textbox.Text)
			TryCast(MyMap.Layers("MyFeatureLayerStates"), FeatureLayer).Renderer = irenderer

		Catch ex As Exception
			MessageBox.Show(ex.Message, "GraphicsLayer creation failed", MessageBoxButton.OK)
		End Try
	End Sub

  Private Sub CreateRendererJson()
    Dim jsonSimple As String = "" & ControlChars.CrLf & "{" & ControlChars.CrLf & "  ""type"": ""simple""," &
      ControlChars.CrLf & "  ""symbol"": {" & ControlChars.CrLf & "    ""type"": ""esriSFS""," &
      ControlChars.CrLf & "    ""style"": ""esriSFSSolid""," & ControlChars.CrLf & "    ""color"": [" &
      ControlChars.CrLf & "      100," & ControlChars.CrLf & "      205," & ControlChars.CrLf & "      250," &
      ControlChars.CrLf & "      100" & ControlChars.CrLf & "    ]," & ControlChars.CrLf & "    ""outline"": {" &
      ControlChars.CrLf & "      ""type"": ""esriSLS""," & ControlChars.CrLf & "      ""style"": ""esriSLSSolid""," &
      ControlChars.CrLf & "      ""color"": [" & ControlChars.CrLf & "        110," & ControlChars.CrLf & "        110," &
      ControlChars.CrLf & "        110," & ControlChars.CrLf & "        255" & ControlChars.CrLf & "      ]," &
      ControlChars.CrLf & "      ""width"": 1.0" & ControlChars.CrLf & "    }" & ControlChars.CrLf & "  }," &
      ControlChars.CrLf & "  ""label"": """"," & ControlChars.CrLf & "  ""description"": """"" & ControlChars.CrLf & "}" &
      ControlChars.CrLf & ""

    JsonTextBoxSimple.Text = jsonSimple

    Dim jsonClassBreaks As String = "" & ControlChars.CrLf & "{" & ControlChars.CrLf & " ""type"": ""classBreaks""," &
      ControlChars.CrLf & " ""field"": ""POP1999""," & ControlChars.CrLf & " ""minValue"": 293782," &
      ControlChars.CrLf & " ""classBreakInfos"": [" & ControlChars.CrLf & "  {" &
      ControlChars.CrLf & "   ""classMinValue"": 293782," & ControlChars.CrLf & "   ""classMaxValue"": 2926324," &
      ControlChars.CrLf & "   ""label"": ""293782 - 2926324""," & ControlChars.CrLf & "   ""description"": """"," &
      ControlChars.CrLf & "   ""symbol"": {" & ControlChars.CrLf & "    ""type"": ""esriSFS""," &
      ControlChars.CrLf & "    ""style"": ""esriSFSSolid""," & ControlChars.CrLf & "    ""outline"": {" &
      ControlChars.CrLf & "     ""type"": ""esriSLS""," & ControlChars.CrLf & "     ""style"": ""esriSLSSolid""," &
      ControlChars.CrLf & "     ""color"": [" & ControlChars.CrLf & "      0," & ControlChars.CrLf & "      0," &
      ControlChars.CrLf & "      0," & ControlChars.CrLf & "      255" & ControlChars.CrLf & "     ]," &
      ControlChars.CrLf & "     ""width"": 1" & ControlChars.CrLf & "    }," & ControlChars.CrLf & "    ""color"": [" &
      ControlChars.CrLf & "     0," & ControlChars.CrLf & "     255," & ControlChars.CrLf & "     0," &
      ControlChars.CrLf & "     255" & ControlChars.CrLf & "    ]" & ControlChars.CrLf & "   }" &
      ControlChars.CrLf & "  }," & ControlChars.CrLf & "  {" & ControlChars.CrLf & "   ""classMinValue"": 2926325," &
      ControlChars.CrLf & "   ""classMaxValue"": 7078515," & ControlChars.CrLf & "   ""label"": ""2926325 - 7078515""," &
      ControlChars.CrLf & "   ""description"": """"," & ControlChars.CrLf & "   ""symbol"": {" &
      ControlChars.CrLf & "    ""type"": ""esriSFS""," & ControlChars.CrLf & "    ""style"": ""esriSFSSolid""," &
      ControlChars.CrLf & "    ""outline"": {" & ControlChars.CrLf & "     ""type"": ""esriSLS""," &
      ControlChars.CrLf & "     ""style"": ""esriSLSSolid""," & ControlChars.CrLf & "     ""color"": [" &
      ControlChars.CrLf & "      0," & ControlChars.CrLf & "      0," & ControlChars.CrLf & "      0," &
      ControlChars.CrLf & "      255" & ControlChars.CrLf & "     ]," & ControlChars.CrLf & "     ""width"": 1" &
      ControlChars.CrLf & "    }," & ControlChars.CrLf & "    ""color"": [" & ControlChars.CrLf & "     0," &
      ControlChars.CrLf & "     255," & ControlChars.CrLf & "     170," & ControlChars.CrLf & "     255" &
      ControlChars.CrLf & "    ]" & ControlChars.CrLf & "   }" & ControlChars.CrLf & "  }," & ControlChars.CrLf & "  {" &
      ControlChars.CrLf & "   ""classMinValue"": 7078516," & ControlChars.CrLf & "   ""classMaxValue"": 15982378," &
      ControlChars.CrLf & "   ""label"": ""7078516 - 15982378""," & ControlChars.CrLf & "   ""description"": """"," &
      ControlChars.CrLf & "   ""symbol"": {" & ControlChars.CrLf & "    ""type"": ""esriSFS""," &
      ControlChars.CrLf & "    ""style"": ""esriSFSSolid""," & ControlChars.CrLf & "    ""outline"": {" &
      ControlChars.CrLf & "     ""type"": ""esriSLS""," & ControlChars.CrLf & "     ""style"": ""esriSLSSolid""," &
      ControlChars.CrLf & "     ""color"": [" & ControlChars.CrLf & "      0," & ControlChars.CrLf & "      0," &
      ControlChars.CrLf & "      0," & ControlChars.CrLf & "      255" & ControlChars.CrLf & "     ]," &
      ControlChars.CrLf & "     ""width"": 1" & ControlChars.CrLf & "    }," & ControlChars.CrLf & "    ""color"": [" &
      ControlChars.CrLf & "     0," & ControlChars.CrLf & "     170," & ControlChars.CrLf & "     255," &
      ControlChars.CrLf & "     255" & ControlChars.CrLf & "    ]" & ControlChars.CrLf & "   }" & ControlChars.CrLf & "  }," &
      ControlChars.CrLf & "  {" & ControlChars.CrLf & "   ""classMinValue"": 15982379," & ControlChars.CrLf & "   ""classMaxValue"": 38871648," &
      ControlChars.CrLf & "   ""label"": ""15982379 - 38871648""," & ControlChars.CrLf & "   ""description"": """"," &
      ControlChars.CrLf & "   ""symbol"": {" & ControlChars.CrLf & "    ""type"": ""esriSFS""," & ControlChars.CrLf & "    ""style"": ""esriSFSSolid""," &
      ControlChars.CrLf & "    ""outline"": {" & ControlChars.CrLf & "     ""type"": ""esriSLS""," & ControlChars.CrLf & "     ""style"": ""esriSLSSolid""," &
      ControlChars.CrLf & "     ""color"": [" & ControlChars.CrLf & "      0," & ControlChars.CrLf & "      0," & ControlChars.CrLf & "      0," &
      ControlChars.CrLf & "      255" & ControlChars.CrLf & "     ]," & ControlChars.CrLf & "     ""width"": 1" & ControlChars.CrLf & "    }," &
      ControlChars.CrLf & "    ""color"": [" & ControlChars.CrLf & "     0," & ControlChars.CrLf & "     0," & ControlChars.CrLf & "     255," &
      ControlChars.CrLf & "     255" & ControlChars.CrLf & "    ]" & ControlChars.CrLf & "   }" & ControlChars.CrLf & "  }" &
      ControlChars.CrLf & " ]" &
      ControlChars.CrLf & "}" & ControlChars.CrLf & ""

    JsonTextBoxClassBreaks.Text = jsonClassBreaks

    Dim jsonUniqueValue As String = "" & ControlChars.CrLf & "{" & ControlChars.CrLf & " ""type"": ""uniqueValue""," &
      ControlChars.CrLf & " ""field1"": ""SUB_REGION""," & ControlChars.CrLf & " ""field2"": """"," &
      ControlChars.CrLf & " ""field3"": """"," & ControlChars.CrLf & " ""fieldDelimiter"": "",""," &
      ControlChars.CrLf & " ""defaultSymbol"": {" & ControlChars.CrLf & "  ""type"": ""esriSFS""," &
      ControlChars.CrLf & "  ""style"": ""esriSFSSolid""," & ControlChars.CrLf & "  ""color"": [" &
      ControlChars.CrLf & "   115," & ControlChars.CrLf & "   76," & ControlChars.CrLf & "   0," &
      ControlChars.CrLf & "   255" & ControlChars.CrLf & "  ]," & ControlChars.CrLf & "  ""outline"": {" &
      ControlChars.CrLf & "   ""type"": ""esriSLS""," & ControlChars.CrLf & "   ""style"": ""esriSLSSolid""," &
      ControlChars.CrLf & "   ""color"": [" & ControlChars.CrLf & "    110," & ControlChars.CrLf & "    110," &
      ControlChars.CrLf & "    110," & ControlChars.CrLf & "    255" & ControlChars.CrLf & "   ]," &
      ControlChars.CrLf & "   ""width"": 1" & ControlChars.CrLf & "  }" & ControlChars.CrLf & " }," &
      ControlChars.CrLf & " ""defaultLabel"": """"," & ControlChars.CrLf & " ""uniqueValueInfos"": [" &
      ControlChars.CrLf & "  {" & ControlChars.CrLf & "   ""value"": ""E N Cen""," & ControlChars.CrLf & "   ""count"": 5," &
      ControlChars.CrLf & "   ""label"": ""E N Cen""," & ControlChars.CrLf & "   ""description"": """"," &
      ControlChars.CrLf & "   ""symbol"": {" & ControlChars.CrLf & "    ""type"": ""esriSFS""," &
      ControlChars.CrLf & "    ""style"": ""esriSFSSolid""," & ControlChars.CrLf & "    ""outline"": {" &
      ControlChars.CrLf & "     ""type"": ""esriSLS""," & ControlChars.CrLf & "     ""style"": ""esriSLSSolid""," &
      ControlChars.CrLf & "     ""color"": [" & ControlChars.CrLf & "      110," & ControlChars.CrLf & "      110," &
      ControlChars.CrLf & "      110," & ControlChars.CrLf & "      255" & ControlChars.CrLf & "     ]," &
      ControlChars.CrLf & "     ""width"": 1" & ControlChars.CrLf & "    }," & ControlChars.CrLf & "    ""color"": [" &
      ControlChars.CrLf & "     115," & ControlChars.CrLf & "     77," & ControlChars.CrLf & "     0," &
      ControlChars.CrLf & "     255" & ControlChars.CrLf & "    ]" & ControlChars.CrLf & "   }" & ControlChars.CrLf & "  }," &
      ControlChars.CrLf & "  {" & ControlChars.CrLf & "   ""value"": ""E S Cen""," & ControlChars.CrLf & "   ""count"": 4," &
      ControlChars.CrLf & "   ""label"": ""E S Cen""," & ControlChars.CrLf & "   ""description"": """"," & ControlChars.CrLf & "   ""symbol"": {" &
      ControlChars.CrLf & "    ""type"": ""esriSFS""," & ControlChars.CrLf & "    ""style"": ""esriSFSSolid""," &
      ControlChars.CrLf & "    ""outline"": {" & ControlChars.CrLf & "     ""type"": ""esriSLS""," &
      ControlChars.CrLf & "     ""style"": ""esriSLSSolid""," & ControlChars.CrLf & "     ""color"": [" &
      ControlChars.CrLf & "      110," & ControlChars.CrLf & "      110," & ControlChars.CrLf & "      110," &
      ControlChars.CrLf & "      255" & ControlChars.CrLf & "     ]," & ControlChars.CrLf & "     ""width"": 1" &
      ControlChars.CrLf & "    }," & ControlChars.CrLf & "    ""color"": [" & ControlChars.CrLf & "     92," &
      ControlChars.CrLf & "     130," & ControlChars.CrLf & "     3," & ControlChars.CrLf & "     255" &
      ControlChars.CrLf & "    ]" & ControlChars.CrLf & "   }" & ControlChars.CrLf & "  }," & ControlChars.CrLf & "  {" &
      ControlChars.CrLf & "   ""value"": ""Mid Atl""," & ControlChars.CrLf & "   ""count"": 3," &
      ControlChars.CrLf & "   ""label"": ""Mid Atl""," & ControlChars.CrLf & "   ""description"": """"," &
      ControlChars.CrLf & "   ""symbol"": {" & ControlChars.CrLf & "    ""type"": ""esriSFS""," &
      ControlChars.CrLf & "    ""style"": ""esriSFSSolid""," & ControlChars.CrLf & "    ""outline"": {" &
      ControlChars.CrLf & "     ""type"": ""esriSLS""," & ControlChars.CrLf & "     ""style"": ""esriSLSSolid""," &
      ControlChars.CrLf & "     ""color"": [" & ControlChars.CrLf & "      110," & ControlChars.CrLf & "      110," &
      ControlChars.CrLf & "      110," & ControlChars.CrLf & "      255" & ControlChars.CrLf & "     ]," &
      ControlChars.CrLf & "     ""width"": 1" & ControlChars.CrLf & "    }," & ControlChars.CrLf & "    ""color"": [" &
      ControlChars.CrLf & "     14," & ControlChars.CrLf & "     148," & ControlChars.CrLf & "     4," &
      ControlChars.CrLf & "     255" & ControlChars.CrLf & "    ]" & ControlChars.CrLf & "   }" & ControlChars.CrLf & "  }," &
      ControlChars.CrLf & "  {" & ControlChars.CrLf & "   ""value"": ""Mtn""," & ControlChars.CrLf & "   ""count"": 8," &
      ControlChars.CrLf & "   ""label"": ""Mtn""," & ControlChars.CrLf & "   ""description"": """"," & ControlChars.CrLf & "   ""symbol"": {" &
      ControlChars.CrLf & "    ""type"": ""esriSFS""," & ControlChars.CrLf & "    ""style"": ""esriSFSSolid""," &
      ControlChars.CrLf & "    ""outline"": {" & ControlChars.CrLf & "     ""type"": ""esriSLS""," &
      ControlChars.CrLf & "     ""style"": ""esriSLSSolid""," & ControlChars.CrLf & "     ""color"": [" &
      ControlChars.CrLf & "      110," & ControlChars.CrLf & "      110," & ControlChars.CrLf & "      110," &
      ControlChars.CrLf & "      255" & ControlChars.CrLf & "     ]," & ControlChars.CrLf & "     ""width"": 1" &
      ControlChars.CrLf & "    }," & ControlChars.CrLf & "    ""color"": [" & ControlChars.CrLf & "     7," &
      ControlChars.CrLf & "     166," & ControlChars.CrLf & "     97," & ControlChars.CrLf & "     255" &
      ControlChars.CrLf & "    ]" & ControlChars.CrLf & "   }" & ControlChars.CrLf & "  }," & ControlChars.CrLf & "  {" &
      ControlChars.CrLf & "   ""value"": ""N Eng""," & ControlChars.CrLf & "   ""count"": 6," & ControlChars.CrLf & "   ""label"": ""N Eng""," &
      ControlChars.CrLf & "   ""description"": """"," & ControlChars.CrLf & "   ""symbol"": {" & ControlChars.CrLf & "    ""type"": ""esriSFS""," &
      ControlChars.CrLf & "    ""style"": ""esriSFSSolid""," & ControlChars.CrLf & "    ""outline"": {" & ControlChars.CrLf & "     ""type"": ""esriSLS""," &
      ControlChars.CrLf & "     ""style"": ""esriSLSSolid""," & ControlChars.CrLf & "     ""color"": [" & ControlChars.CrLf & "      110," &
      ControlChars.CrLf & "      110," & ControlChars.CrLf & "      110," & ControlChars.CrLf & "      255" & ControlChars.CrLf & "     ]," &
      ControlChars.CrLf & "     ""width"": 1" & ControlChars.CrLf & "    }," & ControlChars.CrLf & "    ""color"": [" &
      ControlChars.CrLf & "     9," & ControlChars.CrLf & "     149," & ControlChars.CrLf & "     184," & ControlChars.CrLf & "     255" &
      ControlChars.CrLf & "    ]" & ControlChars.CrLf & "   }" & ControlChars.CrLf & "  }," & ControlChars.CrLf & "  {" &
      ControlChars.CrLf & "   ""value"": ""Pacific""," & ControlChars.CrLf & "   ""count"": 5," & ControlChars.CrLf & "   ""label"": ""Pacific""," &
      ControlChars.CrLf & "   ""description"": """"," & ControlChars.CrLf & "   ""symbol"": {" & ControlChars.CrLf & "    ""type"": ""esriSFS""," &
      ControlChars.CrLf & "    ""style"": ""esriSFSSolid""," & ControlChars.CrLf & "    ""outline"": {" & ControlChars.CrLf & "     ""type"": ""esriSLS""," &
      ControlChars.CrLf & "     ""style"": ""esriSLSSolid""," & ControlChars.CrLf & "     ""color"": [" & ControlChars.CrLf & "      110," &
      ControlChars.CrLf & "      110," & ControlChars.CrLf & "      110," & ControlChars.CrLf & "      255" & ControlChars.CrLf & "     ]," &
      ControlChars.CrLf & "     ""width"": 1" & ControlChars.CrLf & "    }," & ControlChars.CrLf & "    ""color"": [" &
      ControlChars.CrLf & "     14," & ControlChars.CrLf & "     45," & ControlChars.CrLf & "     201," & ControlChars.CrLf & "     255" &
      ControlChars.CrLf & "    ]" & ControlChars.CrLf & "   }" & ControlChars.CrLf & "  }," & ControlChars.CrLf & "  {" &
      ControlChars.CrLf & "   ""value"": ""S Atl""," & ControlChars.CrLf & "   ""count"": 9," & ControlChars.CrLf & "   ""label"": ""S Atl""," &
      ControlChars.CrLf & "   ""description"": """"," & ControlChars.CrLf & "   ""symbol"": {" & ControlChars.CrLf & "    ""type"": ""esriSFS""," &
      ControlChars.CrLf & "    ""style"": ""esriSFSSolid""," & ControlChars.CrLf & "    ""outline"": {" & ControlChars.CrLf & "     ""type"": ""esriSLS""," &
      ControlChars.CrLf & "     ""style"": ""esriSLSSolid""," & ControlChars.CrLf & "     ""color"": [" & ControlChars.CrLf & "      110," &
      ControlChars.CrLf & "      110," & ControlChars.CrLf & "      110," & ControlChars.CrLf & "      255" & ControlChars.CrLf & "     ]," &
      ControlChars.CrLf & "     ""width"": 1" & ControlChars.CrLf & "    }," & ControlChars.CrLf & "    ""color"": [" &
      ControlChars.CrLf & "     112," & ControlChars.CrLf & "     18," & ControlChars.CrLf & "     219," & ControlChars.CrLf & "     255" &
      ControlChars.CrLf & "    ]" & ControlChars.CrLf & "   }" & ControlChars.CrLf & "  }," & ControlChars.CrLf & "  {" &
      ControlChars.CrLf & "   ""value"": ""W N Cen""," & ControlChars.CrLf & "   ""count"": 7," & ControlChars.CrLf & "   ""label"": ""W N Cen""," &
      ControlChars.CrLf & "   ""description"": """"," & ControlChars.CrLf & "   ""symbol"": {" & ControlChars.CrLf & "    ""type"": ""esriSFS""," &
      ControlChars.CrLf & "    ""style"": ""esriSFSSolid""," & ControlChars.CrLf & "    ""outline"": {" & ControlChars.CrLf & "     ""type"": ""esriSLS""," &
      ControlChars.CrLf & "     ""style"": ""esriSLSSolid""," & ControlChars.CrLf & "     ""color"": [" & ControlChars.CrLf & "      110," &
      ControlChars.CrLf & "      110," & ControlChars.CrLf & "      110," & ControlChars.CrLf & "      255" & ControlChars.CrLf & "     ]," &
      ControlChars.CrLf & "     ""width"": 1" & ControlChars.CrLf & "    }," & ControlChars.CrLf & "    ""color"": [" & ControlChars.CrLf & "     237," &
      ControlChars.CrLf & "     21," & ControlChars.CrLf & "     216," & ControlChars.CrLf & "     255" & ControlChars.CrLf & "    ]" &
      ControlChars.CrLf & "   }" & ControlChars.CrLf & "  }," & ControlChars.CrLf & "  {" & ControlChars.CrLf & "   ""value"": ""W S Cen""," &
      ControlChars.CrLf & "   ""count"": 4," & ControlChars.CrLf & "   ""label"": ""W S Cen""," & ControlChars.CrLf & "   ""description"": """"," &
      ControlChars.CrLf & "   ""symbol"": {" & ControlChars.CrLf & "    ""type"": ""esriSFS""," & ControlChars.CrLf & "    ""style"": ""esriSFSSolid""," &
      ControlChars.CrLf & "    ""outline"": {" & ControlChars.CrLf & "     ""type"": ""esriSLS""," & ControlChars.CrLf & "     ""style"": ""esriSLSSolid""," &
      ControlChars.CrLf & "     ""color"": [" & ControlChars.CrLf & "      110," & ControlChars.CrLf & "      110," &
      ControlChars.CrLf & "      110," & ControlChars.CrLf & "      255" & ControlChars.CrLf & "     ]," &
      ControlChars.CrLf & "     ""width"": 1" & ControlChars.CrLf & "    }," & ControlChars.CrLf & "    ""color"": [" & ControlChars.CrLf & "     255," &
      ControlChars.CrLf & "     25," & ControlChars.CrLf & "     87," & ControlChars.CrLf & "     255" &
      ControlChars.CrLf & "    ]" & ControlChars.CrLf & "   }" & ControlChars.CrLf & "  }" & ControlChars.CrLf & " ]" &
      ControlChars.CrLf & "}" & ControlChars.CrLf & ControlChars.CrLf & ""

    JsonTextBoxUniqueValue.Text = jsonUniqueValue

  End Sub

	Private Sub FeatureLayer_Initialized(ByVal sender As Object, ByVal e As EventArgs)
		Dim featureLayer As FeatureLayer = TryCast(sender, FeatureLayer)
		JsonTextBoxCurrent.Text = (TryCast(featureLayer.Renderer, IJsonSerializable)).ToJson()
		featureLayer.MaxAllowableOffset = MyMap.Resolution * 4
	End Sub

	Private Sub Button_ResetMap(ByVal sender As Object, ByVal e As RoutedEventArgs)
		TryCast(MyMap.Layers("MyFeatureLayerStates"), FeatureLayer).Renderer = (TryCast(MyMap.Layers("MyFeatureLayerStates"), FeatureLayer)).LayerInfo.Renderer
	End Sub

	Private Sub FeatureLayer_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
		If e.PropertyName = "Renderer" Then
			Dim featureLayer As FeatureLayer = TryCast(sender, FeatureLayer)
			JsonTextBoxCurrent.Text = (TryCast(featureLayer.Renderer, IJsonSerializable)).ToJson()
		End If
	End Sub
End Class
