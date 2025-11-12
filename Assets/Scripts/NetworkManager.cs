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

    private const string SESSION = "session",
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
            var properties = new Dictionary<string, string>
            {
                { "device_id", Config.deviceId },
            };
            var match = await _SocketIS.JoinMatchAsync(matchId, properties);

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

    // public async UniTask LoginAsync(string username = "", string password = "")
    // {
    //     Debug.Log("Session: " + _SessionIS);
    //     // Lần đầu đăng nhập hoặc session đã hết hạn
    //     if (_SessionIS == null || _SessionIS.IsExpired)
    //     {
    //         // Hết hạn access token nhưng có refresh token
    //         if (_SessionIS?.RefreshToken != null)
    //         {
    //             // Thử làm mới session với refresh token
    //             if (await TryRefreshSessionAsync())
    //             {
    //                 // Làm mới session thành công -> đăng nhập thành công
    //                 Debug.Log("✅ Refresh session thành công.");
    //                 await FinalizeLoginAsync();
    //                 return;
    //             }
    //         }
    //
    //         // Lần đầu đăng nhập / Hết hạn refresh token -> tạo session mới
    //         Debug.Log("Đăng nhập với session mới.");
    //         var isLoginSuccess = await TryLoginWithNewSessionAsync(username, password);
    //         if (!isLoginSuccess)
    //         {
    //             Config.isLoginSuccessful = false;
    //             return;
    //         }
    //     }
    //     Config.userName = username;
    //     Config.userPass = password;
    //     await FinalizeLoginAsync();
    // }
    //
    // private async UniTask FinalizeLoginAsync()
    // {
    //     PlayerPrefs.SetString(AUTH_TOKEN_KEY, _SessionIS.AuthToken);
    //     PlayerPrefs.SetString(REFRESH_TOKEN_KEY, _SessionIS.RefreshToken);
    //     if (Config.loginType == LoginType.NORMAL)
    //     {
    //         PlayerPrefs.SetString(USER_NAME_KEY, _SessionIS.Username);
    //     }
    //
    //     Config.isLoginSuccessful = true;
    // Config.SaveUserData();
    //     Debug.Log($"🔐 Logged in! Token: {_SessionIS.AuthToken}, RefreshToken: {_SessionIS.RefreshToken}");
    //     Debug.Log($"🔐 Session:{_SessionIS}");
    //     await ConnectSocketAsync();
    // }
    //
    // public async UniTask<bool> TryLoginWithNewSessionAsync(string username, string password)
    // {
    //     try
    //     {
    //         switch (Config.loginType)
    //         {
    //             case LoginType.NORMAL:
    //                 Debug.Log("Đăng nhập bằng tài khoản thường.");
    //                 _SessionIS = await _ClientC.AuthenticateEmailAsync("", password, username, create: false);
    //                 break;
    //
    //             case LoginType.PLAYNOW:
    //                 Debug.Log("Đăng nhập bằng PlayNow.");
    //                 string deviceId = Config.deviceId;
    //                 _SessionIS = await _ClientC.AuthenticateDeviceAsync(deviceId);
    //                 break;
    //
    //             default:
    //                 Debug.LogError("❌ Loại đăng nhập không hợp lệ.");
    //                 return false;
    //         }
    //
    //         PlayerPrefs.SetInt(LOGIN_TYPE_KEY, (int)Config.loginType);
    //         Config.isLoginSuccessful = true;
    //         return true;
    //     }
    //     catch (Exception e)
    //     {
    //         UIManager.Instance.OpenDialog(e.Message);
    //         _SessionIS = null;
    //         return false;
    //     }
    // }
    //
    // private async UniTask<bool> TryRefreshSessionAsync()
    // {
    //     try
    //     {
    //         _SessionIS = await _ClientC.SessionRefreshAsync(_SessionIS);
    //         return true;
    //     }
    //     catch (Exception e)
    //     {
    //         UIManager.Instance.OpenDialog(e.Message);
    //         _SessionIS = null;
    //         return false;
    //     }
    // }

    public async UniTask LogoutAsync()
    {
        try
        {
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
        ReceiveMessageWorldChat();
    }

    public async UniTask SendMessageWorldChat(string content)
    {
        var data = new Dictionary<string, string> {{"content", content}}.ToJson();
        Debug.Log("MESSAGE: " + content.ToString());
        var sendAck = await _SocketIS.WriteChatMessageAsync(worldChatChannelId, data);
        Debug.Log("SEND MESSAGE TO WORLD CHAT: " + sendAck.ToString());
    }

    public void ReceiveMessageWorldChat()
    {
        _SocketIS.ReceivedChannelMessage += message =>
        {
            lock (messageQueueLock)
            {
                // Debug.Log("add state queue " + state);
                messageQueue.Enqueue(message);
            }
            Debug.Log("Received: " + message);
            Debug.Log("Message content: " + message.Content);
        };

    }

    public async UniTask<IApiChannelMessageList> GetWorldChatHistory()
    {
        var result = await _ClientC.ListChannelMessagesAsync(_SessionIS, worldChatChannelId, 10, true);
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
        var data = new Dictionary<string, string> {{"content", content}}.ToJson();
        Debug.Log("MESSAGE: " + content.ToString());
        var sendAck = await _SocketIS.WriteChatMessageAsync(CurrentRoomChatChannelId, data);
    }

    public async UniTask LeaveRoomChat()
    {
        if(string.IsNullOrEmpty(CurrentRoomChatChannelId)) return;
        Debug.Log("CurrentRoomChatChannelId: " + CurrentRoomChatChannelId);
        await _SocketIS.LeaveChatAsync(CurrentRoomChatChannelId);
        CurrentRoomChatChannelId = "";
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
        
        // _SocketIS.Closed += async () =>
        // {
        //     if(PlayerPrefs.GetInt(Config.AUTO_LOGIN, 0) == 0) 
        //         return;
        //     UIManager.Instance.ShowProgressing();
        //     Debug.Log("ondisconnect");

        //     await UniTask.Delay(TimeSpan.FromSeconds(1));

        //     if (isKickOff)
        //     {
        //         PlayerPrefs.SetInt(Config.AUTO_LOGIN, 0);
        //         isKickOff = false;
        //     }

        //     UIManager.Instance.HideProgressing();
        //     // Global.IsFreeChipLoaded = false;
        //     await UIManager.Instance.LoadScene(Config.LOGIN_SCENE);
        // };
        _SocketIS.Closed += async () =>
        {
            try
            {
                await UniTask.SwitchToMainThread();
                if (PlayerPrefs.GetInt(Config.AUTO_LOGIN, 0) == 0)
                    return;
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
        RegisterEventSocket();

        try
        {
            await _SocketIS.ConnectAsync(session);
            connected = true;
            UIManager.Instance.HideProgressing();
            Debug.Log("Socket connected");

            
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

    
    private void RegisterEventSocket()
    {
        // _SocketIS.ReceivedMatchmakerMatched += async (matched) =>
        // {
        //     try
        //     {
        //         UnityMainThreadDispatcher.Instance.Enqueue(() =>
        //         {
        //             GameManager.Instance.HandleMatchFound(matched);
        //         });
        //         IMatch match = await _SocketIS.JoinMatchAsync(matched);
        //
        //         UnityMainThreadDispatcher.Instance.Enqueue(() =>
        //         {
        //             GameManager.Instance.HandleMatchJoin(match);
        //             _MatchId = matched.MatchId;
        //         });
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogError("Error joining match: " + e);
        //         throw;
        //     }
        // };
        _SocketIS.ReceivedError += _OnErrorCb;
        
        _SocketIS.ReceivedMatchState += state =>
        {
            lock (queueLock)
            {
                // Debug.Log("add state queue " + state);
                if (isPause && state.OpCode != (int)OpCodeUpdate.OpcodeKickOffTheTable) return;

                matchStateQueue.Enqueue(state);
            }
        };
        
        _SocketIS.ReceivedNotification += notification =>
        {
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

                case 101:
                    isKickOff = true;
                    break;
                
                default:
                    break;
            }
        };
        
        // _SocketIS.ReceivedMatchPresence += presence =>
        // {
        //     UnityMainThreadDispatcher.Instance.Enqueue(() =>
        //     {
        //         GameManager.Instance.HandleMatchPresence(presence);
        //     });
        // };

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
        // _ClientC = new Client("http", "192.168.153.83", 57350, "defaultkey");
        _ClientC = new Client("http", "172.23.112.1", 57350, "defaultkey");
        // _ClientC = new Client("http", "10.251.228.83", 57350, "defaultkey");
        _ClientC = new Client("http", "172.16.56.36", 57350, "defaultkey"); // Máy Huy
        _ClientC = new Client("http", "103.226.250.195", 57350, "defaultkey"); // Server chung
        // _ClientC = new Client("http", "172.16.56.104", 57350, "defaultkey"); // Máy Toàn
        RestoreSession();
        string deviceId;
        if (PlayerPrefs.HasKey(DEVICE_ID)) deviceId = PlayerPrefs.GetString(DEVICE_ID);
        else
        {
            deviceId = SystemInfo.deviceUniqueIdentifier;
            if (deviceId == SystemInfo.unsupportedIdentifier) deviceId = Guid.NewGuid().ToString();
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
                _ClientC = new Client("http", SERVER_TEST_PORT, 57350, "defaultkey");
                UIManager.Instance.ShowToast("Connect to Test Server", 2, transform);
                break;
            case 1:
                _ClientC = new Client("http", SERVER_HUY_PORT, 57350, "defaultkey");
                UIManager.Instance.ShowToast("Connect to Huy Server", 2, transform);
                break;
            case 2:
                _ClientC = new Client("http", SERVER_TOAN_PORT, 57350, "defaultkey");
                UIManager.Instance.ShowToast("Connect to Toan Server", 2, transform);
                break;
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
}