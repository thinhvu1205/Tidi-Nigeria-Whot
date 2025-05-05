using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Nakama;
using SimpleJSON;
using UnityEngine;

public class DataSender
{
    #region ApiNames
    public const string get_world_with_areas = "get_world_with_areas";
    #endregion

    #region RPC
    public static void GetWordWithAreas(int idArea)
    {
        JSONObject data = new()
        {
            ["id"] = idArea
        };
        _ = NetworkManager.INSTANCE.RPCSend(get_world_with_areas, data);
    }
    #endregion

    #region MatchState
    public static void JoinMatch(string matchId) => NetworkManager.INSTANCE.JoinMatch(matchId);
    public static void LeaveMatch() => NetworkManager.INSTANCE.LeaveMatch();
    public static void ExampleSendMatchState(long opCode, string data)
    {
        NetworkManager.INSTANCE.SendMatchState(opCode, data);
    }
    #endregion

    #region Friends
    public static void GetListFriends(int state = 0, int limit = 100, string cursor = "", Action<IApiFriendList> handleCB = null)
    {
        NetworkManager.INSTANCE.GetListFriends(state, limit, cursor, handleCB);
    }
    public static void FindFriendsWithIds(List<string> ids = null, List<string> names = null, Action<IApiUsers> handleCb = null)
    {
        NetworkManager.INSTANCE.GetUsersWithIds(ids, names, handleCb);
    }
    public static void SendFriendRequestToId(string userId, Action handleCb = null)
    {
        NetworkManager.INSTANCE.AddFriend(userId, handleCb);
    }
    #endregion
}
