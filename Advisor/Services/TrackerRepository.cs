using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using HDT.Plugins.Advisor.Models;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Controls.DeckPicker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using HDTCard = Hearthstone_Deck_Tracker.Hearthstone.Card;
using HDTDeck = Hearthstone_Deck_Tracker.Hearthstone.Deck;

namespace HDT.Plugins.Advisor.Services
{
	public class TrackerRepository : ITrackerRepository
	{
		public bool IsInMenu()
		{
			return Core.Game?.IsInMenu ?? false;
		}

		public List<ArchetypeDeck> GetAllArchetypeDecks()
		{
			var decks = DeckList.Instance.Decks
				.Where(d => d.TagList.ToLowerInvariant().Contains("archetype"))
				.ToList();
			var archetypes = new List<ArchetypeDeck>();
			foreach (var d in decks)
			{
				// get the newest version of the deck
				var v = d.VersionsIncludingSelf.OrderByDescending(x => x).FirstOrDefault();
				d.SelectVersion(v);
				if (d == null)
					continue;
				var ad = new ArchetypeDeck(d.Name, KlassKonverter.FromString(d.Class), d.StandardViable);
				ad.Cards = d.Cards
					.Select(x => new Models.Card(x.Id, x.LocalizedName, x.Count, x.Background.Clone()))
					.ToList();
				archetypes.Add(ad);
			}
			return archetypes;
		}

		public Models.Deck GetOpponentDeck()
		{
			var deck = new Models.Deck();
			if (Core.Game.IsRunning)
			{
				var game = Core.Game.CurrentGameStats;
				if (game != null && game.CanGetOpponentDeck)
				{
					Log.Info("Opponent deck available");
					// hero class
					deck.Klass = KlassKonverter.FromString(game.OpponentHero);
					// standard viable, use temp HDT deck
					var hdtDeck = new HDTDeck();
					foreach (var card in game.OpponentCards)
					{
						var c = Database.GetCardFromId(card.Id);
						c.Count = card.Count;
						hdtDeck.Cards.Add(c);
						if (c != null && c != Database.UnknownCard)
						{
							deck.Cards.Add(
								new Models.Card(c.Id, c.LocalizedName, c.Count, c.Background.Clone()));
						}
					}
					deck.IsStandard = hdtDeck.StandardViable;
				}
			}
			return deck;
		}

		public string GetGameNote()
		{
			if (Core.Game.IsRunning && Core.Game.CurrentGameStats != null)
			{
				return Core.Game.CurrentGameStats.Note;
			}
			return null;
		}

		public void UpdateGameNote(string text)
		{
			if (Core.Game.IsRunning && Core.Game.CurrentGameStats != null)
			{
				Core.Game.CurrentGameStats.Note = text;
			}
		}

		public void AddDeck(Models.Deck deck)
		{
			HDTDeck d = new HDTDeck();
			var arch = deck as ArchetypeDeck;
			if (arch != null)
				d.Name = arch.Name;
			d.Class = deck.Klass.ToString();
			d.Cards = new ObservableCollection<HDTCard>(deck.Cards.Select(c => Database.GetCardFromId(c.Id)));
			DeckList.Instance.Decks.Add(d);
			// doesn't refresh the deck picker view
		}

		public void AddDeck(string name, string playerClass, string cards, bool archive, params string[] tags)
		{
            var deck = StringImporter.Import(cards);
            if (deck != null)
            {
                deck.Name = name;
                if (deck.Class != playerClass)
                    deck.Class = playerClass;
                if (tags.Any())
                {
                    var reloadTags = false;
                    foreach (var t in tags)
                    {
                        if (!DeckList.Instance.AllTags.Contains(t))
                        {
                            DeckList.Instance.AllTags.Add(t);
                            reloadTags = true;
                        }
                        deck.Tags.Add(t);
                    }
                    if (reloadTags)
                    {
                        DeckList.Save();
                        Core.MainWindow.ReloadTags();
                    }
                }
                //// hack time!
                //// use MainWindow.ArchiveDeck to update
                //// set deck archive to opposite of desired
                //deck.Archived = !archive;
                //// add and save
                //DeckList.Instance.Decks.Add(deck);
                //DeckList.Save();
                //// now reverse 'archive' of the deck
                //// this should refresh all ui elements
                //Core.MainWindow.ArchiveDeck(deck, archive);

                // Add and save deck
                deck.Archived = archive;
                DeckList.Instance.Decks.Add(deck);
                DeckList.Save();
                // Refresh decklist
                //Core.MainWindow.LoadAndUpdateDecks();
            }
        }

        /// <summary>
        /// Add a deck to the Decklist.
        /// </summary>
        /// <param name="name">Name of the deck</param>
        /// <param name="deck">The new deck to add</param>
        /// <param name="archive">Flag, if the new deck should be archieved</param>
        /// <param name="tags">Tags to be added to the new deck</param>
        public void AddDeck(string name, Hearthstone_Deck_Tracker.Hearthstone.Deck deck, bool archive, params string[] tags)
        {
            deck.Name = name;
            if (tags.Any())
            {
                var reloadTags = false;
                foreach (var t in tags)
                {
                    if (!DeckList.Instance.AllTags.Contains(t))
                    {
                        DeckList.Instance.AllTags.Add(t);
                        reloadTags = true;
                    }
                    deck.Tags.Add(t);
                }
                if (reloadTags)
                {
                    DeckList.Save();
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        Core.MainWindow.ReloadTags(); //TODO: Refresh tags in all tag lists in the UI.
                    }));
                }
            }

            // Add and save deck
            deck.Archived = archive;
            DeckList.Instance.Decks.Add(deck);
        }

        /// <summary>
        /// Delete all decks with given tag.
        /// </summary>
        /// <param name="tag">The tag</param>
        /// <returns></returns>
        public int DeleteAllDecksWithTag(string tag)
		{
			if (string.IsNullOrWhiteSpace(tag))
				return 0;
			var decks = DeckList.Instance.Decks.Where(d => d.Tags.Contains(tag)).ToList();
			Log.Info($"Deleting {decks.Count} archetype decks");
			foreach (var d in decks)
				DeckList.Instance.Decks.Remove(d);
			if (decks.Any())
				DeckList.Save();
            // Refresh decklist
            //Core.MainWindow.LoadAndUpdateDecks();
		    var deletedDecks = decks.Count - DeckList.Instance.Decks.Where(d => d.Tags.Contains(tag)).ToList().Count;
            return deletedDecks;
		}

		public string GetGameMode()
		{
			if (Core.Game.IsRunning && Core.Game.CurrentGameStats != null)
			{
				return Core.Game.CurrentGameStats.GameMode.ToString().ToLowerInvariant();
			}
			return string.Empty;
		}
	}
}