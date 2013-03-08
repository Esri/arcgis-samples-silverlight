Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Toolkit
Imports System.Windows.Data


    Partial Public Class TimeMapService
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub ArcGISDynamicMapServiceLayer_Initialized(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Dim extent As TimeExtent = New TimeExtent(MyTimeSlider.MinimumValue, MyTimeSlider.MaximumValue)
            MyTimeSlider.Intervals = TimeSlider.CreateTimeStopsByTimeInterval(extent, TimeSpan.FromDays(500))

            MyTimeSlider.Value = New TimeExtent(MyTimeSlider.MinimumValue, MyTimeSlider.MinimumValue.AddYears(10))
        End Sub
    End Class

