using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client;

namespace ArcGISSilverlightSDK
{
    public partial class LayerTransitions : UserControl
    {
        int progress = 0;
        Layer fromLayer;
        Layer toLayer;
        Layer pendingLayer;
        Layer animatingLayer;

        public LayerTransitions()
        {
            InitializeComponent();
            MyMap.Progress += MyMap_Progress;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (MyMap != null)
            {
                Layer from = null;
                foreach (Layer layer in MyMap.Layers)
                    if (layer.Visible && layer.Opacity == 1)
                    {
                        from = layer;
                        break;
                    }

                Layer to = MyMap.Layers[radioButton.Content.ToString()];

                Fade(from, to);
            }
        }

        private void Fade(Layer from, Layer to)
        {
            // If in process of animating
            if (animatingLayer != null)
            {
                pendingLayer = to;
                return;
            }
            pendingLayer = null;

            to.Opacity = 0;
            to.Visible = true;
            Dispatcher.BeginInvoke(() =>
            {
                if (progress >= 97)
                    StartFade(from, to);
                else // Wait for layer to load before fading to it
                {
                    EventHandler<ProgressEventArgs> handler = null;
                    handler = (s, e) =>
                    {
                        if (e.Progress >= 97)
                        {
                            MyMap.Progress -= handler;
                            StartFade(from, to);
                        }
                    };
                    MyMap.Progress += handler;
                }
            });
        }

        private void StartFade(Layer from, Layer to)
        {
            fromLayer = from;
            toLayer = to;

            // If fromLayer is below toLayer, layer to animate is toLayer. 
            // If fromLayer is above toLayer, layer to animate is fromLayer. The toLayer opacity 
            // should be set to completely opaque. 
            if (MyMap.Layers.IndexOf(fromLayer) < MyMap.Layers.IndexOf(toLayer))
                animatingLayer = toLayer;
            else
            {
                animatingLayer = fromLayer;
                toLayer.Opacity = 1;
            }

            // Listen for when a frame is rendered
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            // Change the opacity of the fromLayer and toLayer
            double opacity = -1;
            if (animatingLayer == fromLayer)
            {
                opacity = Math.Max(0, fromLayer.Opacity - .05);
                fromLayer.Opacity = opacity;
            }
            else
            {
                opacity = Math.Min(1, toLayer.Opacity + .05);
                toLayer.Opacity = opacity;
            }

            // When transition complete, set reset properties and unhook handler 
            if (opacity == 1 || opacity == 0)
            {
                fromLayer.Opacity = 0;
                fromLayer.Visible = false;
                animatingLayer = null;
                CompositionTarget.Rendering -= CompositionTarget_Rendering;
                // If layer pending animation, start fading
                if (pendingLayer != null)
                    Fade(toLayer, pendingLayer);
                pendingLayer = null;
            }
        }

        // Track overall progress of loading map content 
        private void MyMap_Progress(object sender, ESRI.ArcGIS.Client.ProgressEventArgs e)
        {
            progress = e.Progress;
        }
    }
}
