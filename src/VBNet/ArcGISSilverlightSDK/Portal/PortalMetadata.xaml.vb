Imports System.Windows.Controls
Imports System.Text
Imports ESRI.ArcGIS.Client.Portal
Imports System.Collections.Generic
Imports ESRI.ArcGIS.Client.WebMap
Imports ESRI.ArcGIS.Client
Imports System.Windows
Imports System

Partial Public Class PortalMetadata
  Inherits UserControl
  Public Sub New()
    InitializeComponent()
  End Sub

  Private Sub LoadPortalInfo_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    PropertiesListBox.Items.Clear()
    If String.IsNullOrEmpty(PortalUrltextbox.Text) Then
      Return
    End If
    InitializePortal(PortalUrltextbox.Text)
  End Sub

  Public Sub InitializePortal(ByVal PortalUrl As String)
    Dim arcgisPortal As New ArcGISPortal()
    arcgisPortal.InitializeAsync(PortalUrl, Sub(p, ex)
                                              If ex Is Nothing Then
                                                Dim portalInfo As ArcGISPortalInfo = p.ArcGISPortalInfo
                                                If portalInfo Is Nothing Then
                                                  MessageBox.Show("Portal Information could not be retrieved")
                                                  Exit Sub
                                                End If
                                                PropertiesListBox.Items.Add(String.Format("Current Version: {0}", p.CurrentVersion))
                                                PropertiesListBox.Items.Add(String.Format("Access: {0}", portalInfo.Access))
                                                PropertiesListBox.Items.Add(String.Format("Host Name: {0}", portalInfo.PortalHostname))
                                                PropertiesListBox.Items.Add(String.Format("Name: {0}", portalInfo.PortalName))
                                                PropertiesListBox.Items.Add(String.Format("Mode: {0}", portalInfo.PortalMode))
                                                Dim basemap As ESRI.ArcGIS.Client.WebMap.BaseMap = portalInfo.DefaultBaseMap
                                                PropertiesListBox.Items.Add(String.Format("Default BaseMap Title: {0}", basemap.Title))
                                                PropertiesListBox.Items.Add(String.Format("WebMap Layers ({0}):", basemap.Layers.Count))
                                                For Each webmapLayer As WebMapLayer In basemap.Layers
                                                  PropertiesListBox.Items.Add(webmapLayer.Url)
                                                Next webmapLayer
                                                portalInfo.GetFeaturedGroupsAsync(Sub(portalgroup, exp)
                                                                                    If exp Is Nothing Then
                                                                                      PropertiesListBox.Items.Add("Groups:")
                                                                                      Dim listGroups As New ListBox()
                                                                                      listGroups.ItemTemplate = TryCast(LayoutRoot.Resources("PortalGroupTemplate"), DataTemplate)
                                                                                      listGroups.ItemsSource = portalgroup
                                                                                      PropertiesListBox.Items.Add(listGroups)
                                                                                    End If
                                                                                  End Sub)
                                                portalInfo.SearchFeaturedItemsAsync(New SearchParameters() With {.Limit = 15}, Sub(result, err)
                                                                                                                                 If err Is Nothing Then
                                                                                                                                   FeaturedMapsList.ItemsSource = result.Results
                                                                                                                                 End If
                                                                                                                               End Sub)
                                              Else
                                                MessageBox.Show("Failed to initialize" & ex.Message.ToString())
                                              End If
                                            End Sub)

  End Sub
End Class
