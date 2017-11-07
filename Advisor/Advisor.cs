﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Advisor.Properties;
using Hearthstone_Deck_Tracker;
using CoreAPI = Hearthstone_Deck_Tracker.API.Core;
using HDT.Plugins.Advisor.Services;
using HDT.Plugins.Advisor.Layout;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.Extensions;
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
        //TrackerRepository trackerRepository;
        private static Flyout _settingsFlyout;
        private static Flyout _notificationFlyout;
        //IEnumerable<ArchetypeDeck> archetypeDecks;
        //private IList<Deck> _archetypeDecks;
        private Guid currentArchetypeDeckGuid;

        public Advisor(AdvisorOverlay overlay)
        {
            _notificationFlyout = CreateDialogFlyout();
            _settingsFlyout = CreateSettingsFlyout();
            Settings.Default.PropertyChanged += new PropertyChangedEventHandler(Settings_PropertyChanged);

            _advisorOverlay = overlay;
            //trackerRepository = new TrackerRepository();
            //LoadArchetypeDecks();

            _advisorOverlay.LblArchetype.Text = "";
            _advisorOverlay.LblStats.Text = "";
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

        /// <summary>
        /// Determine if in valid game mode as specified in config
        /// </summary>
        public bool IsValidGameMode
        {
            get
            {
                if (Core.Game.IsRunning && Core.Game.CurrentGameStats != null)
                {
                    switch (Core.Game.CurrentGameMode)
                    {
                        case GameMode.Ranked:
                            return Settings.Default.ActivateInRanked;
                        case GameMode.Casual:
                            return Settings.Default.ActivateInCasual;
                        case GameMode.Friendly:
                            return Settings.Default.ActivateInFriendly;
                        case GameMode.Spectator:
                            return Settings.Default.ActivateInSpectator;
                        case GameMode.Arena:
                            return Settings.Default.ActivateInArena;
                        case GameMode.Brawl:
                            return Settings.Default.ActivateInBrawl;
                        case GameMode.Practice:
                            return Settings.Default.ActivateInPractice;
                        default:
                            return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Determine if in valid game format as specified in config
        /// </summary>
        public bool IsValidGameFormat
        {
            get
            {
                if (Core.Game.IsRunning && Core.Game.CurrentGameStats != null)
                {
                    switch (Core.Game.CurrentFormat)
                    {
                        case Format.Standard:
                            return Settings.Default.ActivateInStandard;
                        case Format.Wild:
                            return Settings.Default.ActivateInWild;
                        default:
                            return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        ///// <summary>
        ///// Load all archetype decks from tracker repository
        ///// </summary>
        //private void GetArchetypeDecks()
        //{
        //    //archetypeDecks = trackerRepository.GetAllArchetypeDecks().Where(d => d.Klass == Models.KlassKonverter.FromString(CoreAPI.Game.Opponent.Class));
        //    _archetypeDecks = DeckList.Instance.Decks.Where(d => d.TagList.ToLowerInvariant().Contains("archetype")).ToList();
        //    // TODO: Select newest version of any all decks like before?
        //}

        /// <summary>
        /// All archetype decks from tracker repository
        /// </summary>
        private IList<Deck> ArchetypeDecks
        {
            get { return DeckList.Instance.Decks.Where(d => d.TagList.ToLowerInvariant().Contains("archetype")).ToList(); }
        }

        /// <summary>
        /// If settings were changed, update overlay and save settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Settings.Default.Save();
            //_advisorOverlay.UpdatePosition();
            GameStart();
        }

        //internal List<Entity> Entities => Helper.DeepClone<Dictionary<int, Entity>>(CoreAPI.Game.Entities).Values.ToList<Entity>();

        //internal Entity Opponent => Entities?.FirstOrDefault(x => x.IsOpponent);

        // Reset on when a new game starts
        internal async void GameStart()
        {
            // Only continue if in valid game mode and game format
            if (IsValidGameMode && IsValidGameFormat)
            {
                _advisorOverlay.UpdatePosition();
                _advisorOverlay.LblArchetype.Text = "";
                _advisorOverlay.LblStats.Text = "";

                await Task.Delay(5000);

                _advisorOverlay.Update(new List<Card>(), true);
                currentArchetypeDeckGuid = Guid.Empty;

                UpdateCardList();
                _advisorOverlay.Show();
            }
            else
            {
                _advisorOverlay.Hide();
            }
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

        /// <summary>
        /// Determine similarity between all archetype decks and all cards played by the opponent yet.
        /// Then update the cardlist displayed in the overlay with the highest matching archetype deck after removing all opponent cards.
        /// </summary>
        internal async void UpdateCardList()
        {
            // Only continue if in valid game mode or game format
            if (!IsValidGameMode || !IsValidGameFormat) return;

            // Small delay to guarantee opponents cards list is up to date (should be 300+ ms in debug mode or with attached debugger, otherwise restarting HDT could lead to an incomplete opponent decklist!)
#if DEBUG
            await Task.Delay(1000);
#else
            await Task.Delay(100);
#endif

            // Get opponent's cards list (all yet revealed cards)
            //var opponentCardlist = Core.Game.Opponent.RevealedCards;
            IList<Card> opponentCardlist = Core.Game.Opponent.OpponentCardList.Where(x => !x.IsCreated).ToList();

            // If opponent's class is unknown yet or we have no imported archetype decks in the database, return empty card list
            //if (!opponentCardlist.Any() || !ArchetypeDecks.Any())
            if (CoreAPI.Game.Opponent.Class == "" || !ArchetypeDecks.Any())
            {
                currentArchetypeDeckGuid = Guid.Empty;
                _advisorOverlay.Update(new List<Card>(), true);
            }
            else
            {
                // Create archetype dictionary
                IDictionary<Deck, float> dict = new Dictionary<Deck, float>();

                // Calculate similarities between all opponent's class archetype decks and all yet known opponent cards. Exclude wild decks in standard format using NAND expression. OR expression should also work: (!d.IsWildDeck || CoreAPI.Game.CurrentFormat == Format.Wild)
                foreach (var archetypeDeck in ArchetypeDecks.Where(d => d.Class == CoreAPI.Game.Opponent.Class && !(d.IsWildDeck && CoreAPI.Game.CurrentFormat == Format.Standard)))
                {
                    // Insert deck with calculated value into dictionary and prevent exception by inserting duplicate decks
                    if (!dict.ContainsKey(archetypeDeck)) dict.Add(archetypeDeck, archetypeDeck.Similarity(opponentCardlist));
                }

                // Get highest similarity value
                var maxSim = dict.Values.DefaultIfEmpty(0).Max(); // Some unreproducable bug threw an exception here. System.InvalidOperationException: Sequence contains no elements @ IEnumerable.Max() => should be fixed by DefaultIfEmpty() now!

                // If any archetype deck matches more than MinimumSimilarity (as percentage) show the deck with the highest similarity
                if (maxSim >= Settings.Default.MinimumSimilarity * 0.01)
                {
                    // Select top decks with highest similarity value
                    var topSimDecks = (from d in dict where Math.Abs(d.Value - maxSim) < 0.001 select d).ToList();
                    // Select top decks with most played games
                    var maxGames = topSimDecks.Max(x => x.Key.GetPlayedGames());
                    var topGamesDecks = (from t in topSimDecks where t.Key.GetPlayedGames() == maxGames select t).ToList();
                    // Select best matched deck with both highest similarity value and most played games
                    var matchedDeck = topGamesDecks.First();

                    // Show matched deck name and similarity value or number of matching cards and number of all played cards
                    if (Settings.Default.ShowAbsoluteSimilarity)
                    {
                        // Count how many cards from opponent deck are in matched deck
                        int matchingCards = matchedDeck.Key.CountMatchingCards(opponentCardlist);
                        _advisorOverlay.LblArchetype.Text = String.Format("{0} ({1}/{2})", matchedDeck.Key.Name, matchingCards, matchedDeck.Key.CountUnion(opponentCardlist));
                    }
                    else
                    {
                        _advisorOverlay.LblArchetype.Text = String.Format("{0} ({1}%)", matchedDeck.Key.Name, Math.Round(matchedDeck.Value * 100, 2));
                    }

                    _advisorOverlay.LblStats.Text = String.Format("{0}", matchedDeck.Key.Note);
                    Deck deck = DeckList.Instance.Decks.Where(d => d.TagList.ToLowerInvariant().Contains("archetype")).First(d => d.Name == matchedDeck.Key.Name);
                    if (deck != null)
                    {
                        var predictedCards = ((Deck)deck.Clone()).Cards.ToList();

                        foreach (var card in opponentCardlist)
                        {
                            // Remove already played opponent cards from predicted archetype deck. But don't remove revealed jousted cards, because they were only seen and not played yet.
                            if (predictedCards.Contains(card))
                            {
                                var item = predictedCards.Find(x => x.Id == card.Id);

                                if (!card.Jousted)
                                {
                                    item.Count -= card.Count;
                                }

                                // highlight jousted cards in green
                                // also highlight when deck has 2 of a card and we have matched one
                                if (item.Count > 0)
                                {
                                    item.HighlightInHand = true;
                                    item.InHandCount += card.Count;
                                }
                                else
                                {
                                    item.HighlightInHand = false;
                                    item.InHandCount = 0;
                                }
                            }
                            if (Settings.Default.ShowNonMatchingCards)
                            {
                                // Show known cards that don't match the archetype deck, in red
                                if (!predictedCards.Contains(card))
                                {
                                    var item = (Card)card.Clone();
                                    item.HighlightInHand = false;
                                    item.WasDiscarded = true;
                                    if (!item.Jousted)
                                        item.Count = 0;
                                    predictedCards.Add(item);
                                }
                            }

                        }
                        //var sortedPredictedCards = predictedCards.OrderBy(x => x.Cost).ThenBy(y => y.Name).ToList();
                        bool isNewArchetypeDeck = currentArchetypeDeckGuid != matchedDeck.Key.DeckId;
                        // Update overlay cards
                        _advisorOverlay.Update(predictedCards.ToSortedCardList(), isNewArchetypeDeck);
                        // Remember current archetype deck guid with highest similarity to opponent's played cards
                        currentArchetypeDeckGuid = matchedDeck.Key.DeckId;
                    }
                }
                else
                {
                    _advisorOverlay.LblArchetype.Text = String.Format("Best match: {0}%", Math.Round(maxSim * 100, 2));
                    _advisorOverlay.LblStats.Text = "";
                    _advisorOverlay.Update(Settings.Default.ShowNonMatchingCards ? opponentCardlist.ToList() : new List<Card>(), currentArchetypeDeckGuid != Guid.Empty);
                    currentArchetypeDeckGuid = Guid.Empty;
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

        public static async Task ImportMetastatsDecks()
        {
            //construct Progress<T>, passing ReportProgress as the Action<T> 
            var progressIndicator = new Progress<Tuple<int, int>>(ReportProgress);
            try
            {
                var importer = new Services.MetaStats.SnapshotImporter(new TrackerRepository()); // TODO: Get back to the IArchetypeImporter interface?
                var count = await importer.ImportDecks(
                    Settings.Default.AutoArchiveArchetypes,
                    Settings.Default.DeletePreviouslyImported,
                    Settings.Default.ShortenDeckNames,
                    progressIndicator);
                // Refresh decklist
                Core.MainWindow.LoadAndUpdateDecks();
                Notify("Import complete", $"{count} decks imported", 10);

            }
            catch (Exception e)
            {
                Log.Error(e);
                Notify("Import failed", e.Message, 15, "error", null);
            }
        }

        /// <summary>
        /// Reports the progress to the UI
        /// </summary>
        /// <param name="value"></param>
        private static void ReportProgress(Tuple<int, int> value)
        {
            int percentage = (int)((double)value.Item1 / value.Item2 * 100); 
            Notify("Import in progress", $"{value.Item1} of {value.Item2} decks ({percentage}%) imported", 0);
        }

        public static async Task ImportTempostormDecks()
        {
            try
            {
                IArchetypeImporter importer = new Services.TempoStorm.SnapshotImporter(new HttpClient(), new TrackerRepository());
                var count = await importer.ImportDecks(
                    Settings.Default.AutoArchiveArchetypes,
                    Settings.Default.DeletePreviouslyImported,
                    Settings.Default.ShortenDeckNames);
                // Refresh decklist
                Core.MainWindow.LoadAndUpdateDecks();
                Notify("Import complete", $"{count} decks imported", 10);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Notify("Import failed", e.Message, 15, "error", null);
            }
        }

        public static void DeleteArchetypeDecks()
        {
            try
            {
                var importer = new Services.MetaStats.SnapshotImporter(new TrackerRepository());
                var count = importer.DeleteDecks();
                // Refresh decklist
                Core.MainWindow.LoadAndUpdateDecks();
                Notify("Deletion complete", $"{count} decks deleted", 10);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Notify("Deletion failed", e.Message, 15, "error", null);
            }
        }

        /// <summary>
        /// Open link to donation website in standard browser
        /// </summary>
        public static void Donate()
        {
            try
            {
                Process.Start("https://paypal.me/djdookie");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        /// <summary>
        /// Open link to plugin website in standard browser
        /// </summary>
        public static void OpenWebsite()
        {
            try
            {
                Process.Start("https://github.com/djdookie/Advisor");
            }
            catch (Exception e)
            {
                Log.Error(e);
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
