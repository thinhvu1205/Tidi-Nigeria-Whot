using System.Collections.Generic;
using Api;

namespace Globals
{
    public class Constants
    {
        public static readonly Dictionary<CardSuit, int> WhotSuitSortOrder = new()
        {
            { CardSuit.SuitCircle, 0 },
            { CardSuit.SuitTriangle, 1 },
            { CardSuit.SuitCross, 2 },
            { CardSuit.SuitStar, 3 },
            { CardSuit.SuitSquare, 4 },
            { CardSuit.SuitUnspecified, 5 }, // Whot
        };

        public static readonly List<CardRank> WhotDefensableCards = new()
        {
            CardRank.Rank2, // Pick 2 
            CardRank.Rank5, // Pick 3
        };
        public static readonly List<CardRank> WhotUndefensableCards = new()
        {
            CardRank.Rank1, // Hold On
            CardRank.Rank8, // Suspension
            CardRank.Rank14, // General Market
        };
    }
}