using System.Collections;
using System.Collections.Generic;
using Api;
using Nakama;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private IGameHandler _currentHandler;

    public void SetGameHandler(IGameHandler handler)
    {
        _currentHandler = handler;
    }

    public void HandleMatchFound(IMatchmakerMatched matchmakerMatched)
    {
        _currentHandler?.OnMatchFound(matchmakerMatched);
    }

    public void HandleMatchJoin(IMatch match)
    {
        _currentHandler?.OnMatchJoin(match);
    }

    public void HandleMatchPresence(IMatchPresenceEvent presenceEvent)
    {
        _currentHandler?.OnMatchPresence(presenceEvent);
    }

    public void HandleMatchLeave()
    {
        _currentHandler?.OnMatchLeave();
    }
    
    public void HandleMatchState(IMatchState matchState)
    {
        switch (matchState.OpCode)
        {
            case (long)OpCodeUpdate.Table:
                _currentHandler.OnUpdateTable(matchState);
                break;
            case (long)OpCodeUpdate.Deal:
                _currentHandler.OnUpdateDeal(matchState);
                break;
            case (long)OpCodeUpdate.CardState:
                _currentHandler.OnUpdateCardState(matchState);
                break;
            case (long)OpCodeUpdate.Turn:
                _currentHandler.OnUpdateTurn(matchState);
                break;
            case (long)OpCodeUpdate.GameState:
                _currentHandler.OnUpdateGameState(matchState);
                break;
            case (long)OpCodeUpdate.Wallet:
                Debug.Log("OpCodeUpdate.Wallet " + matchState.ToString());
                break;
            case (long)OpCodeUpdate.OpcodeKickOffTheTable:
                Debug.Log("OpCodeUpdate.OpcodeKickOffTheTable " + matchState.ToString());
                break;
            case (long)OpCodeUpdate.Finish:
               _currentHandler.OnFinish(matchState);
                break; 
        }
    }
}
