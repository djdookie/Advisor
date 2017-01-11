using System.Windows;
using System.Windows.Controls;

namespace HDT.Plugins.Advisor.Layout
{
	public partial class AdvisorMenu : MenuItem
	{
		public AdvisorMenu()
		{
			InitializeComponent();
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
		}

		private void Menu_Settings_Click(object sender, RoutedEventArgs e)
		{
			Advisor.ShowSettings();
		}

		private async void Menu_ImportDecks_Click(object sender, RoutedEventArgs e)
		{
			await Advisor.ImportMetaDecks();
		}
	}
}