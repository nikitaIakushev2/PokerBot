using Poker.Core.Model;

namespace Poker.Core {
    public static class HandEvaluator {
        public static HandValue EvaluateBestOf7(ReadOnlySpan<Card> cards) {
            if (cards.Length != 7)
                throw new ArgumentException("Must contain exactly 7 cards.");

            HandValue best = default;

            Span<Card> temp = stackalloc Card[5];

            for (int a = 0; a < 3; a++)
                for (int b = a + 1; b < 4; b++)
                    for (int c = b + 1; c < 5; c++)
                        for (int d = c + 1; d < 6; d++)
                            for (int e = d + 1; e < 7; e++) {
                                temp[0] = cards[a];
                                temp[1] = cards[b];
                                temp[2] = cards[c];
                                temp[3] = cards[d];
                                temp[4] = cards[e];

                                var value = Evaluate5(temp);

                                if (value > best)
                                    best = value;
                            }

            return best;
        }
        static HandValue Evaluate5(ReadOnlySpan<Card> cards) {
            Span<int> rankCounts = stackalloc int[13];
            Span<int> suitCounts = stackalloc int[4];

            foreach (var card in cards) {
                rankCounts[card.RankIndex]++;
                suitCounts[card.SuitIndex]++;
            }

            bool isFlush = false;
            for (int i = 0; i < 4; i++)
                if (suitCounts[i] == 5)
                    isFlush = true;

            int straightHigh = GetStraightHigh(rankCounts);

            var orderedRanks = GetRanksDescending(rankCounts);

            if (isFlush && straightHigh > 0)
                return Encode(HandRank.StraightFlush, straightHigh);

            if (TryGetOfAKind(rankCounts, 4, out int quad))
                return Encode(HandRank.FourOfAKind, quad, orderedRanks);

            if (TryGetFullHouse(rankCounts, out int trips, out int pair))
                return Encode(HandRank.FullHouse, trips, pair);

            if (isFlush)
                return Encode(HandRank.Flush, orderedRanks);

            if (straightHigh > 0)
                return Encode(HandRank.Straight, straightHigh);

            if (TryGetOfAKind(rankCounts, 3, out trips))
                return Encode(HandRank.ThreeOfAKind, trips, orderedRanks);

            if (TryGetTwoPair(rankCounts, out int highPair, out int lowPair))
                return Encode(HandRank.TwoPair, highPair, lowPair, orderedRanks);

            if (TryGetOfAKind(rankCounts, 2, out int pairRank))
                return Encode(HandRank.Pair, pairRank, orderedRanks);

            return Encode(HandRank.HighCard, orderedRanks);
        }
        static int GetStraightHigh(Span<int> rankCounts) {
            if (rankCounts[12] > 0 && rankCounts[0] > 0 && rankCounts[1] > 0 && rankCounts[2] > 0 && rankCounts[3] > 0)
                return 5;

            for (int i = 8; i >= 0; i--)
                if (rankCounts[i] > 0 && rankCounts[i + 1] > 0 && rankCounts[i + 2] > 0 && rankCounts[i + 3] > 0 && rankCounts[i + 4] > 0)
                    return i + 6;

            return 0;
        }
        static int[] GetRanksDescending(Span<int> rankCounts) {
            int total = 0;
            for (int i = 0; i < 13; i++)
                total += rankCounts[i];

            var result = new int[total];

            int index = 0;

            for (int i = 12; i >= 0; i--)
                for (int count = 0; count < rankCounts[i]; count++)
                    result[index++] = i + 2;

            return result;
        }
        static bool TryGetOfAKind(Span<int> rankCounts, int countNeeded, out int rank) {
            for (int i = 12; i >= 0; i--) {
                if (rankCounts[i] == countNeeded) {
                    rank = i + 2;
                    return true;
                }
            }

            rank = 0;
            return false;
        }
        static bool TryGetFullHouse(Span<int> rankCounts, out int tripsRank, out int pairRank) {
            tripsRank = 0;
            pairRank = 0;

            for (int i = 12; i >= 0; i--) {
                if (rankCounts[i] == 3) {
                    tripsRank = i + 2;
                    break;
                }
            }

            if (tripsRank == 0)
                return false;

            for (int i = 12; i >= 0; i--) {
                if (rankCounts[i] >= 2 && (i + 2) != tripsRank) {
                    pairRank = i + 2;
                    return true;
                }
            }

            return false;
        }
        static bool TryGetTwoPair(Span<int> rankCounts, out int highPair, out int lowPair) {
            highPair = 0;
            lowPair = 0;

            for (int i = 12; i >= 0; i--) {
                if (rankCounts[i] == 2) {
                    if (highPair == 0)
                        highPair = i + 2;
                    else {
                        lowPair = i + 2;
                        return true;
                    }
                }
            }

            return false;
        }
        static HandValue Encode(HandRank rank, params int[] ranks) {
            ulong value = ((ulong)rank) << 24;

            int shift = 20;
            foreach (var r in ranks) {
                value |= ((ulong)r) << shift;
                shift -= 4;
            }

            return new HandValue(rank, value);
        }
        static HandValue Encode(HandRank rank, int primaryRank, int[] kickers) {
            var all = new int[1 + kickers.Length];
            all[0] = primaryRank;
            Array.Copy(kickers, 0, all, 1, kickers.Length);
            return Encode(rank, all);
        }
        static HandValue Encode(HandRank rank, int primaryRank, int secondaryRank, int[] kickers) {
            var all = new int[2 + kickers.Length];
            all[0] = primaryRank;
            all[1] = secondaryRank;
            Array.Copy(kickers, 0, all, 1, kickers.Length);
            return Encode(rank, all);
        }
        static bool HasStraight(Span<int> rankCounts) {
            if (rankCounts[12] > 0 &&
                rankCounts[0] > 0 &&
                rankCounts[1] > 0 &&
                rankCounts[2] > 0 &&
                rankCounts[3] > 0)
                return true;

            for (int i = 0; i <= 8; i++) {
                if (rankCounts[i] > 0 &&
                    rankCounts[i + 1] > 0 &&
                    rankCounts[i + 2] > 0 &&
                    rankCounts[i + 3] > 0 &&
                    rankCounts[i + 4] > 0)
                    return true;
            }

            return false;
        }
    }
}
