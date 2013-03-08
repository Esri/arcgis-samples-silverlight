using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.WebMap;

namespace ArcGISSilverlightSDK
{
    public partial class GenerateFeatures : UserControl
    {
        private OpenFileDialog openFileDialog;
        public GenerateFeatures()
        {
            InitializeComponent();
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "zip Files | *.zip";
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Call the ShowDialog method to show the dialog box.
            bool? userClickedOK = openFileDialog.ShowDialog();

            if (userClickedOK == true)
            {
                // Open the selected file to read.
                System.IO.FileInfo file = openFileDialog.File;

                // Uri to the ArcGIS Portal API generate operation.
                // Reference documentation available here: http://www.arcgis.com/apidocs/rest/generate.html
                Uri address = new Uri("http://www.arcgis.com/sharing/rest/content/features/generate");

                // Get the file contents for the local file
                FileStream fs = file.OpenRead();

                // Create ArcGISWebClient.StreamContent instance   
                ArcGISWebClient.StreamContent streamContent = new ArcGISWebClient.StreamContent()
                {
                    Name = "file",
                    Filename = file.Name,
                    Stream = fs,
                    ContentType = "application/zip"
                };

                // Create a list of stream content to POST
                IList<ArcGISWebClient.StreamContent> filestream = new List<ArcGISWebClient.StreamContent>();
                filestream.Add(streamContent);

                // Create dictionary to store parameter to POST 
                Dictionary<string, string> postParameters = new Dictionary<string, string>();
               
                // A class created to store publish parameters for the generate operation
                GenerateFeaturesParams param = new GenerateFeaturesParams()
                {
                    name = file.Name.Substring(0, file.Name.LastIndexOf(".")),
                    maxRecordCount = 1000,
                    generalize = false,
                    reducePrecision = true,
                    targetSR = MyMap.SpatialReference
                };
              
                // Must specify the output type (json) the file type (shapefile) and the publish parameters
                postParameters.Add("f", "json");
                postParameters.Add("filetype", "shapefile");
                postParameters.Add("publishParameters", SerializeToJsonString(param));

                // Url to the generate operation, part of the ArcGIS Portal REST API (http://www.arcgis.com/apidocs/rest/generate.html)
                string postURL = "http://www.arcgis.com/sharing/rest/content/features/generate";
 
                // Use ArcGISWebClient POST shapefile to the ArcGIS Portal generate operation.  The generate operation requires a file to be passed
                // in a multi-part post request.
                ArcGISWebClient agsWebClient = new ArcGISWebClient();
                agsWebClient.PostMultipartCompleted += (a, b) =>
                {
                    if (b.Error == null)
                    {
                        try
                        {       
                            // Use the the generic JsonValue to handle dynamic json content.
                            // In this case, generate always returns a "featureCollection" object which contains
                            // a "layers" array with one feature layer.  
                            JsonValue featureCollection = JsonValue.Load(b.Result);
                            string layer = featureCollection["featureCollection"]["layers"][0].ToString();
                            
                            FeatureLayer featureLayer = FeatureLayer.FromJson(layer);

                            if (featureLayer != null)
                            {
                                // Add the feature layer to the map and zoom to it
                                MyMap.Layers.Add(featureLayer);
                                MyMap.ZoomTo(featureLayer.FullExtent.Expand(1.25));
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "FeatureLayer creation failed", MessageBoxButton.OK);
                        }
                    }
                };
                agsWebClient.PostMultipartAsync(new Uri(postURL), postParameters, filestream, null);
            }
        }

        // See MSDN for more info: http://msdn.microsoft.com/en-us/library/bb412179(VS.100).aspx
        public static string SerializeToJsonString(object objectToSerialize)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                DataContractJsonSerializer serializer =
                        new DataContractJsonSerializer(objectToSerialize.GetType());
                serializer.WriteObject(ms, objectToSerialize);
                ms.Position = 0;
                using (StreamReader reader = new StreamReader(ms))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private void Button_ClearMap(object sender, RoutedEventArgs e)
        {
            List<FeatureLayer> featureLayers = new List<FeatureLayer>();

            foreach (Layer layer in MyMap.Layers)
                if (layer is FeatureLayer)
                    featureLayers.Add(layer as FeatureLayer);

            for (int i = 0; i < featureLayers.Count; i++)
                MyMap.Layers.Remove(featureLayers[i]);
        }        
    }

    // Parameter class for Generate operation
    public partial class GenerateFeaturesParams
    {
        public string name { get; set; }
        public SpatialReference targetSR { get; set; }
        public int maxRecordCount { get; set; }
        public bool generalize { get; set; }
        public bool reducePrecision { get; set; }
    }
}
