using System.Collections.Generic;
using Proto;
using DG.Tweening;
using Globals;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private const string SIXIANG_GAME_NAME = "sixiang";
    private const string SIXIANG_BACKGROUND_ANIMATION_PATH = "SiXiang/Spine/BgGame/skeleton_SkeletonData";

    private const string DRAGON_PEARL_GAME_NAME = "dragonpearl";
    private const string DRAGON_PEARL_BACKGROUND_ANIMATION_PATH = "SiXiang/Spine/DragonPearl/BgGame/skeleton_SkeletonData";
    private const string DRAGON_PEARL_ANIMAL_ANIMATION_PATH = "SiXiang/Spine/Animal/Dragon/skeleton_SkeletonData";
    private const string GOLD_PICK_PREFAB_PATH = "BundlePack/Animations/Sixiang/Prefab/GoldPickView";
    private const string GOLD_PICK_ANIMAL_ANIMATION_PATH = "SiXiang/Spine/Animal/Tiger/skeleton_SkeletonData";
    private const string GOLD_PICK_ANIMAL_ANIMATION_NAME = "3";
    private const string RAPID_PAY_PREFAB_PATH = "BundlePack/Animations/Sixiang/Prefab/RapidPayView";
    private const string RAPID_PAY_ANIMAL_ANIMATION_PATH = "SiXiang/Spine/Animal/Phoenix/skeleton_SkeletonData";
    private const string LUCKY_DRAW_GAME_NAME = "luckydraw";
    private const string LUCKY_DRAW_BACKGROUND_ANIMATION_PATH = "SiXiang/Spine/LuckyDraw/BgGame/skeleton_SkeletonData";
    private const string LUCKY_DRAW_PREFAB_PATH = "BundlePack/Animations/Sixiang/Prefab/LuckyDrawView";
    private const string LUCKY_DRAW_ANIMAL_ANIMATION_PATH = "SiXiang/Spine/Animal/Turle/skeleton_SkeletonData";
    private const string MONEY_WIN_ANIMATION_PATH = "SiXiang/Spine/BigWinGoldPick/skeleton_SkeletonData";
    private const string WIN_JACKPOT_ANIMATION_PATH = "SiXiang/Spine/LuckyDraw/BigWin/skeleton_SkeletonData";
    protected override string SPECIAL_WIN_ANIMATION_PATH => "SiXiang/Spine/FourWin/skeleton_SkeletonData";

    private SiXiangScatterView scatterView;
    private SiXiangRapidPayView rapidPayView;
    private SiXiangGoldPickView goldPickView;
    private SiXiangLuckyDrawView luckyDrawView;

    protected override void OnStartSpin()
    {
        // currentGame = SiXiangGame.DragonPearl;
        if (IsInDragonPearl())
        {
            if (!dragonPearlView.IsWinBirdEye)
            {
                UpdateDragonPearlFreeSpinLeft();
            }
            UpdateGameState(SlotGameState.SPINNING);
            SetDarkAllItems();
            foreach (SlotSymbolColumn column in listColumn)
            {
                column.SetRandomFinishView();
                column.StartSpin(spinType);
            }
            IsSpinning = true;
        }
        else
        {
            base.OnStartSpin();
        }
    }

    public override void OnStopSpin()
    {
        if (IsInDragonPearl())
        {
            IsSpinning = false;
            SetDarkAllItems();
            tweenQueue.Enqueue(() => dragonPearlView.OnStopSpin());
            NextTween();
        }
        else
        {
            base.OnStopSpin();
        }
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

    public override void OnSelectBonusGame(int index)
    {
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
        // SoundManager.instance.playEffectFromPath(SOUND_SLOT_BASE.SHOW_ANIMAL);
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
        // SoundManager.instance.playEffectFromPath(SOUND_SLOT_BASE.CUT_SCENE);
        animationCutScene.gameObject.SetActive(true);
        Utility.PlayAnimation(animationCutScene, "animation", false);
        DOTween.Sequence().AppendInterval(1.7f).AppendCallback(() =>
        {
            if (isEndBonusGame)
            {
                HideBackgroundGoldPick();
                UpdateSpinButtonUI();
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

    protected override void ShowChooseBonusGame()
    {
        Debug.Log("CHOOSE BONUS GAME");
        chooseGameBonus.gameObject.SetActive(true);
    }

    #region Dragon Pearl
    private void ShowDragonPearlView()
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
        // SoundManager.instance.playMusicInGame(Globals.SOUND_SLOT_BASE.BG_GAME);
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
        // AudioSource soundMoney = SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.WIN_JACKPOT_START);
        textJackpotWin.SetValue(totalChipWinByGame, true, 4.0f, "", () =>
        {
            // soundMoney.Stop();
            // SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.WIN_JACKPOT_END);
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
                // AudioSource soundMoney = SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.WIN_JACKPOT_START);
                textJackpotWin.SetValue(winAmount, true, 2.0f, "", () =>
                {
                    // soundMoney.Stop();
                    // SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.WIN_JACKPOT_END);
                });
                // AudioSource soundMoney = SoundManager.instance.playEffectFromPath(Globals.SOUND_SLOT_BASE.COUNGTING_MONEY_START);

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
    private void ShowGoldPickView()
    {
        if (goldPickView == null)
        {
            goldPickView = Instantiate(UIManager.Instance.LoadPrefab(GOLD_PICK_PREFAB_PATH), transform).GetComponent<SiXiangGoldPickView>();
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
    private void ShowRapidPayView()
    {
        if (rapidPayView == null)
        {
            rapidPayView = Instantiate(UIManager.Instance.LoadPrefab(RAPID_PAY_PREFAB_PATH), transform).GetComponent<SiXiangRapidPayView>();
            rapidPayView.transform.SetSiblingIndex(animationCutScene.transform.GetSiblingIndex() - 2);
        }
        else
        {
            rapidPayView.gameObject.SetActive(true);
        }
        rapidPayView.SetInfo(this);
    }
    #endregion
    
    #region Lucky Draw
    private void ShowLuckyDrawView()
    {
        if (luckyDrawView == null)
        {
            luckyDrawView = Instantiate(UIManager.Instance.LoadPrefab(LUCKY_DRAW_PREFAB_PATH), transform).GetComponent<SiXiangLuckyDrawView>();
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
            if (isChooseBonusGame)
            {
                ShowChooseBonusGame();
                isChooseBonusGame = false;
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
