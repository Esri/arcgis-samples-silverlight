Imports Microsoft.VisualBasic
Imports System.Windows
Imports System.Windows.Controls


    Partial Public Class AddLayerDynamically
        Inherits UserControl
        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub AddLayerButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            MyMap.Layers.Clear()

            Dim NewTiledLayer As New ESRI.ArcGIS.Client.ArcGISTiledMapServiceLayer()

            AddHandler MyMap.Layers.LayersInitialized, Sub(evtsender, args) MyMap.ZoomTo(NewTiledLayer.InitialExtent)

            NewTiledLayer.Url = UrlTextBox.Text
            MyMap.Layers.Add(NewTiledLayer)
        End Sub

        Private Sub MyMap_ExtentChange(ByVal sender As Object, ByVal args As ESRI.ArcGIS.Client.ExtentEventArgs)
            setExtentText(args.NewExtent)
        End Sub

        Private Sub setExtentText(ByVal newExtent As ESRI.ArcGIS.Client.Geometry.Envelope)
            ExtentTextBlock.Text = String.Format("MinX: {0}" & vbLf & "MinY: {1}" & vbLf & "MaxX: {2}" & vbLf & "MaxY: {3}", newExtent.XMin, newExtent.YMin, newExtent.XMax, newExtent.YMax)
            ExtentGrid.Visibility = Visibility.Visible
            ExtentTextBox.Text = String.Format("{0},{1},{2},{3}", newExtent.XMin.ToString("#0.000"), newExtent.YMin.ToString("#0.000"), newExtent.XMax.ToString("#0.000"), newExtent.YMax.ToString("#0.000"))
        End Sub
    End Class

