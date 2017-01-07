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
//using HDT.Plugins.Advisor.Models;

namespace HDT.Plugins.Advisor
{
    internal class Advisor
    {
        private int mana = 0;
        private CardList cardList = null;
        // Highest deck similarity
        //double maxSim = 0;
        TrackerRepository trackerRepository;

        public Advisor(CardList list)
        {
            cardList = list;
            trackerRepository = new Services.TrackerRepository();

            // Hide in menu, if necessary
            if (Config.Instance.HideInMenu && CoreAPI.Game.IsInMenu) cardList.Hide();
        }

        internal List<Entity> Entities => Helper.DeepClone<Dictionary<int, Entity>>(CoreAPI.Game.Entities).Values.ToList<Entity>();

        internal Entity Opponent => Entities?.FirstOrDefault(x => x.IsOpponent);

        // Reset on when a new game starts
        internal void GameStart()
        {
            mana = 0;
            //maxSim = 0;
            cardList.Update(new List<Card>());
        }

		// Need to handle hiding the element when in the game menu
		internal void InMenu()
		{
			if (Config.Instance.HideInMenu)
			{
                cardList.Hide();
			}
		}


        internal async void OpponentPlay(Card card)
        {
            await Task.Delay(100);
            updateCardList();
        }

        internal void OpponentSecretTiggered(Card card)
        {
            updateCardList();
        }

        internal void updateCardList()
        {
            //var opponentCardlist = Core.Game.Opponent.RevealedCards;
            var opponentCardlist = Core.Game.Opponent.OpponentCardList.Where(x => !x.IsCreated);
            //Log.Info("+++++ Advisor: " + opponentCardlist.Count);

            //Update list of the opponent's played cards
            //cardList.Update(opponentCardlist.ToList());

            var opponentDeck = new Models.Deck(opponentCardlist);

            // Create archetype dictionary and calculate matching similarities to yet known opponent cards
            IDictionary<Models.ArchetypeDeck, float> dict = new Dictionary<Models.ArchetypeDeck, float>();


            foreach (var archetypeDeck in trackerRepository.GetAllArchetypeDecks())
            {
                dict.Add(archetypeDeck, opponentDeck.Similarity(archetypeDeck));
            }

            // Sort dictionary by value
            var sortedDict = from entry in dict orderby entry.Value descending select entry;

            //Log.Info("+++++ ADVISOR: " + sortedDict.First().Key.Name + " " + sortedDict.First().Value);
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

        // Calculate the mana opponent will have on his next turn
        internal int AvailableMana()
		{
			var opp = Opponent;
			if (opp != null)
			{
				var res = opp.GetTag(GameTag.RESOURCES);
				var overload = opp.GetTag(GameTag.OVERLOAD_OWED);
				// looking a turn ahead, so add one mana
				mana = res + 1 - overload;
			}
			return mana;
		}

		// Convert hero class string to enum
		internal CardClass KlassConverter(string klass)
		{
			switch (klass.ToLowerInvariant())
			{
				case "druid":
					return CardClass.DRUID;

				case "hunter":
					return CardClass.HUNTER;

				case "mage":
					return CardClass.MAGE;

				case "paladin":
					return CardClass.PALADIN;

				case "priest":
					return CardClass.PRIEST;

				case "rogue":
					return CardClass.ROGUE;

				case "shaman":
					return CardClass.SHAMAN;

				case "warlock":
					return CardClass.WARLOCK;

				case "warrior":
					return CardClass.WARRIOR;

				default:
					return CardClass.NEUTRAL;
			}
		}
    }
}
