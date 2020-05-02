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

        private async void Menu_ImportMetastatsDecks_Click(object sender, RoutedEventArgs e)
        {
            await Advisor.ImportMetastatsDecks();
        }

        private void Menu_DeleteArchetypeDecks_Click(object sender, RoutedEventArgs e)
        {
            Advisor.DeleteArchetypeDecks();
        }

        private void Menu_Donate_Click(object sender, RoutedEventArgs e)
        {
            Advisor.Donate();
        }

        private void Menu_Website_Click(object sender, RoutedEventArgs e)
        {
            Advisor.OpenWebsite();
        }
    }
}