Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls

Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Toolkit


  Partial Public Class TemporalRendererPoints
    Inherits UserControl
    Public Sub New()
      InitializeComponent()
    End Sub

    Private Sub MyTimeSlider_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
      MyTimeSlider.MinimumValue = Date.Now.Subtract(TimeSpan.FromDays(7)).ToUniversalTime()
      MyTimeSlider.MaximumValue = Date.Now.ToUniversalTime()
      MyTimeSlider.Value = New TimeExtent(MyTimeSlider.MinimumValue, MyTimeSlider.MinimumValue.AddHours(2))
      MyTimeSlider.Intervals = TimeSlider.CreateTimeStopsByTimeInterval(New TimeExtent(MyTimeSlider.MinimumValue, MyTimeSlider.MaximumValue), New TimeSpan(0, 2, 0, 0))
    End Sub
  End Class

