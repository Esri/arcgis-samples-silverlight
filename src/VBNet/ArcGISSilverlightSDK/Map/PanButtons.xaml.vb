Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Geometry


    Partial Public Class PanButtons
        Inherits UserControl
        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub PanClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Dim extent As Envelope = MyMap.Extent
            If extent Is Nothing Then
                Return
            End If
            Dim center As MapPoint = extent.GetCenter()

            Select Case (TryCast(sender, Button)).Tag.ToString()
                Case "W"
                    MyMap.PanTo(New MapPoint(extent.XMin, center.Y))
                Case "E"
                    MyMap.PanTo(New MapPoint(extent.XMax, center.Y))
                Case "N"
                    MyMap.PanTo(New MapPoint(center.X, extent.YMax))
                Case "S"
                    MyMap.PanTo(New MapPoint(center.X, extent.YMin))
                Case "NE"
                    MyMap.PanTo(New MapPoint(extent.XMax, extent.YMax))
                Case "SE"
                    MyMap.PanTo(New MapPoint(extent.XMax, extent.YMin))
                Case "SW"
                    MyMap.PanTo(New MapPoint(extent.XMin, extent.YMin))
                Case "NW"
                    MyMap.PanTo(New MapPoint(extent.XMin, extent.YMax))
                Case Else
            End Select
        End Sub
    End Class

