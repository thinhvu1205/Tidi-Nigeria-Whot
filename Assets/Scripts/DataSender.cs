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
    public const string GET_PROFILE = "get_profile";
    public const string USER_CHANGE_PASS = "user_change_pass";
    public const string LINK_USERNAME = "link_username";
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

        byte[] bytes = Convert.FromBase64String(response.Payload);
        Profile profile = Profile.Parser.ParseFrom(bytes);
        return profile;
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
