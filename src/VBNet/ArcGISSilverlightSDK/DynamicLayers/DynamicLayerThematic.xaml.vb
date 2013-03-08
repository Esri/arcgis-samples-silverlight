Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Tasks
Imports System.Windows.Media
Imports System.Collections.ObjectModel


Partial Public Class DynamicLayerThematic
  Inherits UserControl
  Private generateRendererTask As GenerateRendererTask
  Private colorRamp As ColorRamp

  Private generateClassesParameters As GenerateRendererParameters

  Public Sub New()
    InitializeComponent()

    PopulateCombos()

    InitializeRenderingInfo()
  End Sub

  Private Sub FeatureLayer_Initialized(ByVal sender As Object, ByVal e As EventArgs)
    Dim featureLayer As FeatureLayer = TryCast(sender, FeatureLayer)
    Dim intandDoublefields As IEnumerable(Of Field) = From fld In featureLayer.LayerInfo.Fields Where fld.Type = Field.FieldType.Integer OrElse fld.Type = Field.FieldType.Double Select fld
    If Not intandDoublefields Is Nothing AndAlso intandDoublefields.Count() > 0 Then
      ClassificationFieldCombo.ItemsSource = intandDoublefields
      ClassificationFieldCombo.SelectedIndex = 1
      NormalizationFieldCombo.ItemsSource = intandDoublefields
      NormalizationFieldCombo.SelectedIndex = -1
      RenderButton.IsEnabled = True
    End If
  End Sub

  Private Sub PopulateCombos()
    AlgorithmCombo.ItemsSource = New Object() {Algorithm.CIELabAlgorithm, Algorithm.HSVAlgorithm, Algorithm.LabLChAlgorithm}
    AlgorithmCombo.SelectedIndex = 1

    ClassificationMethodCombo.ItemsSource = New Object() {ClassificationMethod.EqualInterval, ClassificationMethod.NaturalBreaks, ClassificationMethod.Quantile, ClassificationMethod.StandardDeviation}
    ClassificationMethodCombo.SelectedIndex = 0

    NormalizationTypeCombo.ItemsSource = New Object() {NormalizationType.Field, NormalizationType.Log, NormalizationType.PercentOfTotal, NormalizationType.None}
    NormalizationTypeCombo.SelectedIndex = 3

    IntervalCombo.ItemsSource = New Object() {StandardDeviationInterval.One, StandardDeviationInterval.OneHalf, StandardDeviationInterval.OneQuarter, StandardDeviationInterval.OneThird}
    IntervalCombo.SelectedIndex = 0
  End Sub

  Private Sub InitializeRenderingInfo()

    Dim statesFeatureLayer As FeatureLayer = TryCast(MyMap.Layers("StatesFeatureLayer"), FeatureLayer)

    generateClassesParameters = New GenerateRendererParameters()

    generateRendererTask = New GenerateRendererTask(statesFeatureLayer.Url)
    generateClassesParameters.Source = New LayerMapSource() With {.MapLayerID = 3}

    AddHandler generateRendererTask.Failed, Sub(s, e)
                                              MessageBox.Show(String.Format("GenerateRendererTask Failed: {0}", e.Error.Message.ToString))
                                            End Sub
    AddHandler generateRendererTask.ExecuteCompleted, Sub(s, e)
                                                        statesFeatureLayer.Renderer = e.GenerateRendererResult.Renderer
                                                      End Sub
  End Sub

  Private Sub RenderButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    Dim colorRamps As ObservableCollection(Of ColorRamp) = New ObservableCollection(Of ColorRamp)()
    colorRamp = New ColorRamp() With
                {
                  .From = (TryCast(TryCast(StartColorCombo.SelectedItem, ComboBoxItem).Background, SolidColorBrush)).Color,
                  .To = (TryCast(TryCast(EndColorCombo.SelectedItem, ComboBoxItem).Background, SolidColorBrush)).Color,
                  .Algorithm = CType(AlgorithmCombo.SelectedItem, Algorithm)
                }
    colorRamps.Add(colorRamp)


    generateClassesParameters.ClassificationDefinition = New ClassBreaksDefinition() With
                                                         {
    .BaseSymbol = New ESRI.ArcGIS.Client.Symbols.SimpleFillSymbol() With
                 {.Fill = (TryCast(ColorRampCombo.SelectedItem, ComboBoxItem)).Background},
    .BreakCount = Integer.Parse(BreakCountTb.Text.Trim()),
    .ColorRamps = colorRamps,
    .ClassificationMethod = CType(ClassificationMethodCombo.SelectedItem, ClassificationMethod),
    .ClassificationField = (TryCast((ClassificationFieldCombo.SelectedItem), Field)).Name
    }

    Dim classBreakDef As ClassBreaksDefinition =
      TryCast(generateClassesParameters.ClassificationDefinition, ClassBreaksDefinition)

    If classBreakDef.ClassificationMethod = ClassificationMethod.StandardDeviation Then
      classBreakDef.StandardDeviationInterval = CType(IntervalCombo.SelectedItem, StandardDeviationInterval)
    End If

    If Not NormalizationTypeCombo.SelectedItem Is Nothing Then
      classBreakDef.NormalizationType = CType(NormalizationTypeCombo.SelectedItem, NormalizationType)

      If classBreakDef.NormalizationType = NormalizationType.Field Then
        If Not NormalizationFieldCombo.SelectedItem Is Nothing Then
          classBreakDef.NormalizationField = (TryCast((NormalizationFieldCombo.SelectedItem), Field)).Name
        Else
          MessageBox.Show("Normalization Field must be selected")
          Return
        End If
      End If
    End If
    If generateRendererTask.IsBusy Then
      generateRendererTask.CancelAsync()
    End If
    generateRendererTask.ExecuteAsync(generateClassesParameters)
  End Sub


  Private Sub ClassificationMethodCombo_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)

    Dim method As ClassificationMethod = CType(TryCast(sender, ComboBox).SelectedItem, ClassificationMethod)
    If (method = ClassificationMethod.StandardDeviation) Then
      IntervalCombo.IsEnabled = True
    Else
      IntervalCombo.IsEnabled = False
    End If
    If (method = ClassificationMethod.StandardDeviation) Then
      BreakCountTb.IsEnabled = False
    Else
      BreakCountTb.IsEnabled = True
    End If
  End Sub

  Private Sub NormalizationTypeCombo_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)

    Dim normType As NormalizationType = CType(TryCast(sender, ComboBox).SelectedItem, NormalizationType)
    If (normType = NormalizationType.Field) Then
      NormalizationFieldCombo.IsEnabled = True
    Else
      NormalizationFieldCombo.IsEnabled = False
    End If
  End Sub

End Class
