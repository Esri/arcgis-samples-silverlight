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
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks


  Partial Public Class QueryImageService
    Inherits UserControl
    Private myDrawObject As Draw
    Private footprintsGraphicsLayer As GraphicsLayer

    Public Sub New()
      InitializeComponent()

      myDrawObject = New Draw(MyMap) With
                     {
                         .DrawMode = DrawMode.Rectangle,
                         .FillSymbol = TryCast(LayoutRoot.Resources("DrawFillSymbol"), ESRI.ArcGIS.Client.Symbols.FillSymbol),
                         .IsEnabled = True
                     }

      AddHandler myDrawObject.DrawComplete, AddressOf MyDrawObject_DrawComplete
      footprintsGraphicsLayer = TryCast(MyMap.Layers("FootprintsGraphicsLayer"), GraphicsLayer)
    End Sub

    Private Sub MyDrawObject_DrawComplete(ByVal sender As Object, ByVal args As ESRI.ArcGIS.Client.DrawEventArgs)
      myDrawObject.IsEnabled = False

    Dim queryTask As New QueryTask("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Portland/Aerial/ImageServer/query")
      AddHandler queryTask.ExecuteCompleted, AddressOf QueryTask_ExecuteCompleted
      AddHandler queryTask.Failed, AddressOf QueryTask_Failed

      Dim query As Query = New ESRI.ArcGIS.Client.Tasks.Query()
      query.OutFields.Add("*")
      query.Geometry = args.Geometry
      query.ReturnGeometry = True
      query.OutSpatialReference = MyMap.SpatialReference
      query.Where = "Category = 1"

      queryTask.ExecuteAsync(query)
    End Sub

    Private Sub QueryTask_ExecuteCompleted(ByVal sender As Object, ByVal args As ESRI.ArcGIS.Client.Tasks.QueryEventArgs)
      Dim featureSet As FeatureSet = args.FeatureSet

      If featureSet Is Nothing OrElse featureSet.Features.Count < 1 Then
            MessageBox.Show("No features returned from query")
        Return
      End If

      If featureSet IsNot Nothing AndAlso featureSet.Features.Count > 0 Then
        For Each graphic As Graphic In featureSet.Features
          graphic.Symbol = TryCast(LayoutRoot.Resources("FootprintFillSymbol"), Symbols.Symbol)
          footprintsGraphicsLayer.Graphics.Add(graphic)
        Next graphic
      End If

      myDrawObject.IsEnabled = True
    End Sub

    Private Sub QueryTask_Failed(ByVal sender As Object, ByVal args As TaskFailedEventArgs)
      MessageBox.Show("Query failed: " & args.Error.Message)
    End Sub

    Private Sub ClearButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      footprintsGraphicsLayer.Graphics.Clear()
    End Sub

  End Class

