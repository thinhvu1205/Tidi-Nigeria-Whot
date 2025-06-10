using System.Collections;
using System.Collections.Generic;
using Nakama;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private IGameHandler _currentHandler;

    void Awake() {
        Instance = this;
    }

    public void SetGameHandler(IGameHandler handler) {
        _currentHandler = handler;
    }

    public void HandleMatchJoin(IApiMatch match) {
        _currentHandler?.OnMatchJoin(match);
    }

    public void HandleMatchState(IMatchState state) {
        _currentHandler?.OnMatchState(state);
    }

    public void HandleMatchPresence(IMatchPresenceEvent presenceEvent) {
        _currentHandler?.OnMatchPresence(presenceEvent);
    }

    public void HandleMatchLeave() {
        _currentHandler?.OnMatchLeave();
    }
}
