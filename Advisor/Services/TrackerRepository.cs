using System;
using System.Linq;
using System.Windows.Threading;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HDTDeck = Hearthstone_Deck_Tracker.Hearthstone.Deck;

namespace HDT.Plugins.Advisor.Services
{
    public class TrackerRepository
    {
        /// <summary>
        ///     Add a deck to the Decklist.
        /// </summary>
        /// <param name="name">Name of the deck</param>
        /// <param name="deck">The new deck to add</param>
        /// <param name="archive">Flag, if the new deck should be archieved</param>
        /// <param name="tags">Tags to be added to the new deck</param>
        public void AddDeck(string name, HDTDeck deck, bool archive, params string[] tags)
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
        ///     Delete all decks with given tag.
        /// </summary>
        /// <param name="tag">The tag</param>
        /// <returns></returns>
        public int DeleteAllDecksWithTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return 0;
            }

            var decks = DeckList.Instance.Decks.Where(d => d.Tags.Contains(tag)).ToList();
            Log.Info($"Deleting {decks.Count} archetype decks");
            foreach (var d in decks)
            {
                DeckList.Instance.Decks.Remove(d);
            }

            if (decks.Any())
            {
                DeckList.Save();
            }

            // Refresh decklist
            var deletedDecks = decks.Count - DeckList.Instance.Decks.Where(d => d.Tags.Contains(tag)).ToList().Count;
            return deletedDecks;
        }
    }
}