using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing;
using HtmlAgilityPack;

namespace HDT.Plugins.Advisor.Services.MetaStats
{
	public class SnapshotImporter
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
        /// <param name="shortenName">Option to shorten the deck title</param>
        /// <param name="progress">Tuple of two integers holding the progress information for the UI</param>
        /// <returns>The number of imported decks</returns>
		public async Task<int> ImportDecks(bool archive, bool deletePrevious, bool shortenName, IProgress<Tuple<int, int>> progress)
		{
			_logger.Info("Starting archetype deck import");
			//int deckCount = 0;

			// Delete previous snapshot decks
			if (deletePrevious)
			{
			    DeleteDecks();
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
                var task = Task.Run(() => GetClassDecks(BaseUrl + hrefValue, progress));
                tasks.Add(task);
            }

            // Wait for all threads to finish, then combine results
            await Task.WhenAll(tasks);
            
            foreach (var t in tasks)
            {
                decks.AddRange(t.Result);
            }

            // TODO: Remove duplicates if any?
            
            _logger.Info($"Saving {decks.Count} decks to the decklist.");

            // Add all decks to the tracker
            var deckCount = await Task.Run(() => SaveDecks(decks, archive, shortenName));

		    if (deckCount == decks.Count)
		    {
		        _logger.Info($"Import of {deckCount} archetype decks completed.");
		    }
		    else
		    {
		        _logger.Error($"Only {deckCount} of {decks.Count} archetype could be imported. Connection problems?");
		    }

		    return deckCount;
		}

        /// <summary>
        /// Save a list of decks to the Decklist.
        /// </summary>
        /// <param name="decks">A list of HDT decks</param>
        /// <param name="archive">Flag if the decks should be auto-archived</param>
        /// <param name="shortenName">Flag if class name and website name should be removed from the deckname</param>
        /// <returns></returns>
	    private int SaveDecks(IEnumerable<Deck> decks, bool archive, bool shortenName)
	    {
	        var deckCount = 0;
            
	        foreach (var deck in decks)
	        {
	            if (deck == null) throw new ImportException("At least one deck couldn't be imported. Connection problems?");

	            _logger.Info($"Importing deck ({deck.Name})");

	            // Optionally remove player class from deck name
	            // E.g. 'Control Warrior' => 'Control'
	            var deckName = deck.Name;
	            if (shortenName)
	            {
	                deckName = deckName.Replace(deck.Class, "").Trim();
	                deckName = deckName.Replace("- MetaStats ", "");
	                deckName = deckName.Replace("  ", " ");
	            }

	            _tracker.AddDeck(deckName, deck, archive, ArchetypeTag, PluginTag);
	            deckCount++;
	        }
            DeckList.Save();
            return deckCount;
	    }

	    /// <summary>
        /// Gets all decks for a given class URL.
        /// </summary>
        /// <param name="url">The URL of the class</param>
        /// <param name="progress">Tuple of two integers holding the progress information for the UI</param>
        /// <returns>The list of all parsed decks</returns>
        private async Task<IList<Deck>> GetClassDecks(string url, IProgress<Tuple<int, int>> progress)
        {
            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = new HtmlDocument();
            doc = hw.Load(url);

            var deckSites = doc.DocumentNode.SelectNodes("//div[@class='decklist']");

            // Count found decks thread-safe
            Interlocked.Add(ref _decksFound, deckSites.Count);

            // Report progress for UI
            progress.Report(new Tuple<int, int>(_decksImported, _decksFound));

            var decks = new List<Deck>();

            foreach (HtmlNode site in deckSites)
            {
                // Extract link
                HtmlNode link = site.SelectSingleNode("./h4/a/@href");
                string hrefValue = link.GetAttributeValue("href", string.Empty);

                // Extract info
                HtmlNode stats = site.SelectSingleNode("./div");
                string innerText = stats.InnerText;

                // Create deck from site
                var result = await Task.Run(() => GetDeck(BaseUrl + hrefValue, progress));

                // Add info to the deck
                result.Note = innerText;

                // Parse and add Guid to the deck
                string strId = Regex.Match(hrefValue, @"/deck/([0-9]+)/").Groups[1].Value;
                if (!string.IsNullOrEmpty(strId))
                {
                    result.DeckId = new Guid(strId.PadLeft(32, '0'));
                }

                // Set import datetime as LastEdited
                result.LastEdited = DateTime.Now;

                // Add deck to the decks list
                decks.Add(result);
            }

            return decks;
        }

        /// <summary>
        /// Gets a deck from the meta description of a website.
        /// </summary>
        /// <param name="url">The URL to the website</param>
        /// <param name="progress">Tuple of two integers holding the progress information for the UI</param>
        /// <returns>The parsed deck</returns>
        private async Task<Deck> GetDeck(string url, IProgress<Tuple<int, int>> progress)
        {
            // Create deck from metatags
            var result = await MetaTagImporter.TryFindDeck(url);
            
            // Count imported decks thread-safe
            Interlocked.Increment(ref _decksImported);

            // Report progress for UI
            progress.Report(new Tuple<int, int>(_decksImported, _decksFound));

            return result;
        }

        /// <summary>
        /// Deletes all decks with Plugin tag.
        /// </summary>
        public int DeleteDecks()
        {
            _logger.Info("Deleting all archetype decks");
            return _tracker.DeleteAllDecksWithTag(PluginTag);
        }
    }
}