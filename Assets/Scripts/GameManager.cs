using System.Collections;
using System.Collections.Generic;
using Nakama;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private IGameHandler _currentHandler;

    public void SetGameHandler(IGameHandler handler) {
        _currentHandler = handler;
    }
    
    public void HandleMatchFound(IMatchmakerMatched matchmakerMatched)
    {
        Debug.Log("Match found: " + matchmakerMatched.MatchId);
        _currentHandler?.OnMatchFound(matchmakerMatched);
    }

    public void HandleMatchJoin(IMatch match)
    {
        Debug.Log("Joining match: " + match.ToString());
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
