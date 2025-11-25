using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proto;
using Cysharp.Threading.Tasks;
using Globals;
using Google.Protobuf;
using Nakama;
using UnityEngine;
using UnityEngine.SceneManagement;
using Nakama.TinyJson;

public class NetworkManager : MonoBehaviour
{
    #region Variables

    public static NetworkManager INSTANCE { get; private set; }
    public Action<IApiChannelMessage> OnMessageWorldReceived;
    public Action<IApiChannelMessage> OnMessageTableReceived;

    public const string SESSION = "session",
        DEVICE_ID = "deviceId",
        AUTH_TOKEN_KEY = "authToken",
        REFRESH_TOKEN_KEY = "refreshToken",
        LOGIN_TYPE_KEY = "loginType",
        USER_NAME_KEY = "UserName",
        WORLD_CHAT_ROOM_NAME = "world_chat",
        SERVER_TEST_PORT = "103.226.250.195",
        SERVER_HUY_PORT = "172.16.56.36",
        SERVER_TOAN_PORT = "172.16.56.104";

    private IClient _ClientC;
    private ISession _SessionIS;
    private ISocket _SocketIS;
    private List<Action> _DataHandlerAs = new();
    private string _MatchId, worldChatChannelId;
    private readonly Queue<IMatchState> matchStateQueue = new Queue<IMatchState>();
    private readonly Queue<IApiChannelMessage> messageQueue = new Queue<IApiChannelMessage>();
    private readonly object queueLock = new object();
    private readonly object messageQueueLock = new object();
    private bool connected, isKickOff = false, isPause = false;
    public string CurrentRoomChatChannelId { get; private set; }
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

    public async void CreateMatch(string gameCode)
    {
        var match = await _SocketIS.CreateMatchAsync(gameCode);
        Debug.Log("-----Match----- " + match.ToString());
        await JoinMatch(match.Id);
    }

    // public async void MakingMatch(string gameCode)
    // {
    //     try
    //     {
    //         var stringProps = new Dictionary<string, string>
    //         {
    //             { "mode", "quick-match" },
    //             { "game", gameCode },
    //             { "name", "assassin" },
    //             { "password", "" }
    //         };
    //         var numericProps = new Dictionary<string, double>()
    //         {
    //             { "bet", 25 }
    //         };
    //         var matchTicket = await _SocketIS.AddMatchmakerAsync(
    //             query: $"+properties.game:{gameCode} +properties.bet:25",
    //             minCount: 2,
    //             maxCount: 4,
    //             stringProperties: stringProps,
    //             numericProperties: numericProps
    //         );

    //         Debug.Log("Đã gửi yêu cầu ghép trận. Ticket: " + matchTicket.Ticket);
    //     }
    //     catch (System.Exception ex)
    //     {
    //         Debug.LogError("Lỗi khi tìm trận: " + ex.Message);
    //     }
    // }

    public async UniTask<IMatch> JoinMatch(string matchId)
    {
        try
        {
            // var properties = new Dictionary<string, string>
            // {
            //     { "device_id", Config.deviceId },
            // };
            var match = await _SocketIS.JoinMatchAsync(matchId);

            // Lưu lại thông tin match nếu cần
            _MatchId = match.Id;
            Config.currentMatchId = matchId;
            return match;
            
        }
        catch (Exception ex)
        {
            Debug.Log("Err when join match : " + ex.Message);
            Config.currentMatchId = "";
            throw;
        }
    }

    public async UniTask LeaveMatch()
    {
        try
        {
            await _SocketIS.LeaveMatchAsync(_MatchId);
        }
        catch (ApiResponseException e)
        {
            Debug.LogError(e);
            throw;
        }
        
    }

    public void SendMatchState(long opCode, byte[] data) => _SocketIS.SendMatchStateAsync(_MatchId, opCode, data);

    #endregion

    #region Authen

    public async UniTask LoginGuest(string deviceId)
    {
        try
        {
            var session = await _ClientC.AuthenticateDeviceAsync(deviceId);
            OnAuthenSuccess(session);
            
        }
        catch (Exception e)
        {
            Debug.LogError($"Login guest error: {e.Message}");
            throw;
        }
    }

    public async UniTask CreateAccount(string username, string password)
    {
        try
        {
            var email = $"{username}@fake.local";
            Dictionary<string, string> vars = new Dictionary<string, string> { { "device_id", Config.deviceId } };
            var session = await _ClientC.AuthenticateEmailAsync(email, password, username, create: true,vars: vars);
            Debug.Log($"Authenticated successfully. User ID: {session.UserId}");
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
            // var email = $"{username}@fake.local";
            var session = await _ClientC.AuthenticateEmailAsync("", password, username, create: false);
            Debug.Log($"Authenticated successfully. User ID: {session.UserId}");
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
            ISession session = await _ClientC.AuthenticateFacebookAsync(accessToken, create: true, username: "", import: true);
            Debug.Log($"Authenticated Facebook successfully. User ID: {session.UserId}");
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
        Debug.Log("Session stored." + session);
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
        Debug.Log("Now connected to channel id: " + worldChatChannelId);
    }

    public async UniTask SendMessageWorldChat(string content)
    {
        var data = new Dictionary<string, string> {{"content", content}}.ToJson();
        Debug.Log("MESSAGE: " + content.ToString());
        var sendAck = await _SocketIS.WriteChatMessageAsync(worldChatChannelId, data);
        Debug.Log("SEND MESSAGE TO WORLD CHAT: " + sendAck.ToString());
    }
    
    private void OnMessageReceived(IApiChannelMessage message)
    {
        lock (messageQueueLock)
        {
            // Debug.Log("add state queue " + state);
            // if (isPause) return;
            messageQueue.Enqueue(message);
        }
        Debug.Log("Received: " + message);
        Debug.Log("Message content: " + message.Content);
    }

    public async UniTask<IApiChannelMessageList> GetWorldChatHistory()
    {
        var result = await _ClientC.ListChannelMessagesAsync(_SessionIS, worldChatChannelId, 100, false);
        return result; 
    }

    public async UniTask LeaveWorldChat()
    {
        await _SocketIS.LeaveChatAsync(worldChatChannelId);
        worldChatChannelId = "";
    }
    #endregion

    #region Ingame Chat
    public async UniTask JoinRoomChat(string roomName)
    {
        bool persistence = false;
        bool hidden = false;
        IChannel channel = await _SocketIS.JoinChatAsync(roomName, ChannelType.Room, persistence, hidden);
        Debug.Log("Now connected to room channel id: " + channel.Id);
        CurrentRoomChatChannelId = channel.Id;
    }

    public async UniTask SendMessageRoomChat(string content)
    {
        if (string.IsNullOrEmpty(CurrentRoomChatChannelId)) return;
        var data = new Dictionary<string, string> {{"content", content}}.ToJson();
        Debug.Log("MESSAGE: " + content.ToString());
        var sendAck = await _SocketIS.WriteChatMessageAsync(CurrentRoomChatChannelId, data);
    }

    public async UniTask LeaveRoomChat()
    {
        if (string.IsNullOrEmpty(CurrentRoomChatChannelId)) return;
        Debug.Log("CurrentRoomChatChannelId: " + CurrentRoomChatChannelId);
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

    public async void GetListFriends(int state, int limit, string cursor, Action<IApiFriendList> handleCb = null)
    {
        IApiFriendList iafl = await _ClientC.ListFriendsAsync(_SessionIS, state, limit, cursor);
        if (iafl == null || iafl.Friends.Count() <= 0) return;
        _DataHandlerAs.Add(() => { handleCb?.Invoke(iafl); });
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
    public async UniTask<IApiLeaderboardRecordList> GetListLeaderboard(string leaderboardId, int limit = 100)
    {
        IApiLeaderboardRecordList leaderboardRecordList = await _ClientC.ListLeaderboardRecordsAsync(_SessionIS, leaderboardId, null, limit);
        return leaderboardRecordList;
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
            Debug.Log("Socket connected");
            RegisterEventSocket();
            
            await JoinWorldChat();
            
            // InitSocketChat();
            // InitSocketNotifications();
            // InitSocketMatchData();
            // InitSocketMatchPresence();
        }
        catch (Exception e)
        {
            Debug.LogError($"connect failed: {e}");
            // Global.IsFreeChipLoaded = false;
            await UIManager.Instance.LoadScene(Config.LOGIN_SCENE);
        }
    }

    [Serializable]
    public class StreamKickMessage
    {
        public string type;
        public string reason;
    }
    
    private void RegisterEventSocket()
    {
        _SocketIS.Closed += async () =>
        {
            try
            {
                await UniTask.SwitchToMainThread();
                UIManager.Instance.ShowProgressing();
                Debug.Log("ondisconnect");

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
                Debug.LogError($"Closed socker callback failed: {e}");
            }
        };
        
        _SocketIS.ReceivedError += _OnErrorCb;
        
        _SocketIS.ReceivedChannelMessage += OnMessageReceived;
        
        _SocketIS.ReceivedMatchState += state =>
        {
            lock (queueLock)
            {
                // Debug.Log("add state queue " + state);
                if (isPause && state.OpCode != (int)OpCodeUpdate.OpcodeKickOffTheTable) return;

                matchStateQueue.Enqueue(state);
            }
        };

        _SocketIS.ReceivedStreamState += async state =>
        {
            await UniTask.SwitchToMainThread();
            Debug.Log($"Received Stream State: {state}");
            if (!string.IsNullOrEmpty(state.State))
            {
                try
                {
                    var msg = JsonUtility.FromJson<StreamKickMessage>(state.State);
                    if (msg.type == "kick")
                    {
                        Config.loginType = LoginType.NONE;
                        PlayerPrefs.SetInt(Config.AUTO_LOGIN, 0);
                        UIManager.Instance.ShowGlobalDialog(msg.reason);
                        isKickOff = true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse stream data: {e}");
                }
            }

        };
        
        _SocketIS.ReceivedNotification += async notification =>
        {
            await UniTask.SwitchToMainThread();
            Debug.Log($"Received Notification: {notification}");

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

                case 2:
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
        string ipServer = PlayerPrefs.GetString("IpServer", SERVER_TEST_PORT);
        // _ClientC = new Client("http", "172.23.112.1", 57350, "defaultkey");
        // _ClientC = new Client("http", "172.16.56.36", 57350, "defaultkey"); // Máy Huy
        // _ClientC = new Client("http", "103.226.250.195", 57350, "defaultkey"); // Server chung
        // _ClientC = new Client("http", "172.16.56.104", 57350, "defaultkey"); // Máy Toàn
        _ClientC = new Client("http", ipServer, 57350, "defaultkey");
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
        Debug.Log("DEVICE ID: " + deviceId);
        Config.deviceId = deviceId;
        // _SocketIS = _ClientC.NewSocket();
    }

    public void SwitchServer(int serverId, Transform transform)
    {
        switch(serverId)
        {
            case 0:
                PlayerPrefs.SetString("IpServer", SERVER_TEST_PORT);
                _ClientC = new Client("http", SERVER_TEST_PORT, 57350, "defaultkey");
                UIManager.Instance.ShowToast("Connect to Test Server", 2, transform);
                break;
            case 1:
                PlayerPrefs.SetString("IpServer", SERVER_HUY_PORT);
                _ClientC = new Client("http", SERVER_HUY_PORT, 57350, "defaultkey");
                UIManager.Instance.ShowToast("Connect to Huy Server", 2, transform);
                break;
            case 2:
                PlayerPrefs.SetString("IpServer", SERVER_TOAN_PORT);
                _ClientC = new Client("http", SERVER_TOAN_PORT, 57350, "defaultkey");
                UIManager.Instance.ShowToast("Connect to Toan Server", 2, transform);
                break;
        }
        PlayerPrefs.SetInt("serverId", serverId);
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
                if (!UIManager.Instance.gameView)
                {
                    return;
                }
                var state = matchStateQueue.Dequeue();
                // Debug.Log("get state dequeue " + state);
                if (state.MatchId != _MatchId) return;
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
                else
                {
                    OnMessageTableReceived?.Invoke(message);
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
                Debug.Log("Socket closed safely.");
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error closing socket: " + e);
        }
    }
}