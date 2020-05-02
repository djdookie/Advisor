using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace HDT.Plugins.Advisor
{
    public static class ExtensionMethods
    {
        /// <summary>
        ///     Uses the Jaccard index to give a indication of similarity between a deck and a cardlist.
        /// </summary>
        /// <returns>The Jaccard index, a float between 0 and 1</returns>
        public static float Similarity(this Deck thisDeck, IList<Card> cards)
        {
            if (cards == null)
            {
                return 0;
            }

            var lenA = thisDeck.Cards.Sum(x => x.Count);
            var lenB = cards.Sum(x => x.Count);

            if (lenA == 0 && lenB == 0)
            {
                return 1;
            }

            var lenAnB = thisDeck.Cards.Sum(i => cards.Where(i.Equals).Sum(j => Math.Min(i.Count, j.Count)));

            return (float) Math.Round((float) lenAnB / (lenA + lenB - lenAnB), 4);
        }

        /// <summary>
        ///     Counts the absolute number of matching/intersecting cards between a deck and a cardlist.
        /// </summary>
        /// <returns>The number of intersecting cards</returns>
        public static int CountMatchingCards(this Deck thisDeck, IList<Card> cards)
        {
            if (cards == null)
            {
                return 0;
            }

            return thisDeck.Cards.Sum(i => cards.Where(i.Equals).Sum(j => Math.Min(i.Count, j.Count)));
        }

        public static int CountUnion(this Deck thisDeck, IList<Card> cards)
        {
            if (cards == null)
            {
                return 0;
            }

            var lenA = thisDeck.Cards.Sum(x => x.Count);
            var lenB = cards.Sum(x => x.Count);

            var count = thisDeck.Cards.Sum(i => cards.Where(i.Equals).Sum(j => Math.Min(i.Count, j.Count)));

            return lenA + lenB - count;
        }

        /// <summary>
        ///     Gets the number of played games stored in the deck's note field.
        /// </summary>
        /// <param name="thisDeck">The deck</param>
        /// <returns>Number of played games with the given deck. If no info is found or parse is unsuccessful, return 0.</returns>
        public static int GetPlayedGames(this Deck thisDeck)
        {
            var success = int.TryParse(Regex.Match(thisDeck.Note, @"Games: ([0-9]+)").Groups[1].Value, out var result);
            return success ? result : 0;
        }
    }
}