using System;
using System.Collections.Generic;
using System.Linq;
using Proto;
using Cysharp.Threading.Tasks;
using Globals;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Nakama;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
// Server Yuujins (namespace Yuujins.* — không trùng Proto)
using Yuujins.User.V1;
using Yuujins.Match.V1;
using Yuujins.Cfg.Game.V1;
using Yuujins.Cfg.Banner.V1;
using Yuujins.Cfg.Bet.V1;
using Yuujins.Rank.V1;
using Yuujins.Rewards.V1;
using Yuujins.Friend.V1;
using Yuujins.Common.V1;
using Yuujins.Communication.Mail.V1;
using Yuujins.Economy.Wallet.V1;
using Yuujins.Promotion.Coupon.V1;
using Config = Globals.Config;
using Game = Yuujins.Cfg.Game.V1.Game;
using Profile = Proto.Profile;
using User = Yuujins.User.V1.User;

/// <summary>
/// API layer: các region dùng ApiNames (LIST_GAME, GET_PROFILE, …) = server <b>Nigeria cũ</b> (nakama-nigeria).
/// Các region <b>Yuujins</b> (Identity*, Match*, Cfg*, Rank*, Rewards*, Social*, Mail*, Bank*, Coupon*) = server <b>yuujins</b>. Chọn API theo backend đang kết nối.
/// </summary>
public class DataSender
{
    #region ApiNames (server Nigeria cũ – giữ nguyên)
    public const string LIST_GAME = "list_game";
    public const string LIST_BET = "list_bet";
    public const string CREATE_MATCH = "create_match";
    public const string FIND_MATCH = "find_match";
    public const string QUICK_MATCH = "quick_match";
    public const string GET_PROFILE = "get_profile";
    public const string UPDATE_PROFILE = "update_profile";
    public const string UPDATE_AVATAR = "update_avatar";
    public const string PRE_SIGN_PUT = "pre_sign_put";
    public const string LINK_USERNAME = "link_username";
    public const string CHANGE_PASS = "user_change_pass";
    public const string DELETE_ACCOUNT = "delete_account";
    public const string PUSH_TO_BANK = "push_to_bank";
    public const string WITH_DRAW = "with_draw";
    public const string SEND_GIFT = "send_gift";
    public const string WALLET_TRANSACTION = "wallet_transaction";
    public const string LIST_CLAIMABLE_FREECHIPS = "list_claimable_freechip";
    public const string CLAIM_FREECHIP = "claim_freechip";
    public const string SUBMIT_FEEDBACK = "submit_feedback";
    public const string LIST_DEAL = "list_deal";
    public const string GET_QUICKCHAT = "get_quickchat";
    public const string UPDATE_QUICKCHAT = "update_quickchat";
    public const string EXCHANGE_ADD = "exchange_add";
    public const string EXCHANGE_CANCEL = "exchange_cancel";
    public const string LIST_EXCHANGE_DEAL = "list_exchange_deal";
    public const string LIST_EXCHANGE = "list_exchange";
    public const string DAILY_REWARD_TEMPLATE = "dailyrewardtemplate";
    public const string CAN_CLAIM_DAILY_REWARD = "canclaimdailyreward";
    public const string CLAIM_DAILY_REWARD = "claimdailyreward";
    public const string WEEKLY_REWARD_TEMPLATE = "weekly_bonus_template";
    public const string CAN_CLAIM_WEEKLY_REWARD = "can_claim_weekly_bonus";
    public const string CLAIM_WEEKLY_REWARD = "claim_weekly_bonus";
    public const string GIFT_CODE_CLAIM = "gift_code_claim";
    public const string LIST_IN_APP_MESSAGE = "list_in_app_message";
    public const string LIST_NOTIFICATION = "list_notification";
    public const string READ_NOTIFICATION = "read_notification";
    public const string DELETE_NOTIFICATION = "delete_notification";
    public const string READ_ALL_NOTIFICATION = "read_all_notification";
    public const string DELETE_ALL_NOTIFICATION = "delete_all_notification";
    public const string LEADERBOARD_INFO = "leaderboard_info";
    public const string GET_JACKPOT = "jackpot";
    public const string INFO_MATCH = "info_match";
    public const string BUY_LOTTERY_TICKET = "buy_lottery_ticket";
    public const string BUY_MULTIPLE_LOTTERY_TICKETS = "buy_multiple_lottery_tickets";
    public const string GET_LOTTERY_HISTORY = "get_lottery_history";
    public const string GET_AVAILABLE_DRAWS = "get_available_draws";
    public const string GET_LATEST_DRAW_RESULT = "get_latest_draw_result";
    public const string QUICK_PICK = "quick_pick";
    public const string TRIGGER_DRAW = "trigger_draw";
    public const string VIP_FARM_PROGRESS = "vip-farm-progress";
    public const string VIP_FARM_CLAIM = "vip-farm-claim";
    public const string LIST_FRIEND = "friend_list";
    public const string FRIEND_TAB_COUNTS = "friend_tab_counts";
    public const string SEND_GIFT_FRIEND = "friend_send_gift";
    public const string INVITE_FRIEND = "friend_invite";
    public const string ACCEPT_FRIEND = "friend_accept";
    public const string REJECT_FRIEND = "friend_reject";
    public const string LIST_RECENT_CONVERSATIONS = "list_recent_conversations";
    public const string FRIEND_LIST_UPGRADE_CANDIDATES = "friend_list_upgrade_candidates";
    public const string FRIEND_REQUEST_TIER_UPGRADE = "friend_request_tier_upgrade";
    public const string FRIEND_ACCEPT_TIER_UPGRADE = "friend_accept_tier_upgrade";
    public const string FRIEND_REFUSE_TIER_UPGRADE = "friend_refuse_tier_upgrade";
    public const string FRIEND_DOWNGRADE = "friend_downgrade";
    #endregion

    #region RPC IDs – Yuujins (server yuujins)
    public const string IDENTITY_USER_REGISTER = "identity_user_register";
    public const string IDENTITY_USER_LOGIN = "identity_user_login";
    public const string IDENTITY_USER_CHANGE_PASSWORD = "identity_user_change_password";
    public const string IDENTITY_USER_GET_ACCOUNT = "identity_user_get_account";
    public const string CFG_BANNER_LIST = "cfg_banner_list";
    public const string CFG_BET_READ = "cfg_bet_read";
    public const string CFG_GAME_LIST = "cfg_game_list";
    public const string MATCH_LIST_GAMES = "match_list_games";
    public const string MATCH_LIST_BET_LEVELS = "match_list_bet_levels";
    public const string MATCH_JOIN_GAME = "match_join_game";
    public const string MATCH_GET_MATCH_INFO = "match_get_match_info";
    public const string MATCH_FIND_MATCH = "match_find_match";
    public const string MATCH_ENTER_SLOT = "match_enter_slot";
    public const string MATCH_CREATE_PRIVATE_TABLE = "match_create_private_table";
    public const string MATCH_JOIN_PRIVATE_TABLE = "match_join_private_table";
    public const string MAIL_LIST = "mail_list";
    public const string MAIL_MARK_AS_READ = "mail_mark_as_read";
    public const string MAIL_MARK_AS_DELETED = "mail_mark_as_deleted";
    public const string MAIL_REDEEM = "mail_redeem";
    public const string BANK_DEPOSIT = "bank_deposit";
    public const string BANK_WITHDRAW = "bank_withdraw";
    public const string COUPON_LIST = "coupon_list";
    public const string RANK_RANK_LIST_TOP_BY_GAME = "rank_rank_list_top_by_game";
    public const string REWARDS_CHECKIN_GET_CHECKIN_CONFIG = "rewards_checkin_get_checkin_config";
    public const string REWARDS_CHECKIN_GET_CHECKIN_STATE = "rewards_checkin_get_checkin_state";
    public const string REWARDS_CHECKIN_CLAIM_CHECKIN = "rewards_checkin_claim_checkin";
    public const string SOCIAL_FRIEND_LIST = "social_friend_list";
    public const string SOCIAL_FRIEND_LIST_FRIEND_REQUESTS_RECEIVED = "social_friend_list_friend_requests_received";
    public const string SOCIAL_FRIEND_LIST_FRIEND_REQUESTS_SENT = "social_friend_list_friend_requests_sent";
    public const string SOCIAL_FRIEND_LIST_UPGRADE_INVITES_RECEIVED = "social_friend_list_upgrade_invites_received";
    public const string SOCIAL_FRIEND_LIST_UPGRADE_INVITES_SENT = "social_friend_list_upgrade_invites_sent";
    public const string SOCIAL_FRIEND_SEND_FRIEND_REQUEST = "social_friend_send_friend_request";
    public const string SOCIAL_FRIEND_ACCEPT_FRIEND_REQUEST = "social_friend_accept_friend_request";
    public const string SOCIAL_FRIEND_DECLINE_FRIEND_REQUEST = "social_friend_decline_friend_request";
    public const string SOCIAL_FRIEND_SEND_UPGRADE_INVITE = "social_friend_send_upgrade_invite";
    public const string SOCIAL_FRIEND_ACCEPT_UPGRADE_INVITE = "social_friend_accept_upgrade_invite";
    public const string SOCIAL_FRIEND_DECLINE_UPGRADE_INVITE = "social_friend_decline_upgrade_invite";
    public const string SOCIAL_FRIEND_REMOVE_FRIEND = "social_friend_remove_friend";
    public const string SOCIAL_FRIEND_BLOCK_FRIEND = "social_friend_block_friend";
    public const string SOCIAL_FRIEND_GET_INTIMACY = "social_friend_get_intimacy";
    public const string SOCIAL_FRIEND_SEND_GIFT = "social_friend_send_gift";
    public const string SOCIAL_FRIEND_GET_LEVEL_CONFIG = "social_friend_get_level_config";
    public const string SOCIAL_FRIEND_GET_GIFT_ITEMS = "social_friend_get_gift_items";
    #endregion
    
    #region ConvertProtobuf
    private static T DecodeFromBase64<T>(string base64) where T : IMessage<T>, new()
    {
        byte[] data = Convert.FromBase64String(base64);
        var parser = new MessageParser<T>(() => new T());
        return parser.ParseFrom(data);
    }

    public static T DecodeFromJson<T>(string json) where T : IMessage<T>, new()
    {
        var parser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
        return parser.Parse<T>(json);
    }

    // Server Yuujins: lỗi trả qua RPC exception — exception.Message là chuỗi từ errs.ToPresenterSafe/Context (mapper.go safeMessages hoặc err.Error()). Không dùng Proto.Error.
    // Server Nigeria cũ: có thể trả JSON Proto.Error trong exception message → decode và xử lý ErrorType (ChipNotEnough v.v.).
    public static void ParseError(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        try
        {
            Error error = DecodeFromJson<Error>(message);
            if (error != null && !string.IsNullOrEmpty(error.Error_))
            {
                if (error.ErrorType == ErrorType.ChipNotEnough)
                    UIManager.Instance.ShowConfirmDialog(error.Error_, () => UIManager.Instance.OpenShop(), null, "Get More Chips");
                else
                    UIManager.Instance.ShowAlertDialog(error.Error_);
                return;
            }
        }
        catch (Exception) { /* message không phải Proto.Error (Yuujins hoặc plain text) */ }
        UIManager.Instance.ShowAlertDialog(message);
    }

    private static async UniTask<IApiRpc> RpcSendProto(string apiName, IMessage protoRequest)
    {
        if (NetworkManager.INSTANCE == null) throw new InvalidOperationException("NetworkManager not ready");
        return await NetworkManager.INSTANCE.RPCSend(apiName, protoRequest);
    }

    /// <summary>Decode JSON payload to protobuf; trả default khi payload rỗng hoặc parse lỗi (log warning). Dùng khi không cần throw.</summary>
    private static T DecodePayload<T>(string payload) where T : IMessage<T>, new()
    {
        if (string.IsNullOrWhiteSpace(payload)) return default;
        try { return DecodeFromJson<T>(payload); }
        catch (Exception e) { Debug.LogWarning("DecodePayload<" + typeof(T).Name + ">: " + e.Message); return default; }
    }

    /// <summary>Gọi RPC Yuujins: khi server trả lỗi (exception), show popup qua ParseError(exception.Message); khi success decode payload. Rỗng → default.</summary>
    private static async UniTask<T> RpcDecodeOrShowError<T>(string apiName, IMessage request) where T : IMessage<T>, new()
    {
        try
        {
            var rpc = await RpcSendProto(apiName, request);
            var payload = rpc?.Payload ?? "";
            if (string.IsNullOrWhiteSpace(payload)) return default;
            return DecodeFromJson<T>(payload);
        }
        catch (Exception e)
        {
            ParseError(e.Message);
            return default;
        }
    }

    /// <summary>Gọi RPC trả payload string (Identity v.v.): lỗi → ParseError + return null.</summary>
    private static async UniTask<string> RpcSendAndGetPayloadOrShowError(string apiName, IMessage request)
    {
        try
        {
            var rpc = await RpcSendProto(apiName, request);
            return rpc?.Payload ?? "";
        }
        catch (Exception e)
        {
            ParseError(e.Message);
            return null;
        }
    }

    #endregion
    
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
    
    #region RPC
    
    public static async UniTask<GameListResponse> GetListGame()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(LIST_GAME);
        return DecodeFromJson<GameListResponse>(response.Payload);
    }
    
    public static async UniTask<Bets> GetListBet(string gameCode)
    {
        BetListRequest betListRequest = new() { Code = gameCode };
        var response = await NetworkManager.INSTANCE.RPCSend(LIST_BET, betListRequest);
        return DecodeFromJson<Bets>(response.Payload);
    }

    #endregion
    
    #region Notifications

    public static async UniTask<ListNotification> GetListNotification(int limit = 100, TypeNotification type = TypeNotification.MailBox)
    {
        NotificationRequest notificationRequest = new()
        {
            Limit = limit,
            Type = type
        };
        var response = await NetworkManager.INSTANCE.RPCSend(LIST_NOTIFICATION, notificationRequest);
        return DecodeFromJson<ListNotification>(response.Payload);
    }

    public static async UniTask<Notification> ReadNotification()
    {
        Notification notification = new()
        {

        };
        var response = await NetworkManager.INSTANCE.RPCSend(READ_NOTIFICATION, notification);
        return DecodeFromJson<Notification>(response.Payload);
    }

    public static async UniTask<Notification> ReadAllNotifications()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(READ_ALL_NOTIFICATION);
        return DecodeFromJson<Notification>(response.Payload);
    }

    public static async UniTask<Notification> DeleteNotification(long notiId)
    {
        Notification notification = new()
        {
            Id = notiId
        };
        var response = await NetworkManager.INSTANCE.RPCSend(DELETE_NOTIFICATION, notification);
        return DecodeFromJson<Notification>(response.Payload);
    }

    public static async UniTask<Notification> DeleteAllNotifications()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(DELETE_ALL_NOTIFICATION);
        return DecodeFromJson<Notification>(response.Payload);
    }
    
    #endregion
    
    #region Account
    
    public static async UniTask<Profile> GetProfile()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(GET_PROFILE);
        return DecodeFromJson<Profile>(response.Payload);
    }

    public static async UniTask<string> ChangePassword(string oldPassword = "", string password = "")
    {
        ChangePasswordRequest data = new()
        {
            OldPassword = oldPassword,
            Password = password
        };
        var response = await NetworkManager.INSTANCE.RPCSend(CHANGE_PASS, data);
        return response.Payload;
    }

    public static async UniTask<string> LinkUsername(string username = "", string password = "")
    {
        RegisterRequest data = new()
        {
            UserName = username,
            Password = password
        };
        var response = await NetworkManager.INSTANCE.RPCSend(LINK_USERNAME, data);
        return response.Payload;
    }

    public static void UpdateProfile(string username = "", string password = "")
    {
        Profile data = new()
        {

        };
        _ = NetworkManager.INSTANCE.RPCSend(UPDATE_PROFILE, data);
    }

    public static async UniTask<Profile> UpdateAvatar(string avatarId = "")
    {
        Profile data = new()
        {
            AvatarId = avatarId
        };
        var response = await NetworkManager.INSTANCE.RPCSend(UPDATE_PROFILE, data);
        return DecodeFromJson<Profile>(response.Payload);
    }

    public static async UniTask<Profile> UpdateConfig(string config)
    {
        Profile data = new()
        {
            AppConfig = config
        };
        var response = await NetworkManager.INSTANCE.RPCSend(UPDATE_PROFILE, data);
        return DecodeFromJson<Profile>(response.Payload);
    }

    public static async UniTask<string> DeleteAccount()
    {
        try
        {
            var response = await NetworkManager.INSTANCE.RPCSend(DELETE_ACCOUNT);
            return response.Payload;
        }
        catch (Exception e)
        {
            UIManager.Instance.HideProgressing();
            UIManager.Instance.ShowAlertDialog("An error has occurred.");
            throw;
        }
        
    }
    
    #endregion
    
    #region Match
    
    public static async UniTask<RpcFindMatchResponse> FindMatch(string gameCode, int markUnit, bool isCreateGame, bool isWithNonOpen = false, string tableId = "", string userData = "")
    {
        Debug.Log("isWithNonOpen: "+ isWithNonOpen);
        try
        {
            RpcFindMatchRequest rpcFindMatchRequest = new()
            {
                GameCode = gameCode,
                MarkUnit = markUnit,
                Create = isCreateGame,
                WithNonOpen = isWithNonOpen,
                TableId = tableId,
                UserData = userData
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
            // Debug.LogError("FindMatch failed: " + ex.Message);
            UIManager.Instance.HideProgressing();
            ParseError(ex.Message);
            return null;
        }
    }

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
            ParseError(ex.Message);
            return null;
        }
    }

    public static async UniTask<Match> JoinMatch(string matchId, string passWord = "")
    {
        try
        {
            var match = await NetworkManager.INSTANCE.JoinMatch(matchId, passWord);
            Match data = DecodeFromJson<Match>(match.Label);
            Debug.Log("JOIN MATCH SUCCESS");
            return data;
        }
        catch (Exception ex)
        {
            // Debug.LogError("JoinMatch failed: " + ex.Message);
            // Error error = DecodeFromJson<Error>(ex.Message);
            // if (error.ErrorType == ErrorType.ChipNotEnough)
            // {
            //     UIManager.Instance.ShowConfirmDialog(error.Error_, () => UIManager.Instance.OpenShop(), null, "Get More Chips");
            // }
            // else
            // {
            //     UIManager.Instance.ShowAlertDialog(error.Error_);
            // }
            return null;
        }
    }

    public static async UniTask<RpcFindMatchResponse> QuickMatch(string gameCode)
    {
        try
        {
            RpcCreateMatchRequest rpcFindMatchRequest = new() { GameCode = gameCode };
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
            ParseError(ex.Message);
            return null;
        }
    }

    public static async UniTask LeaveMatch()
    {
        try
        {
            await NetworkManager.INSTANCE.LeaveMatch();
        }
        catch (Exception e)
        {
            Debug.Log("Error leave match : " + e.Message);
        }

    }

    public static void SendMatchState(long opCode, byte[] data)
    {
        Debug.Log("SendMatchState opCode: " + opCode + ", data: " + BitConverter.ToString(data));
        NetworkManager.INSTANCE.SendMatchState(opCode, data);
    }
    
    #endregion

    #region Friends
    public static async UniTask<IApiFriendList> GetListFriends()
    {
        try
        {
            var friendListRequest = new FriendListRequest()
            {
            };
            IApiFriendList response = await NetworkManager.INSTANCE.GetListFriends(0, 1000, "", null);
            return response;
        }
        catch (Exception ex)
        {
            ParseError(ex.Message);
            return null;
        }
    }
    
    public static async UniTask<FriendListResponse> GetListFriends(FriendTab currentTab = FriendTab.Friend, string nextCursor = "")
    {
        try
        {
            var friendListRequest = new FriendListRequest()
            {
                Tab = currentTab,
                Limit = 10,
                Cursor = nextCursor
            };
            var response = await NetworkManager.INSTANCE.RPCSend(LIST_FRIEND, friendListRequest);
            return DecodeFromJson<FriendListResponse>(response.Payload);
        }
        catch (Exception ex)
        {
            ParseError(ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Chỉ lấy tab counts (count/max từng tab). Gọi khi vào UI Friends.
    /// </summary>
    public static async UniTask<FriendListResponse> GetFriendTabCounts()
    {
        try
        {
            var response = await NetworkManager.INSTANCE.RPCSend(FRIEND_TAB_COUNTS);
            return DecodeFromJson<FriendListResponse>(response.Payload);
        }
        catch (Exception ex)
        {
            ParseError(ex.Message);
            return null;
        }
    }
    
    public static async UniTask SendGiftFriend()
    {
        try
        {
            var friendSendGiftRequest = new FriendSendGiftRequest()
            {
                FriendUserId = "",
                AmountOrItemId = 0,
                GiftType = ""
            };
            var response = await NetworkManager.INSTANCE.RPCSend(SEND_GIFT_FRIEND, friendSendGiftRequest);
        }
        catch (Exception e) 
        {
            ParseError(e.Message);
        }
    }
    
    public static async UniTask SendFriendRequest(List<string> listUserId)
    {
        try
        {
            var friendInviteRequest = new FriendInviteRequest();
            friendInviteRequest.UserIds.AddRange(listUserId);
            var response = await NetworkManager.INSTANCE.RPCSend(INVITE_FRIEND, friendInviteRequest);
        }
        catch (Exception e)
        {
            ParseError(e.Message);
        }
    }

    public static async UniTask AcceptFriendRequest(List<string> listUserId)
    {
        try
        {
            var friendAcceptRequest = new FriendAcceptRequest();
            friendAcceptRequest.UserIds.AddRange(listUserId);
            var response = await NetworkManager.INSTANCE.RPCSend(ACCEPT_FRIEND, friendAcceptRequest);
        }
        catch (Exception e)
        {
            ParseError(e.Message);
        }
    }
    
    public static async UniTask RejectFriendRequest(List<string> listUserId)
    {
        try
        {
            var friendRejectRequest = new FriendRejectRequest();
            friendRejectRequest.UserIds.AddRange(listUserId);
            var response = await NetworkManager.INSTANCE.RPCSend(REJECT_FRIEND, friendRejectRequest);
        }
        catch (Exception e)
        {
            ParseError(e.Message);
        }
    }

    public static async UniTask<FriendListUpgradeCandidatesResponse> GetFriendUpgradeCandidates(int targetTier = 0 , string nextCursor = "")
    {
        try
        {
            var request = new FriendListUpgradeCandidatesRequest()
            {
                TargetTier = targetTier,
                Limit = 10,
                Cursor = nextCursor
            };
            var response = await NetworkManager.INSTANCE.RPCSend(FRIEND_LIST_UPGRADE_CANDIDATES, request);
            return DecodeFromJson<FriendListUpgradeCandidatesResponse>(response.Payload);
        }
        catch (Exception e)
        {
            ParseError(e.Message);
            return null;
        }
    }

    public static async UniTask<FriendRequestTierUpgradeResponse> SendRequestTierUpgrade(int targetTier = 1, string friendID = "")
    {
        try
        {
            var request = new FriendRequestTierUpgradeRequest()
            {
                TargetTier = targetTier,
                FriendUserId = friendID
            };
            var response = await NetworkManager.INSTANCE.RPCSend(FRIEND_REQUEST_TIER_UPGRADE, request);
            return DecodeFromJson<FriendRequestTierUpgradeResponse>(response.Payload);
        }
        catch (Exception e)
        {
            ParseError(e.Message);
            return null;
        }
    }
    
    public static async UniTask<FriendAcceptTierUpgradeResponse> SendAcceptTierUpgrade(string friendID = "")
    {
        try
        {
            var request = new FriendAcceptTierUpgradeRequest()
            {
                FriendUserId = friendID,
            };
            var response = await NetworkManager.INSTANCE.RPCSend(FRIEND_ACCEPT_TIER_UPGRADE, request);
            return DecodeFromJson<FriendAcceptTierUpgradeResponse>(response.Payload);
        }
        catch (Exception e)
        {
            ParseError(e.Message);
            return null;
        }
    }
    
    public static async UniTask<FriendRefuseTierUpgradeResponse> SendRefuseTierUpgrade(string friendID = "")
    {
        try
        {
            var request = new FriendRefuseTierUpgradeRequest()
            {
               FriendUserId = friendID
            };
            var response = await NetworkManager.INSTANCE.RPCSend(FRIEND_REFUSE_TIER_UPGRADE, request);
            return DecodeFromJson<FriendRefuseTierUpgradeResponse>(response.Payload);
        }
        catch (Exception e)
        {
            ParseError(e.Message);
            return null;
        }
    }
    
    public static async UniTask<FriendDowngradeResponse> SendDownTierUpgrade(string friendID = "")
    {
        try
        {
            var request = new FriendDowngradeRequest()
            {
              FriendUserId = friendID
            };
            var response = await NetworkManager.INSTANCE.RPCSend(FRIEND_DOWNGRADE, request);
            return DecodeFromJson<FriendDowngradeResponse>(response.Payload);
        }
        catch (Exception e)
        {
            ParseError(e.Message);
            return null;
        }
    }
    
    public static async UniTask<ListRecentConversationsResponse> GetListRecentConversations()
    {
        try
        {
            var listRecent = new ListRecentConversationsRequest()
            {
                Limit = 10
            };
            var response = await NetworkManager.INSTANCE.RPCSend(LIST_RECENT_CONVERSATIONS, listRecent);
            return DecodeFromJson<ListRecentConversationsResponse>(response.Payload);
        }
        catch (Exception e)
        {
            ParseError(e.Message);
            return null;
        }
    }

    public static async UniTask<AdminFriendConfigGetResponse> GetFriendConfig()
    {
        try
        {
            var response = await NetworkManager.INSTANCE.RPCSend(FRIEND_TAB_COUNTS);
            return DecodeFromJson<AdminFriendConfigGetResponse>(response.Payload);
        }
        catch (Exception e)
        {
            ParseError(e.Message);
            return null;
        }
    }
    
    #endregion

    #region PopupView

    public static async UniTask<SubmitFeedbackResponse> SubmitFeedBack(string txtFeedBack)
    {
        try
        {
            SubmitFeedbackRequest feedbackRequest = new SubmitFeedbackRequest(){
                FeedbackText = txtFeedBack
            };
            var response = await NetworkManager.INSTANCE.RPCSend(SUBMIT_FEEDBACK, feedbackRequest);
            return DecodeFromJson<SubmitFeedbackResponse>(response.Payload);
        }
        catch (Exception e)
        {
            ParseError(e.Message);
            // throw;
            return null;
        }
    }
    
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

    public static async UniTask<FreeChip> SendGift(long amountChip = 0, string recipientId = "")
    {
        try
        {
            Bank bank = new Bank() { ChipsInBank = amountChip, RecipientId = recipientId };
            var response = await NetworkManager.INSTANCE.RPCSend(SEND_GIFT, bank);
            return DecodeFromJson<FreeChip>(response.Payload);

        }
        catch (Exception ex)
        {
            ParseError(ex.Message);
            return null;
        }
    }

    public static async UniTask<ListFreeChip> GetListClaimableFreeChips()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(LIST_CLAIMABLE_FREECHIPS);
        return DecodeFromJson<ListFreeChip>(response.Payload);
    }

    public static async UniTask<Constants.WalletTransaction> GetTransactionHistory(long limit = 0, string metaAction = "bank_topup", string metaBankAction = "1")
    {
        WalletTransRequest bank = new WalletTransRequest()
        {
            Limit = limit,
            MetaAction = metaAction,
            MetaBankAction = metaBankAction
        };
        var response = await NetworkManager.INSTANCE.RPCSend(WALLET_TRANSACTION, bank);
        return JsonConvert.DeserializeObject<Constants.WalletTransaction>(response.Payload);
    }

    public static async UniTask<ListFreeChip> GetListFreeChip()
    {
        FreeChipRequest freeChipRequest = new()
        {

        };
        var response = await NetworkManager.INSTANCE.RPCSend(LIST_GAME, freeChipRequest);
        return DecodeFromJson<ListFreeChip>(response.Payload);
    }

    public static async UniTask<FreeChip> ClaimFreeChip(FreeChip freeChip)
    {
        var response = await NetworkManager.INSTANCE.RPCSend(CLAIM_FREECHIP, freeChip);
        return DecodeFromJson<FreeChip>(response.Payload);
    }

    public static async UniTask<QuickChatResponse> GetListQuickChat()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(GET_QUICKCHAT);
        return DecodeFromJson<QuickChatResponse>(response.Payload);
    }

    public static async UniTask<QuickChatUpdateRequest> UpdateQuickChat()
    {
        QuickChatUpdateRequest quickChatUpdateRequest = new QuickChatUpdateRequest()
        {

        };
        var response = await NetworkManager.INSTANCE.RPCSend(UPDATE_QUICKCHAT, quickChatUpdateRequest);
        return DecodeFromJson<QuickChatUpdateRequest>(response.Payload);
    }

    public static async UniTask<ExchangeInfo> AddExchange(string cashId, string idDeal)
    {
        ExchangeInfo exchangeInfo = new ExchangeInfo()
        {
            CashId = cashId,
            IdDeal = idDeal,
            CashType = "gcash"
        };
        var response = await NetworkManager.INSTANCE.RPCSend(EXCHANGE_ADD, exchangeInfo);
        return DecodeFromJson<ExchangeInfo>(response.Payload);
    }

    public static async UniTask<ExchangeInfo> CancelExchange(string exchangeId)
    {
        ExchangeInfo exchangeInfo = new ExchangeInfo()
        {
            Id = exchangeId
        };
        var response = await NetworkManager.INSTANCE.RPCSend(EXCHANGE_CANCEL, exchangeInfo);
        return DecodeFromJson<ExchangeInfo>(response.Payload);
    }

    public static async UniTask<ExchangeDealInShop> GetListExchangeDeal()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(LIST_EXCHANGE_DEAL);
        return DecodeFromJson<ExchangeDealInShop>(response.Payload);
    }

    public static async UniTask<ListExchangeInfo> GetListExchange()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(LIST_EXCHANGE);
        return DecodeFromJson<ListExchangeInfo>(response.Payload);
    }

    public static async UniTask<GiftCode> ClaimGiftCode(string code)
    {
        GiftCode giftCode = new GiftCode()
        {
            Code = code
        };
        var response = await NetworkManager.INSTANCE.RPCSend(GIFT_CODE_CLAIM, giftCode);
        return DecodeFromJson<GiftCode>(response.Payload);
    }

    public static async UniTask<ListInAppMessage> GetListInAppMessage(TypeInAppMessage typeInAppMessage)
    {
        InAppMessageRequest inAppMessageRequest = new()
        {
            Type = typeInAppMessage
        };
        var response = await NetworkManager.INSTANCE.RPCSend(LIST_IN_APP_MESSAGE, inAppMessageRequest);
        return DecodeFromJson<ListInAppMessage>(response.Payload);
    }
    
    #endregion

    #region Leaderboard
    public static async UniTask<LeaderBoardRecord> GetLeaderBoardRecord(string gameCode)
    {
        LeaderBoardRecord leaderBoardRecord = new()
        {
            GameCode = gameCode
        };
        var response = await NetworkManager.INSTANCE.RPCSend(LEADERBOARD_INFO, leaderBoardRecord);
        return DecodeFromJson<LeaderBoardRecord>(response.Payload);
    }
    #endregion

    #region Check In Bonus

    public static async UniTask<DailyRewardTemplate> GetDailyRewardTemplate()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(DAILY_REWARD_TEMPLATE);
        return DecodeFromJson<DailyRewardTemplate>(response.Payload);
    }

    public static async UniTask<Reward> GetClaimableDailyReward()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(CAN_CLAIM_DAILY_REWARD, new RequestReward
        {
            DeviceId = Config.deviceId,
        });
        return DecodeFromJson<Reward>(response.Payload);
    }

    public static async UniTask<Reward> ClaimDailyReward()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(CLAIM_DAILY_REWARD, new RequestReward
        {
            DeviceId = Config.deviceId,
        });
        return DecodeFromJson<Reward>(response.Payload);
    }

    public static async UniTask<WeeklyBonusTemplate> GetWeeklyRewardTemplate()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(WEEKLY_REWARD_TEMPLATE);
        return DecodeFromJson<WeeklyBonusTemplate>(response.Payload);
    }

    public static async UniTask<Reward> GetClaimableWeeklyReward()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(CAN_CLAIM_WEEKLY_REWARD, new RequestReward
        {
            DeviceId = Config.deviceId,
        });
        return DecodeFromJson<Reward>(response.Payload);
    }

    public static async UniTask<Reward> ClaimWeeklyReward()
    {
        var response = await NetworkManager.INSTANCE.RPCSend(CLAIM_WEEKLY_REWARD, new RequestReward
        {
            DeviceId = Config.deviceId,
        });
        return DecodeFromJson<Reward>(response.Payload);
    }

    #endregion

    #region Lottery

    /// <summary>
    /// Buy a single lottery ticket
    /// </summary>
    public static async UniTask<BuyLotteryTicketResponse> BuyLotteryTicket(List<int> numbers, long drawId)
    {
        try
        {
            BuyLotteryTicketRequest request = new()
            {
                DrawId = drawId
            };
            request.Numbers.AddRange(numbers);

            var response = await NetworkManager.INSTANCE.RPCSend(BUY_LOTTERY_TICKET, request);
            return DecodeFromJson<BuyLotteryTicketResponse>(response.Payload);
        }
        catch (Exception ex)
        {
            // Debug.LogError("BuyLotteryTicket failed: " + ex.Message);
            ParseError(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Buy multiple lottery tickets at once
    /// </summary>
    public static async UniTask<BuyMultipleLotteryTicketsResponse> BuyMultipleLotteryTickets(List<LotteryTicketInput> tickets)
    {
        try
        {
            BuyMultipleLotteryTicketsRequest request = new();
            request.Tickets.AddRange(tickets);

            var response = await NetworkManager.INSTANCE.RPCSend(BUY_MULTIPLE_LOTTERY_TICKETS, request);
            return DecodeFromJson<BuyMultipleLotteryTicketsResponse>(response.Payload);
        }
        catch (Exception ex)
        {
            // Debug.LogError("BuyMultipleLotteryTickets failed: " + ex.Message);
            ParseError(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Get lottery ticket history for current user
    /// </summary>
    public static async UniTask<GetLotteryHistoryResponse> GetLotteryHistory(int limit = 20, int offset = 0, LotteryTicketStatus status = LotteryTicketStatus.All  , string userId = null)
    {
        try
        {
            GetLotteryHistoryRequest request = new()
            {
                Limit = limit,
                Offset = offset,
                Status = status
            };

            if (!string.IsNullOrEmpty(userId))
            {
                request.UserId = userId;
            }

            var response = await NetworkManager.INSTANCE.RPCSend(GET_LOTTERY_HISTORY, request);
            return DecodeFromJson<GetLotteryHistoryResponse>(response.Payload);
        }
        catch (Exception ex)
        {
            Debug.LogError("GetLotteryHistory failed: " + ex.Message);
            ParseError(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Get available lottery draws
    /// </summary>
    public static async UniTask<GetAvailableDrawsResponse> GetAvailableDraws(int limit = 3)
    {
        try
        {
            GetAvailableDrawsRequest request = new()
            {
                Limit = limit
            };

            var response = await NetworkManager.INSTANCE.RPCSend(GET_AVAILABLE_DRAWS, request);
            return DecodeFromJson<GetAvailableDrawsResponse>(response.Payload);
        }
        catch (Exception ex)
        {
            Debug.LogError("GetAvailableDraws failed: " + ex.Message);
            ParseError(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Get latest draw result with user's tickets
    /// </summary>
    public static async UniTask<GetLatestDrawResultResponse> GetLatestDrawResult()
    {
        try
        {
            GetLatestDrawResultRequest request = new();

            var response = await NetworkManager.INSTANCE.RPCSend(GET_LATEST_DRAW_RESULT, request);
            return DecodeFromJson<GetLatestDrawResultResponse>(response.Payload);
        }
        catch (Exception ex)
        {
            // Debug.LogError("GetLatestDrawResult failed: " + ex.Message);
            ParseError(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Generate random lottery numbers (Quick Pick)
    /// </summary>
    public static async UniTask<QuickPickResponse> QuickPick(int count = 1)
    {
        try
        {
            QuickPickRequest request = new()
            {
                Count = count
            };

            var response = await NetworkManager.INSTANCE.RPCSend(QUICK_PICK, request);
            return DecodeFromJson<QuickPickResponse>(response.Payload);
        }
        catch (Exception ex)
        {
            // Debug.LogError("QuickPick failed: " + ex.Message);
            ParseError(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Trigger a lottery draw (Admin only)
    /// </summary>
    public static async UniTask<TriggerDrawResponse> TriggerDraw(long drawId)
    {
        try
        {
            TriggerDrawRequest request = new()
            {
                DrawId = drawId
            };

            var response = await NetworkManager.INSTANCE.RPCSend(TRIGGER_DRAW, request);
            return DecodeFromJson<TriggerDrawResponse>(response.Payload);
        }
        catch (Exception ex)
        {
            Debug.LogError("TriggerDraw failed: " + ex.Message);
            UIManager.Instance.ShowAlertDialog("Failed to trigger draw: " + ex.Message, null);
            return null;
        }
    }

    #endregion

    #region Vip Farm
    public static async UniTask<UserVipFarmProgress> GetVipFarmProgress()
    {
        try
        {
            var response = await NetworkManager.INSTANCE.RPCSend(VIP_FARM_PROGRESS);
            return DecodeFromJson<UserVipFarmProgress>(response.Payload);
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public static async UniTask<UserVipFarm> ClaimVipFarm()
    {
        try
        {
            var response = await NetworkManager.INSTANCE.RPCSend(VIP_FARM_CLAIM);
            return DecodeFromJson<UserVipFarm>(response.Payload);
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    #endregion

    #region ChatVoice

    public static async UniTask<PreSignPutResponse> GetVoiceUploadPresignedUrl(string fileName)
    {
        try
        {
            var preSignPutRequest = new PreSignPutRequest()
            {
                FileName = fileName,
                BucketName = "voice-chat"
            };
            
            IApiRpc rpcResponse = await NetworkManager.INSTANCE.RPCSend(PRE_SIGN_PUT, preSignPutRequest);
            return DecodeFromJson<PreSignPutResponse>(rpcResponse.Payload);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error uploading voice file: {e.Message}");
            return null;
        }
    }

    #endregion

    #region Yuujins – Identity (User.V1)
    public static async UniTask<string> IdentityUserRegister(string userName, string password, string deviceId)
    {
        var req = new User.Types.Request.Types.Register { UserName = userName ?? "", Password = password ?? "", DeviceId = deviceId ?? Config.deviceId ?? "" };
        return await RpcSendAndGetPayloadOrShowError(IDENTITY_USER_REGISTER, req) ?? "";
    }
    public static async UniTask<string> IdentityUserLogin(string userName, string password)
    {
        var req = new User.Types.Request.Types.Register { UserName = userName ?? "", Password = password ?? "" };
        return await RpcSendAndGetPayloadOrShowError(IDENTITY_USER_LOGIN, req) ?? "";
    }
    public static async UniTask<string> IdentityUserChangePassword(string oldPassword, string newPassword)
    {
        var req = new User.Types.Request.Types.ChangePassword { Password = oldPassword ?? "", NewPassword = newPassword ?? "" };
        return await RpcSendAndGetPayloadOrShowError(IDENTITY_USER_CHANGE_PASSWORD, req) ?? "";
    }
    public static async UniTask<string> IdentityUserGetAccount()
    {
        return await RpcSendAndGetPayloadOrShowError(IDENTITY_USER_GET_ACCOUNT, new User.Types.Request.Types.GetAccount()) ?? "";
    }
    public static async UniTask<User.Types.Response.Types.GetAccount> IdentityUserGetAccountTyped()
    {
        var payload = await IdentityUserGetAccount();
        if (string.IsNullOrWhiteSpace(payload)) return null;
        try { return DecodeFromJson<User.Types.Response.Types.GetAccount>(payload); }
        catch (Exception e) { Debug.LogWarning("IdentityUserGetAccountTyped: " + e.Message); return null; }
    }
    public static bool TryParseGetAccountResponse(string payload, out UserAccount account)
    {
        account = null;
        if (string.IsNullOrWhiteSpace(payload)) return false;
        try { var resp = DecodeFromJson<User.Types.Response.Types.GetAccount>(payload); account = resp?.Account; return account != null; }
        catch (Exception e) { Debug.LogWarning("TryParseGetAccountResponse: " + e.Message); return false; }
    }
    #endregion
    
    #region Yuujins – Match (Match.V1)
    public static async UniTask<ListGamesResponse> MatchListGamesAsync(Yuujins.Cfg.Game.V1.Game.Types.Type type = Yuujins.Cfg.Game.V1.Game.Types.Type.Unspecified)
    {
        return await RpcDecodeOrShowError<ListGamesResponse>(MATCH_LIST_GAMES, new ListGamesRequest { Type = type });
    }
    public static async UniTask<ListBetLevelsResponse> MatchListBetLevelsAsync(uint gameId)
    {
        return await RpcDecodeOrShowError<ListBetLevelsResponse>(MATCH_LIST_BET_LEVELS, new ListBetLevelsRequest { GameId = gameId });
    }
    public static async UniTask<GetMatchInfoResponse> MatchGetMatchInfoAsync(string matchId)
    {
        return await RpcDecodeOrShowError<GetMatchInfoResponse>(MATCH_GET_MATCH_INFO, new GetMatchInfoRequest { MatchId = matchId ?? "" });
    }
    public static async UniTask<JoinGameResponse> MatchJoinGameAsync(uint gameId, long markUnit)
    {
        return await RpcDecodeOrShowError<JoinGameResponse>(MATCH_JOIN_GAME, new JoinGameRequest { GameId = gameId, MarkUnit = markUnit });
    }
    public static async UniTask<EnterSlotResponse> MatchEnterSlotAsync(uint gameId)
    {
        return await RpcDecodeOrShowError<EnterSlotResponse>(MATCH_ENTER_SLOT, new EnterSlotRequest { GameId = gameId });
    }
    public static async UniTask<FindMatchResponse> MatchFindMatchAsync(uint gameId, long markUnit, bool createIfEmpty = false, string excludeMatchId = null)
    {
        return await RpcDecodeOrShowError<FindMatchResponse>(MATCH_FIND_MATCH, new FindMatchRequest { GameId = gameId, MarkUnit = markUnit, CreateIfEmpty = createIfEmpty, ExcludeMatchId = excludeMatchId ?? "" });
    }
    public static async UniTask<CreatePrivateTableResponse> MatchCreatePrivateTableAsync(uint gameId, long markUnit, string password)
    {
        return await RpcDecodeOrShowError<CreatePrivateTableResponse>(MATCH_CREATE_PRIVATE_TABLE, new CreatePrivateTableRequest { GameId = gameId, MarkUnit = markUnit, Password = password ?? "" });
    }
    public static async UniTask<JoinPrivateTableResponse> MatchJoinPrivateTableAsync(string inviteCode, string password)
    {
        return await RpcDecodeOrShowError<JoinPrivateTableResponse>(MATCH_JOIN_PRIVATE_TABLE, new JoinPrivateTableRequest { InviteCode = inviteCode ?? "", Password = password ?? "" });
    }
    #endregion
    
    #region Yuujins – Config (Cfg)
    public static async UniTask<Game.Types.Response.Types.List> CfgGameListAsync(Yuujins.Cfg.Game.V1.Game.Types.Type type = Yuujins.Cfg.Game.V1.Game.Types.Type.Unspecified)
    {
        return await RpcDecodeOrShowError<Game.Types.Response.Types.List>(CFG_GAME_LIST, new Game.Types.Request.Types.List { Type = type });
    }
    public static async UniTask<Yuujins.Cfg.Banner.V1.Banner.Types.Response.Types.List> CfgBannerListAsync(int offset = 0, int limit = 50)
    {
        return await RpcDecodeOrShowError<Yuujins.Cfg.Banner.V1.Banner.Types.Response.Types.List>(CFG_BANNER_LIST, new Yuujins.Cfg.Banner.V1.Banner.Types.Request.Types.List { Offset = offset, Limit = limit });
    }
    public static async UniTask<Yuujins.Cfg.Bet.V1.Template> CfgBetReadAsync(uint gameId)
    {
        return await RpcDecodeOrShowError<Yuujins.Cfg.Bet.V1.Template>(CFG_BET_READ, new Yuujins.Cfg.Bet.V1.Template.Types.Request.Types.Read { GameId = gameId });
    }
    #endregion
    
    #region Yuujins – Rank, Rewards
    public static async UniTask<ListTopByGameResponse> RankListTopByGameAsync(uint gameId)
    {
        return await RpcDecodeOrShowError<ListTopByGameResponse>(RANK_RANK_LIST_TOP_BY_GAME, new ListTopByGameRequest { GameId = gameId });
    }
    public static async UniTask<GetCheckinConfigResponse> RewardsCheckinGetConfigAsync(string region)
    {
        return await RpcDecodeOrShowError<GetCheckinConfigResponse>(REWARDS_CHECKIN_GET_CHECKIN_CONFIG, new GetCheckinConfigRequest { Region = region ?? "" });
    }
    public static async UniTask<GetCheckinStateResponse> RewardsCheckinGetStateAsync(string region)
    {
        return await RpcDecodeOrShowError<GetCheckinStateResponse>(REWARDS_CHECKIN_GET_CHECKIN_STATE, new GetCheckinStateRequest { Region = region ?? "" });
    }
    public static async UniTask<ClaimCheckinResponse> RewardsCheckinClaimAsync(string region)
    {
        return await RpcDecodeOrShowError<ClaimCheckinResponse>(REWARDS_CHECKIN_CLAIM_CHECKIN, new ClaimCheckinRequest { Region = region ?? "" });
    }
    #endregion

    #region Yuujins – Social Friend (Friend.V1)
    public static async UniTask<ListResponse> SocialFriendListAsync(int filterLevel = 0, int limit = 50, string cursor = null)
    {
        return await RpcDecodeOrShowError<ListResponse>(SOCIAL_FRIEND_LIST, new ListRequest { FilterLevel = filterLevel, Limit = limit, Cursor = cursor ?? "" });
    }
    public static async UniTask<ListFriendRequestsReceivedResponse> SocialFriendListFriendRequestsReceivedAsync(int limit = 50, string cursor = null)
    {
        return await RpcDecodeOrShowError<ListFriendRequestsReceivedResponse>(SOCIAL_FRIEND_LIST_FRIEND_REQUESTS_RECEIVED, new ListFriendRequestsReceivedRequest { Limit = limit, Cursor = cursor ?? "" });
    }
    public static async UniTask<ListFriendRequestsSentResponse> SocialFriendListFriendRequestsSentAsync(int limit = 50, string cursor = null)
    {
        return await RpcDecodeOrShowError<ListFriendRequestsSentResponse>(SOCIAL_FRIEND_LIST_FRIEND_REQUESTS_SENT, new ListFriendRequestsSentRequest { Limit = limit, Cursor = cursor ?? "" });
    }
    public static async UniTask<ListUpgradeInvitesReceivedResponse> SocialFriendListUpgradeInvitesReceivedAsync(int limit = 50, int offset = 0)
    {
        return await RpcDecodeOrShowError<ListUpgradeInvitesReceivedResponse>(SOCIAL_FRIEND_LIST_UPGRADE_INVITES_RECEIVED, new ListUpgradeInvitesReceivedRequest { Limit = limit, Offset = offset });
    }
    public static async UniTask<ListUpgradeInvitesSentResponse> SocialFriendListUpgradeInvitesSentAsync(int limit = 50, int offset = 0)
    {
        return await RpcDecodeOrShowError<ListUpgradeInvitesSentResponse>(SOCIAL_FRIEND_LIST_UPGRADE_INVITES_SENT, new ListUpgradeInvitesSentRequest { Limit = limit, Offset = offset });
    }
    public static async UniTask<SendFriendRequestResponse> SocialFriendSendFriendRequestAsync(string userId, string username)
    {
        return await RpcDecodeOrShowError<SendFriendRequestResponse>(SOCIAL_FRIEND_SEND_FRIEND_REQUEST, new SendFriendRequestRequest { UserId = userId ?? "", Username = username ?? "" });
    }
    public static async UniTask<AcceptFriendRequestResponse> SocialFriendAcceptFriendRequestAsync(string userId)
    {
        return await RpcDecodeOrShowError<AcceptFriendRequestResponse>(SOCIAL_FRIEND_ACCEPT_FRIEND_REQUEST, new AcceptFriendRequestRequest { UserId = userId ?? "" });
    }
    public static async UniTask<DeclineFriendRequestResponse> SocialFriendDeclineFriendRequestAsync(string userId)
    {
        return await RpcDecodeOrShowError<DeclineFriendRequestResponse>(SOCIAL_FRIEND_DECLINE_FRIEND_REQUEST, new DeclineFriendRequestRequest { UserId = userId ?? "" });
    }
    public static async UniTask<SendUpgradeInviteResponse> SocialFriendSendUpgradeInviteAsync(string friendUserId, int toLevel)
    {
        return await RpcDecodeOrShowError<SendUpgradeInviteResponse>(SOCIAL_FRIEND_SEND_UPGRADE_INVITE, new SendUpgradeInviteRequest { FriendUserId = friendUserId ?? "", ToLevel = toLevel });
    }
    public static async UniTask<AcceptUpgradeInviteResponse> SocialFriendAcceptUpgradeInviteAsync(long inviteId)
    {
        return await RpcDecodeOrShowError<AcceptUpgradeInviteResponse>(SOCIAL_FRIEND_ACCEPT_UPGRADE_INVITE, new AcceptUpgradeInviteRequest { InviteId = inviteId });
    }
    public static async UniTask<DeclineUpgradeInviteResponse> SocialFriendDeclineUpgradeInviteAsync(long inviteId)
    {
        return await RpcDecodeOrShowError<DeclineUpgradeInviteResponse>(SOCIAL_FRIEND_DECLINE_UPGRADE_INVITE, new DeclineUpgradeInviteRequest { InviteId = inviteId });
    }
    public static async UniTask<RemoveFriendResponse> SocialFriendRemoveFriendAsync(string userId)
    {
        return await RpcDecodeOrShowError<RemoveFriendResponse>(SOCIAL_FRIEND_REMOVE_FRIEND, new RemoveFriendRequest { UserId = userId ?? "" });
    }
    public static async UniTask<BlockFriendResponse> SocialFriendBlockFriendAsync(string userId)
    {
        return await RpcDecodeOrShowError<BlockFriendResponse>(SOCIAL_FRIEND_BLOCK_FRIEND, new BlockFriendRequest { UserId = userId ?? "" });
    }
    public static async UniTask<GetIntimacyResponse> SocialFriendGetIntimacyAsync(string friendUserId)
    {
        return await RpcDecodeOrShowError<GetIntimacyResponse>(SOCIAL_FRIEND_GET_INTIMACY, new GetIntimacyRequest { FriendUserId = friendUserId ?? "" });
    }
    public static async UniTask<SendGiftResponse> SocialFriendSendGiftAsync(string friendUserId, GiftType giftType, long amount = 0, string itemId = null)
    {
        return await RpcDecodeOrShowError<SendGiftResponse>(SOCIAL_FRIEND_SEND_GIFT, new SendGiftRequest { FriendUserId = friendUserId ?? "", GiftType = giftType, Amount = amount, ItemId = itemId ?? "" });
    }
    public static async UniTask<GetLevelConfigResponse> SocialFriendGetLevelConfigAsync(string region)
    {
        return await RpcDecodeOrShowError<GetLevelConfigResponse>(SOCIAL_FRIEND_GET_LEVEL_CONFIG, new GetLevelConfigRequest { Region = region ?? "" });
    }
    public static async UniTask<GetGiftItemsResponse> SocialFriendGetGiftItemsAsync(string region)
    {
        return await RpcDecodeOrShowError<GetGiftItemsResponse>(SOCIAL_FRIEND_GET_GIFT_ITEMS, new GetGiftItemsRequest { Region = region ?? "" });
    }
    #endregion

    #region Yuujins – Mail (proto Communication.Mail.V1)
    /// <summary>Danh sách mail – request/response typed.</summary>
    public static async UniTask<Mail.Types.Response.Types.List> MailListAsync(
        string fromUid = null, string toUid = null, bool? isRead = null, bool? isDeleted = null, bool? isRedeemed = null, int limit = 50, int offset = 0)
    {
        var req = new Mail.Types.Request.Types.List { Limit = limit, Offset = offset };
        if (!string.IsNullOrEmpty(fromUid)) req.FromUid = fromUid;
        if (!string.IsNullOrEmpty(toUid)) req.ToUid = toUid;
        if (isRead.HasValue) req.IsRead = isRead.Value;
        if (isDeleted.HasValue) req.IsDeleted = isDeleted.Value;
        if (isRedeemed.HasValue) req.IsRedeemed = isRedeemed.Value;
        return await RpcDecodeOrShowError<Mail.Types.Response.Types.List>(MAIL_LIST, req);
    }
    /// <summary>Đánh dấu đã đọc – trả về Mail (updated).</summary>
    public static async UniTask<Mail> MailMarkAsReadAsync(long mailId)
    {
        return await RpcDecodeOrShowError<Mail>(MAIL_MARK_AS_READ, new Mail.Types.Request.Types.MarkAsRead { Id = mailId });
    }
    /// <summary>Đánh dấu đã xóa – trả về Mail (updated).</summary>
    public static async UniTask<Mail> MailMarkAsDeletedAsync(long mailId)
    {
        return await RpcDecodeOrShowError<Mail>(MAIL_MARK_AS_DELETED, new Mail.Types.Request.Types.MarkAsDeleted { Id = mailId });
    }
    /// <summary>Đổi thưởng mail – response Empty.</summary>
    public static async UniTask<EmptyResponse> MailRedeemAsync(long mailId)
    {
        return await RpcDecodeOrShowError<EmptyResponse>(MAIL_REDEEM, new Mail.Types.Request.Types.Redeem { Id = mailId });
    }
    #endregion

    #region Yuujins – Bank (proto Economy.Wallet.V1)
    /// <summary>Nạp vào bank – request typed, response Empty.</summary>
    public static async UniTask<EmptyResponse> BankDepositAsync(long amount)
    {
        return await RpcDecodeOrShowError<EmptyResponse>(BANK_DEPOSIT, new Wallet.Types.Request.Types.DepositBank { Amount = amount });
    }
    /// <summary>Rút từ bank – request typed, response Empty.</summary>
    public static async UniTask<EmptyResponse> BankWithdrawAsync(long amount)
    {
        return await RpcDecodeOrShowError<EmptyResponse>(BANK_WITHDRAW, new Wallet.Types.Request.Types.WithdrawBank { Amount = amount });
    }
    #endregion

    #region Yuujins – Coupon (proto Promotion.Coupon.V1)
    /// <summary>Danh sách coupon – request/response typed.</summary>
    public static async UniTask<Coupon.Types.Response.Types.List> CouponListAsync(
        string issuedBy = null, string userId = null, long? depositCent = null, bool? isRedeemed = null, int limit = 50, int offset = 0)
    {
        var req = new Coupon.Types.Request.Types.List { Limit = limit, Offset = offset };
        if (!string.IsNullOrEmpty(issuedBy)) req.IssuedBy = issuedBy;
        if (!string.IsNullOrEmpty(userId)) req.UserId = userId;
        if (depositCent.HasValue) req.DepositCent = depositCent.Value;
        if (isRedeemed.HasValue) req.IsRedeemed = isRedeemed.Value;
        return await RpcDecodeOrShowError<Coupon.Types.Response.Types.List>(COUPON_LIST, req);
    }
    #endregion
}
