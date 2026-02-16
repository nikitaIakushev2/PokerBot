using System;
using System.Collections.Generic;
using System.Text;

namespace Poker.Core.Model {
    public readonly struct Card {
        public Rank Rank { get; }
        public Suit Suit { get; }

        public Card(Rank rank, Suit suit) {
            Rank = rank;
            Suit = suit;
        }
    }
}
