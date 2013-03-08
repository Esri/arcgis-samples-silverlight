using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Printing;

namespace ArcGISSilverlightSDK
{
    public partial class ExportWebMap : UserControl
    {
        PrintTask printTask;

        public ExportWebMap()
        {
            InitializeComponent();

            printTask = new PrintTask("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Utilities/PrintingTools/GPServer/Export%20Web%20Map%20Task");
            printTask.DisableClientCaching = true;
            printTask.ExecuteCompleted += printTask_PrintCompleted;
            printTask.GetServiceInfoCompleted += printTask_GetServiceInfoCompleted;
            printTask.GetServiceInfoAsync();
        }

        private void printTask_GetServiceInfoCompleted(object sender, ServiceInfoEventArgs e)
        {
            LayoutTemplates.ItemsSource = e.ServiceInfo.LayoutTemplates;
            Formats.ItemsSource = e.ServiceInfo.Formats;
        }

        private void printTask_PrintCompleted(object sender, PrintEventArgs e)
        {
            System.Windows.Browser.HtmlPage.Window.Navigate(e.PrintResult.Url, "_blank");
        }

        private void ExportMap_Click(object sender, RoutedEventArgs e)
        {
            if (printTask == null || printTask.IsBusy) return;

            PrintParameters printParameters = new PrintParameters(MyMap)
            {
                ExportOptions = new ExportOptions() { Dpi = 96, OutputSize = new Size(MyMap.ActualWidth, MyMap.ActualHeight) },
                LayoutTemplate = (string)LayoutTemplates.SelectedItem ?? string.Empty,
                Format = (string)Formats.SelectedItem,

            };
            printTask.ExecuteAsync(printParameters);
        }
    }
}
