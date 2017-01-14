using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing;
using HtmlAgilityPack;

namespace HDT.Plugins.Advisor.Services.MetaStats
{
	public class SnapshotImporter : IArchetypeImporter
	{
        private const string BaseUrl = "http://metastats.net";
	    private const string BaseSnapshotUrl = "http://metastats.net/decks/";

        private static int _decksFound;
        private static int _decksImported;

        private const string ArchetypeTag = "Archetype";
		private const string PluginTag = "Advisor";

		private ITrackerRepository _tracker;
		private ILoggingService _logger;

		public SnapshotImporter(ITrackerRepository tracker)
		{
			_tracker = tracker;
			_logger = new TrackerLogger();
            _decksFound = 0;
            _decksImported = 0;

		}

        /// <summary>
        /// Task to import decks.
        /// </summary>
        /// <param name="archive">Option to auto-archive imported decks</param>
        /// <param name="deletePrevious">Option to delete all previously imported decks</param>
        /// <param name="removeClass">Option to remove classname from deck title</param>
        /// <returns></returns>
		public async Task<int> ImportDecks(bool archive, bool deletePrevious, bool removeClass)
		{
			_logger.Info("Starting meta deck import");
			int deckCount = 0;

			// Delete previous snapshot decks
			if (deletePrevious)
			{
				_logger.Info("Deleting previous meta decks");
				_tracker.DeleteAllDecksWithTag(PluginTag);
			}

            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = new HtmlDocument();
            doc = hw.Load(BaseSnapshotUrl);

            // Get link for each class
            var classSites = doc.DocumentNode.SelectNodes("//div[@id='meta-nav']/ul/li/a/@href");

            var tasks = new List<Task<IList<Deck>>>();
            var decks = new List<Deck>();

            foreach (HtmlNode link in classSites)
            {
                // Get the value of the HREF attribute
                string hrefValue = link.GetAttributeValue("href", string.Empty);
                // Create tasks to parallel process all classites and speed up the deck collection
                var task = Task.Run(() => GetClassDecks(BaseUrl + hrefValue));
                tasks.Add(task);
            }

            // Wait for all threads to finish, then combine results
            await Task.WhenAll(tasks);
            foreach (var t in tasks)
            {
                decks.AddRange(t.Result);
            }

            // TODO: Remove duplicates if any?

            // Add all decks to the tracker
		    foreach (var deck in decks)
		    {
                _logger.Info($"Importing deck ({deck.Name})");

                // Optionally remove player class from deck name
                // E.g. 'Control Warrior' => 'Control'
                var deckName = deck.Name;
                if (removeClass)
                    deckName = deckName.Replace(deck.Class, "").Trim();

                _tracker.AddDeck(deckName, deck, archive, ArchetypeTag, PluginTag);
                deckCount++;
            }

			return deckCount;
		}

        /// <summary>
        /// Gets all decks for a given class URL.
        /// </summary>
        /// <param name="url">The URL of the class</param>
        /// <param name="decks">The list of decks to be filled</param>
        /// <returns></returns>
        private async Task<IList<Deck>> GetClassDecks(string url)
        {
            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = new HtmlDocument();
            doc = hw.Load(url);

            var deckSites = doc.DocumentNode.SelectNodes("//div[@class='decklist']/div/h4/a/@href");
            //var deckUrls = new List<string>();

            // Count found decks thread-safe
            Interlocked.Add(ref _decksFound, deckSites.Count);

            var decks = new List<Deck>();

            foreach (HtmlNode link in deckSites)
            {
                string hrefValue = link.GetAttributeValue("href", string.Empty);
                var result = await Task.Run(() => GetDeck(BaseUrl + hrefValue));
                decks.Add(result);
            }

            return decks;
        }

        /// <summary>
        /// Gets a deck from the meta description of a website.
        /// </summary>
        /// <param name="url">The URL to the website</param>
        /// <returns></returns>
        private async Task<Deck> GetDeck(string url)
        {
            var result = await MetaTagImporter.TryFindDeck(url);

            // Count imported decks thread-safe
            Interlocked.Increment(ref _decksImported);

            return result;
        }
    }
}