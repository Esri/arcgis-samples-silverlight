using System;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;

namespace ArcGISSilverlightSDK
{
    public partial class EditToolsSelectionOnly : UserControl
    {
        bool _featureDataFormOpen = false;

        public EditToolsSelectionOnly()
        {
            InitializeComponent();
        }

        private void FeatureLayer_Initialized(object sender, EventArgs e)
        {
            var editor = LayoutRoot.Resources["MyEditor"] as Editor;
            if (editor.Select.CanExecute("new"))
                editor.Select.Execute("new");
        }

        private void Editor_EditCompleted(object sender, Editor.EditEventArgs e)
        {
            var editor = sender as Editor;
            if (e.Action == Editor.EditAction.Select)
            {
                foreach (var edit in e.Edits)
                {
                    if (edit.Graphic != null && edit.Graphic.Selected)
                    {
                        var layer = edit.Layer as FeatureLayer;
                        if (layer != null && layer.IsGeometryUpdateAllowed(edit.Graphic))
                        {
                            if (editor.EditVertices.CanExecute(edit.Graphic))
                                editor.EditVertices.Execute(edit.Graphic);

                            FeatureDataFormBorder.Visibility = System.Windows.Visibility.Visible;
                            _featureDataFormOpen = true;

                            LayerDefinition layerDefinition = new LayerDefinition()
                            {
                                LayerID = 2,
                                Definition = string.Format("{0} <> {1}", layer.LayerInfo.ObjectIdField,
                                edit.Graphic.Attributes[layer.LayerInfo.ObjectIdField].ToString())
                            };

                            (MyMap.Layers["WildFireDynamic"] as ArcGISDynamicMapServiceLayer).LayerDefinitions =
                               new System.Collections.ObjectModel.ObservableCollection<LayerDefinition>() { layerDefinition };

                            (MyMap.Layers["WildFireDynamic"] as
                                    ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer).Refresh();
                        }

                        MyFeatureDataForm.GraphicSource = edit.Graphic;
                        break;
                    }
                }
            }
            else if (e.Action == Editor.EditAction.ClearSelection)
            {
                FeatureDataFormBorder.Visibility = System.Windows.Visibility.Collapsed;
                MyFeatureDataForm.GraphicSource = null;
                (MyMap.Layers["WildFirePolygons"] as FeatureLayer).ClearSelection();
                (MyMap.Layers["WildFireDynamic"] as ArcGISDynamicMapServiceLayer).LayerDefinitions = null;
                (MyMap.Layers["WildFireDynamic"] as ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer).Refresh();                
            }
        }

        void ResetEditableSelection()
        {
            if (!_featureDataFormOpen)
            {
                MyFeatureDataForm.GraphicSource = null;
                (MyMap.Layers["WildFireDynamic"] as ArcGISDynamicMapServiceLayer).LayerDefinitions = null;
                (MyMap.Layers["WildFirePolygons"] as FeatureLayer).ClearSelection();
            }

            (MyMap.Layers["WildFireDynamic"] as
                ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer).Refresh();
        }

        private void FeatureLayer_EndSaveEdits(object sender, ESRI.ArcGIS.Client.Tasks.EndEditEventArgs e)
        {
            ResetEditableSelection();
        }
        
        private void FeatureLayer_SaveEditsFailed(object sender, ESRI.ArcGIS.Client.Tasks.TaskFailedEventArgs e)
        {
            ResetEditableSelection();
        }

        private void MyFeatureDataForm_EditEnded(object sender, EventArgs e)
        {
            FeatureDataFormBorder.Visibility = System.Windows.Visibility.Collapsed;
            _featureDataFormOpen = false;
        }
    }
}
