using System.Collections.Generic;
using Proto;
using DG.Tweening;
using Globals;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Nakama;
using System.Linq;

public class SlotSixiangView : BaseSlotSymbolView
{
    [SerializeField] private Transform backgroundGoldPick;
    [SerializeField] private SkeletonGraphic animationGameName, animationAnimal, animationCutScene;
    [SerializeField] private SixiangChooseGameBonus chooseGameBonus;

    protected override Dictionary<SiXiangSymbol, int> SymbolDictionary => new()
    {
        { SiXiangSymbol._10, 0 },
        { SiXiangSymbol.J, 1 },
        { SiXiangSymbol.Q, 2 },
        { SiXiangSymbol.K, 3 },
        { SiXiangSymbol.A, 4 },
        { SiXiangSymbol.BlueDragon, 5 },
        { SiXiangSymbol.WhiteTiger, 6 },
        { SiXiangSymbol.Warrior, 7 },
        { SiXiangSymbol.VermilionBird, 8 },
        { SiXiangSymbol.Scatter, 9 },
        { SiXiangSymbol.Wild, 10 },

    };

    protected override Dictionary<SiXiangGame, int> GemDictionary => new()
    {
        { SiXiangGame.DragonPearl, 0 },
        { SiXiangGame.Goldpick, 1 },
        { SiXiangGame.Rapidpay, 2 },
        { SiXiangGame.Luckdraw, 3 },
    };
    protected override string SOUND_BACKGROUND_ANIMATION_PATH => SoundSlot.BG_SIXIANG;
    private const string SIXIANG_GAME_NAME = "sixiang";
    private const string SIXIANG_BACKGROUND_ANIMATION_PATH = "SiXiang/Spine/BgGame/skeleton_SkeletonData";
    private const string SCATTER_PREFAB_PATH = "Sixiang/ScatterView";
    private const string DRAGON_PEARL_GAME_NAME = "dragonpearl";
    private const string DRAGON_PEARL_BACKGROUND_ANIMATION_PATH = "SiXiang/Spine/DragonPearl/BgGame/skeleton_SkeletonData";
    private const string DRAGON_PEARL_ANIMAL_ANIMATION_PATH = "SiXiang/Spine/Animal/Dragon/skeleton_SkeletonData";
    private const string GOLD_PICK_PREFAB_PATH = "Sixiang/GoldPickView";
    private const string GOLD_PICK_ANIMAL_ANIMATION_PATH = "SiXiang/Spine/Animal/Tiger/skeleton_SkeletonData";
    private const string GOLD_PICK_ANIMAL_ANIMATION_NAME = "3";
    private const string RAPID_PAY_GAME_NAME = "rapidplay";
    private const string RAPID_PAY_PREFAB_PATH = "Sixiang/RapidPayView";
    private const string RAPID_PAY_ANIMAL_ANIMATION_PATH = "SiXiang/Spine/Animal/Phoenix/skeleton_SkeletonData";
    private const string LUCKY_DRAW_GAME_NAME = "luckydraw";
    private const string LUCKY_DRAW_BACKGROUND_ANIMATION_PATH = "SiXiang/Spine/LuckyDraw/BgGame/skeleton_SkeletonData";
    private const string LUCKY_DRAW_PREFAB_PATH = "Sixiang/LuckyDrawView";
    private const string LUCKY_DRAW_ANIMAL_ANIMATION_PATH = "SiXiang/Spine/Animal/Turle/skeleton_SkeletonData";
    private const string MONEY_WIN_ANIMATION_PATH = "SiXiang/Spine/BigWinGoldPick/skeleton_SkeletonData";
    private const string WIN_JACKPOT_ANIMATION_PATH = "SiXiang/Spine/LuckyDraw/BigWin/skeleton_SkeletonData";
    protected override string SPECIAL_WIN_ANIMATION_PATH => "SiXiang/Spine/FourWin/skeleton_SkeletonData";

    private SiXiangScatterView scatterView;
    private SiXiangRapidPayView rapidPayView;
    private SiXiangGoldPickView goldPickView;
    private SiXiangLuckyDrawView luckyDrawView;
    public Queue<TweenCallback> TweenQueue => tweenQueue;

    public class OnUpdateTableEventArgs : EventArgs
    {
        public SlotDesk data;
    }

    public event Action<OnUpdateTableEventArgs> OnUpdateTable;


    public override void HandleUpdateTable(IMatchState matchState)
    {
        SlotDesk data = SlotDesk.Parser.ParseFrom(matchState.State);
        Debug.Log("Slot : " + data.ToString());
        listSpinSymbol = data.Matrix.SpinLists.ToList();
        listGem = data.SixiangGems.ToList();
        gemPrice = data.ChipsBuyGem;
        winType = data.BigWin switch
        {
            BigWin.Nice => WinType.NICE_WIN,
            BigWin.Huge => WinType.HUGE_WIN,
            BigWin.Big => WinType.BIG_WIN,
            BigWin.Mega => WinType.MEGA_WIN,
            _ => WinType.NONE,
        };

        CheckBonusGame(data);
        UpdateColumnView(data);
        UpdateReward(data);

        if (!hasSetupStartView)
        {
            SetupStartView(data);
            UpdateJackpot(data);
            UpdateGem();
        }
        else
        {
            if (IsSpinning)
            {
                OnStartSpin();
            }
            else
            {

                UpdateJackpot(data);
                UpdateGem();
            }
        }

        OnUpdateTable?.Invoke(new OnUpdateTableEventArgs
        {
            data = data
        });
                // ShowScatterView();

        if (!hasSetupStartView) hasSetupStartView = true;
    }

    protected override void OnStartSpin()
    {
        IsSpinning = true;
        if (IsInDragonPearl())
        {
            freeSpinLeft--;
            UpdateDragonPearlFreeSpinLeft();
            UpdateGameState(SlotGameState.SPINNING);
            SetDarkAllItems();
            SoundManager.Instance.PlayEffectFromPath(SoundSlot.SPIN_REEL);
            foreach (SlotSymbolColumn column in listColumn)
            {
                column.SetRandomFinishView();
                column.StartSpin(spinType);
            }
        }
        else
        {
            base.OnStartSpin();
        }
    }

    public override void OnStopSpin()
    {
        IsSpinning = false;
        if (IsInDragonPearl())
        {
            SetDarkAllItems();
            tweenQueue.Enqueue(() => dragonPearlView.OnStopSpin());
            NextTween();
        }
        else
        {
            IsSpinning = false;
            HideThirdScatter();
            ///------------------CHECK SPREAD WILD--------------------///
            if (CheckWild())
            {
                tweenQueue.Enqueue(() => ShowAnimationWild());
                tweenQueue.Enqueue(() => ShowSpreadWild());
            }

            ///------------------CHECK SHOW ALL LINE--------------------///
            if (listPayline.Count > 0)
            {
                tweenQueue.Enqueue(() => ShowAllWinLines());
            }

            ///------------------CHECK SHOW TYPE WIN--------------------///
            if (!isInFreeSpin)
            {
                switch (winType)
                {
                    case WinType.BIG_WIN:
                        tweenQueue.Enqueue(() => ShowSpecialWinAnimation(WinType.BIG_WIN, currentChipWin));
                        break;
                    case WinType.MEGA_WIN:
                        tweenQueue.Enqueue(() => ShowSpecialWinAnimation(WinType.MEGA_WIN, currentChipWin));
                        break;
                    case WinType.HUGE_WIN:
                        tweenQueue.Enqueue(() => ShowSpecialWinAnimation(WinType.HUGE_WIN, currentChipWin));
                        break;
                }
            }

            ///------------------CHECK SHOW ONE BY ONE--------------------//
            if (listPayline.Count > 0)
            {
                if (spinType == SpinType.NORMAL)
                {
                    // if (freespinLeft == 0) listActionHandleSpin.Add(acShowOneWinLine);
                    tweenQueue.Enqueue(() => ShowSymbolOneByOne());
                }
                else if (spinType == SpinType.AUTO || spinType == SpinType.FREE_AUTO)
                {
                    if (listPayline.Count == 1) tweenQueue.Enqueue(() => ShowSymbolOneByOne());
                    // if (!isInFreeSpin) listActionHandleSpin.Add(acShowAnimChipBay);
                }
            }

            ///------------------CHECK SHOW WIN SCATTER--------------------///
            if (CheckWinThirdScatter())
            {
                SoundManager.Instance.PlayEffectFromPath(SoundSlot.SCATTER_WIN);
                tweenQueue.Enqueue(() => ShowAnimationCutScene());
                tweenQueue.Enqueue(() => ShowScatterView());
            }
            NextTween();
        }
    }

    private void ShowScatterView()
    {
        if (scatterView == null)
        {
            scatterView = Instantiate(UIManager.Instance.LoadPrefabGame(SCATTER_PREFAB_PATH), transform).GetComponent<SiXiangScatterView>();
            scatterView.transform.SetSiblingIndex(animationCutScene.transform.GetSiblingIndex() - 1);
        }
        else
        {
            scatterView.gameObject.SetActive(true);
        }
        scatterView.SetInfo(this, currentBetLevel);
    }

    protected override void UpdateReward(SlotDesk data)
    {
        base.UpdateReward(data);
        if (IsInDragonPearl())
        {
            isInFreeSpin = true;
            if (isInFreeSpin)
            {
                // spinType = SpinType.FREE_AUTO;
                UpdateSpinButtonUI();
            }
        }
    }

    #region Bonus Game
    protected void CheckBonusGame(SlotDesk data)
    {
        currentGame = data.CurrentSixiangGame;
        nextGame = data.NextSixiangGame;
        isInSixiangBonus = data.IsInSixiangBonus;
        if (nextGame == SiXiangGame.Normal) return;
        SetCurrentChipValue(data.GameReward.BalanceChipsWalletAfter);
        isChooseBonusGame = data.SixiangGems.Count == 4;
        Dictionary<SiXiangGame, int> miniGameDictionary = new()
        {
            { SiXiangGame.DragonPearl, 0 },
            { SiXiangGame.Goldpick, 1 },
            { SiXiangGame.Rapidpay, 2 },
            { SiXiangGame.Luckdraw, 3 },
        };
        Dictionary<SiXiangGame, int> bonusGameDictionary = new()
        {
            { SiXiangGame.SixangbonusDragonPearl, 0 },
            { SiXiangGame.SixangbonusGoldpick, 1 },
            { SiXiangGame.SixangbonusRapidpay, 2 },
            { SiXiangGame.SixangbonusLuckdraw, 3 },
        };

        // Chọn miễn phí 1 trong 4 bonus game
        if (currentGame == SiXiangGame.Sixangbonus)
        {
            if (nextGame== SiXiangGame.Sixangbonus)
            {
                ShowChooseBonusGame();
            }
            else if (bonusGameDictionary.TryGetValue(data.NextSixiangGame, out int bonusIndex))
            {
                OnSelectBonusGame(bonusIndex);
            }
            AnimateHideGemButtons();
        }

        // Mua 1 trong 4 bonus game
        if (data.InfoBet.EmitNewgameEvent && miniGameDictionary.TryGetValue(currentGame, out int index))
        {
            OnSelectBonusGame(index);
            AnimateHideGemButtons();
        }

    }
    #endregion

    private void OnSelectBonusGame(int index)
    {
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.SHOW_ANIMAL);
        foreach (Button button in listBuyGemButton)
        {
            button.interactable = false;
        }
        if (chooseGameBonus.gameObject.activeSelf)
        {
            chooseGameBonus.OnClose();
        }
        effectContainer.gameObject.SetActive(true);
        animationAnimal.transform.parent.gameObject.SetActive(true);
        animationAnimal.gameObject.SetActive(true);
        paylineInfoContainer.gameObject.SetActive(false);
        totalChipWinByGame = 0;
        string animationPath = "", animationName = "";
        tweenQueue.Enqueue(() => ShowAnimationCutScene());

        switch (index)
        {
            case 0:
                animationPath = DRAGON_PEARL_ANIMAL_ANIMATION_PATH;
                animationName = "animation";
                paylineInfoContainer.gameObject.SetActive(true);
                tweenQueue.Enqueue(() => ShowDragonPearlView());
                break;
            case 1:
                animationPath = GOLD_PICK_ANIMAL_ANIMATION_PATH;
                animationName = GOLD_PICK_ANIMAL_ANIMATION_NAME;
                tweenQueue.Enqueue(() => ShowGoldPickView());
                break;
            case 2:
                animationPath = RAPID_PAY_ANIMAL_ANIMATION_PATH;
                animationName = "animation";
                tweenQueue.Enqueue(() => ShowRapidPayView());
                break;
            case 3:
                animationPath = LUCKY_DRAW_ANIMAL_ANIMATION_PATH;
                animationName = "animation";
                tweenQueue.Enqueue(() => ShowLuckyDrawView());
                break;
        }
        Utility.PlayAnimationByPath(animationAnimal, animationPath, animationName, false);

        animationAnimal.AnimationState.Complete += delegate
        {
            animationAnimal.transform.parent.gameObject.SetActive(false);
            animationAnimal.gameObject.SetActive(false);
            effectContainer.gameObject.SetActive(false);
            // Show animation Cut Scene
            NextTween();
        };
    }

    public void ShowAnimationCutScene(bool isEndBonusGame = false)
    {
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.CUT_SCENE);
        animationCutScene.gameObject.SetActive(true);
        Utility.PlayAnimation(animationCutScene, "animation", false);
        DOTween.Sequence().AppendInterval(1.7f).AppendCallback(() =>
        {
            spinType = SpinType.NORMAL;
            UpdateSpinButtonUI();
            HideThirdScatter();
            if (isEndBonusGame)
            {
                HideBackgroundGoldPick();
                columnContainer.gameObject.SetActive(true);
                paylineInfoContainer.gameObject.SetActive(true);
                dragonPearlView.gameObject.SetActive(false);
                if (rapidPayView != null)
                {
                    rapidPayView.gameObject.SetActive(false);
                }
                if (luckyDrawView != null)
                {
                    luckyDrawView.gameObject.SetActive(false);
                }
            }
            else
            {
                // Show BonusGame view
                HideAllGemButtons();
                NextTween();
            }
        }
        );
        animationCutScene.AnimationState.Complete += delegate
        {
            animationAnimal.transform.parent.gameObject.SetActive(false);
            animationAnimal.gameObject.SetActive(false);
            effectContainer.gameObject.SetActive(false);

            if (isEndBonusGame)
            {
                OnFinishBonusGame();
            }
        };
    }

    private void ShowChooseBonusGame()
    {
        chooseGameBonus.gameObject.SetActive(true);
    }

    #region Dragon Pearl
    public void ShowDragonPearlView()
    {
        Debug.Log("SHOW DRAGON PEARL VIEW");
        dragonPearlView.gameObject.SetActive(true);
        dragonPearlView.SetInfo(this);
        SetAnimationGameName(DRAGON_PEARL_GAME_NAME);
        SetAnimationBackground(DRAGON_PEARL_GAME_NAME);
        spinType = SpinType.FREE_NORMAL;
        UpdateSpinButtonUI();
    }

    public void UpdateDragonPearlFreeSpinLeft(bool isWithShadow = false)
    {
        Debug.Log("ANIMATE DRAGON PEARL FREE SPIN LEFT");
        textInfoSession.gameObject.SetActive(true);
        if (isWithShadow)
        {
            AnimateShadowDragonPearlFreeSpinLeft();
        }
        else
        {
            string text = freeSpinLeft + " remaining spins";
            SetInfoSessionText(text);
        }
    }

    public void AnimateShadowDragonPearlFreeSpinLeft()
    {
        Debug.Log("ANIMATE SHADOW");
        TextMeshProUGUI shadowText = Instantiate(textInfoSession.gameObject, paylineInfoContainer.transform).GetComponent<TextMeshProUGUI>();

        shadowText.transform.localPosition = transform.InverseTransformPoint(textInfoSession.transform.position);
        shadowText.transform.DOScale(new Vector2(1.3f, 1.3f), 1.0f).SetEase(Ease.OutSine);
        shadowText.DOFade(0, 1.0f).SetEase(Ease.OutSine);
        shadowText.transform.DOLocalMoveY(1.0f, shadowText.transform.localPosition.y + 50).SetEase(Ease.OutSine).OnComplete(() =>
        {
            // shadowText.gameObject.SetActive(false);
        });
    }

    public void OnFinishDragonPearl(bool isWinGrandJackpot)
    {
        SoundManager.Instance.PlayMusicInGame(SOUND_BACKGROUND_ANIMATION_PATH);
        SetLightAllItems();
        foreach (SlotSymbolColumn column in listColumn)
        {
            column.UpdateStartViewUI();
        }
        spinType = SpinType.NORMAL;
        UpdateSpinButtonUI();
        if (isWinGrandJackpot)
        {
            tweenQueue.Enqueue(() =>
            {
                ShowAnimationWinJackpot(WinJackpotType.JACKPOT_GRAND);
            });
        }
        tweenQueue.Enqueue(() =>
        {
            ShowAnimationResultMoney(totalChipWinByGame);
        });
        tweenQueue.Enqueue(() =>
        {
            ShowAnimationCutScene(true);
        });
        NextTween();
            // await showResultMoneyAnim(PATH_ANIM_WINRESULT_DP, "eng", winAmount, new Vector2(0, -68));
    }

    private void ShowAnimationWinJackpot(WinJackpotType winJackpotType)
    {
        string animationName = "";
        switch (winJackpotType)
        {
            case WinJackpotType.JACKPOT_MINOR:
                {
                    animationName = "minor";
                    break;
                }
            case WinJackpotType.JACKPOT_MAJOR:
                {
                    animationName = "major";
                    break;
                }
            case WinJackpotType.JACKPOT_MEGA:
                {
                    animationName = "mega";
                    break;
                }
            case WinJackpotType.JACKPOT_GRAND:
                {
                    animationName = "grand";
                    break;
                }
        }
        effectContainer.gameObject.SetActive(true);
        animationJackpotWin.gameObject.SetActive(true);
        animationJackpotWin.transform.parent.gameObject.SetActive(true);
        textJackpotWin.gameObject.SetActive(true);
        Utility.PlayAnimationByPath(animationBackgroundMoney, SPECIAL_WIN_ANIMATION_PATH, "money", true);
        Utility.PlayAnimationByPath(animationJackpotWin, WIN_JACKPOT_ANIMATION_PATH, animationName, false);
        textJackpotWin.transform.localPosition = new Vector2(0, -70);
        buttonConfirmJackpotWin.gameObject.SetActive(false);
        textJackpotWin.ResetValue();
        AudioSource soundJackpot = SoundManager.Instance.PlayEffectFromPath(SoundSlot.WIN_JACKPOT_START);
        textJackpotWin.SetValue(totalChipWinByGame, true, 4.0f, "", () =>
        {
            soundJackpot.Stop();
            SoundManager.Instance.PlayEffectFromPath(SoundSlot.WIN_JACKPOT_END);
        });
        animationJackpotWin.AnimationState.Complete += delegate
        {
            buttonConfirmJackpotWin.gameObject.SetActive(true);
            buttonConfirmJackpotWin.onClick.RemoveAllListeners();
            buttonConfirmJackpotWin.onClick.AddListener(() =>
            {
                NextTween();
            });
        };
    }

    private void ShowAnimationResultMoney(long winAmount)
    {
        Debug.Log("ANIMATE RESULT MONEY");
        effectContainer.gameObject.SetActive(true);

        DOTween.Sequence()
            .AppendInterval(0.2f)
            .AppendCallback(() =>
            {
                Utility.PlayAnimationByPath(animationBackgroundMoney, SPECIAL_WIN_ANIMATION_PATH, "money", true);
                Utility.PlayAnimationByPath(animationJackpotWin, MONEY_WIN_ANIMATION_PATH, "eng", false);
                animationJackpotWin.transform.parent.gameObject.SetActive(true);
                textJackpotWin.gameObject.SetActive(true);
                textJackpotWin.transform.localPosition = new Vector2(0, -70);
                buttonConfirmJackpotWin.gameObject.SetActive(false);
                textJackpotWin.ResetValue();
                AudioSource soundMoney = SoundManager.Instance.PlayEffectFromPath(SoundSlot.COUNGTING_MONEY_START);
                textJackpotWin.SetValue(winAmount, true, 2.0f, "", () =>
                {
                    soundMoney.Stop();
                    SoundManager.Instance.PlayEffectFromPath(SoundSlot.COUNGTING_MONEY_END);
                });

                animationJackpotWin.AnimationState.Complete += delegate
                {
                    buttonConfirmJackpotWin.gameObject.SetActive(true);
                    buttonConfirmJackpotWin.onClick.AddListener(() =>
                    {
                        animationJackpotWin.transform.parent.gameObject.SetActive(false);
                        animationBackgroundMoney.gameObject.SetActive(false);
                        animationJackpotWin.gameObject.SetActive(false);
                        effectContainer.gameObject.SetActive(false);
                        buttonConfirmJackpotWin.gameObject.SetActive(false);
                        NextTween();
                    });
                };
            });
            // .AppendInterval(3f)
            // .AppendCallback(() =>
            // {
               
            // });
    }

    private bool IsInDragonPearl()
    {
        return currentGame == SiXiangGame.DragonPearl || currentGame == SiXiangGame.SixangbonusDragonPearl || nextGame == SiXiangGame.SixangbonusDragonPearl || isInSixiangBonus;
    }
    #endregion

    #region Gold Pick
    public void ShowGoldPickView()
    {
        if (goldPickView == null)
        {
            goldPickView = Instantiate(UIManager.Instance.LoadPrefabGame(GOLD_PICK_PREFAB_PATH), transform).GetComponent<SiXiangGoldPickView>();
            goldPickView.transform.SetSiblingIndex(animationCutScene.transform.GetSiblingIndex() - 3);
        }
        else
        {
            goldPickView.gameObject.SetActive(true);
        }
        goldPickView.SetInfo(this);
        backgroundGoldPick.gameObject.SetActive(true);
    }

    public void HideBackgroundGoldPick()
    {
        backgroundGoldPick.gameObject.SetActive(false);
    }
    #endregion

    #region Rapid Pay
    public void ShowRapidPayView()
    {
        if (rapidPayView == null)
        {
            rapidPayView = Instantiate(UIManager.Instance.LoadPrefabGame(RAPID_PAY_PREFAB_PATH), transform).GetComponent<SiXiangRapidPayView>();
            rapidPayView.transform.SetSiblingIndex(animationCutScene.transform.GetSiblingIndex() - 2);
        }
        else
        {
            rapidPayView.gameObject.SetActive(true);
        }
        SetAnimationGameName(RAPID_PAY_GAME_NAME);
        rapidPayView.SetInfo(this);
    }
    #endregion
    
    #region Lucky Draw
    public void ShowLuckyDrawView()
    {
        if (luckyDrawView == null)
        {
            luckyDrawView = Instantiate(UIManager.Instance.LoadPrefabGame(LUCKY_DRAW_PREFAB_PATH), transform).GetComponent<SiXiangLuckyDrawView>();
            luckyDrawView.transform.SetSiblingIndex(animationCutScene.transform.GetSiblingIndex() - 1);
        }
        else
        {
            luckyDrawView.gameObject.SetActive(true);
        }
        luckyDrawView.SetInfo(this);
        SetAnimationGameName(LUCKY_DRAW_GAME_NAME);
        SetAnimationBackground(LUCKY_DRAW_GAME_NAME);
        columnContainer.gameObject.SetActive(false);
    }
    #endregion

    public void OnFinishBonusGame()
    {
        Debug.Log("FINISH BONUS GAMEEEEE");
        currentGame = SiXiangGame.Normal;
        SetAnimationGameName(SIXIANG_GAME_NAME);
        SetAnimationBackground(SIXIANG_GAME_NAME);

        SetWinType(totalChipWinByGame);
        UpdateTotalChipWinValue();
        tweenQueue.Enqueue(() =>
        {
            Debug.Log("IS CHOOES BONUS GAME: " + isChooseBonusGame);
            if (isChooseBonusGame)
            {
                ShowChooseBonusGame();
                isChooseBonusGame = false;
            }
            else
            {
                AnimateCoinsFly();
            }
        });
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                switch (winType)
                {
                    case WinType.BIG_WIN:
                        ShowSpecialWinAnimation(WinType.BIG_WIN, totalChipWinByGame);
                        break;
                    case WinType.MEGA_WIN:
                        ShowSpecialWinAnimation(WinType.MEGA_WIN, totalChipWinByGame);
                        break;
                    case WinType.HUGE_WIN:
                        ShowSpecialWinAnimation(WinType.HUGE_WIN, totalChipWinByGame);
                        break;
                    default:
                        AnimateCoinsFly();
                        break;
                }

            });
        UpdateGem();
        // tweenQueue.Clear();
        // NextTween();
    }

    private void SetWinType(long winAmount)
    {

        winType = WinType.NONE;
        // if (isGrandJackpot)
        // {
        //     winAmount = winAmount + validBetLevels[currentBetLevel] * jackpotLevel[3];
        // }
        if (winAmount > 50 * currentBetLevel)
        {
            winType = WinType.MEGA_WIN;
        }
        else if (winAmount > 25 * currentBetLevel)
        {
            winType = WinType.HUGE_WIN;
        }
        else if (winAmount > 10 * currentBetLevel)
        {
            winType = WinType.BIG_WIN;
        }
        else if (winAmount > 5 * currentBetLevel)
        {
            winType = WinType.NICE_WIN;
        }
    }
    
    private void SetAnimationGameName(string gameName)
    {
        Utility.PlayAnimation(animationGameName, gameName, true);
    }

    private void SetAnimationBackground(string gameName)
    {
        switch (gameName)
        {
            case SIXIANG_GAME_NAME:
                Utility.PlayAnimationByPath(animationBackground, SIXIANG_BACKGROUND_ANIMATION_PATH, "animation", false);
                break;
            case DRAGON_PEARL_GAME_NAME:
                Utility.PlayAnimationByPath(animationBackground, DRAGON_PEARL_BACKGROUND_ANIMATION_PATH, "animation", false);
                break;
            case LUCKY_DRAW_GAME_NAME:
                Utility.PlayAnimationByPath(animationBackground, LUCKY_DRAW_BACKGROUND_ANIMATION_PATH, "animation", false);
                break;

        }
    }
}
