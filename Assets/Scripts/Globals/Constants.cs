using System.Collections.Generic;
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
        // public const string BLACKJACK_GAME_ID = "blackjack";
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
    }
}