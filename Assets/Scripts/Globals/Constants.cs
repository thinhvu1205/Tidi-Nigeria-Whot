using System.Collections.Generic;
using Api;

namespace Globals
{
    public class Constants
    {
        public const string WhotGameID = "whot-game";
        public static readonly Dictionary<CardSuit, int> WhotSuitSortOrder = new()
        {
            { CardSuit.SuitCircle, 0 },
            { CardSuit.SuitTriangle, 1 },
            { CardSuit.SuitCross, 2 },
            { CardSuit.SuitStar, 3 },
            { CardSuit.SuitSquare, 4 },
            { CardSuit.SuitUnspecified, 5 }, // Whot
        };
    }
}