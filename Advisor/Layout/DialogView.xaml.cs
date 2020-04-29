using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls;

namespace HDT.Plugins.Advisor.Layout
{
    public partial class DialogView : UserControl
    {
        private readonly Flyout _container;
        private readonly Regex regex = new Regex(@"(?<pre>[^\[]*)\[(?<text>[^\]\(]+)\]\((?<url>[^\)]+)\)\s*(?<post>.*)", RegexOptions.Compiled);

        public DialogView(Flyout container, string title, string message, int autoClose)
        {
            InitializeComponent();

            _container = container;

            TitleText.Text = title;

            var match = regex.Match(message);
            if (match.Success)
            {
                Log.Debug("matched: ");
                MessageText.Inlines.Clear();
                MessageText.Inlines.Add(match.Groups["pre"].Value);
                var hyperLink = new Hyperlink
                {
                    Foreground = Brushes.White,
                    NavigateUri = new Uri(match.Groups["url"].Value)
                };
                hyperLink.Inlines.Add(match.Groups["text"].Value);
                hyperLink.RequestNavigate += HyperLink_RequestNavigate;
                MessageText.Inlines.Add(hyperLink);
                MessageText.Inlines.Add(" " + match.Groups["post"].Value);
            }
            else
            {
                MessageText.Text = message;
            }

            AutoClose(autoClose);
        }

        public void SetUtilityButton(Action action, string icon)
        {
            var unicode = string.Empty;
            switch (icon.ToLower())
            {
                case "download":
                    unicode = "\u21e9";
                    break;
                case "error":
                    unicode = "\u20e0";
                    break;
                default:
                    unicode = "!";
                    break;
            }

            UtilityButton.Content = unicode;
            UtilityButton.IsEnabled = true;
            if (action != null)
            {
                UtilityButton.Click += (s, e) => { action.Invoke(); };
            }
            else
            {
                UtilityButton.IsEnabled = false;
            }

            UtilityButton.UpdateLayout();
        }

        private void HyperLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        private async void AutoClose(int seconds)
        {
            // zero means no auto close
            if (seconds <= 0)
            {
                return;
            }

            await Task.Delay(seconds * 1000);
            _container.IsOpen = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _container.IsOpen = false;
        }
    }
}