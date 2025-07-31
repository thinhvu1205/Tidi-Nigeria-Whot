using System.Collections;
using System.Collections.Generic;
using Proto;
using Nakama;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    
    public void HandleMatchFound(IMatchmakerMatched matchmakerMatched)
    {
        UIManager.Instance.gameView.HandleMatchFound(matchmakerMatched);
    }

    public void HandleMatchJoin(Match match)
    {
        UIManager.Instance.gameView.HandleMatchJoin(match);
    }

    public void HandleMatchPresence(IMatchPresenceEvent presenceEvent)
    {
        UIManager.Instance.gameView.HandleMatchPresence(presenceEvent);
    }

    public void HandleMatchLeave()
    {
        UIManager.Instance.gameView.HandleMatchLeave();
    }
    
    public void HandleMatchState(IMatchState matchState)
    {
        if (UIManager.Instance.gameView != null)
        {
            switch (matchState.OpCode)
            {
                case (long)OpCodeUpdate.Table:
                    UIManager.Instance.gameView.HandleUpdateTable(matchState);
                    break;
                case (long)OpCodeUpdate.Deal:
                    UIManager.Instance.gameView.HandleUpdateDeal(matchState);
                    break;
                case (long)OpCodeUpdate.CardState:
                    UIManager.Instance.gameView.HandleUpdateCardState(matchState);
                    break;
                case (long)OpCodeUpdate.Turn:
                    UIManager.Instance.gameView.HandleUpdateTurn(matchState);
                    break;
                case (long)OpCodeUpdate.GameState:
                    UIManager.Instance.gameView.HandleUpdateGameState(matchState);
                    break;
                case (long)OpCodeUpdate.OpcodeUserInTableInfo:
                    UIManager.Instance.gameView.HandleUpdateUserInTable(matchState);
                    break;
                case (long)OpCodeUpdate.Wallet:
                    UIManager.Instance.gameView.HandleUpdateWallet(matchState);
                    break;
                case (long)OpCodeUpdate.OpcodeKickOffTheTable:
                    UIManager.Instance.gameView.HandleUpdateKickOffTheTable(matchState);
                    break;
                case (long)OpCodeUpdate.Finish:
                    UIManager.Instance.gameView.HandleFinish(matchState);
                    break; 
            }
        }
    }
}
