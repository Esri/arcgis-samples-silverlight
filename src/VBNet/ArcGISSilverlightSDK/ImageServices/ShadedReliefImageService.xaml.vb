Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client


  Partial Public Class ShadedReliefImageService
    Inherits UserControl
    Public Sub New()
      InitializeComponent()
    End Sub

    Private Sub ApplyButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      Try
        Dim imageLayer As ArcGISImageServiceLayer = TryCast(MyMap.Layers("ImageServiceLayer"), ArcGISImageServiceLayer)
        Dim renderingRule As New RenderingRule()
        Dim rasterParams As New Dictionary(Of String, Object)()

        If SRRadioButton.IsChecked.Value Then
          renderingRule.RasterFunctionName = "ShadedRelief"
          renderingRule.VariableName = "Raster"

          rasterParams.Add("Azimuth", If(String.IsNullOrEmpty(AzimuthTextBox.Text), 0, Double.Parse(AzimuthTextBox.Text)))
          rasterParams.Add("Altitude", If(String.IsNullOrEmpty(AltitudeTextBox.Text), 0, Double.Parse(AltitudeTextBox.Text)))
          rasterParams.Add("ZFactor", If(String.IsNullOrEmpty(ZFactorTextBox.Text), 0, Double.Parse(ZFactorTextBox.Text)))

          If ColormapCheckBox.IsChecked.Value Then
            rasterParams.Add("Colormap", CreateColorMap())
          Else
            renderingRule.RasterFunctionName = "Hillshade"
            renderingRule.VariableName = "DEM"
          End If
          renderingRule.RasterFunctionArguments = rasterParams
        Else
          renderingRule.RasterFunctionName = "Slope"
          renderingRule.VariableName = "DEM"

          rasterParams.Add("ZFactor", If(String.IsNullOrEmpty(ZFactorTextBox.Text), 0, Double.Parse(ZFactorTextBox.Text)))

          renderingRule.RasterFunctionArguments = rasterParams
        End If

        imageLayer.RenderingRule = renderingRule
        imageLayer.Refresh()

      Catch ex As Exception
        MessageBox.Show(ex.Message)
      End Try
    End Sub

    Private Function CreateColorMap() As Integer()()
      Dim sampleColormap()() As Integer =
          {
              New Integer() {0, 175, 240, 233},
              New Integer() {3, 175, 240, 222},
              New Integer() {7, 177, 242, 212},
              New Integer() {11, 177, 242, 198},
              New Integer() {15, 176, 245, 183},
              New Integer() {19, 185, 247, 178},
              New Integer() {23, 200, 247, 178},
              New Integer() {27, 216, 250, 177},
              New Integer() {31, 232, 252, 179},
              New Integer() {35, 248, 252, 179},
              New Integer() {39, 238, 245, 162},
              New Integer() {43, 208, 232, 135},
              New Integer() {47, 172, 217, 111},
              New Integer() {51, 136, 204, 88},
              New Integer() {55, 97, 189, 66},
              New Integer() {59, 58, 176, 48},
              New Integer() {63, 32, 161, 43},
              New Integer() {67, 18, 148, 50},
              New Integer() {71, 5, 133, 58},
              New Integer() {75, 30, 130, 62},
              New Integer() {79, 62, 138, 59},
              New Integer() {83, 88, 145, 55},
              New Integer() {87, 112, 153, 50},
              New Integer() {91, 136, 158, 46},
              New Integer() {95, 162, 166, 41},
              New Integer() {99, 186, 171, 34},
              New Integer() {103, 212, 178, 25},
              New Integer() {107, 237, 181, 14},
              New Integer() {111, 247, 174, 2},
              New Integer() {115, 232, 144, 2},
              New Integer() {119, 219, 118, 2},
              New Integer() {123, 204, 93, 2},
              New Integer() {127, 191, 71, 2},
              New Integer() {131, 176, 51, 2},
              New Integer() {135, 163, 34, 2},
              New Integer() {139, 148, 21, 1},
              New Integer() {143, 135, 8, 1},
              New Integer() {147, 120, 5, 1},
              New Integer() {151, 117, 14, 2},
              New Integer() {155, 117, 22, 5},
              New Integer() {159, 115, 26, 6},
              New Integer() {163, 112, 31, 7},
              New Integer() {167, 112, 36, 8},
              New Integer() {171, 110, 37, 9},
              New Integer() {175, 107, 41, 11},
              New Integer() {179, 107, 45, 12},
              New Integer() {183, 105, 48, 14},
              New Integer() {187, 115, 61, 28},
              New Integer() {191, 122, 72, 40},
              New Integer() {195, 133, 86, 57},
              New Integer() {199, 140, 99, 73},
              New Integer() {203, 148, 111, 90},
              New Integer() {207, 153, 125, 109},
              New Integer() {213, 163, 148, 139},
              New Integer() {217, 168, 163, 160},
              New Integer() {223, 179, 179, 179},
              New Integer() {227, 189, 189, 189},
              New Integer() {231, 196, 196, 196},
              New Integer() {235, 207, 204, 207},
              New Integer() {239, 217, 215, 217},
              New Integer() {243, 224, 222, 224},
              New Integer() {247, 235, 232, 235},
              New Integer() {251, 245, 242, 245},
              New Integer() {255, 255, 252, 255}
          }

      Return sampleColormap
    End Function

  End Class

