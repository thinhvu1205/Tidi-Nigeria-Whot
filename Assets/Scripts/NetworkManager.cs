using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api;
using Cysharp.Threading.Tasks;
using Globals;
using Google.Protobuf;
using Nakama;
using Nakama.TinyJson;
using SimpleJSON;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    #region Variables

    public static NetworkManager INSTANCE { get; private set; }

    private const string SESSION = "session",
        DEVICE_ID = "deviceId",
        AUTH_TOKEN_KEY = "authToken",
        REFRESH_TOKEN_KEY = "refreshToken",
        LOGIN_TYPE_KEY = "loginType",
        USER_NAME_KEY = "UserName";

    private IClient _ClientC;
    private ISession _SessionIS;
    private ISocket _SocketIS;
    private List<Action> _DataHandlerAs = new();
    private string _MatchId;
    private readonly Queue<IMatchState> matchStateQueue = new Queue<IMatchState>();
    private readonly object queueLock = new object();
    #endregion

    #region RPC

    public async UniTask<IApiRpc> RPCSend(string apiName, IMessage protoMessage = null)
    {
        try
        {
            Debug.Log("--Send--/ " + apiName + "/ " + protoMessage?.ToString());
            byte[] payload = protoMessage != null ? protoMessage.ToByteArray() : Array.Empty<byte>();
            IApiRpc rpc = await _SocketIS.RpcAsync(apiName, payload);
            Debug.Log("--Receive--/ " + apiName + "/ " + rpc.Payload);
            return rpc;
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ RPC [{apiName}] failed: {e.Message}");
            throw;
        }
    }

    // public async UniTask RPCSend(string apiName, JSONObject jsonData)
    // {
    //     Debug.Log("--Send--/ " + apiName + "/ " + jsonData.ToString());
    //     IApiRpc rpc = await _SocketIS.RpcAsync(apiName, jsonData.ToString());
    //     if (rpc == null || string.IsNullOrEmpty(rpc.Payload)) return;
    //     JSONObject obj = JSON.Parse(rpc.Payload).AsObject;
    //     Debug.Log("--Receive--/ " + apiName + "/ " + rpc.Payload);
    //     _DataHandlerAs.Add(() =>
    //     {
    //         _PreHandleData(apiName, obj);
    //         for (int i = _ListenerGLs.Count - 1; i >= 0; i--)
    //         {
    //             GameListener gl = _ListenerGLs[i];
    //             if (gl.IsReady()) gl.HandleData(apiName, obj);
    //         }
    //     });
    // }

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

    public void LeaveMatch() => _SocketIS.LeaveMatchAsync(_MatchId);
    
    public void SendMatchState(long opCode, byte[] data) => _SocketIS.SendMatchStateAsync(_MatchId, opCode, data);

    #endregion

    #region Authen

    public async UniTask LoginAsync(string username = "", string password = "")
    {
        Debug.Log("Session: " + _SessionIS);
        // Lần đầu đăng nhập hoặc session đã hết hạn
        if (_SessionIS == null || _SessionIS.IsExpired)
        {
            // Hết hạn access token nhưng có refresh token
            if (_SessionIS?.RefreshToken != null)
            {
                // Thử làm mới session với refresh token
                if (await TryRefreshSessionAsync())
                {
                    // Làm mới session thành công -> đăng nhập thành công
                    Debug.Log("✅ Refresh session thành công.");
                    await FinalizeLoginAsync();
                    return;
                }
            }

            // Lần đầu đăng nhập / Hết hạn refresh token -> tạo session mới
            Debug.Log("Đăng nhập với session mới.");
            var isLoginSuccess = await TryLoginWithNewSessionAsync(username, password);
            if (!isLoginSuccess)
            {
                Config.isLoginSuccessful = false;
                return;
            }
        }
        Config.userName = username;
        Config.userPass = password;
        await FinalizeLoginAsync();
    }

    private async UniTask FinalizeLoginAsync()
    {
        PlayerPrefs.SetString(AUTH_TOKEN_KEY, _SessionIS.AuthToken);
        PlayerPrefs.SetString(REFRESH_TOKEN_KEY, _SessionIS.RefreshToken);
        if (Config.loginType == LoginType.NORMAL)
        {
            PlayerPrefs.SetString(USER_NAME_KEY, _SessionIS.Username);
        }

        Config.isLoginSuccessful = true;
        Config.SaveUserData();
        Debug.Log($"🔐 Logged in! Token: {_SessionIS.AuthToken}, RefreshToken: {_SessionIS.RefreshToken}");
        Debug.Log($"🔐 Session:{_SessionIS}");
        await ConnectSocketAsync();
    }

    public async UniTask<bool> TryLoginWithNewSessionAsync(string username, string password)
    {
        try
        {
            switch (Config.loginType)
            {
                case LoginType.NORMAL:
                    Debug.Log("Đăng nhập bằng tài khoản thường.");
                    _SessionIS = await _ClientC.AuthenticateEmailAsync("", password, username, create: false);
                    break;

                case LoginType.PLAYNOW:
                    Debug.Log("Đăng nhập bằng PlayNow.");
                    string deviceId = Config.deviceId;
                    _SessionIS = await _ClientC.AuthenticateDeviceAsync(deviceId);
                    break;

                default:
                    Debug.LogError("❌ Loại đăng nhập không hợp lệ.");
                    return false;
            }

            PlayerPrefs.SetInt(LOGIN_TYPE_KEY, (int)Config.loginType);
            Config.isLoginSuccessful = true;
            return true;
        }
        catch (Exception e)
        {
            UIManager.Instance.OpenDialog(e.Message);
            _SessionIS = null;
            return false;
        }
    }

    private async UniTask<bool> TryRefreshSessionAsync()
    {
        try
        {
            _SessionIS = await _ClientC.SessionRefreshAsync(_SessionIS);
            return true;
        }
        catch (Exception e)
        {
            UIManager.Instance.OpenDialog(e.Message);
            _SessionIS = null;
            return false;
        }
    }

    public async UniTask LogoutAsync()
    {
        try
        {
            await _ClientC.SessionLogoutAsync(_SessionIS.AuthToken, _SessionIS.RefreshToken);
            Debug.Log("✅ Đã logout thành công.");
            Config.loginType = LoginType.NONE;
            PlayerPrefs.SetInt(Config.TYPE_LOGIN_KEY, (int)Config.loginType);
            PlayerPrefs.DeleteKey(AUTH_TOKEN_KEY);
            PlayerPrefs.DeleteKey(REFRESH_TOKEN_KEY);
            Config.isLoginSuccessful = false;
            _SessionIS = null;
            SceneManager.LoadScene(Config.LOGIN_SCENE);
        }
        catch (Exception e)
        {
            UIManager.Instance.OpenDialog(e.Message);
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

    #region Socket

    private void RegisterCallback()
    {
        _SocketIS.Connected += _OnConnectCb;
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
        _SocketIS.Closed += _OnCloseCb;
        _SocketIS.ReceivedError += err => _OnErrorCb(err);
        _SocketIS.ReceivedMatchState += state =>
        {
            lock (queueLock)
            {
                // Debug.Log("add state queue " + state);
                matchStateQueue.Enqueue(state);
            }
        };
        _SocketIS.ReceivedNotification += notification =>
        {

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


    private async UniTask ConnectSocketAsync()
    {
        if (!_SocketIS.IsConnected)
        {
            RegisterCallback();
            await _SocketIS.ConnectAsync(_SessionIS, true);
        }

    }

    private void _OnConnectCb()
    {
        Debug.Log("Socket Connected");
    }

    private void _OnCloseCb()
    {
        Debug.Log("Socket Closed");
    }

    private void _OnErrorCb(Exception exception)
    {
        Debug.Log("Socket Error: " + exception.ToString());
    }

    #endregion

    #region Config

    public void PreConnect()
    {
        // _ClientC = new Client("http", "103.226.250.195", 7353, "defaultkey");
        // _ClientC = new Client("http", "172.16.56.71", 57350, "defaultkey");
        // _ClientC = new Client("http", "172.23.112.71", 57350, "defaultkey");
        _ClientC = new Client("http", "103.226.250.195", 57350, "defaultkey");
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
        _SocketIS = _ClientC.NewSocket();
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
                GameManager.Instance.HandleMatchState(state);
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
}