using System.Collections.Generic;
using System.Linq;
using Games.Card;

/// <summary>
/// Kiểm tra phỏm giống server <c>tongits/group.go</c> (rank proto 1–13, suit 0–3).
/// </summary>
public static class TongitsMeldValidator
{
    public static int CardRankForCompare(int protoRank1To13) => protoRank1To13 == 1 ? 1 : protoRank1To13;

    public static bool IsGroup(IReadOnlyList<CardModel> cards)
    {
        if (cards == null || cards.Count < 3)
            return false;
        return IsSameNumberMeld(cards) || IsFlushStraightMeld(cards);
    }

    public static bool IsSameNumberMeld(IReadOnlyList<CardModel> cards)
    {
        if (cards == null || cards.Count < 3)
            return false;
        int r = CardRankForCompare(cards[0].data.rank);
        for (int i = 1; i < cards.Count; i++)
        {
            if (CardRankForCompare(cards[i].data.rank) != r)
                return false;
        }
        return true;
    }

    public static bool IsFlushStraightMeld(IReadOnlyList<CardModel> cards)
    {
        if (cards == null || cards.Count < 3)
            return false;
        int suit = cards[0].data.suit;
        for (int i = 1; i < cards.Count; i++)
        {
            if (cards[i].data.suit != suit)
                return false;
        }

        var ranks = cards.Select(c => CardRankForCompare(c.data.rank)).OrderBy(x => x).ToArray();
        bool consecutive = true;
        for (int i = 1; i < ranks.Length; i++)
        {
            if (ranks[i] - ranks[i - 1] != 1)
            {
                consecutive = false;
                break;
            }
        }
        if (consecutive)
            return true;
        // Q-K-A: compare ranks 12,13,1 → sorted 1,12,13
        return ranks.Length == 3 && ranks[0] == 1 && ranks[1] == 12 && ranks[2] == 13;
    }
}
