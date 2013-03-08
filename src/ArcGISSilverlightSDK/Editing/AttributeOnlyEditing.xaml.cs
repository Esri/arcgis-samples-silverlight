using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using System.Collections.Generic;

namespace ArcGISSilverlightSDK
{
    public partial class AttributeOnlyEditing : UserControl
    {
        public AttributeOnlyEditing()
        {
            InitializeComponent();
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
                            // edit geometry
                            if (editor.EditVertices.CanExecute(null))
                                editor.EditVertices.Execute(null);
                        }

                        // edit attribute
                        MyFeatureDataGrid.SelectedItem = edit.Graphic;
                        break;
                    }
                }
            }
            else if (e.Action == Editor.EditAction.EditVertices)
            {
                // should never happen since feature service AllowGeometryUpdates=False
                if (editor.Select.CanExecute("new"))
                    editor.Select.Execute("new");
            }
        }

        private void FeatureLayer_Initialized(object sender, EventArgs e)
        {
            var editor = LayoutRoot.Resources["MyEditor"] as Editor;
            if (editor.Select.CanExecute("new"))
                editor.Select.Execute("new");
        }

        private void FeatureLayer_EndSaveEdits(object sender, ESRI.ArcGIS.Client.Tasks.EndEditEventArgs e)
        {
            if (e.Success)            
                (MyMap.Layers["PoolPermitDynamicLayer"] as ArcGISDynamicMapServiceLayer).Refresh();            
        }
    }
}
