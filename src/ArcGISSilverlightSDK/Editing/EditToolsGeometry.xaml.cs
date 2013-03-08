using System;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISSilverlightSDK
{
    public partial class EditToolsGeometry : UserControl
    {
        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
                new ESRI.ArcGIS.Client.Projection.WebMercator();

        EditGeometry editGeometry;
        int actionCount = 0;

        Graphic selectedPointGraphic;

        public EditToolsGeometry()
        {
            InitializeComponent();

            editGeometry = this.LayoutRoot.Resources["MyEditGeometry"] as EditGeometry;
        }

        private void GraphicsLayer_MouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs e)
        {
            e.Handled = true;            

            if (e.Graphic.Geometry is MapPoint)
            {
                e.Graphic.Selected = true;
                selectedPointGraphic = e.Graphic;
            }
            else
            {
                editGeometry.StartEdit(e.Graphic);
            }
        }

        private void StopEdit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            editGeometry.StopEdit();
        }

        private void CancelEdit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            editGeometry.CancelEdit();
        }

        private void UndoLastEdit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            editGeometry.UndoLastEdit();
        }

        private void RedoLastEdit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            editGeometry.RedoLastEdit();
        }

        private void EditGeometry_GeometryEdit(object sender, EditGeometry.GeometryEditEventArgs e)
        {
            if (ActionTextBox.Text != string.Empty)
            {
                ActionTextBox.Select(0, 0);
                ActionTextBox.SelectedText = string.Format("{0}:{1}{2}", actionCount, e.Action, Environment.NewLine);
            }
            else
                ActionTextBox.Text = string.Format("{0}:{1}", actionCount, e.Action);
            actionCount++;
        }

        private void GraphicsLayer_Initialized(object sender, EventArgs e)
        {
            GraphicsLayer graphicsLayer = sender as GraphicsLayer;
            foreach (Graphic g in graphicsLayer.Graphics)
            {
                g.Geometry = _mercator.FromGeographic(g.Geometry);
            }
        }

        private void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {
            editGeometry.StopEdit();
        }

        private void MyMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedPointGraphic != null)
                selectedPointGraphic.Geometry = MyMap.ScreenToMap(e.GetPosition(MyMap));          
        }

        private void MyMap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (selectedPointGraphic != null)
            {
                selectedPointGraphic.Selected = false;
                selectedPointGraphic = null;
            }
        }
    }
}
