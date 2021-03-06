﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Advisor.Properties;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Core = Hearthstone_Deck_Tracker.API.Core;
using System.Windows.Media;

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

        /// <summary>
        /// Update overlay position, scaling and opacity
        /// </summary>
        public void UpdatePosition()
        {
            // Set overlay position
            //Canvas.SetTop(this, Core.OverlayWindow.Height * 1 / 100);
            //Canvas.SetLeft(this, Core.OverlayWindow.Width * 12 / 100);
            Canvas.SetLeft(this, Settings.Default.OverlayPositionX);
            Canvas.SetTop(this, Settings.Default.OverlayPositionY);

            // Set overlay scale
            StackPanelOverlay.RenderTransform = new ScaleTransform(Settings.Default.OverlayScaling / 100.0, Settings.Default.OverlayScaling / 100.0);

            // Set overlay opacity
            StackPanelOverlay.Opacity = Settings.Default.OverlayOpacity / 100.0;
        }

        public void Show()
        {
            this.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = Visibility.Hidden;
        }

        //public bool ShowStatistics => Settings.Default.ShowStatistics;
    }
}