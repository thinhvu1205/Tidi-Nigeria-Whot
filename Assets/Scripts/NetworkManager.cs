using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proto;
using Cysharp.Threading.Tasks;
using Globals;
using Google.Protobuf;
using Nakama;
using UnityEngine;
using UnityEngine.SceneManagement;
using Nakama.TinyJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class NetworkManager : MonoBehaviour
{
    #region Variables

    public static NetworkManager INSTANCE { get; private set; }
    public Action<IApiChannelMessage> OnMessageWorldReceived;
    public Action<IApiChannelMessage> OnMessageTableReceived;
    public Action<IApiChannelMessage> OnMessageDirectReceived;
    public Action OnAnnouncementTickerUpdated; // Event khi announcement ticker được update

    public const string SESSION = "session",
        DEVICE_ID = "deviceId",
        AUTH_TOKEN_KEY = "authToken",
        REFRESH_TOKEN_KEY = "refreshToken",
        LOGIN_TYPE_KEY = "loginType",
        USER_NAME_KEY = "UserName",
        WORLD_CHAT_ROOM_NAME = "world_chat",
        IP_SERVER_TEST = "103.226.250.195",
        IP_SERVER_HUY = "172.16.56.121",
        IP_SERVER_TOAN = "172.16.56.104",
        LINK_STORAGE_COLLECTION = "link_global",
        LINK_STORAGE_KEY = "links",
        KFeatureConfigCollection = "feature_config_global",
        KFeatureConfigKey = "config_mode";
    public const int SERVER_DEFAULT_PORT = 7350;


    private IClient _ClientC;
    private ISession _SessionIS;
    private ISocket _SocketIS;
    private List<Action> _DataHandlerAs = new();
    private string worldChatChannelId;
    private readonly Queue<IMatchState> matchStateQueue = new Queue<IMatchState>();
    private readonly Queue<IApiChannelMessage> messageQueue = new Queue<IApiChannelMessage>();
    private readonly object queueLock = new object();
    private readonly object messageQueueLock = new object();
    private bool connected, isKickOff = false, isPause = false;
    public string CurrentRoomChatChannelId { get; private set; }
    public string CurrentDirectChatChannelId { get; private set; }
    #endregion

    #region RPC

    public async UniTask<IApiRpc> RPCSend(string apiName, IMessage protoMessage = null)
    {
        try
        {
            Debug.Log("--Send--/ " + apiName + "/ " + protoMessage);
            string payload = protoMessage != null ? JsonFormatter.Default.Format(protoMessage) : "";
            IApiRpc rpc = await _ClientC.RpcAsync(_SessionIS, apiName, payload);
            Debug.Log("--Receive--/ " + apiName + "/ " + rpc.Payload);
            return rpc;
        }
        catch (ApiResponseException e)
        {
            Debug.LogError($"❌ RPC [{apiName}] failed: {e.Message}");
            throw;
        }
    }
    
    #endregion

    #region Match
    public async UniTask<IMatch> JoinMatch(string matchId, string passWord = "")
    {
        try
        {
            Dictionary<string, string> properties = new Dictionary<string, string>{};
            if (!string.IsNullOrEmpty(passWord))
            {
                properties = new Dictionary<string, string>
                {
                    { "password", passWord },
                };
            }
           
            Config.currentMatchId = string.Empty;
            var match = await _SocketIS.JoinMatchAsync(matchId, properties);
            Config.currentMatchId = matchId;
            return match;
        }
        catch (Exception ex)
        {
            UIManager.Instance.HideProgressing();
            DataSender.ParseError(ex.Message);
            Config.currentMatchId = string.Empty;
            return null;
        }
    }

    public async UniTask LeaveMatch()
    {
        try
        {
            await _SocketIS.LeaveMatchAsync(Config.currentMatchId);
        }
        catch (ApiResponseException e)
        {
            Debug.LogError(e);
            throw;
        }
        
    }

    public void SendMatchState(long opCode, byte[] data) => _SocketIS.SendMatchStateAsync(Config.currentMatchId, opCode, data);

    #endregion

    #region Authen

    /// <summary>Build metadata gửi kèm khi auth — key phải khớp identity hook server: bundle_id, version (bắt buộc), package, model.</summary>
    public static Dictionary<string, string> GetBuildMetadata()
    {
        // Server beforeAuthenticate bắt buộc: bundle_id (tra platform), version (có mặt).
        // Server afterAuthenticate dùng: package (last_login_package / creation), model (device model).
        string bundleId = Application.identifier;
        if (string.IsNullOrEmpty(bundleId))
            bundleId = Application.productName ?? "com.unknown";
        string packageName = Application.identifier;
        if (string.IsNullOrEmpty(packageName))
            packageName = Application.productName ?? "";
        var meta = new Dictionary<string, string>
        {
            { "bundle_id", bundleId },
            { "version", Application.version },
            { "package", packageName },
            { "model", SystemInfo.deviceModel ?? Application.platform.ToString() }
        };
        return meta;
    }

    public async UniTask LoginGuest(string deviceId)
    {
        try
        {
            var session = await _ClientC.AuthenticateDeviceAsync(deviceId, null, true, GetBuildMetadata());
            OnAuthenSuccess(session);
            
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async UniTask CreateAccount(string username, string password)
    {
        try
        {
            var email = $"{username}@fake.local";
            var vars = GetBuildMetadata();
            vars["device_id"] = Config.deviceId ?? "";
            var session = await _ClientC.AuthenticateEmailAsync(email, password, username, create: true, vars: vars);
            OnAuthenSuccess(session);
        }
        catch (Exception e)
        {
            Debug.LogError($"Authenticate failed: {e}");
            throw;
        }
    }
    
    public async UniTask LoginWithId(string username, string password)
    {
        try
        {
            var session = await _ClientC.AuthenticateEmailAsync("", password, username, create: false, vars: GetBuildMetadata());
            OnAuthenSuccess(session);
        }
        catch (Exception e)
        {
            Debug.LogError($"Authenticate failed: {e}");
            throw;
        }
    }
    
    public async UniTask LoginFacebook(string accessToken)
    {
        try
        {
            ISession session = await _ClientC.AuthenticateFacebookAsync(accessToken, create: true, username: "", import: true, vars: GetBuildMetadata());
            OnAuthenSuccess(session);
        }
        catch (Exception e)
        {
            Debug.LogError($"Authenticate failed: {e}");
            throw;
        }
    }

    private void OnAuthenSuccess(ISession session)
    {
        _SessionIS = session;
        StoreSession(session);
        _ = InitSocket(session);
    }
    
    public void StoreSession(ISession session) {
        PlayerPrefs.SetString(AUTH_TOKEN_KEY, session.AuthToken);
        PlayerPrefs.SetString(REFRESH_TOKEN_KEY, session.RefreshToken);
    }

    public async UniTask LogoutAsync()
    {
        try
        {
            worldChatChannelId = "";
            await _ClientC.SessionLogoutAsync(_SessionIS.AuthToken, _SessionIS.RefreshToken);
            _SessionIS = null;
        }
        catch (Exception e)
        {
            UIManager.Instance.ShowConfirmDialog(e.Message);
        }
    }

    private void RestoreSession()
    {
        var authToken = PlayerPrefs.GetString(AUTH_TOKEN_KEY, "");
        var refreshToken = PlayerPrefs.GetString(REFRESH_TOKEN_KEY, "");
        _SessionIS = !string.IsNullOrEmpty(authToken)
            ? Session.Restore(authToken, refreshToken)
            : null;
    }

    #endregion

    #region World Chat
    public async UniTask JoinWorldChat()
    {
        bool persistence = true;
        bool hidden = false;
        if (!string.IsNullOrEmpty(worldChatChannelId))
        {
            return;
        }
        IChannel channel = await _SocketIS.JoinChatAsync(WORLD_CHAT_ROOM_NAME, ChannelType.Room, persistence, hidden);
        worldChatChannelId = channel.Id;
    }

    public async UniTask SendMessageWorldChat(string content)
    {
        var data = new Dictionary<string, string> {{"content", content}}.ToJson();
        var sendAck = await _SocketIS.WriteChatMessageAsync(worldChatChannelId, data);
    }
    
    private void OnMessageReceived(IApiChannelMessage message)
    {
        lock (messageQueueLock)
        {
            // Debug.Log("add state queue " + state);
            // if (isPause) return;
            messageQueue.Enqueue(message);
        }
    }

    public async UniTask<IApiChannelMessageList> GetWorldChatHistory(string nextCursor)
    {
        if (!string.IsNullOrEmpty(nextCursor))
        {
            return await _ClientC.ListChannelMessagesAsync(_SessionIS, worldChatChannelId, 20, false, nextCursor);;   
        }
        else
        {
            return await _ClientC.ListChannelMessagesAsync(_SessionIS, worldChatChannelId, 20, false);
        }
    }

    public async UniTask LeaveWorldChat()
    {
        await _SocketIS.LeaveChatAsync(worldChatChannelId);
        worldChatChannelId = "";
    }
    #endregion

    #region Direct Chat
    public async UniTask JoinDirectChat(string userId)
    {
        var persistence = true;
        var hidden = false;
        IChannel channel = await _SocketIS.JoinChatAsync(userId, ChannelType.DirectMessage, persistence, hidden);
        CurrentDirectChatChannelId = channel.Id;
    }

    public async UniTask SendMessageDirectChat(string content)
    {
        if (string.IsNullOrEmpty(CurrentDirectChatChannelId)) return;
        var data = new Dictionary<string, string> {{"content", content}}.ToJson();
        var sendAck = await _SocketIS.WriteChatMessageAsync(CurrentDirectChatChannelId, data);
    }

    public async UniTask LeaveDirectChat()
    {
        if (string.IsNullOrEmpty(CurrentDirectChatChannelId)) return;
        await _SocketIS.LeaveChatAsync(CurrentDirectChatChannelId);
        CurrentDirectChatChannelId = "";
    }

    public async UniTask<IApiChannelMessageList> GetDirectChatHistory(string nextCursor)
    {
        if (!string.IsNullOrEmpty(nextCursor))
        {
            return await _ClientC.ListChannelMessagesAsync(_SessionIS, CurrentDirectChatChannelId, 20, false, nextCursor);;   
        }
        else
        {
            return await _ClientC.ListChannelMessagesAsync(_SessionIS, CurrentDirectChatChannelId, 20, false);
        }       
    }
    #endregion

    #region Ingame Chat
    public async UniTask JoinRoomChat(string roomName)
    {
        bool persistence = false;
        bool hidden = false;
        IChannel channel = await _SocketIS.JoinChatAsync(roomName, ChannelType.Room, persistence, hidden);
        CurrentRoomChatChannelId = channel.Id;
    }

    public async UniTask SendMessageRoomChat(string content)
    {
        if (string.IsNullOrEmpty(CurrentRoomChatChannelId)) return;
        var data = new Dictionary<string, string> {{"text", content}}.ToJson();
        var sendAck = await _SocketIS.WriteChatMessageAsync(CurrentRoomChatChannelId, data);
    }

    public async UniTask SendEmojiToPlayerRoomChat(string senderId, string receiverId, string emojiId)
    {
        if (string.IsNullOrEmpty(CurrentRoomChatChannelId)) return;
        var data = new Dictionary<string, string> {
            { "senderId", senderId },
            { "receiverId", receiverId },
            { "emojiId", emojiId},
            }.ToJson();
        var sendAck = await _SocketIS.WriteChatMessageAsync(CurrentRoomChatChannelId, data);
    }

    public async UniTask SendEmoji(string emojiId, string senderId)
    {
        if (string.IsNullOrEmpty(CurrentRoomChatChannelId)) return;
        var data = new Dictionary<string, string> {
            { "isEmoji", "true" },
            { "emojiId", emojiId},
            { "senderId", senderId},
            }.ToJson();
        var sendAck = await _SocketIS.WriteChatMessageAsync(CurrentRoomChatChannelId, data);
    }

    public async UniTask SendChatVoice(string senderId, string voiceUrl)
    {
        if (string.IsNullOrEmpty(CurrentRoomChatChannelId)) return;
        var data = new Dictionary<string, string> {
            { "senderId", senderId},
            { "isAudio", "true"},
            { "voice_url", voiceUrl},
            }.ToJson();
        var sendAck = await _SocketIS.WriteChatMessageAsync(CurrentRoomChatChannelId, data);
    }

    public async UniTask LeaveRoomChat()
    {
        if (string.IsNullOrEmpty(CurrentRoomChatChannelId)) return;
        await _SocketIS.LeaveChatAsync(CurrentRoomChatChannelId);
        CurrentRoomChatChannelId = "";
    }

    public async UniTask<IApiChannelMessageList> GetRoomChatHistory()
    {
        var result = await _ClientC.ListChannelMessagesAsync(_SessionIS, CurrentRoomChatChannelId, 100, false);
        return result; 
    }
    
    #endregion

    #region Friends

    public async UniTask<IApiFriendList> GetListFriends(int state, int limit, string cursor, Action<IApiFriendList> handleCb = null)
    {
        IApiFriendList iafl = await _ClientC.ListFriendsAsync(_SessionIS, state, limit, cursor);
        if (iafl == null || iafl.Friends.Count() <= 0) return null;
        return iafl;
        // _DataHandlerAs.Add(() => { handleCb?.Invoke(iafl); });
    }

    public async void GetUsersWithIds(List<string> userIds = null, List<string> usernames = null,
        Action<IApiUsers> handleCb = null)
    {
        if ((userIds == null || userIds.Count <= 0) && (usernames == null || usernames.Count <= 0)) return;
        IApiUsers users = await _ClientC.GetUsersAsync(_SessionIS, userIds, usernames);
        if (users.Users == null || users.Users.Count() <= 0) return;
        _DataHandlerAs.Add(() => { handleCb?.Invoke(users); });
    }

    public async void AddFriend(string userId, Action handleCb = null)
    {
        if (string.IsNullOrEmpty(userId)) return;
        await _ClientC.AddFriendsAsync(_SessionIS, new[] { userId });
        _DataHandlerAs.Add(() => { handleCb?.Invoke(); });
    }

    #endregion

    #region Leaderboard
    public async UniTask<IApiLeaderboardRecordList> GetListLeaderboard(string leaderboardId, string userId = null,  int limit = 15)
    {
        try
        {
            string[] ownerIds = null;

            if (!string.IsNullOrEmpty(userId))
            {
                ownerIds = new[] { userId };
            }
            var leaderboardRecordList = await _ClientC.ListLeaderboardRecordsAsync(
                session: _SessionIS,
                leaderboardId: leaderboardId,
                ownerIds: ownerIds,
                expiry: null,
                limit: limit,
                cursor: null
            );

            return leaderboardRecordList;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    #endregion
    
    #region Socket

    public async UniTask InitSocket(ISession session)
    {
        _SocketIS = _ClientC.NewSocket();
        
        try
        {
            await _SocketIS.ConnectAsync(session);
            connected = true;
            RegisterEventSocket();
            
            // await JoinWorldChat();
            
            // InitSocketChat();
            // InitSocketNotifications();
            // InitSocketMatchData();
            // InitSocketMatchPresence();
        }
        catch (Exception e)
        {
            // Global.IsFreeChipLoaded = false;
            await UIManager.Instance.LoadScene(Config.LOGIN_SCENE);
        }
    }

    
    
    private void RegisterEventSocket()
    {
        _SocketIS.Closed += async () =>
        {
            try
            {
                await UniTask.SwitchToMainThread();
                UIManager.Instance.ShowProgressing();

                await UniTask.Delay(TimeSpan.FromSeconds(1));

                if (isKickOff)
                {
                    PlayerPrefs.SetInt(Config.AUTO_LOGIN, 0);
                    isKickOff = false;
                }

                UIManager.Instance.HideProgressing();
                // Global.IsFreeChipLoaded = false;
                await UIManager.Instance.LoadScene(Config.LOGIN_SCENE);
            }
            catch (Exception e)
            {
            }
        };
        
        _SocketIS.ReceivedError += _OnErrorCb;
        
        _SocketIS.ReceivedChannelMessage += OnMessageReceived;
        
        _SocketIS.ReceivedMatchState += state =>
        {
            if (isPause && state.OpCode != (int)OpCodeUpdate.OpcodeKickOffTheTable && state.OpCode != (int)OpCodeUpdate.ChangeTable) return;
            lock (queueLock)
            {
                matchStateQueue.Enqueue(state);
            }
        };

        _SocketIS.ReceivedStreamState += async state =>
        {
            await UniTask.SwitchToMainThread();
            if (!string.IsNullOrEmpty(state.State))
            {
                try
                {
                    switch (state.Stream.Mode)
                    {
                        case 0:
                            if (state.Stream.Label == "session_kick")
                            {
                                Config.loginType = LoginType.NONE;
                                PlayerPrefs.SetInt(Config.AUTO_LOGIN, 0);
                                UIManager.Instance.ShowGlobalDialog(state.State);
                                isKickOff = true;
                            }
                            break;
                        case 2:
                            if (state.Stream.Label == "hot_news")
                            {
                                // UIManager.Instance.ShowHotNews();
                                try
                                {
                                    var msg = JsonUtility.FromJson<HotNewsMessage>(state.State);
            
                                    // Check VIP range để quyết định có hiển thị không
                                    var userVipLevel = User.userProfile.VipLevel; // Implement method này
                                    // bool shouldShow = false;
            
                                    if (msg.vip_ranges is { Length: > 0 })
                                    {
                                        foreach(VipRange vipRange in msg.vip_ranges)
                                        {
                                            bool shouldShow = userVipLevel >= vipRange.min && userVipLevel <= vipRange.max;
                                            if (shouldShow)
                                            {
                                                // Hiển thị popup hot news
                                                // ShowHotNewsPopup(msg);
                                                UIManager.Instance.ShowHotNews(msg);
                                                return;
                                            }
                                            
                                        }
                                    }
            
                                }
                                catch (Exception e)
                                {
                                }
                            }else if (state.Stream.Label == "announcement_ticker")
                            {
                                // Nhận event từ server → refresh announcement ticker list luôn
                                // Không cần parse vì chỉ cần biết có update là refresh
                                OnAnnouncementTickerUpdated?.Invoke();
                            }
                            break;
                    }
                    
                }
                catch (Exception e)
                {
                }
            }

        };
        
        _SocketIS.ReceivedNotification += async notification =>
        {
            await UniTask.SwitchToMainThread();

            switch (notification.Code)
            {
                case -1:
                    var senderId = notification.SenderId;
                    // var usersResult = await GetUsers(new List<string> { senderId });
                    //
                    // if (usersResult.Users.Count > 0)
                    // {
                    //     Global.ChatView.AddUserToQueue(usersResult.Users[0]);
                    // }
                    break;
                
                case (int)TypeNotification.MailBox:
                    var obj = JsonConvert.DeserializeObject<JObject>(notification.Content);
                    string content = obj["content"]?.ToString();
                    UIManager.Instance.ShowAlertDialog(content);
                    _ = UIManager.Instance.LoadProfileUser();
                    break;

                case (int)TypeNotification.Gift:
                    var uiManager = UIManager.Instance;

                    if (UIManager.Instance.gameView != null)
                    {
                        // var toast = Instantiate(Global.GameView.ToastPrefab);
                        // toast.transform.SetParent(Global.GameView.transform, false);
                        // toast.GetComponent<Toast>().ShowToast(uiManager.GetText("has_mail_show_gold_ingame"));
                    }
                    else if (UIManager.Instance.gameView != null)
                    {
                        // Global.MainView.ListFreechipNotifications();
                    }
                    else
                    {
                        // Global.AlertView.SetData(uiManager.GetText("has_mail_show_gold"));
                        // Global.AlertView.IsFreechip = true;
                        // uiManager.ShowAlert();
                    }
                    break;

                case 101: // duplicate connection, kick old connection
                    UIManager.Instance.ShowAlertDialog(notification.Subject);
                    isKickOff = true;
                    break;
                
                default:
                    break;
            }
        };

    }

    private void UnregisterCallback()
    {
        if (_SocketIS == null) return;

        // _SocketIS.Connected -= _OnConnectCb;
        // _SocketIS.ReceivedMatchmakerMatched -= null;
        // _SocketIS.Closed -= _OnCloseCb;
        // _SocketIS.ReceivedError -= null;
        // _SocketIS.ReceivedMatchState = null;
        // _SocketIS.ReceivedNotification -= null;
        // _SocketIS.ReceivedMatchPresence -= null;
    }
    
    private void _OnErrorCb(Exception exception)
    {
        Debug.Log("Socket Error: " + exception.ToString());
    }

    #endregion

    #region Config

    public void PreConnect()
    {
        string ipServer = PlayerPrefs.GetString("IpServer", IP_SERVER_HUY);
        // _ClientC = new Client("http", "172.23.112.1", 57350, "defaultkey");
        // _ClientC = new Client("http", "172.16.56.36", 57350, "defaultkey"); // Máy Huy
        // _ClientC = new Client("http", "103.226.250.195", 57350, "defaultkey"); // Server chung
        // _ClientC = new Client("http", "172.16.56.104", 57350, "defaultkey"); // Máy Toàn
        _ClientC = new Client("http", ipServer, SERVER_DEFAULT_PORT, "defaultkey");
        RestoreSession();
        string deviceId;
        if (PlayerPrefs.HasKey(DEVICE_ID)) deviceId = PlayerPrefs.GetString(DEVICE_ID);
        else
        {
            deviceId = Guid.NewGuid().ToString();
            // deviceId = SystemInfo.deviceUniqueIdentifier;
            // if (deviceId == SystemInfo.unsupportedIdentifier) deviceId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(DEVICE_ID, deviceId);
        }
        Config.deviceId = deviceId;
        // _SocketIS = _ClientC.NewSocket();
    }

    public void SwitchServer(int serverId, Transform transform)
    {
        switch(serverId)
        {
            case 0:
                PlayerPrefs.SetString("IpServer", IP_SERVER_TEST);
                _ClientC = new Client("http", IP_SERVER_TEST, SERVER_DEFAULT_PORT, "defaultkey");
                UIManager.Instance.ShowToast("Connect to Test Server", 2, transform);
                break;
            case 1:
                PlayerPrefs.SetString("IpServer", IP_SERVER_HUY);
                _ClientC = new Client("http", IP_SERVER_HUY, SERVER_DEFAULT_PORT, "defaultkey");
                UIManager.Instance.ShowToast("Connect to Huy Server", 2, transform);
                break;
            case 2:
                PlayerPrefs.SetString("IpServer", IP_SERVER_TOAN);
                _ClientC = new Client("http", IP_SERVER_TOAN, SERVER_DEFAULT_PORT, "defaultkey");
                UIManager.Instance.ShowToast("Connect to Toan Server", 2, transform);
                break;
        }
        PlayerPrefs.SetInt("serverId", serverId);
    }

    public async UniTask GetLinkConfigFromStorage()
    {
        try
        {
            IApiStorageObjects result = await _ClientC.ReadStorageObjectsAsync(_SessionIS, new IApiReadStorageObjectId[] {
                new StorageObjectId {
                    Collection = LINK_STORAGE_COLLECTION,
                    Key = LINK_STORAGE_KEY
                }
            });
            string json = result.Objects.FirstOrDefault()?.Value.ToString();
            LinkGlobalValue data =
                JsonUtility.FromJson<LinkGlobalValue>(json);
            Config.ruleLink = data.rule_link;
            Config.groupLink = data.group_link;
            Config.facebookLink = data.facebook_link;
            Config.feedBackLink = data.feedback_link;
            Config.privacyPolicyLink = "";
            
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }
    
    [Serializable]
    public class ConfigModeData
    {
        public bool use_config_on;
    }
    public async UniTask GetConfigModeFromStorage()
    {
        try
        {
            IApiStorageObjects result = await _ClientC.ReadStorageObjectsAsync(_SessionIS, new IApiReadStorageObjectId[] {
                new StorageObjectId {
                    Collection = KFeatureConfigCollection,
                    Key = KFeatureConfigKey
                }
            });
            
            var storageObject = result.Objects.FirstOrDefault();
            if (storageObject == null)
            {
                return ;
            }

            string json = storageObject.Value;
            ConfigModeData data = JsonUtility.FromJson<ConfigModeData>(json);
            Config.isConfigMode = data.use_config_on;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            // throw;
        }
    }

    #endregion

    private void Awake()
    {
        if (INSTANCE == null) INSTANCE = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        PreConnect();
    }

    // void OnApplicationPause(bool paused)
    // {
    //     if (paused)
    //     {
    //         if (_SocketIS != null && _SocketIS.IsConnected)
    //         {
    //             _SocketIS.CloseAsync(); // hoặc await socket.DisconnectAsync()
    //         }
    //     }
    //     else
    //     {
    //         if (_SocketIS != null && !_SocketIS.IsConnected)
    //         {
    //             Debug.Log("[Nakama] App resumed -> reconnecting socket");
    //             // _ = _SocketIS.ConnectAsync(_SessionIS, true);
    //             _ = InitSocket(_SessionIS);

    //         }
    //     }
    // }

    // void OnApplicationPause(bool pause)
    // {
    //     if (pause)
    //     {
    //             if (_SocketIS != null && _SocketIS.IsConnected)
    //             {
    //             Debug.Log("[Nakama] App paused -> disconnecting socket");
    //             _SocketIS.CloseAsync();

    //         }
    //     }
    //     else
    //     {
    //         if (_SessionIS != null)
    //         {
    //             Debug.Log("[Nakama] App resumed -> reconnecting socket");
    //             _ = InitSocket(_SessionIS);
    //         }
    //     }
    // }

    private void Update()
    {
        lock (queueLock)
        {
            while (matchStateQueue.Count > 0)
            {
                if (!UIManager.Instance.gameView || string.IsNullOrEmpty(Config.currentMatchId))
                {
                    return;
                }
                var state = matchStateQueue.Dequeue();
                if (state.MatchId != Config.currentMatchId) return;
                GameManager.Instance.HandleMatchState(state);
            }
        }
        lock (messageQueueLock)
        {
            while (messageQueue.Count > 0)
            {
                var message = messageQueue.Dequeue();
                if (message.ChannelId == worldChatChannelId)
                {
                    OnMessageWorldReceived?.Invoke(message);
                }
                else if (message.ChannelId == CurrentRoomChatChannelId)
                {
                    OnMessageTableReceived?.Invoke(message);
                }
                else if (message.ChannelId == CurrentDirectChatChannelId)
                {
                    OnMessageDirectReceived?.Invoke(message);
                }
            }
        }
    }

    void LateUpdate()
    {
        while (_DataHandlerAs.Any())
        {
            _DataHandlerAs[0].Invoke();
            _DataHandlerAs.RemoveAt(0);
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        isPause = pauseStatus;
    }

    private async void OnDisable()
    {
        await SafeClose();
    }

    private async UniTask SafeClose()
    {
        try
        {
            if (_SocketIS != null)
            {
                await _SocketIS.CloseAsync();
                _SocketIS = null;
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error closing socket: " + e);
        }
    }
}

[Serializable]
public class VipRange
{
    public int min;
    public int max;
}

[Serializable]
public class HotNewsMessage
{
    public string type;        // "co" | "big_win"
    public string user_id;
    public string user_name;
    public string avatar_id;
    public VipRange[] vip_ranges;

    // Optional fields - chỉ có khi type tương ứng
    public long co_value;      // Chỉ có khi type = "co"
    public long chips_win;     // Chỉ có khi type = "big_win"
    public string game_name;   // Chỉ có khi type = "big_win"

    public override string ToString()
    {
        if (type == "co")
        {
            return $"Hot News CO: User {user_name} exchanged {co_value} USD, vip range: {vip_ranges[0].min} - {vip_ranges[0].max}";
        }
        else if (type == "big_win")
        {
            return $"Hot News Big Win: User {user_name} won {chips_win} chips in {game_name}, vip range: {vip_ranges[0].min} - {vip_ranges[0].max}";
        }
        return $"Hot News: {type} from {user_name}";
    }
}