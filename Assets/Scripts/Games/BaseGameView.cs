using System.Collections;
using System.Collections.Generic;
using Proto;
using Games;
using Games.Card;
using Globals;
using Nakama;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using GameState = Proto.GameState;
using System.Linq;
using Cysharp.Threading.Tasks;

public class BaseGameView : BaseView
{
    // public string soundBg = SOUND_GAME.IN_GAME_COMMON;
    
    public GameState GameState { get; protected set; } = GameState.Idle;
    public virtual GameState[] AvailableLeaveStates => new GameState[]{ GameState.Idle, GameState.Matching };
    public virtual bool CanLeaveTable => true;
    protected bool hasBet = false;
    protected bool isPause = false;

    protected override void OnDestroy()
    {
        SoundManager.Instance.StopAllCurrentEffect();
    }

    protected virtual void OnApplicationPause(bool pause)
    {
        isPause = pause;     
    }

    public virtual void LoadInfoMatch(Match match)
    {
        
    }
    
    public virtual void OnClickBack()
    {
        // SoundManager.instance.soundClick();
        var subView = Instantiate(UIManager.Instance.LoadPrefabPopup("GroupMenu"), transform);
        subView.transform.localScale = Vector3.one;
    }
    
    public virtual void OpenRule()
    {

    }

    #region Handle API
    
    public virtual void HandleMatchFound(IMatchmakerMatched matchmakerMatched)
    {
        
    }

    public virtual void HandleMatchJoin(Match match)
    {

    }

    public virtual void HandleMatchPresence(IMatchPresenceEvent presenceEvent)
    {
    }

    public virtual void HandleMatchLeave()
    {
        
    }

    public virtual void HandleUpdateTable(IMatchState matchState)
    {
        if (isPause) return;
    }
    
    public virtual void HandleUpdateUserInTable(IMatchState matchState)
    {
        if (isPause) return;
    }
    
    public virtual void HandleUpdateDeal(IMatchState matchState)
    {
        if (isPause) return;
    }

    public virtual void HandleUpdateTurn(IMatchState matchState)
    {
        if (isPause) return;
    }

    public virtual void HandleUpdateCardState(IMatchState matchState)
    {
        if (isPause) return;
    }

    public virtual void HandleUpdateGameState(IMatchState matchState)
    {
        if (isPause) return;
        UpdateGameState data = UpdateGameState.Parser.ParseFrom(matchState.State);
        GameState = data.State;
    }

    public virtual void HandleUpdateWallet(IMatchState matchState)
    {
        if (isPause) return;
    }

    public virtual async UniTask HandleUpdateKickOffTheTable(IMatchState matchState)
    {
        Debug.Log("kick off the table base view " );
        if (UIManager.Instance.gameView == null) return;
        Destroy(UIManager.Instance.gameView.gameObject);
        UIManager.Instance.gameView = null;
        await NetworkManager.INSTANCE.LeaveRoomChat();
        await NetworkManager.INSTANCE.JoinWorldChat();
        await UIManager.Instance.LoadProfileUser();
        await UIManager.Instance.ReloadTableView();
    }

    public virtual void HandleFinish(IMatchState matchState)
    {
        if (isPause) return;
    }

    public virtual void HandleError(IMatchState matchState)
    {
        if(UIManager.Instance.gameView == null) return;
        var error = Error.Parser.ParseFrom(matchState.State);
        if (error is { ErrorType: ErrorType.CannotLeaveGame})
        {
            UIManager.Instance.ShowToast("You cannot leave while the match is in progress!", 2, transform);
        }
    }

    public virtual void HandleTipInGame(IMatchState matchState)
    {
        
    }
    
    // HK Poker specific handlers
    public virtual void HandleUpdatePlayerAction(IMatchState matchState)
    {
        if (isPause) return;
        // Override in HongKongPokerView
    }
    
    public virtual void HandleUpdateNewRound(IMatchState matchState)
    {
        if (isPause) return;
        // Override in HongKongPokerView
    }
    
    public virtual void HandleUpdateCardSwap(IMatchState matchState)
    {
        if (isPause) return;
        // Override in HongKongPokerView
    }
    
    public virtual async UniTask HandleUpdateShowdown(IMatchState matchState)
    {
        if (isPause) return;
        // Override in HongKongPokerView
    }
    
    public virtual void HandleBettingState(IMatchState matchState)
    {
        if (isPause) return;
        // Override in HongKongPokerView
    }
    
    #endregion
}
