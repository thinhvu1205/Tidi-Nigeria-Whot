using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api;
using Cysharp.Threading.Tasks;
using Globals;
using Nakama;
using SimpleJSON;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    #region Variables
    public static NetworkManager INSTANCE { get; private set; }
    private NetworkManager() { }
    private const string SESSION = "session", DEVICE_ID = "deviceId", AUTH_TOKEN_KEY = "authToken", REFRESH_TOKEN_KEY = "refreshToken", LOGIN_TYPE_KEY = "loginType", USER_NAME_KEY = "UserName";
    private List<GameListener> _ListenerGLs = new();
    private Client _ClientC;
    private ISession _SessionIS;
    private ISocket _SocketIS;
    private List<Action> _DataHandlerAs = new();
    private string _MatchId;
    #endregion

    public void AddListener(GameListener listenerGL) => _ListenerGLs.Add(listenerGL);
    public void RemoveListener(GameListener listenerGL) => _ListenerGLs.Remove(listenerGL);

    #region PreHandle
    private void _PreHandleData(string apiName, JSONObject data)
    {
        Debug.Log("-----API----- " + apiName + " / " + data.ToString());
        // switch case by apiName
    }
    private void _PreHandleReceivedMatchState(IMatchState data)
    {
        string strData = Encoding.UTF8.GetString(data.State);
        // switch case by data.OpCode
    }
    private void _PreHandleNotification(IApiNotification data)
    {
        JSONObject dataObj = JSON.Parse(data.Content) as JSONObject;
        // switch case by data.Code
    }
    #endregion

    #region RPC
    public async UniTask RPCSend(string apiName, JSONObject jsonData)
    {
        Debug.Log("--Send--/ " + apiName + "/ " + jsonData.ToString());
        IApiRpc rpc = await _SocketIS.RpcAsync(apiName, jsonData.ToString());
        if (rpc == null || string.IsNullOrEmpty(rpc.Payload)) return;
        JSONObject obj = JSON.Parse(rpc.Payload).AsObject;
        Debug.Log("--Receive--/ " + apiName + "/ " + rpc.Payload);
        _DataHandlerAs.Add(() =>
        {
            _PreHandleData(apiName, obj);
            for (int i = _ListenerGLs.Count - 1; i >= 0; i--)
            {
                GameListener gl = _ListenerGLs[i];
                if (gl.IsReady()) gl.HandleData(apiName, obj);
            }
        });
    }
    #endregion

    #region Match
    public async void CreateMatch()
    {
        try
            {
                // Gửi yêu cầu ghép trận với điều kiện tối thiểu và tối đa người chơi
                var matchTicket = await _SocketIS.AddMatchmakerAsync(
                    query: "*",       // tất cả phòng đều hợp lệ
                    minCount: 2,
                    maxCount: 4,
                    stringProperties: null,
                    numericProperties: null
                );

                Debug.Log("Đã gửi yêu cầu ghép trận. Ticket: " + matchTicket.Ticket);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Lỗi khi tìm trận: " + ex.Message);
            }
    }

    public void JoinMatch(string matchId)
    {
        _MatchId = "";
        _SocketIS.JoinMatchAsync(matchId);
    }
    public void LeaveMatch() => _SocketIS.LeaveMatchAsync(_MatchId);
    public void SendMatchState(long opCode, string data) => _SocketIS.SendMatchStateAsync(_MatchId, opCode, data);
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
                Debug.LogError("❌ Đăng nhập thất bại.");
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
        SceneManager.LoadScene(Config.MAIN_SCENE);
    }

    public async UniTask<bool> TryLoginWithNewSessionAsync(string username, string password)
    {
        try
        {
            switch (Config.loginType)
            {
                case LoginType.NORMAL:
                    Debug.Log("Đăng nhập bằng tài khoản thường.");
                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    {
                        Debug.LogWarning("⚠️ Tên đăng nhập hoặc mật khẩu không được để trống.");
                        return false;
                    }
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
            Debug.Log("✅ Login mới thành công.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Login thất bại: {e.Message}");
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
            Debug.LogWarning($"⚠️ RefreshSession thất bại: {e.Message}");
            _SessionIS = null;
            return false;
        }
    }

    public async UniTask LogoutAsync()
    {
        if (_SessionIS == null)
        {
            Debug.LogWarning("⚠️ Không có session để logout.");
            return;
        }

        try
        {
            await _ClientC.SessionLogoutAsync(_SessionIS.AuthToken, _SessionIS.RefreshToken);
            Debug.Log("✅ Đã logout thành công.");
            Config.loginType = LoginType.NONE;
            PlayerPrefs.DeleteKey(AUTH_TOKEN_KEY);
            PlayerPrefs.DeleteKey(REFRESH_TOKEN_KEY);
            Config.isLoginSuccessful = false;
            _SessionIS = null;
            SceneManager.LoadScene(Config.LOGIN_SCENE);
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Logout thất bại: {e.Message}");
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
    public async void GetUsersWithIds(List<string> userIds = null, List<string> usernames = null, Action<IApiUsers> handleCb = null)
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
    private void _OnConnectCb() { Debug.Log("Socket Connected"); }
    private void _OnCloseCb() { Debug.Log("Socket Closed"); }
    private void _OnErrorCb(Exception exception) { Debug.Log("Socket Error: " + exception.ToString()); }
    public async void PreConnect()
    {

        // _ClientC = new("http", "103.226.250.195", 7353, "defaultkey");
        _ClientC = new("http", "172.16.56.51", 7350, "defaultkey");
        string storedSessionToken = PlayerPrefs.GetString(SESSION);
        _SessionIS = null;
        if (!string.IsNullOrEmpty(storedSessionToken))
        {
            ISession restoredSessionS = Session.Restore(storedSessionToken);
            if (!restoredSessionS.IsExpired) _SessionIS = restoredSessionS;
        }
        string deviceId;
        if (PlayerPrefs.HasKey(DEVICE_ID)) deviceId = PlayerPrefs.GetString(DEVICE_ID);
        else
        {
            deviceId = SystemInfo.deviceUniqueIdentifier;
            if (deviceId == SystemInfo.unsupportedIdentifier) deviceId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(DEVICE_ID, deviceId);
            Config.deviceId = deviceId;
        }
        // if (_SessionIS == null)
        // {
        //     _SessionIS = await _ClientC.AuthenticateDeviceAsync(deviceId);
        //     PlayerPrefs.SetString(SESSION, _SessionIS.AuthToken);
        // }
        _SocketIS = _ClientC.NewSocket();
        _SocketIS.Connected += _OnConnectCb;
        _SocketIS.Closed += _OnCloseCb;
        _SocketIS.ReceivedError += err => _OnErrorCb(err);
        _SocketIS.ReceivedMatchState += state =>
        {
            _DataHandlerAs.Add(() =>
            {
                _PreHandleReceivedMatchState(state);
                for (int i = _ListenerGLs.Count - 1; i >= 0; i--)
                {
                    GameListener gl = _ListenerGLs[i];
                    if (gl.IsReady()) gl.HandleReceivedMatchState(state);
                }
            });
        };
        _SocketIS.ReceivedNotification += notification =>
        {
            _DataHandlerAs.Add(() =>
            {
                _PreHandleNotification(notification);
                for (int i = _ListenerGLs.Count - 1; i >= 0; i--)
                {
                    GameListener gl = _ListenerGLs[i];
                    if (gl.IsReady()) gl.HandleNotification(notification);
                }
            });
        };
    }

    private async UniTask ConnectSocketAsync()
    {
        if (!_SocketIS.IsConnected)
        {
            await _SocketIS.ConnectAsync(_SessionIS, true);
            Debug.Log("🔌 Socket connected.");
        }
    }

    void LateUpdate()
    {
        while (_DataHandlerAs.Count() > 0)
        {
            _DataHandlerAs[0].Invoke();
            _DataHandlerAs.RemoveAt(0);
        }
    }
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
        RestoreSession();
    }
}
