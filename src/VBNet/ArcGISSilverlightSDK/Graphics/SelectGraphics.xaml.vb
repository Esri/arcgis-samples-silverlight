Imports System.Windows.Controls


    Partial Public Class SelectGraphics
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub GraphicsLayer_MouseLeftButtonDown(ByVal sender As System.Object, ByVal e As ESRI.ArcGIS.Client.GraphicMouseButtonEventArgs)
            e.Graphic.Selected = Not (e.Graphic.Selected)
        End Sub
    End Class

