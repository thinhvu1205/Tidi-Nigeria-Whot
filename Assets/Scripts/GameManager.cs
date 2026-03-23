using System.Collections;
using System.Collections.Generic;
using Proto;
using Nakama;
using UnityEngine;
using Yuujins.Api.V1;

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
        Debug.Log("match state "+ matchState.ToString());
        if (UIManager.Instance.gameView != null)
        {
            // Match layer (e.g. opcode 100 roster) — mọi game view
            if (MatchRosterService.DispatchMatchState(matchState))
                return;
            // Client → server only; nếu có echo 101 thì bỏ qua (không có payload roster).
            if (matchState.OpCode == (long)MatchOpCodeUpdate.MatchRequestRosterSnapshot)
                return;

            // Tongits (Nakama): opcode 1–6 = protobuf Yuujins.Api.V1 (TongitsOpCodeUpdate)
            if (UIManager.Instance.gameView is TongitsView &&
                TongitsService.DispatchMatchState(matchState))
            {
                return;
            }
            // Yuujins Baccarat opcodes: 1=GameState, 2=Table, 3=Deal, 4=Finish, 5=Reject
            if (UIManager.Instance.gameView is BaccaratView)
            {
                switch (matchState.OpCode)
                {
                    case 1: UIManager.Instance.gameView.HandleUpdateGameState(matchState); break;
                    case 2: UIManager.Instance.gameView.HandleUpdateTable(matchState); break;
                    case 3: UIManager.Instance.gameView.HandleUpdateDeal(matchState); break;
                    case 4: UIManager.Instance.gameView.HandleFinish(matchState); break;
                    case 5: (UIManager.Instance.gameView as BaccaratView)?.HandleBaccaratReject(matchState); break;
                    default: break;
                }
                return;
            }
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
                    _ = UIManager.Instance.gameView.HandleUpdateKickOffTheTable(matchState);
                    break;
                case (long)OpCodeUpdate.Finish:
                    UIManager.Instance.gameView.HandleFinish(matchState);
                    break; 
                case (long)OpCodeUpdate.OpcodeError:
                    UIManager.Instance.gameView.HandleError(matchState);
                    break;
                case (long) OpCodeUpdate.OpcodeResponseTipIngame:
                    UIManager.Instance.gameView.HandleTipInGame(matchState);
                    break;
                
                // HK Poker OpCodes
                case (long)OpCodeUpdate.PlayerAction: // OPCODE_UPDATE_PLAYER_ACTION
                    UIManager.Instance.gameView.HandleUpdatePlayerAction(matchState);
                    break;
                case (long)OpCodeUpdate.NewRound: // OPCODE_UPDATE_NEW_ROUND
                    UIManager.Instance.gameView.HandleUpdateNewRound(matchState);
                    break;
                case (long)OpCodeUpdate.CardSwap: // OPCODE_UPDATE_CARD_SWAP
                    UIManager.Instance.gameView.HandleUpdateCardSwap(matchState);
                    break;
                case (long)OpCodeUpdate.Showdown: // OPCODE_UPDATE_SHOWDOWN
                    _ = UIManager.Instance.gameView.HandleUpdateShowdown(matchState);
                    break;
                case (long)OpCodeUpdate.BettingState:
                    UIManager.Instance.gameView.HandleBettingState(matchState);
                    break;
                case (long)OpCodeUpdate.ChangeTable:
                    UIManager.Instance.gameView.HandleSwitchTable(matchState);
                    break; 
                
            }
        }
    }
}
