using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto;
using Globals;
using Nakama;
using UnityEngine;
using Color = UnityEngine.Color;
using DG.Tweening;
using UnityEngine.UI;

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
    
    protected override void ShowWinAnimation(WinType winType)
    {
        float delay = 7f;
        effectContainer.gameObject.SetActive(true);
        animationEffect.gameObject.SetActive(true);

        switch (winType)
        {
            case WinType.BIG_WIN:
                SoundManager.Instance.PlayEffectFromPath(SoundSlot.BIG_WIN);
                bigWinText.transform.parent.gameObject.SetActive(true);
                bigWinText.gameObject.SetActive(true);
                Utility.TweenNumberToNumber(bigWinText, totalChipWinByGame, 0, delay - 1);
                // animationEffect.transform.localScale = new Vector2(0.9f, 0.9f);
                animationEffect.transform.localScale = new Vector2(1f, 1f);
                // animationEffect.transform.localPosition = new Vector2(0, -70);
                Utility.PlayAnimationByPath(animationEffect, BIG_WIN_ANIMATION_PATH, BIG_WIN_ANIMATION_NAME, false);
                delay = 5.5f;
                break;
            case WinType.MEGA_WIN:
                SoundManager.Instance.PlayEffectFromPath(SoundSlot.MEGA_WIN);
                bigWinText.transform.parent.gameObject.SetActive(true);
                bigWinText.gameObject.SetActive(true);
                Utility.TweenNumberToNumber(bigWinText, totalChipWinByGame, 0, delay - 1);
                // animationEffect.transform.localScale = new Vector2(0.9f, 0.9f);
                animationEffect.transform.localScale = new Vector2(1f, 1f);
                // animationEffect.transform.localPosition = new Vector2(0, -70);
                Utility.PlayAnimationByPath(animationEffect, MEGA_WIN_ANIMATION_PATH, MEGA_WIN_ANIMATION_NAME, false);
                break;
            case WinType.HUGE_WIN:
                SoundManager.Instance.PlayEffectFromPath(SoundSlot.MEGA_WIN);
                bigWinText.transform.parent.gameObject.SetActive(true);
                bigWinText.gameObject.SetActive(true);
                Utility.TweenNumberToNumber(bigWinText, totalChipWinByGame, 0, delay - 1);
                // animationEffect.transform.localScale = new Vector2(0.9f, 0.9f);
                animationEffect.transform.localScale = new Vector2(1f, 1f);
                // animationEffect.transform.localPosition = new Vector2(0, -70);
                Utility.PlayAnimationByPath(animationEffect, HUGE_WIN_ANIMATION_PATH, HUGE_WIN_ANIMATION_NAME, false);
                break;
            case WinType.FIVE_OF_A_KIND:
                animationEffect.transform.localScale = Vector2.one;
                animationEffect.transform.localPosition = Vector2.zero;
                bigWinText.transform.parent.gameObject.SetActive(false);
                Utility.PlayAnimationByPath(animationEffect, FIVE_OF_A_KIND_ANIMATION_PATH, FIVE_OF_A_KIND_ANIMATION_NAME, false);
                break;
            case WinType.FREE_SPIN:
                SoundManager.Instance.PlayEffectFromPath(SoundSlot.FREESPIN);
                animationEffect.transform.localScale = Vector2.one;
                animationEffect.transform.localPosition = Vector2.zero;
                bigWinText.transform.parent.gameObject.SetActive(false);
                Utility.PlayAnimationByPath(animationEffect, FREE_SPIN_ANIMATION_PATH, FREE_SPIN_ANIMATION_NAME, false);
                break;
        }
        if (new WinType[] { WinType.BIG_WIN, WinType.HUGE_WIN, WinType.MEGA_WIN }.Contains(winType))
        {
            DOVirtual.DelayedCall(delay, () =>
            {
                bigWinText.transform.parent.gameObject.SetActive(false);
            });
        }

        animationEffect.AnimationState.Complete += delegate
        {
            effectContainer.gameObject.SetActive(false);
            if (new WinType[] { WinType.BIG_WIN, WinType.HUGE_WIN, WinType.MEGA_WIN }.Contains(winType))
            {
                DOVirtual.DelayedCall(delay, () =>
                {
                    bigWinText.transform.parent.gameObject.SetActive(false);
                    AnimateCoinsFly();
                });
            }
            NextTween();
            effectContainer.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
        };
        
    }
}
