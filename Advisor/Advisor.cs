using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Advisor.Properties;
using Hearthstone_Deck_Tracker;
using CoreAPI = Hearthstone_Deck_Tracker.API.Core;
using HDT.Plugins.Advisor.Services;
using HDT.Plugins.Advisor.Layout;
using HDT.Plugins.Advisor.Services.TempoStorm;
using Hearthstone_Deck_Tracker.Utility.Logging;
//using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;
using Deck = Hearthstone_Deck_Tracker.Hearthstone.Deck;

//using HDT.Plugins.Advisor.Models;

namespace HDT.Plugins.Advisor
{

    internal class Advisor
    {
        //private int mana = 0;
        private AdvisorOverlay _advisorOverlay = null;
        // Highest deck similarity
        //double maxSim = 0;
        TrackerRepository trackerRepository;
        private static Flyout _settingsFlyout;
        private static Flyout _notificationFlyout;

        public Advisor(AdvisorOverlay overlay)
        {
            _notificationFlyout = CreateDialogFlyout();
            _settingsFlyout = CreateSettingsFlyout();
            Settings.Default.PropertyChanged += new PropertyChangedEventHandler(Settings_PropertyChanged);

            _advisorOverlay = overlay;
            trackerRepository = new TrackerRepository();

            _advisorOverlay.LblArchetype.Text = "No matching archetype yet";
            //_advisorOverlay.Update(new List<Card>());
            UpdateCardList();

            // Hide in menu, if necessary
            if (Config.Instance.HideInMenu && CoreAPI.Game.IsInMenu)
            {
                _advisorOverlay.Hide();
            } else
            {
                _advisorOverlay.Show();
            }
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _advisorOverlay.UpdatePosition();
            Settings.Default.Save();
        }

        //internal List<Entity> Entities => Helper.DeepClone<Dictionary<int, Entity>>(CoreAPI.Game.Entities).Values.ToList<Entity>();

        //internal Entity Opponent => Entities?.FirstOrDefault(x => x.IsOpponent);

        // Reset on when a new game starts
        internal void GameStart()
        {
            //mana = 0;
            //maxSim = 0;
            _advisorOverlay.LblArchetype.Text = "No matching archetype yet";
            //_advisorOverlay.Update(new List<Card>());
            UpdateCardList();
            _advisorOverlay.Show();
        }

        // Need to handle hiding the element when in the game menu
        internal void InMenu()
        {
            if (Config.Instance.HideInMenu)
            {
                _advisorOverlay.Hide();
            }
        }

        internal void OpponentHandDiscard(Card card)
        {
            UpdateCardList();
        }

        internal void OpponentJoustReveal(Card card)
        {
            UpdateCardList();
        }

        internal void OpponentDeckToPlay(Card card)
        {
            UpdateCardList();
        }

        internal void OpponentDeckDiscard(Card card)
        {
            UpdateCardList();
        }

        internal void OpponentPlay(Card card)
        {
            UpdateCardList();
        }

        internal void OpponentSecretTiggered(Card card)
        {
            UpdateCardList();
        }

        internal async void UpdateCardList()
        {
            // Small delay to guarantee opponents cards list is up to date
            await Task.Delay(100);

            // Get opponent's cards list (all yet revealed cards)
            //var opponentCardlist = Core.Game.Opponent.RevealedCards;
            List<Card> opponentCardlist = Core.Game.Opponent.OpponentCardList.Where(x => !x.IsCreated).ToList();

            // If no opponent's cards were revealed yet, return empty card list
            if (!opponentCardlist.Any())
            {
                _advisorOverlay.Update(new List<Card>(), true);
            }
            else
            {
                //Log.Info("+++++ Advisor: " + opponentCardlist.Count);

                //Update list of the opponent's played cards
                //_advisorOverlay.Update(opponentCardlist.ToList());
                var opponentDeck = new Models.Deck(opponentCardlist);

                // Create archetype dictionary
                IDictionary<Models.ArchetypeDeck, float> dict = new Dictionary<Models.ArchetypeDeck, float>();

                // Calculate matching similarities to yet known opponent cards
                foreach (var archetypeDeck in trackerRepository.GetAllArchetypeDecks().Where(d => d.Klass == Models.KlassKonverter.FromString(CoreAPI.Game.Opponent.Class)))
                {
                    dict.Add(archetypeDeck, opponentDeck.Similarity(archetypeDeck));
                }

                // Sort dictionary by value
                var sortedDict = from entry in dict orderby entry.Value descending select entry;

                // If any archetype deck matches more than 0% show the deck with the highest similarity
                if (sortedDict.FirstOrDefault().Value > 0)
                {
                    _advisorOverlay.LblArchetype.Text = String.Format("{0} ({1}%)", sortedDict.FirstOrDefault().Key.Name, Math.Round(sortedDict.FirstOrDefault().Value * 100, 2));
                    Deck deck = DeckList.Instance.Decks.Where(d => d.TagList.ToLowerInvariant().Contains("archetype")).FirstOrDefault(d => d.Name == sortedDict.FirstOrDefault().Key.Name);
                    if (deck != null)
                    {
                        var predictedCards = ((Deck)deck.Clone()).Cards.ToList();
                        //_advisorOverlay.Update(opponentCards);

                        //Remove already played cards from predicted archetype deck
                        foreach (var card in opponentCardlist)
                        {
                            if (predictedCards.Contains(card))
                            {
                                var item = predictedCards.Find(x => x.Id == card.Id);
                                if (item.Count >= card.Count)
                                {
                                    item.Count -= card.Count;
                                }
                            }
                        }
                        _advisorOverlay.Update(predictedCards, false);
                    }
                }
            }
        }

        public static void CloseOpenNoteWindows()
        {
            //foreach (var x in Application.Current.Windows.OfType<NoteView>())
            //    x.Close();
            //foreach (var x in Application.Current.Windows.OfType<BasicNoteView>())
            //    x.Close();
        }

        public static void ShowSettings()
        {
            if (_settingsFlyout == null)
                _settingsFlyout = CreateSettingsFlyout();
            _settingsFlyout.IsOpen = true;
        }

        public static void CloseSettings()
        {
            if (_settingsFlyout != null)
                _settingsFlyout.IsOpen = false;
        }

        public static void CloseNotification()
        {
            if (_notificationFlyout != null)
                _notificationFlyout.IsOpen = false;
        }

        public static void Notify(string title, string message, int autoClose, string icon = null, Action action = null)
        {
            if (_notificationFlyout == null)
                _notificationFlyout = CreateDialogFlyout();
            var view = new DialogView(_notificationFlyout, title, message, autoClose);
            if (!string.IsNullOrEmpty(icon))
            {
                view.SetUtilityButton(action, icon);
            }
            _notificationFlyout.Content = view;
            _notificationFlyout.IsOpen = true;
        }

        public static async Task ImportMetaDecks()
        {
            try
            {
                IArchetypeImporter importer =
                    new SnapshotImporter(new HttpClient(), new TrackerRepository());
                var count = await importer.ImportDecks(
                    Settings.Default.AutoArchiveArchetypes,
                    Settings.Default.DeletePreviouslyImported,
                    Settings.Default.RemoveClassFromName);
                Notify("Import Complete", $"{count} decks imported", 10);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Notify("Import Failed", e.Message, 15, "error", null);
            }
        }

        private static Flyout CreateSettingsFlyout()
        {
            var settings = new Flyout();
            settings.Name = "AdvisorSettingsFlyout";
            settings.Position = Position.Left;
            Panel.SetZIndex(settings, 100);
            settings.Header = "Advisor Settings";
            settings.Content = new SettingsView();
            Core.MainWindow.Flyouts.Items.Add(settings);
            return settings;
        }

        private static Flyout CreateDialogFlyout()
        {
            var dialog = new Flyout();
            dialog.Name = "AdvisorDialogFlyout";
            dialog.Theme = FlyoutTheme.Accent;
            dialog.Position = Position.Bottom;
            dialog.TitleVisibility = Visibility.Collapsed;
            dialog.CloseButtonVisibility = Visibility.Collapsed;
            dialog.IsPinned = false;
            dialog.Height = 50;
            Panel.SetZIndex(dialog, 1000);
            Core.MainWindow.Flyouts.Items.Add(dialog);
            return dialog;
        }
    }
}
