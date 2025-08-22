using System;
using System.Collections.Generic;
using System.Linq;
using Proto;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Nakama;
using Newtonsoft.Json;
using SimpleJSON;
using UnityEngine;

public class DataSender
{
    #region ApiNames
        public const string LIST_GAME = "list_game";
        public const string LIST_BET = "list_bet";
        public const string CREATE_MATCH = "create_match";
        public const string FIND_MATCH = "find_match";
        public const string QUICK_MATCH = "quick_match";
        public const string GET_PROFILE = "get_profile";
        public const string UPDATE_PROFILE = "update_profile";
        public const string UPDATE_AVATAR = "update_avatar";
        public const string LINK_USERNAME = "link_username";
        public const string CHANGE_PASS = "user_change_pass";
        public const string PUSH_TO_BANK = "push_to_bank";
        public const string WITH_DRAW = "with_draw";
        public const string SEND_GIFT = "send_gift";
        public const string WALLET_TRANSACTION = "wallet_transaction";
        public const string LIST_CLAIMABLE_FREECHIPS = "list_claimable_freechip";
        public const string CLAIM_FREECHIP = "claim_freechip";
        public const string LIST_DEAL = "list_deal";
        public const string GET_QUICKCHAT = "get_quickchat";
        public const string UPDATE_QUICKCHAT = "update_quickchat";
        public const string EXCHANGE_ADD = "exchange_add";
        public const string EXCHANGE_CANCEL = "exchange_cancel";
        public const string LIST_EXCHANGE_DEAL = "list_exchange_deal";
        public const string LIST_EXCHANGE = "list_exchange";
        public const string CAN_CLAIM_DAILY_REWARD = "canclaimdailyreward";
        public const string CLAIM_DAILY_REWARD = "claimdailyreward";
        public const string GIFT_CODE_CLAIM = "gift_code_claim";
        public const string LIST_IN_APP_MESSAGE = "list_in_app_message";
        public const string LIST_NOTIFICATION = "list_notification";
        public const string READ_NOTIFICATION = "read_notification";
        public const string DELETE_NOTIFICATION = "delete_notification";
        public const string READ_ALL_NOTIFICATION = "read_all_notification";
        public const string DELETE_ALL_NOTIFICATION = "delete_all_notification";
        public const string LEADER_BOARD_INFO = "leaderboard_info";
        public const string GET_JACKPOT = "jackpot";
        public const string INFO_MATCH = "info_match";
    #endregion

    
    #region ConvertProtobuf
    private static T DecodeFromBase64<T>(string base64) where T : IMessage<T>, new()
    {
        byte[] data = Convert.FromBase64String(base64);
        var parser = new MessageParser<T>(() => new T());
        return parser.ParseFrom(data);
    }

    private static T DecodeFromJson<T>(string json) where T : IMessage<T>, new()
    {
        var parser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
        return parser.Parse<T>(json);
    }
    
    #endregion
    
    #region RPC

    #region Login
    // public static async UniTask LoginAsGuest()
    // {
    //     await NetworkManager.INSTANCE.LoginAsync();
    // }
    //
    // public static async UniTask LoginWithAccount(string username, string password)
    // {
    //     await NetworkManager.INSTANCE.LoginAsync(username, password);
    // }
    //
    // public static async UniTask Logout()
    // {
    //     await NetworkManager.INSTANCE.LogoutAsync();
    // }
    #endregion

    public static async UniTask<Profile> GetProfile()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(GET_PROFILE);
        return DecodeFromJson<Profile>(response.Payload);
    }

    public static async UniTask<GameListResponse> GetListGame()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(LIST_GAME);
        return DecodeFromJson<GameListResponse>(response.Payload);
    }

    public static async UniTask<ListNotification> GetListNotification()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(LIST_NOTIFICATION);
        return DecodeFromJson<ListNotification>(response.Payload);
    }

    public static void ChangePassword(string oldPassword = "", string password = "")
    {
        ChangePasswordRequest data = new()
        {
            OldPassword = oldPassword,
            Password = password
        };
        _ = NetworkManager.INSTANCE.RPCSend(CHANGE_PASS, data);
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
        var response = await NetworkManager.INSTANCE.RPCSend(LIST_BET, betListRequest);
        return DecodeFromJson<Bets>(response.Payload);
    }
    
    // public static async UniTask<PlayerCountByBetResponse> GetPlayerCountByBet(string gameCode)
    // {
    //     BetListRequest betListRequest = new(){Code = gameCode};
    //     var response = await NetworkManager.INSTANCE.RPCSend(GET_PLAYER_COUNT_BY_BET, betListRequest);
    //     return DecodeFromJson<PlayerCountByBetResponse>(response.Payload);
    // }
    
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

            return DecodeFromJson<RpcFindMatchResponse>(response.Payload);
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
            return DecodeFromJson<RpcCreateMatchResponse>(response.Payload);
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
            return DecodeFromJson<RpcFindMatchResponse>(response.Payload);
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

    #region PopupView

    public static async UniTask<DealInShop> GetListDeal()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(LIST_DEAL);
        return DecodeFromJson<DealInShop>(response.Payload);
    }

    public static async UniTask<Bank> PushToBank(long amountChip = 0)
    {
        Bank bank = new Bank() { ChipsInBank = amountChip };
        var response = await NetworkManager.INSTANCE.RPCSend(PUSH_TO_BANK, bank);
        return DecodeFromJson<Bank>(response.Payload);
    }
    
    public static async UniTask<Bank> WithDraw(long amountChip = 0)
    {
        Bank bank = new Bank() { Chips = amountChip };
        var response = await NetworkManager.INSTANCE.RPCSend(WITH_DRAW, bank);
        return DecodeFromJson<Bank>(response.Payload);
    }
    
    public static async UniTask<FreeChip> SendGift(long amountChip = 0, string recipientId = "" )
    {
        Bank bank = new Bank() { ChipsInBank = amountChip, RecipientId = recipientId};
        var response = await NetworkManager.INSTANCE.RPCSend(SEND_GIFT, bank);
        return DecodeFromJson<FreeChip>(response.Payload);
    }
    
    // public class WalletTransaction
    // {
    //     [JsonProperty("transactions")]
    //     public List<IApiWalletLedgerList> Transactions { get; set; }
    //     [JsonProperty("cusor")]
    //     public string Cusor { get; set; }
    // }

    
    public static async UniTask<WalletTransRequest> LoadTransactionHistory(long limit = 0, string metaAction = "bank_topup" )
    {
       
        WalletTransRequest bank = new WalletTransRequest() { Limit = limit, MetaAction = metaAction };
        var response = await NetworkManager.INSTANCE.RPCSend(SEND_GIFT, bank);
        return DecodeFromJson<WalletTransRequest>(response.Payload);
    }
        

    #endregion
}
