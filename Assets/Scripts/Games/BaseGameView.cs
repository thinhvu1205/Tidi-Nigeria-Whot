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

public class BaseGameView : BaseView
{
    // public string soundBg = SOUND_GAME.IN_GAME_COMMON;
    
    public GameState GameState { get; protected set; } = GameState.Idle;
    public virtual GameState[] AvailableLeaveStates => new GameState[]{ GameState.Idle, GameState.Matching };
    public virtual bool CanLeaveTable => AvailableLeaveStates.Contains(GameState) && !hasBet;
    protected bool hasBet = false;
    protected override void OnDestroy()
    {
        SoundManager.Instance.StopAllCurrentEffect();
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
        
    }
    
    public virtual void HandleUpdateUserInTable(IMatchState matchState)
    {
        
    }
    
    public virtual void HandleUpdateDeal(IMatchState matchState)
    {
        
    }

    public virtual void HandleUpdateTurn(IMatchState matchState)
    {
        
    }

    public virtual void HandleUpdateCardState(IMatchState matchState)
    {
        
    }

    public virtual void HandleUpdateGameState(IMatchState matchState)
    {
        UpdateGameState data = UpdateGameState.Parser.ParseFrom(matchState.State);
        GameState = data.State;
    }

    public virtual void HandleUpdateWallet(IMatchState matchState)
    {
        
    }

    public virtual void HandleUpdateKickOffTheTable(IMatchState matchState)
    {
        if (UIManager.Instance.gameView == null) return;
        Destroy(UIManager.Instance.gameView.gameObject);
        UIManager.Instance.gameView = null;
    }

    public virtual void HandleFinish(IMatchState matchState)
    {
        
    }
    
    #endregion
}
