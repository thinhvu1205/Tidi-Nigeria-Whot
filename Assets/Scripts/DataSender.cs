using System;
using System.Collections.Generic;
using System.Linq;
using Api;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Nakama;
using SimpleJSON;
using UnityEngine;

public class DataSender
{
    #region ApiNames
    public const string get_world_with_areas = "get_world_with_areas";
    public const string GET_LIST_BET = "list_bet";
    public const string GET_PLAYER_COUNT_BY_BET = "get_player_count_by_bet";
    public const string FIND_MATCH = "find_match";
    public const string GET_PROFILE = "get_profile";
    public const string USER_CHANGE_PASS = "user_change_pass";
    public const string LINK_USERNAME = "link_username";
    public const string CREATE_MATCH = "create_match";
    #endregion
    
    #region ConvertProtobuf
    private static T DecodeFromBase64<T>(string base64) where T : IMessage<T>, new()
    {
        byte[] data = Convert.FromBase64String(base64);
        var parser = new MessageParser<T>(() => new T());
        return parser.ParseFrom(data);
    }
    #endregion
    
    #region RPC
    public static void GetWordWithAreas(int idArea)
    {
        JSONObject data = new()
        {
            ["id"] = idArea
        };
        _ = NetworkManager.INSTANCE.RPCSend(get_world_with_areas);
    }

    #region Login
    public static async UniTask LoginAsGuest()
    {
        await NetworkManager.INSTANCE.LoginAsync();
    }

    public static async UniTask LoginWithAccount(string username, string password)
    {
        await NetworkManager.INSTANCE.LoginAsync(username, password);
    }

    public static async UniTask Logout()
    {
        await NetworkManager.INSTANCE.LogoutAsync();
    }
    #endregion

    public static async UniTask<Profile> GetProfile()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(GET_PROFILE);
        return DecodeFromBase64<Profile>(response.Payload);
    }

    public static void ChangePassword(string oldPassword = "", string password = "")
    {
        ChangePasswordRequest data = new()
        {
            OldPassword = oldPassword,
            Password = password
        };
        _ = NetworkManager.INSTANCE.RPCSend(USER_CHANGE_PASS, data);
    }

    public static void LinkUsername(string username = "", string password = "")
    {
        RegisterRequest data = new()
        {
            UserName = username,
            Password = password
        };
        _ = NetworkManager.INSTANCE.RPCSend(LINK_USERNAME, data);
    }
    #endregion

    public static async UniTask<Bets> GetListBet(string gameCode)
    {
        BetListRequest betListRequest = new(){Code = gameCode};
        var response = await NetworkManager.INSTANCE.RPCSend(GET_LIST_BET, betListRequest);
        return DecodeFromBase64<Bets>(response.Payload);
    }
    
    public static async UniTask<PlayerCountByBetResponse> GetPlayerCountByBet(string gameCode)
    {
        BetListRequest betListRequest = new(){Code = gameCode};
        var response = await NetworkManager.INSTANCE.RPCSend(GET_PLAYER_COUNT_BY_BET, betListRequest);
        return DecodeFromBase64<PlayerCountByBetResponse>(response.Payload);
    }
    #region Match


    public static async UniTask<RpcFindMatchResponse> FindMatch(string gameCode, int markUnit)
    {
        RpcFindMatchRequest rpcFindMatchRequest = new() { GameCode = gameCode, MarkUnit = markUnit, Create = true };
        var response = await NetworkManager.INSTANCE.RPCSend(FIND_MATCH, rpcFindMatchRequest);
        Debug.Log("FindMatch response: " + response.Payload);
        return DecodeFromBase64<RpcFindMatchResponse>(response.Payload);
    }

    public static void MakingMatch(string gameCode)
    {
        NetworkManager.INSTANCE.MakingMatch(gameCode);
    }

    public static async UniTask<RpcCreateMatchResponse> CreateMatch(string gameCode)
    {
         // NetworkManager.INSTANCE.CreateMatch(gameCode);
         RpcCreateMatchRequest rpcCreateMatchRequest = new() { GameCode = gameCode, MaxSize = 4, Name = "assassin", MarkUnit = 0};
         var response = await NetworkManager.INSTANCE.RPCSend(CREATE_MATCH, rpcCreateMatchRequest);
         return DecodeFromBase64<RpcCreateMatchResponse>(response.Payload);
    }
    public static void JoinMatch(string matchId) => NetworkManager.INSTANCE.JoinMatch(matchId);
    public static void LeaveMatch() => NetworkManager.INSTANCE.LeaveMatch();
    public static void SendMatchState(long opCode, byte[] data)
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
