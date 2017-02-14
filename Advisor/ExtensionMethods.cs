using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace HDT.Plugins.Advisor
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Uses the Jaccard index to give a indication of similarity between the two decks.</summary>
        /// <returns>Returns a float between 0 and 1 inclusive</returns>
        public static float Similarity(this Deck thisDeck, Deck deck)
        {
            if (deck == null)
                return 0;

            var lenA = thisDeck.Cards.Sum(x => x.Count);
            var lenB = deck.Cards.Sum(x => x.Count);
            var lenAnB = 0;

            if (lenA == 0 && lenB == 0)
                return 1;

            foreach (var i in thisDeck.Cards)
            {
                foreach (var j in deck.Cards)
                {
                    if (i.Equals(j))
                    {
                        lenAnB += Math.Min(i.Count, j.Count);
                    }
                }
            }

            return (float)Math.Round((float)lenAnB / (lenA + lenB - lenAnB), 4);
        }

        /// <summary>
        /// Uses the Jaccard index to give a indication of similarity between the two decks.</summary>
        /// <returns>Returns a float between 0 and 1 inclusive</returns>
        public static float Similarity(this Deck thisDeck, IList<Card> cards)
        {
            if (cards == null)
                return 0;

            var lenA = thisDeck.Cards.Sum(x => x.Count);
            var lenB = cards.Sum(x => x.Count);
            var lenAnB = 0;

            if (lenA == 0 && lenB == 0)
                return 1;

            foreach (var i in thisDeck.Cards)
            {
                foreach (var j in cards)
                {
                    if (i.Equals(j))
                    {
                        lenAnB += Math.Min(i.Count, j.Count);
                    }
                }
            }

            return (float)Math.Round((float)lenAnB / (lenA + lenB - lenAnB), 4);
        }

        /// <summary>
        /// Gets the number of played games stored in the deck's note field.
        /// </summary>
        /// <param name="thisDeck">The deck</param>
        /// <returns>Number of played games with the given deck</returns>
        public static int GetPlayedGames(this Deck thisDeck)
        {
            return Int32.Parse(Regex.Match(thisDeck.Note, @"[0-9]+$").ToString());
        }
    }
}
