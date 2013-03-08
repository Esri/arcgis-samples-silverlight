using System.Windows;
using System.Windows.Controls;

namespace ArcGISSilverlightSDK
{
	public partial class ElementLayer : UserControl
	{
		public ElementLayer()
		{
			InitializeComponent();
		}

		private void RedlandsButton_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("You found Redlands");
		}
	}
}
