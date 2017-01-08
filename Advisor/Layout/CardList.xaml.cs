using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace HDT.Plugins.Advisor.Layout
{
    public partial class CardList
    {
        public CardList()
        {
            InitializeComponent();
        }

        public void Update(List<Card> cards)
        {
            // hide if card list is empty
            //this.Visibility = cards.Count <= 0 ? Visibility.Hidden : Visibility.Visible;
            this.icCardlist.ItemsSource = cards;
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            Canvas.SetTop(this, Core.OverlayWindow.Height * 2 / 100);
            Canvas.SetLeft(this, Core.OverlayWindow.Width * 12 / 100);
        }

        public void Show()
        {
            this.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = Visibility.Hidden;
        }
    }
}