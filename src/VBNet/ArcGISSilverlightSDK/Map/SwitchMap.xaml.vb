Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client


	Partial Public Class SwitchMap
		Inherits UserControl
		Public Sub New()
			InitializeComponent()
		End Sub

		Private Sub RadioButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
			Dim arcgisLayer As ArcGISTiledMapServiceLayer = TryCast(MyMap.Layers("AGOLayer"), ArcGISTiledMapServiceLayer)
			arcgisLayer.Url = TryCast((CType(sender, RadioButton)).Tag, String)
		End Sub

        Private Sub MyMap_Loaded(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
            MyMap.Focus()
        End Sub
    End Class

