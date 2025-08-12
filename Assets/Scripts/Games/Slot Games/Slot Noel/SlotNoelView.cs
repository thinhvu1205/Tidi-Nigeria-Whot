using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto;
using Globals;
using Nakama;
using UnityEngine;
using Color = UnityEngine.Color;

public class SlotNoelView : BaseSlotView
{
    protected override Dictionary<SiXiangSymbol, int> SymbolDictionary => new()
    {
        { SiXiangSymbol.K, 0 },
        { SiXiangSymbol.J, 1 },
        { SiXiangSymbol.A, 2 },
        { SiXiangSymbol.Q, 3 },
        { SiXiangSymbol.SuitHearts, 4 },
        { SiXiangSymbol.SuitDiamonds, 5 },
        { SiXiangSymbol.SuitSpades, 6 },
        { SiXiangSymbol.SuitClubs, 7 },
        { SiXiangSymbol.ChrismasRing, 8 },
        { SiXiangSymbol.ChrismasCandy, 9 },
        { SiXiangSymbol.ChrismasGift, 10 },
        { SiXiangSymbol.Wild, 11 },
        { SiXiangSymbol.Scatter, 12 }
    };
    protected override string SOUND_BACKGROUND_ANIMATION_PATH => SoundSlot.BG_NOEL;
    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    public void Init()
    {
   
    }
    
    #region handle Match

    public override void HandleMatchFound(IMatchmakerMatched matchmakerMatched)
    {
        base.HandleMatchFound(matchmakerMatched);
    }

    public override void HandleMatchPresence(IMatchPresenceEvent presenceEvent)
    {
        base.HandleMatchPresence(presenceEvent);
    }

    public override void HandleMatchLeave()
    {
        base.HandleMatchLeave();
    }

    public override void HandleUpdateTable(IMatchState matchState)
    {
        base.HandleUpdateTable(matchState);
    }

    public override void HandleUpdateDeal(IMatchState matchState)
    {
        base.HandleUpdateDeal(matchState);
    }

    public override void HandleUpdateTurn(IMatchState matchState)
    {
        base.HandleUpdateTurn(matchState);
    }

    public override void HandleUpdateCardState(IMatchState matchState)
    {
        base.HandleUpdateCardState(matchState);
    }

    public override void HandleUpdateGameState(IMatchState matchState)
    {
        base.HandleUpdateGameState(matchState);
    }

    public override void HandleUpdateWallet(IMatchState matchState)
    {
        base.HandleUpdateWallet(matchState);
    }

    public override void HandleUpdateKickOffTheTable(IMatchState matchState)
    {
        base.HandleUpdateKickOffTheTable(matchState);
    }

    public override void HandleFinish(IMatchState matchState)
    {
        base.HandleFinish(matchState);
    }

    #endregion
    
    protected override void SetSpinAnimation(SpinType type)
    {
        buttonSpinAnimation.startingAnimation = type switch
        {
            SpinType.NORMAL => "autospin",
            SpinType.FREE_NORMAL or SpinType.FREE_AUTO => "freespin",
            SpinType.AUTO => "stop",
            _ => "autospin"
        };
    }
}
