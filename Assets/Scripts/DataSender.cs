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
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

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
    public const string GET_FRIEND_CHAT_CHANNEL = "get_friend_chat_channel";
    public const string LIST_RECENT_CONVERSATIONS = "list_recent_conversations";
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

    public static void ParseError(string message)
    {
        try
        {
            if (string.IsNullOrEmpty(message)) return;
            Error error = DecodeFromJson<Error>(message);
            if (error == null) return;
            if (error.ErrorType == ErrorType.ChipNotEnough)
            {
                UIManager.Instance.ShowConfirmDialog(error.Error_, () => UIManager.Instance.OpenShop(), null, "Get More Chips");
            }
            else
            {
                UIManager.Instance.ShowAlertDialog(error.Error_);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Parse Error Fail " + e);
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

    // public static async UniTask<GetFriendChatChannelResponse> GetFriendChatChannel(string friendId = "")
    // {
    //     try
    //     {
    //         var request = new GetFriendChatChannelRequest()
    //         {
    //             FriendUserId = friendId
    //         };
    //         var response = await NetworkManager.INSTANCE.RPCSend(GET_FRIEND_CHAT_CHANNEL, request);
    //         return DecodeFromJson<GetFriendChatChannelResponse>(response.Payload);
    //     }
    //     catch (Exception e)
    //     {
    //         ParseError(e.Message);
    //         return null;
    //     }
    // }

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
}
