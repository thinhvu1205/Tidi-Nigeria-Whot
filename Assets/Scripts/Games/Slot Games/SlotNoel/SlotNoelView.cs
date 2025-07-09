using System.Collections;
using System.Collections.Generic;
using Globals;
using Nakama;
using UnityEngine;

public class SlotNoelView : BaseSlotView
{
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

    public override void HandleMatchJoin(IMatch match)
    {
        base.HandleMatchJoin(match);
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

    protected override void UpdateSpinButtonUI()
    {
        base.UpdateSpinButtonUI();

        // Mặc định màu trắng
        buttonSpinAnimation.color = Color.white;

        // Hàm set animation theo loại spin


        // Xử lý theo state
        switch (gameState)
        {
            case SlotGameState.SPINNING:
                SetSpinAnimation(spinType);

                // Nếu đang spin mà không phải auto, set màu xám
                if (spinType == SpinType.NORMAL || spinType == SpinType.FREE_NORMAL)
                {
                    buttonSpinAnimation.color = Color.gray;
                }
                break;

            case SlotGameState.SHOWING_RESULT:
                SetSpinAnimation(spinType);
                break;

            case SlotGameState.PREPARE:
            case SlotGameState.JOIN_GAME:
                SetSpinAnimation(spinType);

                // Nếu hết tiền và không phải free spin => disable
                // if (listBetRoom.Count > 0 && agPlayer < totalListBetRoom[currentMarkBet] && !isFreeSpin)
                // {
                //     buttonSpinAnimation.color = Color.gray;
                // }
             
                break;
        }

        buttonSpinAnimation.Initialize(true);
    }
    
    private void SetSpinAnimation(SpinType type)
    {
        buttonSpinAnimation.startingAnimation = type switch
        {
            SpinType.NORMAL => "autospin",
            SpinType.FREE_NORMAL => "freespin",
            SpinType.AUTO or SpinType.FREE_AUTO => "stop",
            _ => "autospin"
        };
    }
}
