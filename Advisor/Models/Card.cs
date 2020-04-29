using System.Windows.Media;

namespace HDT.Plugins.Advisor.Models
{
    public class Card
    {
        public Card()
        {
        }

        public Card(string id, string name, int count, DrawingBrush image)
        {
            Id = id;
            Name = name;
            Count = count;
            Image = image;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public DrawingBrush Image { get; set; }

        // two cards are equal once the ids are the same
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var c = obj as Card;
            if (c == null)
            {
                return false;
            }

            return Id == c.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}