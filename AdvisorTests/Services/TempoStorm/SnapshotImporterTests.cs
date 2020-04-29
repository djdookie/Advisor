using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HDT.Plugins.Advisor.Services.TempoStorm.Tests
{
    [TestClass]
    public class SnapshotImporterTests
    {
        [TestMethod]
        public void SnapshotImporterTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void GetSnapshotSlugTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void GetSnapshotMockTest()
        {
            var mock = new Mock<IHttpClient>();
            mock.Setup(x => x.JsonGet(It.IsAny<string>()))
                .ReturnsAsync(@"{""title"":""Salt for all""}");
            var importer = new SnapshotImporter(mock.Object, null);

            var result = importer.GetSnapshot(new Tuple<string, string>("standard", "2016-09-10")).Result;

            Assert.AreEqual("Salt for all", result.Title);
        }

        [TestMethod]
        public void GetSnapshotTest()
        {
            //var mock = new Mock<IHttpClient>();
            //mock.Setup(x => x.JsonGet(It.IsAny<string>())).ReturnsAsync(@"{""title"":""Salt for all""}");
            var httpClient = new HttpClient();
            var trackerRepository = new TrackerRepository();
            var importer = new SnapshotImporter(httpClient, trackerRepository);

            var result = importer.GetSnapshot(new Tuple<string, string>("standard", "2016-09-10")).Result;

            Assert.AreEqual("Salt for all", result.Title);
        }

        [TestMethod]
        public void ImportDecksTest()
        {
            var httpClient = new HttpClient();
            var trackerRepository = new TrackerRepository();
            var importer = new SnapshotImporter(httpClient, trackerRepository);

            //_logger.Info("Starting meta deck import");
            var deckCount = 0;
            // delete previous snapshot decks
            //if (deletePrevious)
            //{
            //    _logger.Info("Deleting previous meta decks");
            //    _tracker.DeleteAllDecksWithTag(PluginTag);
            //}
            // get the lastest meta snapshot slug/date
            var slug = importer.GetSnapshotSlug();
            // use the slug to request the actual snapshot details
            //var snapshot = await GetSnapshot(slug);
            // add all decks to the tracker
            //foreach (var dt in snapshot.DeckTiers)
            //{
            //    var cards = "";
            //    _logger.Info($"Importing deck ({dt.Name})");
            //    foreach (var cd in dt.Deck.Cards)
            //    {
            //        cards += cd.Detail.Name;
            //        // don't add count if only one
            //        if (cd.Quantity > 1)
            //            cards += $" x {cd.Quantity}";
            //        cards += "\n";
            //    }
            //    // remove trailing newline
            //    if (cards.Length > 1)
            //        cards = cards.Substring(0, cards.Length - 1);

            //    // optionally remove player class from deck name
            //    // e.g. 'Control Warrior' => 'Control'
            //    var deckName = dt.Name;
            //    if (removeClass)
            //        deckName = deckName.Replace(dt.Deck.PlayerClass, "").Trim();

            //    _tracker.AddDeck(deckName, dt.Deck.PlayerClass, cards, archive, ArchetypeTag, PluginTag);
            //    deckCount++;
        }
    }
}