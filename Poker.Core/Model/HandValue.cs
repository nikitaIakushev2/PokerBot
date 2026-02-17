using System;
using System.Collections.Generic;
using System.Text;

namespace Poker.Core.Model {
    public readonly struct HandValue : IComparable<HandValue> {
        public HandRank Rank { get; }
        public ulong Value { get; }

        public HandValue(HandRank rank, ulong value) {
            Rank = rank;
            Value = value;
        }

        public int CompareTo(HandValue other) {
            return Value.CompareTo(other.Value);
        }

        public static bool operator >(HandValue a, HandValue b) => a.CompareTo(b) > 0;
        public static bool operator <(HandValue a, HandValue b) => a.CompareTo(b) < 0;
    }
}
