using System;
using System.Collections.Generic;
using System.Linq;
using Api;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Nakama;
using Newtonsoft.Json;
using SimpleJSON;
using UnityEngine;

public class DataSender
{
    #region ApiNames
    public const string GET_PROFILE = "get_profile";
    public const string GET_LIST_GAME = "list_game";
    public const string GET_LIST_NOTIFICATION = "list_notification";
    public const string GET_LIST_BET = "list_bet";
    public const string GET_PLAYER_COUNT_BY_BET = "get_player_count_by_bet";
    public const string FIND_MATCH = "find_match";
    public const string QUICK_MATCH = "quick_match";
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

    public static async UniTask<GameListResponse> GetListGame()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(GET_LIST_GAME);
        return DecodeFromBase64<GameListResponse>(response.Payload);
    }

    public static async UniTask<ListNotification> GetListNotification()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(GET_LIST_NOTIFICATION);
        return DecodeFromBase64<ListNotification>(response.Payload);
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
    
    public static async UniTask<RpcFindMatchResponse> FindMatch(string gameCode, int markUnit, bool isCreateGame)
    {
        try
        {
            RpcFindMatchRequest rpcFindMatchRequest = new()
            {
                GameCode = gameCode,
                MarkUnit = markUnit,
                Create = isCreateGame,
                WithNonOpen = false
            };

            var response = await NetworkManager.INSTANCE.RPCSend(FIND_MATCH, rpcFindMatchRequest);
            
            if (string.IsNullOrEmpty(response.Payload) || response.Payload == "[]")
            {
                Debug.Log("FindMatch response payload is empty");
                return null;
            }

            return DecodeFromBase64<RpcFindMatchResponse>(response.Payload);
        }
        catch (Exception ex)
        {
            Debug.LogError("FindMatch failed: " + ex.Message);
            UIManager.Instance.OpenDialog("FindMatch failed : " + ex.Message, null, null);
            return null;
        }
    }


    // public static void MakingMatch(string gameCode)
    // {
    //     NetworkManager.INSTANCE.MakingMatch(gameCode);
    // }

    public static async UniTask<RpcCreateMatchResponse> CreateMatch(string gameCode, string passWord, int markUnit, string customData)
    {
        try
        {
            RpcCreateMatchRequest rpcCreateMatchRequest = new() { GameCode = gameCode, Password = passWord, MarkUnit = markUnit, CustomData = customData };
            Debug.Log("CreateMatch with data : " + rpcCreateMatchRequest.ToString());
            var response = await NetworkManager.INSTANCE.RPCSend(CREATE_MATCH, rpcCreateMatchRequest);
            if (string.IsNullOrEmpty(response.Payload) || response.Payload == "[]")
            {
                Debug.Log("CreateMatch response payload is empty");
                return null;
            }
            return DecodeFromBase64<RpcCreateMatchResponse>(response.Payload);
        }
        catch (Exception ex)
        {
            Debug.LogError("CreateMatch failed : " + ex.Message);
            UIManager.Instance.OpenDialog("CreateMatch failed : " + ex.Message, null, null);
            return null;
        }
    }

    public static async UniTask<Match> JoinMatch(string matchId)
    {
        try
        {
            var match = await NetworkManager.INSTANCE.JoinMatch(matchId);
            Match data = JsonConvert.DeserializeObject<Match>(match.Label);
            return data;
        }
        catch (Exception ex)
        {
            Debug.LogError("JoinMatch failed: " + ex.Message);
            UIManager.Instance.OpenDialog("Lỗi khi vào trận : " + ex.Message, null, null);
            return null;
        }
    }
    
    public static async UniTask<RpcFindMatchResponse> QuickMatch(string gameCode)
    {
        try
        {
            RpcCreateMatchRequest rpcFindMatchRequest = new() { GameCode = gameCode};
            var response = await NetworkManager.INSTANCE.RPCSend(QUICK_MATCH, rpcFindMatchRequest);
            Debug.Log("QuickMatch response: " + response.Payload);
            if (string.IsNullOrEmpty(response.Payload) || response.Payload == "[]")
            {
                Debug.Log("QuickMatch response payload is empty");
                return null;
            }
            return DecodeFromBase64<RpcFindMatchResponse>(response.Payload);
        }
        catch (Exception ex)
        {
            Debug.LogError("QuickMatch failed: " + ex.Message);
            UIManager.Instance.OpenDialog("Lỗi khi vào trận : " + ex.Message, null, null);
            return null;
        }
    }
    
    public static void LeaveMatch() => NetworkManager.INSTANCE.LeaveMatch();
    
    public static void SendMatchState(long opCode, byte[] data)
    {
        Debug.Log("SendMatchState opCode: " + opCode + ", data: " + BitConverter.ToString(data));
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
