using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HDT.Plugins.Advisor.Layout;
using HDT.Plugins.Advisor.Properties;
using HDT.Plugins.Advisor.Services;
using HDT.Plugins.Advisor.Services.MetaStats;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls;
using CoreAPI = Hearthstone_Deck_Tracker.API.Core;

namespace HDT.Plugins.Advisor
{
    internal class Advisor
    {
        private static Flyout _settingsFlyout;
        private static Flyout _notificationFlyout;
        private readonly AdvisorOverlay _advisorOverlay;
        private Guid _currentArchetypeDeckGuid;

        public Advisor(AdvisorOverlay overlay)
        {
            _notificationFlyout = CreateDialogFlyout();
            _settingsFlyout = CreateSettingsFlyout();
            Settings.Default.PropertyChanged += Settings_PropertyChanged;

            _advisorOverlay = overlay;

            _advisorOverlay.LblArchetype.Text = "";
            _advisorOverlay.LblStats.Text = "";

            // TODO: CoreAPI.Game.IsInMenu is true, so Advisor overlay is not shown after instantiating this addon while a game is running. How do we repair this?
            UpdateCardList();

            // Hide in menu, if necessary
            if (Config.Instance.HideInMenu && CoreAPI.Game.IsInMenu)
            {
                _advisorOverlay.Hide();
            }
            else
            {
                _advisorOverlay.Show();
            }
        }

        /// <summary>
        ///     Determine if in valid game mode as specified in config
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

                return false;
            }
        }

        /// <summary>
        ///     Determine if in valid game format as specified in config
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

                return false;
            }
        }

        /// <summary>
        ///     All archetype decks from tracker repository
        /// </summary>
        private static IEnumerable<Deck> ArchetypeDecks => DeckList.Instance.Decks.Where(d => d.TagList.ToLowerInvariant().Contains("archetype"));

        /// <summary>
        ///     If settings were changed, update overlay and save settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Settings.Default.Save();
            GameStart();
        }

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
                _currentArchetypeDeckGuid = Guid.Empty;

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
        ///     Determine similarity between all archetype decks and all cards played by the opponent yet.
        ///     Then update the cardlist displayed in the overlay with the highest matching archetype deck after removing all
        ///     opponent cards.
        /// </summary>
        internal async void UpdateCardList()
        {
            // Small delay to guarantee opponents cards list is up to date (should be 300+ ms in debug mode or with attached debugger, otherwise restarting HDT could lead to an incomplete opponent decklist!)
#if DEBUG
            await Task.Delay(1000);
#else
            await Task.Delay(100);
#endif

            // Only continue if in valid game mode or game format
            if (!IsValidGameMode || !IsValidGameFormat)
            {
                return;
            }

            // Get opponent's cards list (all yet revealed cards)
            IList<Card> opponentCardlist = Core.Game.Opponent.OpponentCardList.Where(x => !x.IsCreated).ToList();

            // If opponent's class is unknown yet or we have no imported archetype decks in the database, return empty card list
            if (CoreAPI.Game.Opponent.Class == "" || !ArchetypeDecks.Any())
            {
                _currentArchetypeDeckGuid = Guid.Empty;
                _advisorOverlay.Update(new List<Card>(), true);
            }
            else
            {
                // Create archetype dictionary
                // Calculate similarities between all opponent's class archetype decks and all yet known opponent cards. Exclude wild decks in standard format using NAND expression. OR expression should also work: (!d.IsWildDeck || CoreAPI.Game.CurrentFormat == Format.Wild)
                IDictionary<Deck, float> dict = ArchetypeDecks
                    .Where(d => d.Class == CoreAPI.Game.Opponent.Class && !(d.IsWildDeck && CoreAPI.Game.CurrentFormat == Format.Standard))
                    .Distinct()
                    .ToDictionary(d => d, d => d.Similarity(opponentCardlist));

                // Get highest similarity value
                // Some unreproducable bug threw an exception here. System.InvalidOperationException: Sequence contains no elements @ IEnumerable.Max() => should be fixed by DefaultIfEmpty() now!
                var maxSim = dict.Values.DefaultIfEmpty(0).Max();

                // If any archetype deck matches more than MinimumSimilarity (as percentage) show the deck with the highest similarity (important: we need at least 1 deck in dict, otherwise we can't show any results.)
                if (dict.Count > 0 && maxSim >= Settings.Default.MinimumSimilarity * 0.01)
                {
                    // Select top decks with highest similarity value
                    var topSimDecks = dict.Where(d => Math.Abs(d.Value - maxSim) < 0.001).ToList();
                    // Select top decks with most played games
                    // If class was something like "Groddo the Bogwarden" in monster hunt, we got an InvalidOperationException at Max() because dict was empty. Now we check for dict > 0 above to prevent this.
                    var maxGames = topSimDecks.Max(x => x.Key.GetPlayedGames());
                    var topGamesDecks = topSimDecks.Where(t => t.Key.GetPlayedGames() == maxGames).ToList();
                    // Select best matched deck with both highest similarity value and most played games
                    var matchedDeck = topGamesDecks.First();

                    // Show matched deck name and similarity value or number of matching cards and number of all played cards
                    if (Settings.Default.ShowAbsoluteSimilarity)
                    {
                        // Count how many cards from opponent deck are in matched deck
                        var matchingCards = matchedDeck.Key.CountMatchingCards(opponentCardlist);
                        _advisorOverlay.LblArchetype.Text = $"{matchedDeck.Key.Name} ({matchingCards}/{matchedDeck.Key.CountUnion(opponentCardlist)})";
                    }
                    else
                    {
                        _advisorOverlay.LblArchetype.Text = $"{matchedDeck.Key.Name} ({Math.Round(matchedDeck.Value * 100, 2)}%)";
                    }

                    _advisorOverlay.LblStats.Text = matchedDeck.Key.Note;
                    var deck = DeckList.Instance.Decks.Where(d => d.TagList.ToLowerInvariant().Contains("archetype")).First(d => d.Name == matchedDeck.Key.Name);

                    var predictedCards = ((Deck) deck.Clone()).Cards.ToList();

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

                            // Highlight jousted cards in green. Also highlight when deck has 2 of a card and we have matched one.
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
                        else if (Settings.Default.ShowNonMatchingCards)
                        {
                            // Show known cards that don't match the archetype deck, in red
                            var item = (Card) card.Clone();
                            item.HighlightInHand = false;
                            item.WasDiscarded = true;
                            if (!item.Jousted)
                            {
                                item.Count = 0;
                            }

                            predictedCards.Add(item);
                        }
                    }

                    var isNewArchetypeDeck = _currentArchetypeDeckGuid != matchedDeck.Key.DeckId;

                    // Remove cards with 0 left when setting is set to true.
                    if (Settings.Default.RemovePlayedCards)
                    {
                        predictedCards = predictedCards.Where(x => x.Count > 0).ToList();
                    }

                    // Update overlay cards.
                    _advisorOverlay.Update(predictedCards.ToSortedCardList(), isNewArchetypeDeck);
                    // Remember current archetype deck guid with highest similarity to opponent's played cards.
                    _currentArchetypeDeckGuid = matchedDeck.Key.DeckId;
                }
                else
                {
                    // If no archetype deck matches more than MinimumSimilarity clear the list and show the best match percentage
                    _advisorOverlay.LblArchetype.Text = $"Best match: {Math.Round(maxSim * 100, 2)}%";
                    _advisorOverlay.LblStats.Text = "";
                    _advisorOverlay.Update(Settings.Default.ShowNonMatchingCards ? opponentCardlist.ToList() : new List<Card>(), _currentArchetypeDeckGuid != Guid.Empty);
                    _currentArchetypeDeckGuid = Guid.Empty;
                }
            }
        }

        public static void ShowSettings()
        {
            if (_settingsFlyout == null)
            {
                _settingsFlyout = CreateSettingsFlyout();
            }

            _settingsFlyout.IsOpen = true;
        }

        public static void Notify(string title, string message, int autoClose, string icon = null, Action action = null)
        {
            if (_notificationFlyout == null)
            {
                _notificationFlyout = CreateDialogFlyout();
            }

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
                // TODO: Get back to the IArchetypeImporter interface?
                var importer = new SnapshotImporter(new TrackerRepository());
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
                Notify("Import failed", e.Message, 15, "error");
            }
        }

        /// <summary>
        ///     Reports the progress to the UI
        /// </summary>
        /// <param name="value"></param>
        private static void ReportProgress(Tuple<int, int> value)
        {
            var percentage = (int) ((double) value.Item1 / value.Item2 * 100);
            Notify("Import in progress", $"{value.Item1} of {value.Item2} decks ({percentage}%) imported", 0);
        }

        public static void DeleteArchetypeDecks()
        {
            try
            {
                var importer = new SnapshotImporter(new TrackerRepository());
                var count = importer.DeleteDecks();
                // Refresh decklist
                Core.MainWindow.LoadAndUpdateDecks();
                Notify("Deletion complete", $"{count} decks deleted", 10);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Notify("Deletion failed", e.Message, 15, "error");
            }
        }

        /// <summary>
        ///     Open link to donation website in standard browser
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
        ///     Open link to plugin website in standard browser
        /// </summary>
        public static void OpenWebsite()
        {
            try
            {
                Process.Start("https://github.com/kimsey0/Advisor");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }


        private static Flyout CreateSettingsFlyout()
        {
            var settings = new Flyout
            {
                Name = "AdvisorSettingsFlyout",
                Position = Position.Left,
                Header = "Advisor Settings",
                Content = new SettingsView()
            };
            Panel.SetZIndex(settings, 100);
            Core.MainWindow.Flyouts.Items.Add(settings);
            return settings;
        }

        private static Flyout CreateDialogFlyout()
        {
            var dialog = new Flyout
            {
                Name = "AdvisorDialogFlyout",
                Theme = FlyoutTheme.Accent,
                Position = Position.Bottom,
                TitleVisibility = Visibility.Collapsed,
                CloseButtonVisibility = Visibility.Collapsed,
                IsPinned = false,
                Height = 50
            };
            Panel.SetZIndex(dialog, 1000);
            Core.MainWindow.Flyouts.Items.Add(dialog);
            return dialog;
        }
    }
}