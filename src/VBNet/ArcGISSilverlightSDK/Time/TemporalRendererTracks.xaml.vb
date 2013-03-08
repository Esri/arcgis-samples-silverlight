Imports System
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Toolkit


    Partial Public Class TemporalRendererTracks
        Inherits UserControl
        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub FeatureLayer_Initialized(ByVal sender As Object, ByVal e As EventArgs)
            Dim extent As New TimeExtent(MyTimeSlider.MinimumValue, MyTimeSlider.MaximumValue)
            MyTimeSlider.Intervals = TimeSlider.CreateTimeStopsByTimeInterval(extent, New TimeSpan(0, 6, 0, 0, 0))
        End Sub
    End Class

