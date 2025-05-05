using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Nakama;
using SimpleJSON;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    #region Variables
    public static NetworkManager INSTANCE { get; private set; }
    private NetworkManager() { }
    private const string SESSION = "session", DEVICE_ID = "deviceId";
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
        _PreHandleData(apiName, obj);
        for (int i = _ListenerGLs.Count - 1; i >= 0; i--)
        {
            GameListener gl = _ListenerGLs[i];
            if (gl.IsReady()) gl.HandleData(apiName, obj);
        }
    }
    #endregion

    #region Match
    public void JoinMatch(string matchId)
    {
        _MatchId = "";
        _SocketIS.JoinMatchAsync(matchId);
    }
    public void LeaveMatch() => _SocketIS.LeaveMatchAsync(_MatchId);
    public void SendMatchState(long opCode, string data) => _SocketIS.SendMatchStateAsync(_MatchId, opCode, data);
    #endregion

    #region Friends
    public async void GetListFriends(int state, int limit, string cursor, Action<IApiFriendList> handleCb = null)
    {
        IApiFriendList iafl = await _ClientC.ListFriendsAsync(_SessionIS, state, limit, cursor);
        if (iafl == null || iafl.Friends.Count() <= 0) return;
        handleCb?.Invoke(iafl);
    }
    public async void GetUsersWithIds(List<string> userIds = null, List<string> usernames = null, Action<IApiUsers> handleCb = null)
    {
        if ((userIds == null || userIds.Count <= 0) && (usernames == null || usernames.Count <= 0)) return;
        IApiUsers users = await _ClientC.GetUsersAsync(_SessionIS, userIds, usernames);
        if (users.Users == null || users.Users.Count() <= 0) return;
        handleCb?.Invoke(users);
    }
    public async void AddFriend(string userId, Action handleCb = null)
    {
        if (string.IsNullOrEmpty(userId)) return;
        await _ClientC.AddFriendsAsync(_SessionIS, new[] { userId });
        handleCb?.Invoke();
    }
    #endregion
    private void _OnConnectCb() { Debug.Log("Socket Connected"); }
    private void _OnCloseCb() { Debug.Log("Socket Closed"); }
    private void _OnErrorCb(Exception exception) { Debug.Log("Socket Error: " + exception.ToString()); }
    public async void Connect()
    {
        _ClientC = new("http", "103.226.250.195", 7354, "defaultkey");
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
        }
        if (_SessionIS == null)
        {
            _SessionIS = await _ClientC.AuthenticateDeviceAsync(deviceId);
            PlayerPrefs.SetString(SESSION, _SessionIS.AuthToken);
        }
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
        await _SocketIS.ConnectAsync(_SessionIS, true);
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
    }
}
