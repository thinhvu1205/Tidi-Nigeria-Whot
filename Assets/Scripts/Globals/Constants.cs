using System.Collections.Generic;
using System.Linq;
using Proto;

namespace Globals
{
    public class Constants
    {
        public const string WHOT_GAME_ID = "whot-game";
        // public const string COLOR_GAME_ID = "color-game";
        public const string ROULETTE_GAME_ID = "roulette";
        public const string FRUIT_SLOT_GAME_ID = "fruit";
        // public const string SABONG_CARDS_GAME_ID = "sabong-cards";
        public const string CHINESE_POKER_GAME_ID = "chinese-poker";
        public const string BACCARAT_GAME_ID = "baccarat";
        // public const string LUCKY_NUMBER_GAME_ID = "lucky-number";
        public const string SIXIANG_GAME_ID = "sixiang";
        public const string TARZAN_GAME_ID = "tarzan";
        public const string JUICY_GARDEN_GAME_ID = "juicygarden";
        public const string BLACKJACK_GAME_ID = "blackjack";
        // public const string BANDARQQ_GAME_ID = "bandarqq";
        // public const string SICBO_GAME_ID = "sicbo";
        // public const string DRAGONTIGER_GAME_ID = "dragontiger";
        public const string INCA_GAME_ID = "inca";
        public const string NOEL_GAME_ID = "noel";
        // public const string FRUIT_GAME_ID = "fruit";
        // public const string GAPLE_GAME_ID = "gaple";

        public static readonly string[] SLOT_GAMES_ID = new string[]
        {
            FRUIT_SLOT_GAME_ID,
            INCA_GAME_ID,
            JUICY_GARDEN_GAME_ID,
            NOEL_GAME_ID,
            TARZAN_GAME_ID,
            SIXIANG_GAME_ID,
        };

        public static readonly string[] INGAME_RULES_ID = new string[]
        {
            FRUIT_SLOT_GAME_ID,
            INCA_GAME_ID,
            JUICY_GARDEN_GAME_ID,
            NOEL_GAME_ID,
            TARZAN_GAME_ID,
            SIXIANG_GAME_ID,
        };

        public static readonly string[] SELECT_TABLE_GAMES_ID = new string[]
        {
            WHOT_GAME_ID,
            BACCARAT_GAME_ID,
            CHINESE_POKER_GAME_ID
        };

        public static readonly Dictionary<WhotCardSuit, int> WhotSuitSortOrder = new()
        {
            { WhotCardSuit.WhotSuitCircle, 0 },
            { WhotCardSuit.WhotSuitTriangle, 1 },
            { WhotCardSuit.WhotSuitCross, 2 },
            { WhotCardSuit.WhotSuitStar, 3 },
            { WhotCardSuit.WhotSuitSquare, 4 },
            { WhotCardSuit.WhotSuitUnspecified, 5 }, // Whot
        };

        public static readonly Dictionary<int, int[]> HongKongPokerNumberDictionary = new()
        {
            { 0, new[] { 0 } },
            { 1, new[] { 1 } },
            { 2, new[] { 2 } },
            { 3, new[] { 3 } },
            { 4, new[] { 4 } },
            { 5, new[] { 5 } },
            { 6, new[] { 6 } },
            { 7, new[] { 7 } },
            { 8, new[] { 8 } },
            { 9, new[] { 9 } },
            { 10, new[] { 10 } },
            { 11, new[] { 11 } },
            { 12, new[] { 12 } },
            { 13, new[] { 13 } },
            { 14, new[] { 14 } },
            { 15, new[] { 15 } },
            { 16, new[] { 16 } },
            { 17, new[] { 17 } },
            { 18, new[] { 18 } },
            { 19, new[] { 19 } },
            { 20, new[] { 20 } },
            { 21, new[] { 21 } },
            { 22, new[] { 22 } },
            { 23, new[] { 23 } },
            { 24, new[] { 24 } },
            { 25, new[] { 25 } },
            { 26, new[] { 26 } },
            { 27, new[] { 27 } },
            { 28, new[] { 28 } },
            { 29, new[] { 29 } },
            { 30, new[] { 30 } },
            { 31, new[] { 31 } },
            { 32, new[] { 32 } },
            { 33, new[] { 33 } },
            { 34, new[] { 34 } },
            { 35, new[] { 35 } },
            { 36, new[] { 36 } },
            { 37, new[] { 1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34 } },
            { 38, new[] { 2, 5, 8, 11, 14, 17, 20, 23, 26, 29, 32, 35 } },
            { 39, new[] { 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36 } },
            { 40, Enumerable.Range(1, 12).ToArray() },
            { 41, Enumerable.Range(13, 12).ToArray() },
            { 42, Enumerable.Range(25, 12).ToArray() },
            { 43, Enumerable.Range(1, 18).ToArray() },
            { 44, Enumerable.Range(1, 36).Where(n => n % 2 == 0).ToArray() },
            { 45, new[] { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 } },
            { 46, new[] { 2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35 } },
            { 47, Enumerable.Range(1, 36).Where(n => n % 2 != 0).ToArray() },
            { 48, Enumerable.Range(19, 18).ToArray() },
            { 49, new[] { 0, 3 } },
            { 50, new[] { 3, 6 } },
            { 51, new[] { 6, 9 } },
            { 52, new[] { 9, 12 } },
            { 53, new[] { 12, 15 } },
            { 54, new[] { 15, 18 } },
            { 55, new[] { 18, 21 } },
            { 56, new[] { 21, 24 } },
            { 57, new[] { 24, 27 } },
            { 58, new[] { 27, 30 } },
            { 59, new[] { 30, 33 } },
            { 60, new[] { 33, 36 } },
            { 61, new[] { 0, 2 } },
            { 62, new[] { 2, 5 } },
            { 63, new[] { 5, 8 } },
            { 64, new[] { 8, 11 } },
            { 65, new[] { 11, 14 } },
            { 66, new[] { 14, 17 } },
            { 67, new[] { 17, 20 } },
            { 68, new[] { 20, 23 } },
            { 69, new[] { 23, 26 } },
            { 70, new[] { 26, 29 } },
            { 71, new[] { 29, 32 } },
            { 72, new[] { 32, 35 } },
            { 73, new[] { 0, 1 } },
            { 74, new[] { 1, 4 } },
            { 75, new[] { 4, 7 } },
            { 76, new[] { 7, 10 } },
            { 77, new[] { 10, 13 } },
            { 78, new[] { 13, 16 } },
            { 79, new[] { 16, 19 } },
            { 80, new[] { 19, 22 } },
            { 81, new[] { 22, 25 } },
            { 82, new[] { 25, 28 } },
            { 83, new[] { 28, 31 } },
            { 84, new[] { 31, 34 } },
            { 85, new[] { 0, 2, 3 } },
            { 86, new[] { 2, 3, 5, 6 } },
            { 87, new[] { 5, 6, 8, 9 } },
            { 88, new[] { 8, 9, 11, 12 } },
            { 89, new[] { 11, 12, 14, 15 } },
            { 90, new[] { 14, 15, 17, 18 } },
            { 91, new[] { 17, 18, 20, 21 } },
            { 92, new[] { 20, 21, 23, 24 } },
            { 93, new[] { 23, 24, 26, 27 } },
            { 94, new[] { 26, 27, 29, 30 } },
            { 95, new[] { 29, 30, 32, 33 } },
            { 96, new[] { 32, 33, 35, 36 } },
            { 97, new[] { 0, 1, 2 } },
            { 98, new[] { 1, 2, 4, 5 } },
            { 99, new[] { 4, 5, 7, 8 } },
            { 100, new[] { 7, 8, 10, 11 } },
            { 101, new[] { 10, 11, 13, 14 } },
            { 102, new[] { 13, 14, 16, 17 } },
            { 103, new[] { 16, 17, 19, 20 } },
            { 104, new[] { 19, 20, 22, 23 } },
            { 105, new[] { 22, 23, 25, 26 } },
            { 106, new[] { 25, 26, 28, 29 } },
            { 107, new[] { 28, 29, 31, 32 } },
            { 108, new[] { 31, 32, 34, 35 } },
            { 109, new[] { 0, 1, 2, 3 } },
            { 110, new[] { 1, 2, 3, 4, 5, 6 } },
            { 111, new[] { 4, 5, 6, 7, 8, 9 } },
            { 112, new[] { 7, 8, 9, 10, 11, 12 } },
            { 113, new[] { 10, 11, 12, 13, 14, 15 } },
            { 114, new[] { 13, 14, 15, 16, 17, 18 } },
            { 115, new[] { 16, 17, 18, 19, 20, 21 } },
            { 116, new[] { 19, 20, 21, 22, 23, 24 } },
            { 117, new[] { 22, 23, 24, 25, 26, 27 } },
            { 118, new[] { 25, 26, 27, 28, 29, 30 } },
            { 119, new[] { 28, 29, 30, 31, 32, 33 } },
            { 120, new[] { 31, 32, 33, 34, 35, 36 } },
            { 121, new[] { 2, 3 } },
            { 122, new[] { 1, 2 } },
            { 123, new[] { 1, 2, 3 } },
            { 124, new[] { 4, 5, 6 } },
            { 125, new[] { 7, 8, 9 } },
            { 126, new[] { 10, 11, 12 } },
            { 127, new[] { 13, 14, 15 } },
            { 128, new[] { 16, 17, 18 } },
            { 129, new[] { 19, 20, 21 } },
            { 130, new[] { 22, 23, 24 } },
            { 131, new[] { 25, 26, 27 } },
            { 132, new[] { 28, 29, 30 } },
            { 133, new[] { 31, 32, 33 } },
            { 134, new[] { 34, 35, 36 } },
            { 135, new[] { 4, 5 } },
            { 136, new[] { 7, 8 } },
            { 137, new[] { 10, 11 } },
            { 138, new[] { 13, 14 } },
            { 139, new[] { 16, 17 } },
            { 140, new[] { 19, 20 } },
            { 141, new[] { 22, 23 } },
            { 142, new[] { 25, 26 } },
            { 143, new[] { 28, 29 } },
            { 144, new[] { 31, 32 } },
            { 145, new[] { 34, 35 } },
            { 146, new[] { 5, 6 } },
            { 147, new[] { 8, 9 } },
            { 148, new[] { 11, 12 } },
            { 149, new[] { 14, 15 } },
            { 150, new[] { 17, 18 } },
            { 151, new[] { 20, 21 } },
            { 152, new[] { 23, 24 } },
            { 153, new[] { 26, 27 } },
            { 154, new[] { 29, 30 } },
            { 155, new[] { 32, 33 } },
            { 156, new[] { 35, 36 } }
        };
    }
}