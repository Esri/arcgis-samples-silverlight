Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client


  Partial Public Class StretchImageService
    Inherits UserControl
    Public Sub New()
      InitializeComponent()
    End Sub

    Private Sub ApplyButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      Try
        Dim imageLayer As ArcGISImageServiceLayer = TryCast(MyMap.Layers("ImageServiceLayer"), ArcGISImageServiceLayer)
        Dim renderingRule As New RenderingRule()
        renderingRule.VariableName = "Raster"

        Dim rasterParams As New Dictionary(Of String, Object)()

        Dim strArray() As String = BandIDsTextBox.Text.Split(New Char() {","c}, StringSplitOptions.RemoveEmptyEntries)

        If strArray.Length = 1 OrElse strArray.Length = 2 OrElse strArray.Length > 3 Then
          ValidBandIdsTextBlock.Visibility = System.Windows.Visibility.Visible
          Return
        Else
          ValidBandIdsTextBlock.Visibility = System.Windows.Visibility.Collapsed
        End If

        renderingRule.RasterFunctionName = "Stretch"
        renderingRule.VariableName = "Raster"

        Dim stretchType As Integer = 0
        If StandardDevCheckBox.IsChecked.Value Then
          stretchType = 3
        ElseIf HistogramCheckBox.IsChecked.Value Then
          stretchType = 4
        ElseIf MinMaxCheckBox.IsChecked.Value Then
          stretchType = 5
        End If

        rasterParams.Add("StretchType", stretchType)
        rasterParams.Add("NumberOfStandardDeviations",
                         If(String.IsNullOrEmpty(StnDevTextBox.Text), 1, Double.Parse(StnDevTextBox.Text)))

        Dim statistics()() As Double =
            {
                New Double(3) {0.2, 222.46, 99.35, 1.64},
                New Double(3) {5.56, 100.345, 45.4, 3.96},
                New Double(3) {0, 352.37, 172.284, 2}
            }

        rasterParams.Add("Statistics", statistics)

        Dim gamma() As Double = {1.25, 2, 3.95}
        rasterParams.Add("Gamma", gamma)


        Dim numArray(strArray.Length - 1) As Integer
        For i As Integer = 0 To strArray.Length - 1
          numArray(i) = Integer.Parse(strArray(i))
        Next i

        imageLayer.BandIds = If(numArray.Length < 1, Nothing, numArray)


        renderingRule.RasterFunctionArguments = rasterParams
        imageLayer.RenderingRule = renderingRule
        imageLayer.Refresh()
      Catch ex As Exception
        MessageBox.Show(ex.Message)
      End Try
    End Sub

  End Class

