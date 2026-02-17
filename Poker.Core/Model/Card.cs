namespace Poker.Core.Model {
    public readonly struct Card {
        private readonly byte value; // 0–51

        public Card(byte value) {
            if (value > 51)
                throw new ArgumentOutOfRangeException(nameof(value), "Card value is incorrect!");

            this.value = value;
        }

        public byte Value => value;

        public Rank Rank => (Rank)RankIndex;
        public Suit Suit => (Suit)SuitIndex;
        public int RankIndex => (value % 13) + 2; // 2–14
        public int SuitIndex => value / 13; // 0–3
    }
}
