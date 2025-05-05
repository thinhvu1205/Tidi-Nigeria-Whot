using System.Collections;
using System.Collections.Generic;
using Nakama;
using SimpleJSON;
using UnityEngine;

public class GameListener : MonoBehaviour
{
    private bool _IsReady;
    public bool IsReady() { return _IsReady; }
    public virtual void HandleData(string apiName, JSONObject data) { }
    public virtual void HandleReceivedMatchState(IMatchState data) { }
    public virtual void HandleNotification(IApiNotification data) { }

    protected virtual void OnDestroy() => NetworkManager.INSTANCE.RemoveListener(this);
    protected virtual void OnDisable() => _IsReady = false;
    protected virtual void OnEnable() => _IsReady = true;
    protected virtual void Awake() => NetworkManager.INSTANCE.AddListener(this);
}
