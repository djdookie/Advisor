using System;

namespace HDT.Plugins.Advisor.Models
{
    public class MatchResult : IComparable<MatchResult>
    {
        public const float THRESHOLD = 0.09f;

        public MatchResult(ArchetypeDeck deck, float similarity)
        {
            Deck = deck;
            Similarity = similarity;
        }

        public MatchResult(ArchetypeDeck deck, float similarity, float containment)
        {
            Deck = deck;
            Similarity = similarity;
            Containment = containment;
        }

        public ArchetypeDeck Deck { get; }
        public float Similarity { get; }
        public float Containment { get; }

        public int CompareTo(MatchResult other)
        {
            if (Similarity == other.Similarity)
            {
                return Containment.CompareTo(other.Containment);
            }

            return Similarity.CompareTo(other.Similarity);
        }
    }
}