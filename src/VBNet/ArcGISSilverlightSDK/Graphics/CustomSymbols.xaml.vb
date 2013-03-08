Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Shapes


    Partial Public Class CustomSymbols
        Inherits UserControl
        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub GraphicsLayer_MouseLeftButtonDown(ByVal sender As System.Object, ByVal e As ESRI.ArcGIS.Client.GraphicMouseButtonEventArgs)
            If (e.Graphic.Selected) Then
                e.Graphic.UnSelect()
            Else
                e.Graphic.Select()
            End If
        End Sub
    End Class

