using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Advisor.Properties;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Core = Hearthstone_Deck_Tracker.API.Core;

namespace HDT.Plugins.Advisor.Layout
{
    public partial class AdvisorOverlay
    {
        public AdvisorOverlay()
        {
            InitializeComponent();
        }

        public void Update(List<Card> cards, bool reset)
        {
            // hide if card list is empty
            //this.Visibility = cards.Count <= 0 ? Visibility.Hidden : Visibility.Visible;
            //this.acList.ItemsSource = cards;
            this.acList.Update(cards, reset);
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            //Canvas.SetTop(this, Core.OverlayWindow.Height * 1 / 100);
            //Canvas.SetLeft(this, Core.OverlayWindow.Width * 12 / 100);
            Canvas.SetLeft(this, Settings.Default.OverlayPositionX);
            Canvas.SetTop(this, Settings.Default.OverlayPositionY);
        }

        public void Show()
        {
            this.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = Visibility.Hidden;
        }

        public bool ShowToolTip => Config.Instance.WindowCardToolTips;
    }
}