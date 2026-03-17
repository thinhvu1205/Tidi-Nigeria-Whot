using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Yuujins.Cfg.Game.V1;
using Yuujins.Cfg.Layout.V1;
using Yuujins.User.V1;

namespace Globals
{
    /// <summary>
    /// Config toàn cục từ server Yuujins. Lưu account, layout (tabs + tiles), danh sách game (để lấy tên theo game_id).
    /// Client gọi GetLayout + ListGames, lưu tại đây; UI dựa vào Layout để hiển thị, dùng GameList để resolve tên game.
    /// </summary>
    public static class ServerConfig
    {
        /// <summary>Account từ identity_user_get_account.</summary>
        public static UserAccount Account { get; set; }

        /// <summary>Layout từ lobby_match_get_layout / cfg_layout_read (tabs + tiles). Dùng để hiển thị thứ tự / tab game.</summary>
        public static Layout Layout { get; set; }

        /// <summary>Danh sách game từ match_list_games / cfg_game_list. Lưu local để lấy name theo game_id (khi hiển thị theo Layout).</summary>
        public static List<Game> GameList { get; set; }

        public static Dictionary<uint, Game> GameMap;

        public static async UniTask LoadInfoLayoutGame()
        {
            var dMatchListGamesAsync = await DataSender.MatchListGamesAsync();
            var dMatchGetLayoutAsync = await DataSender.MatchGetLayoutAsync(Application.identifier);
            GameList = dMatchListGamesAsync.Games.ToList();
            Layout = dMatchGetLayoutAsync.Layout;
            GameMap = GameList.ToDictionary(g => g.Id, g => g);
        }
    }
}
