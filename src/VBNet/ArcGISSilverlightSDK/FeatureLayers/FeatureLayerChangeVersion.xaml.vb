Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Tasks
Imports ESRI.ArcGIS.Client

Partial Public Class FeatureLayerChangeVersion
  Inherits UserControl
  Private Fl As FeatureLayer
  Public Sub New()
    InitializeComponent()
    Dim gp_ListVersions As New Geoprocessor("http://sampleserver6.arcgisonline.com/arcgis/rest/services/GDBVersions/GPServer/ListVersions")

    AddHandler gp_ListVersions.Failed, Sub(s, a) MessageBox.Show("Geoprocessing service failed: " & a.Error.ToString())

    AddHandler gp_ListVersions.ExecuteCompleted, Sub(c, d)
                                                   VersionsCombo.DataContext = (TryCast(d.Results.OutParameters(0), GPRecordSet)).FeatureSet
                                                   VersionsCombo.SelectedIndex = 0
                                                 End Sub

    Dim gpparams As New List(Of GPParameter)()
    gpparams.Add(New GPRecordSet("Versions", New FeatureSet()))
    gp_ListVersions.ExecuteAsync(gpparams)
  End Sub

  Private Sub VersionsCombo_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
    Fl = (TryCast(MyMap.Layers("ServiceConnections"), FeatureLayer))
    Fl.GdbVersion = (TryCast(e.AddedItems(0), Graphic)).Attributes("name").ToString()
    Fl.Update()
  End Sub
End Class
