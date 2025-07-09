using System.Collections;
using System.Collections.Generic;
using Api;
using Nakama;
using UnityEngine;

public class BaseGameView : BaseView
{
    public virtual void OpenRule()
    {

    }

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
        
    }

    public virtual void HandleUpdateWallet(IMatchState matchState)
    {
        
    }

    public virtual void HandleUpdateKickOffTheTable(IMatchState matchState)
    {
        
    }

    public virtual void HandleFinish(IMatchState matchState)
    {
        
    }
}
