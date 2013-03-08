Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports System.Windows.Data


Partial Public Class TimeFeatureLayer
  Inherits UserControl
  Public Sub New()
    InitializeComponent()
  End Sub

  Private Sub FeatureLayer_Initialized(ByVal sender As Object, ByVal e As EventArgs)
    Dim intervals As New List(Of DateTime)()
    Dim dt As DateTime = MyTimeSlider.MinimumValue
    Do While dt < MyTimeSlider.MaximumValue
      intervals.Add(dt)
      dt = dt.AddYears(2)
    Loop
    intervals.Add(MyTimeSlider.MaximumValue)
    MyTimeSlider.Intervals = intervals
  End Sub
End Class

