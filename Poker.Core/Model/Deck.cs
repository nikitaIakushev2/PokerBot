namespace Poker.Core.Model {
    public sealed class Deck {
        private readonly Card[] cards = new Card[52];
        private int position;

        public Deck() {
            for (byte i = 0; i < 52; i++)
                cards[i] = new Card(i);
        }

        public void Shuffle(Random rng) {
            for (int i = 51; i > 0; i--) {
                int j = rng.Next(i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
            position = 0;
        }

        public Card Deal() {
            return cards[position++];
        }

        public void Reset() {
            position = 0;
        }
    }
}
