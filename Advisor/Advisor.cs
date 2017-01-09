using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using CoreAPI = Hearthstone_Deck_Tracker.API.Core;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HDT.Plugins.Advisor.Services;
using System.Collections;
using HDT.Plugins.Advisor.Layout;
//using HDT.Plugins.Advisor.Models;

namespace HDT.Plugins.Advisor
{
    internal class Advisor
    {
        //private int mana = 0;
        private CardList cardList = null;
        // Highest deck similarity
        //double maxSim = 0;
        TrackerRepository trackerRepository;

        public Advisor(CardList list)
        {
            cardList = list;
            trackerRepository = new Services.TrackerRepository();

            cardList.LblArchetype.Text = "No matching archetype yet";
            //cardList.Update(new List<Card>());
            updateCardList();

            // Hide in menu, if necessary
            if (Config.Instance.HideInMenu && CoreAPI.Game.IsInMenu)
            {
                cardList.Hide();
            } else
            {
                cardList.Show();
            }
        }

        //internal List<Entity> Entities => Helper.DeepClone<Dictionary<int, Entity>>(CoreAPI.Game.Entities).Values.ToList<Entity>();

        //internal Entity Opponent => Entities?.FirstOrDefault(x => x.IsOpponent);

        // Reset on when a new game starts
        internal void GameStart()
        {
            //mana = 0;
            //maxSim = 0;
            cardList.LblArchetype.Text = "No matching archetype yet";
            //cardList.Update(new List<Card>());
            updateCardList();
            cardList.Show();
        }

        // Need to handle hiding the element when in the game menu
        internal void InMenu()
        {
            if (Config.Instance.HideInMenu)
            {
                cardList.Hide();
            }
        }

        internal void OpponentHandDiscard(Card card)
        {
            updateCardList();
        }

        internal void OpponentJoustReveal(Card card)
        {
            updateCardList();
        }

        internal void OpponentDeckToPlay(Card card)
        {
            updateCardList();
        }

        internal void OpponentDeckDiscard(Card card)
        {
            updateCardList();
        }

        internal void OpponentPlay(Card card)
        {
            updateCardList();
        }

        internal void OpponentSecretTiggered(Card card)
        {
            updateCardList();
        }

        internal async void updateCardList()
        {
            // Small delay to guarantee opponents cards list is up to date
            await Task.Delay(100);

            // Get opponent's cards list (all yet revealed cards)
            //var opponentCardlist = Core.Game.Opponent.RevealedCards;
            var opponentCardlist = Core.Game.Opponent.OpponentCardList.Where(x => !x.IsCreated);

            // If no opponent's cards were revealed yet, return empty card list
            if (opponentCardlist.Count() <= 0)
            {
                cardList.Update(new List<Card>());
                return;
            }
            else
            {
                //Log.Info("+++++ Advisor: " + opponentCardlist.Count);

                //Update list of the opponent's played cards
                //cardList.Update(opponentCardlist.ToList());

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
                    cardList.LblArchetype.Text = String.Format("{0} ({1}%)", sortedDict.FirstOrDefault().Key.Name, Math.Round(sortedDict.FirstOrDefault().Value * 100, 2));
                    var opponentCards = DeckList.Instance.Decks.Where(d => d.TagList.ToLowerInvariant().Contains("archetype")).Where(d => d.Name == sortedDict.FirstOrDefault().Key.Name).FirstOrDefault().Cards.ToList();

                    cardList.Update(opponentCards);
                    
                    // Remove already played cards from archetype deck
                    //foreach (var card in opponentCardlist)
                    //{
                    //    if (opponentCards.Contains(card))
                    //    {
                    //        var item = opponentCards.Find(x => x.Id == card.Id);
                    //        if (item.Count > 1)
                    //        {
                    //            item.Count -= 1;
                    //        }
                    //        else
                    //        {
                    //            opponentCards.Remove(item);
                    //        }
                    //    }
                    //}
                    //cardList.Update(opponentCards);

                    //Log.Info("+++++ ADVISOR: " + sortedDict.FirstOrDefault().Key.Name + " " + sortedDict.FirstOrDefault().Value);
                }
            }
        }

        //      // Update the card list on player's turn
        //      internal void TurnStart(ActivePlayer player)
        //{
        //          //if (player == ActivePlayer.Player && Opponent != null)
        //          //{
        //          //             cardList.Show();
        //          //	var mana = AvailableMana();
        //          //	var klasse = KlassenConverter(CoreAPI.Game.Opponent.Class);
        //          //	var cards = HearthDb.Cards.Collectible.Values
        //          //		.Where(c => c.Cost == mana && c.Class == klasse)
        //          //		.Select(c => new Card(c))
        //          //		.OrderBy(c => c.Rarity)
        //          //		.ToList<Card>();
        //          //             cardList.Update(cards);
        //          //}

        //          //cardList.Show();
        //          //var mana = AvailableMana();
        //          //var klasse = KlassConverter(CoreAPI.Game.Opponent.Class);
        //          //var trackerRepository = new Services.TrackerRepository();
        //          ////var cards = trackerRepository.GetAllArchetypeDecks().FirstOrDefault().Cards.Select(x => new Models.Card((x.Id, x.Name, x.Count, x.Image.Clone())).ToList();
        //          //var decks = DeckList.Instance.Decks.Where(d => d.TagList.ToLowerInvariant().Contains("archetype")).ToList();
        //          //var cards = decks.FirstOrDefault().Cards.ToList();
        //          //cardList.Update(cards);
        //      }

  //      // Calculate the mana opponent will have on his next turn
  //      internal int AvailableMana()
		//{
		//	var opp = Opponent;
		//	if (opp != null)
		//	{
		//		var res = opp.GetTag(GameTag.RESOURCES);
		//		var overload = opp.GetTag(GameTag.OVERLOAD_OWED);
		//		// looking a turn ahead, so add one mana
		//		mana = res + 1 - overload;
		//	}
		//	return mana;
		//}

		//// Convert hero class string to enum
		//internal CardClass KlassConverter(string klass)
		//{
		//	switch (klass.ToLowerInvariant())
		//	{
		//		case "druid":
		//			return CardClass.DRUID;

		//		case "hunter":
		//			return CardClass.HUNTER;

		//		case "mage":
		//			return CardClass.MAGE;

		//		case "paladin":
		//			return CardClass.PALADIN;

		//		case "priest":
		//			return CardClass.PRIEST;

		//		case "rogue":
		//			return CardClass.ROGUE;

		//		case "shaman":
		//			return CardClass.SHAMAN;

		//		case "warlock":
		//			return CardClass.WARLOCK;

		//		case "warrior":
		//			return CardClass.WARRIOR;

		//		default:
		//			return CardClass.NEUTRAL;
		//	}
		//}
    }
}
