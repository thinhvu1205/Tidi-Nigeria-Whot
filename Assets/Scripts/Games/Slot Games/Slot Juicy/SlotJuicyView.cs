using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto;
using DG.Tweening;
using Globals;
using Google.Protobuf;
using Nakama;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class SlotJuicyView : BaseSlotView
{
    [SerializeField] private Transform holdPackageContainer, basketGetFreeGame, basketGetFruitRain;
    [SerializeField] private TextNumberControl grandJackpotText, majorJackpotText, minorJackpotText, miniJackpotText;
    [SerializeField] private TextMeshProUGUI totalPackageValueText, packageValueWinText, jackpotText, titleBasketAddedWild, titleBasketTotalFreeSpin;
    [SerializeField] private Image titleBasketGetFruitRain;
    [SerializeField] private SkeletonGraphic popupChooseABucketAnimation, popupResultPackageAnimation, jackpotAnimation, totalMoneyPackageAnimation, effectPackageAnimation;
    [SerializeField] private DialogView popupGetFruitRain;
    [SerializeField] private Material grandJackpotMaterial, majorJackpotMaterial, minorJackpotMaterial, miniJackpotMaterial;
    [SerializeField] private Button bucketLeft, bucketRight;
    protected override Vector2 RECT_SIZE => new(140f, 135f);
    protected override string SOUND_BACKGROUND_ANIMATION_PATH => SoundSlot.BG_JUICY_GARDEN;
    protected override string BIG_WIN_ANIMATION_PATH => "SlotSpine/JuicyGarden/big_megawinJuicy/skeleton_SkeletonData";
    protected override string MEGA_WIN_ANIMATION_PATH => "GameView/SlotSpine/JuicyGarden/big_megawinJuicy/skeleton_SkeletonData";
    protected override string FREE_SPIN_ANIMATION_PATH => "GameView/SlotSpine/JuicyGarden/AnimBox/skeleton_SkeletonData";
    protected override string BACKGROUND_FREE_SPIN_ANIMATION_PATH => "SlotSpine/JuicyGarden/BgFreeSpin/skeleton_SkeletonData";
    private const string BACKGROUND_MONEY_PACKAGE_ANIMATION_PATH = "SlotSpine/JuicyGarden/EffectPackage/skeleton_SkeletonData";
    private const string BACKGROUND_CHOOSE_A_BUCKET_ANIMATION_PATH = "SlotSpine/JuicyGarden/AnimBox/skeleton_SkeletonData";
    private const string RESULT_BONUSGAME_ANIMPATH = "SlotSpine/JuicyGarden/EndGame/skeleton_SkeletonData";
    private const string JACKPOT_ANIMPATH = "SlotSpine/JuicyGarden/Jackpot/skeleton_SkeletonData";
    private List<SpinSymbol> spinList;
    private WinJackpot? winJackpot;
    private JackpotHistory jackpotHistory;
    private long rateJackpotGrand = 0, rateJackpotMajor = 0, rateJackpotMinor = 0, rateJackpotMini = 0, jpGrandPlayer = 0, jpMajorPlayer = 0, totalPackageValue = 0;
    private bool isInFruitRain, isInJuiceFree, isStartFruitRain, isEndFruitRain, isChooseFreeGame, isChooseFruitRain, isEndFreeGame, isChooseBasket, isBetLevelChanged, isFinishGame, isSetUpFruitRainGame = false;
    private Tween tweenChooseBasket = null;
    private SiXiangGame nextGame, currentGame;
    private WinType _currentWinType;
    private bool _currentIsCoinFlyAfterwards;

    protected override Dictionary<SiXiangSymbol, int> SymbolDictionary => new()
    {
        { SiXiangSymbol.K, 0 },
        { SiXiangSymbol.Q, 1 },
        { SiXiangSymbol.J, 2 },
        { SiXiangSymbol.A, 3 },
        { SiXiangSymbol.JuicePinapple, 4 },
        { SiXiangSymbol.JuiceMangosteen, 5 },
        { SiXiangSymbol.JuiceWatermelon, 6 },
        { SiXiangSymbol.JuiceStrawberry, 7 },
        { SiXiangSymbol.JuiceStoneDiamond, 8 },
        { SiXiangSymbol.JuiceStoneViolet, 9 },
        { SiXiangSymbol.JuiceStoneGreen, 10 },
        { SiXiangSymbol.Wild, 11 },
        { SiXiangSymbol.Scatter, 12 },
        { SiXiangSymbol.JuiceFruitbasketSpin, 13 },
        { SiXiangSymbol.JuiceFruitbasketMajor, 14 },
        { SiXiangSymbol.JuiceFruitbasketMinor, 15 },
        { SiXiangSymbol.JuiceFruitbasketMini, 16 },
        { SiXiangSymbol.JuiceFruitbasketRandom1, 13 },
        { SiXiangSymbol.JuiceFruitbasketRandom2, 13 },
        { SiXiangSymbol.JuiceFruitbasketRandom3, 13 },
        { SiXiangSymbol.JuiceFruitbasketRandom4, 13 },
        { SiXiangSymbol.JuiceFruitbasketRandom5, 13 },
        { SiXiangSymbol.JuiceFruitbasketRandom6, 13 },
        { SiXiangSymbol.JuiceFruitbasketRandom7, 13 },
    };

    protected override void OnValueChangeSlider(int selectID)
    {
        switch (selectID)
        {
            case 0:
                ReqSpecGame = 0;
                break;
            case 1:
                ReqSpecGame = (int)SiXiangGame.JuiceFruitBasket;
                break;
            case 2 :
                ReqSpecGame = (int)SiXiangGame.JuiceFruitRain; 
                break;
            case 3 :
                ReqSpecGame = -1; // 3 scatter va 6 gio
                break;
            default:
                ReqSpecGame = 0;
                break;
        }
    }
    
    public override void HandleUpdateTable(IMatchState matchState)
    {
        SlotDesk data = SlotDesk.Parser.ParseFrom(matchState.State);
        Debug.Log("Slot : " +data.ToString());

        // Update UI cho các item
        List<SiXiangSymbol> listSymbols = data.Matrix.Lists.ToList();
        int totalCol = data.Matrix.Cols;
        spinList = data.Matrix.SpinLists.ToList();
        jackpotHistory = data.WinJpHistory;
        winJackpot = data.WinJp;
        nextGame = data.NextSixiangGame;
        currentGame = data.CurrentSixiangGame;
        isInFreeSpin = data.CurrentSixiangGame == SiXiangGame.JuiceFreeGame || data.CurrentSixiangGame == SiXiangGame.JuiceFruitRain;
        isInFruitRain = data.CurrentSixiangGame == SiXiangGame.JuiceFruitRain;
        isInJuiceFree = data.CurrentSixiangGame == SiXiangGame.JuiceFreeGame;
        isStartFruitRain = data.NextSixiangGame == SiXiangGame.JuiceFruitRain && data.CurrentSixiangGame != SiXiangGame.JuiceFruitRain;
        isEndFruitRain = data.CurrentSixiangGame == SiXiangGame.JuiceFruitRain && data.NextSixiangGame != SiXiangGame.JuiceFruitRain;
        isEndFreeGame = data.CurrentSixiangGame == SiXiangGame.JuiceFreeGame && data.NextSixiangGame != SiXiangGame.JuiceFreeGame;
        isChooseFreeGame = data.NextSixiangGame == SiXiangGame.JuiceFreeGame && data.CurrentSixiangGame == SiXiangGame.JuiceFruitBasket;
        isChooseFruitRain = data.NextSixiangGame == SiXiangGame.JuiceFruitRain && data.CurrentSixiangGame == SiXiangGame.JuiceFruitBasket;
        isChooseBasket = data.NextSixiangGame == SiXiangGame.JuiceFruitBasket && data.CurrentSixiangGame != SiXiangGame.JuiceFruitBasket;
        isFinishGame = data.IsFinishGame;

        winType = data.BigWin switch
        {
            BigWin.Big => WinType.BIG_WIN,
            BigWin.Mega => WinType.MEGA_WIN,
            _ => WinType.NONE,
        };
        SetWinType(data.GameReward.ChipsWin);     

        if (totalCol >= 5)
        {   
            for (int col = 0; col < totalCol; col++)
            {
                SlotColumn column = slotColumnList[col];
                int[] columnArray = new int[3];
                long[] valuePackageColumnArray = new long[3];

                for (int row = 0; row < 3; row++)
                {
                    // Tính index theo layout ngang
                    int index = row * 5 + col;
                    SiXiangSymbol symbol = listSymbols[index];
                    if (SymbolDictionary.TryGetValue(symbol, out int mappedValue))
                    {
                        columnArray[row] = mappedValue;
                    }
                    else
                    {
                        columnArray[row] = -1;
                    }
                    if (spinList.Count > 0)
                    {
                        valuePackageColumnArray[row] = spinList[index].WinAmount;
                    }
                }

                // Thay đổi mcb thì ko làm mất giá trị package từ trc
                // if (!isBetLevelChanged && !isInFruitRain)
                // {
                SetPackageValue(column, valuePackageColumnArray);
                
               
                // }

                // Khi đã bấm Spin
                if (hasSetupStartView && !isSetUpFruitRainGame)
                {
                    column.SetFinishView(columnArray);
                    paylineList = data.Paylines.ToList();

                }
                // Khi lần đầu vào game -> Setup Views
                else
                {
                    if (isSetUpFruitRainGame)
                    {
                        column.SetFinishViewNow(columnArray);
                    }
                    else
                    {
                        column.SetStartView(columnArray);
                    }

                    if (data.CurrentSixiangGame == SiXiangGame.JuiceFruitRain)
                    {
                        column.ShowPackageValue();
                    }
                    else if (data.CurrentSixiangGame == SiXiangGame.JuiceFruitBasket)
                    {
                        column.SetRandomFinishView();
                        ShowPopupChooseABucket();
                    }
                }
            }
        }

        if (data.GameConfig != null)
        {
            freeSpinLeft = (int)data.GameConfig.NumFreeSpin;
            isLastFreeSpin = data.GameConfig.NumFreeSpin <= 0;
            lastTotalChipWinByGame = totalChipWinByGame;
            totalChipWinByGame = data.GameReward.TotalChipsWinByGame;
        }
        else
        {
            freeSpinLeft = (int)data.NumSpinLeft;
            isLastFreeSpin = false;
            lastTotalChipWinByGame = 0;
        }

        if (hasSetupStartView && !isSetUpFruitRainGame)
        {
            // Bấm Spin
            if (IsSpinning && !isBetLevelChanged)
            {
                OnStartSpin();
            }
            // Chỉ thay đổi mức cược (Update Jackpot)
            else
            {
                // currentBetLevel = data.ChipsMcb;
                // SetCurrentBetText(currentBetLevel);

                UpdateJackpot();
                isBetLevelChanged = false;

                if (isChooseFreeGame || isChooseFruitRain)
                {
                    AnimateChooseBucket();
                }
                return;
            }
        }
        else
        {
            UpdateJackpot();
            betLevelList = data.BetLevels.ToList();
            Debug.Log("data.ChipsMcb:  " + data.ChipsMcb);
            currentBetLevel = data.ChipsMcb;
            Debug.Log("currentBetlevel:  " + currentBetLevel);
            SetInfoSessionText("Press SPIN to play");
            SetCurrentBetText(currentBetLevel);
            SetCurrentChipValue(data.GameReward.BalanceChipsWalletAfter);
            if (data.CurrentSixiangGame == SiXiangGame.JuiceFruitRain)
            {
                spinType = SpinType.FREE_NORMAL;
                ShowBackGroundFreeSpin();
                CreateHolderPackageView();
                UpdateSpinButtonUI();
                isSetUpFruitRainGame = false;
            }
            else if (data.CurrentSixiangGame == SiXiangGame.JuiceFreeGame)
            {
                totalChipWinByGame = data.GameReward.TotalChipsWinByGame;
                UpdateTotalChipWinValue();
                spinType = SpinType.FREE_NORMAL;
                ShowBackGroundFreeSpin();
                UpdateSpinButtonUI();
            }
            else if (data.CurrentSixiangGame == SiXiangGame.JuiceFruitBasket)
            {
                foreach(SlotColumn column in slotColumnList)
                {
                    column.SetRandomFinishView();
                }
                ShowPopupChooseABucket();
            }
        }

        // Update Reward
        if (data.GameReward.UpdateWallet)
        {
            lastChipWin = currentChipWin;
            currentChipWin = data.GameReward.ChipsWin;
            // totalChipWinByGame = data.GameReward.TotalChipsWinByGame;
            totalChipWinByGame = data.GameReward.TotalChipsWinByGame;
            playerWalletAfter = data.GameReward.BalanceChipsWalletAfter;
        }

        hasSetupStartView = true;
    }

    public override void OnStopSpin()
    {
        if (isInFreeSpin)
        {
            ShowBackGroundFreeSpin();
        }
        IsSpinning = false;
        UpdateJackpot();
        
        ///------------------CHECK END FRUIT RAIN --------------------//
        if (isEndFruitRain || (isFinishGame && currentGame == SiXiangGame.JuiceFruitRain))
        {
            totalPackageValue = 0;
            ShowTotalMoneyPackage();
            tweenQueue.Enqueue(ShowPackageResult);
      
        }
        else
        {
            // Nếu đang là Fruit Rain thì giữ lại các item là giỏ
            if (freeSpinLeft > 0 && isInFruitRain)
            {
                CreateHolderPackageView();
            }
        }

        ///------------------CHECK SHOW WIN SCATTER--------------------//
        if (CheckWinScatter() && !isInFruitRain)
        {
            tweenQueue.Enqueue(ShowWinScatter);
        }

        ///------------------CHECK FREE GAME--------------------//
        if (isChooseFreeGame)
        {
            spinType = SpinType.FREE_NORMAL;
            UpdateGameState(SlotGameState.PREPARE);
            ShowBackGroundFreeSpin();
        }

        ///------------------CHECK END FREE GAME--------------------//
        if (isEndFreeGame)
        {
            tweenQueue.Enqueue(() =>
            {
                if (winType == WinType.NONE && totalChipWinByGame > 0)
                {
                    AnimateCoinsFly();
                }

                if (totalChipWinByGame > lastTotalChipWinByGame)
                {
                    UpdateTotalChipWinValue();
                }

                HideBackgroundFreeSpin();
                UpdateGameState(SlotGameState.PREPARE);
                spinType = SpinType.NORMAL;
                StartCoroutine(DelayNextTween());
            });
        }


        ///------------------CHECK SHOW FIVE OF A KIND--------------------///
        if (CheckFiveOfAKind())
        {
            tweenQueue.Enqueue(() => ShowWinAnimation(WinType.FIVE_OF_A_KIND));
        }

        ///------------------CHECK SHOW ALL LINE--------------------///
        if (paylineList.Count > 0)
        {
            tweenQueue.Enqueue(ShowAllWinLines);
        }


        ///------------------CHECK SHOW TYPE WIN--------------------///
        if (!isInFreeSpin)
        {
            switch (winType)
            {
                case WinType.BIG_WIN:
                    tweenQueue.Enqueue(() => ShowWinAnimation(WinType.BIG_WIN));
                    break;
                case WinType.MEGA_WIN:
                    tweenQueue.Enqueue(() => ShowWinAnimation(WinType.MEGA_WIN));
                    break;
                case WinType.HUGE_WIN:
                    tweenQueue.Enqueue(() => ShowWinAnimation(WinType.HUGE_WIN));
                    break;
            }
        }

        ///------------------CHECK SHOW ONE BY ONE--------------------//
        if (paylineList.Count > 0 && !isEndFreeGame)
        {
            if (spinType == SpinType.NORMAL)
            {
                // if (freespinLeft == 0) listActionHandleSpin.Add(acShowOneWinLine);
                tweenQueue.Enqueue(ShowWinLineOneByOne);
            }
            else if (spinType == SpinType.AUTO || spinType == SpinType.FREE_AUTO)
            {
                if (paylineList.Count == 1) tweenQueue.Enqueue(ShowWinLineOneByOne);
                // if (!isInFreeSpin) listActionHandleSpin.Add(acShowAnimChipBay);
            }
        }

        if (isChooseBasket)
        {
            tweenQueue.Enqueue(ShowPopupChooseABucket);
        }

        ///------------------CHECK FRUIT RAIN--------------------//
        if (isStartFruitRain || (isFinishGame && nextGame == SiXiangGame.JuiceFruitRain))
        {
            tweenQueue.Enqueue(() => SetupFruitRainGame());
        }
        
        NextTween();
    }
    
    protected override void ShowWinAnimation(WinType winType, bool isCoinFlyAfterwards = true)
    {
        Debug.Log("winType: " + winType);
        if(winType == WinType.NONE) return;
        _currentWinType = winType;
        _currentIsCoinFlyAfterwards = isCoinFlyAfterwards;
        effectContainer.gameObject.SetActive(true);
        animationEffect.gameObject.SetActive(true);
        bigWinText.gameObject.SetActive(false);
        Sequence sequence = DOTween.Sequence();
        switch (winType)
        {
            case WinType.BIG_WIN:
                sequence.AppendInterval(1.2f)
                    .AppendCallback(() =>
                    {
                        bigWinText.gameObject.SetActive(true);
                        Utility.TweenNumberToNumber(bigWinText, currentChipWin, 0, 2.7f);
                    })
                    .AppendInterval(3.3f)
                    .OnComplete(() =>
                    {
                        bigWinText.transform.gameObject.SetActive(false);
                    });
                SoundManager.Instance.PlayEffectFromPath(SoundSlot.BIG_WIN);
                // animationEffect.transform.localScale = new Vector2(0.9f, 0.9f);
                animationEffect.transform.localScale = new Vector2(1f, 1f);
                // animationEffect.transform.localPosition = new Vector2(0, -70);
                Utility.PlayAnimationByPath(animationEffect, BIG_WIN_ANIMATION_PATH, BIG_WIN_ANIMATION_NAME, false);
                break;
            case WinType.MEGA_WIN:
                sequence.AppendInterval(0.8f)
                    .AppendCallback(() =>
                    {
                        Debug.Log("SAO K SETACTIVE ?");
                        bigWinText.gameObject.SetActive(true);
                        Utility.TweenNumberToNumber(bigWinText, currentChipWin, 0, 4.7f);
                    })
                    .AppendInterval(5f)
                    .AppendCallback(() =>
                    {
                        bigWinText.gameObject.SetActive(false);
                    });
                SoundManager.Instance.PlayEffectFromPath(SoundSlot.MEGA_WIN);
                // Utility.TweenNumberToNumber(bigWinText, currentChipWin, 0, delay - 1);
                // animationEffect.transform.localScale = new Vector2(0.9f, 0.9f);
                animationEffect.transform.localScale = new Vector2(1f, 1f);
                // animationEffect.transform.localPosition = new Vector2(0, -70);
                Utility.PlayAnimationByPath(animationEffect, MEGA_WIN_ANIMATION_PATH, MEGA_WIN_ANIMATION_NAME, false);
                break;
            case WinType.HUGE_WIN:
                sequence.AppendInterval(0.5f)
                    .AppendCallback(() =>
                    {
                        Utility.TweenNumberToNumber(bigWinText, currentChipWin, 0, 4.3f);
                    })
                    .AppendInterval(0.5f)
                    .OnComplete(() =>
                    {
                        bigWinText.transform.parent.gameObject.SetActive(false);
                    });
                SoundManager.Instance.PlayEffectFromPath(SoundSlot.MEGA_WIN);
                bigWinText.transform.parent.gameObject.SetActive(true);
                bigWinText.gameObject.SetActive(true);
                // Utility.TweenNumberToNumber(bigWinText, currentChipWin, 0, delay - 1);
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
            default:
                effectContainer.gameObject.SetActive(false);
                animationEffect.gameObject.SetActive(false);
                break;
        }

        animationEffect.AnimationState.Complete -= OnAnimationComplete;
        animationEffect.AnimationState.Complete += OnAnimationComplete;

    }
    
    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        effectContainer.gameObject.SetActive(false);
        animationEffect.gameObject.SetActive(false);

        if (new[] { WinType.BIG_WIN, WinType.HUGE_WIN, WinType.MEGA_WIN }.Contains(_currentWinType))
        {
            if (_currentIsCoinFlyAfterwards)
                AnimateCoinsFly();
        }
        NextTween();
        effectContainer.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
    }


    private void SetupFruitRainGame(bool isKeep6FirstBasket = true)
    {
        Debug.Log("SetupFruitRainGame");
        isSetUpFruitRainGame = true;
        SetDarkAllItems(isBackgroundDark: false);
        freeSpinLeft = 3;
        ShowBackGroundFreeSpin();
        // if (isKeep6FirstBasket)
        // {
        //     CreateHolderPackageView();
        // }
        tweenQueue.Enqueue(ShowPopupGetFruitRain);

        // Fruit Rain sẽ autospin cho đến khi hết Fruit Rain
        if (spinType == SpinType.NORMAL || spinType == SpinType.AUTO || spinType == SpinType.FREE_AUTO)
        {
            spinType = SpinType.FREE_NORMAL;
        }
        UpdateSpinButtonUI();
        UpdateStateWinUI(StateWin.TOTAL_WIN);
        chipWinText.text = "0";
        NextTween();
    }

    private void SetupJuiceFreeGame()
    {
        // freeSpinLeft = 3;
        ShowBackGroundFreeSpin();
        if (spinType == SpinType.NORMAL || spinType == SpinType.AUTO)
        {
            spinType = SpinType.FREE_AUTO;
        }
        UpdateSpinButtonUI();
        UpdateStateWinUI(StateWin.TOTAL_WIN);
        chipWinText.text = "0";
        NextTween();
    }

    private void SetPackageValue(SlotColumn column, long[] packageValues)
    {
        Debug.Log("PACKAGE VALUES: " + string.Join(", ", packageValues));
        column.SetPackageValue(packageValues);
    }

    #region Buttons

    public override void OnClickMaxBetButton()
    {
        if (gameState == SlotGameState.SPINNING || gameState == SlotGameState.SHOWING_RESULT || !hasSetupStartView )
        {
            return;
        }

        SoundManager.Instance.PlayEffectFromPath(SoundSlot.CLICK);
        lastBetLevel = currentBetLevel;
        currentBetLevel = betLevelList[^1];
        if (lastBetLevel == currentBetLevel)
        {
            Debug.Log("SPIN CHO TOAOOOO");
            // isBetLevelChanged = false;
            HandleSpin();
            // HandleSpin();
        }
        else
        {
            OnBetLevelChanged();
        }
        SetCurrentBetText(currentBetLevel);
        SetCurrentBetImage(currentBetLevel);
    }

    public override void OnClickMinusBetButton()
    {
        if (gameState == SlotGameState.SPINNING || gameState == SlotGameState.SHOWING_RESULT || !hasSetupStartView)
        {
            return; 
        }
        base.OnClickMinusBetButton();
        OnBetLevelChanged();
    }

    public override void OnClickPlusBetButton()
    {
        if (gameState == SlotGameState.SPINNING || gameState == SlotGameState.SHOWING_RESULT || !hasSetupStartView)
        {
            return; 
        }
        base.OnClickPlusBetButton();
        OnBetLevelChanged();
    }

    public void OnClickClosePopupResult()
    {
        effectContainer.gameObject.SetActive(false);
        popupResultPackageAnimation.gameObject.SetActive(false);
        isInFreeSpin = false;
        AnimateCoinsFly();
        UpdateTotalChipWinValue();
        StartCoroutine(DelayNextTween());
    }
    
    private void OnBetLevelChanged()
    {
        Debug.Log("OnBetLevelChanged");
        if (gameState == SlotGameState.SPINNING || gameState == SlotGameState.SHOWING_RESULT || !hasSetupStartView)
        {
            return;
        }
        isBetLevelChanged = true;
        InfoBet infoBet = new()
        {
            Chips = currentBetLevel,
        };
        DataSender.SendMatchState((long)OpCodeRequest.Bet, infoBet.ToByteArray()); 
        
    }
    #endregion

    protected override bool CheckWinScatter()
    {
        int consecutiveColumns = 0;
        for (int i = 0; i < slotColumnList.Count; i++)
        {
            bool hasScatter = slotColumnList[i].ResultItem.GetFinishView().Contains(12);
            if (hasScatter)
            {
                consecutiveColumns++; // tăng nếu có scatter
                if (consecutiveColumns >= 3)
                {
                    hasGotFreeSpin = true;
                    return true; // tìm thấy 3 cột liền nhau có scatter
                }
            }
            else
            {
                consecutiveColumns = 0; // reset nếu gặp cột không có scatter
            }
        }

        return false; // không tìm thấy 3 cột liền nhau
    }

    #region Popups
    private void ShowPopupGetFruitRain()
    {
        effectContainer.gameObject.SetActive(true);
        popupGetFruitRain.gameObject.SetActive(true);
        popupGetFruitRain.Show();
 
        DOTween.Sequence()
            .AppendInterval(10.0f)
            .AppendCallback(() =>
            {
                if (popupGetFruitRain.gameObject.activeSelf)
                {
                    HidePopupGetFruitRain();
                }
            });
        
    }

    private void ShowPopupChooseABucket()
    {
        Debug.Log("ShowPopupChooseABucket");
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.FREESPIN);
        effectContainer.gameObject.SetActive(true);
        popupChooseABucketAnimation.gameObject.SetActive(true);
        bucketLeft.interactable = true;
        bucketRight.interactable = true;
        Utility.PlayAnimationByPath(popupChooseABucketAnimation, BACKGROUND_CHOOSE_A_BUCKET_ANIMATION_PATH, "thung", true);
    }

    private void ShowPopupResultPackage()
    {
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.FREESPIN);
        effectContainer.gameObject.SetActive(true);
        popupResultPackageAnimation.gameObject.SetActive(true);
        packageValueWinText.text = Utility.FormatNumber(totalChipWinByGame);
        Utility.PlayAnimationByPath(popupResultPackageAnimation, RESULT_BONUSGAME_ANIMPATH, "eng", true);
        popupResultPackageAnimation.transform.localScale = new Vector2(0.8f, 0.8f);
        popupResultPackageAnimation.transform.DOScale(new Vector2(1.0f, 1.0f), 0.3f).SetEase(Ease.OutBack);

        DOTween.Sequence()
            .AppendInterval(10.0f)
            .AppendCallback(() =>
            {
                if (popupResultPackageAnimation.gameObject.activeSelf)
                {
                    OnClickClosePopupResult();
                }
            });
    }
    #endregion

    #region Choose A Bucket
    public void ChooseBucket(int type)
    {
        bucketLeft.interactable = true;
        bucketRight.interactable = true;
        Debug.Log("Choose " + type + " bucket");
        InfoBet infoBet = new()
        {
            Id = type,
        };
        DataSender.SendMatchState((long)OpCodeRequest.Spin, infoBet.ToByteArray());
        string animationName = type switch
        {
            0 => "thungtrai",
            1 => "thungphai",
            _ => "thung"
        };
        Utility.PlayAnimationByPath(popupChooseABucketAnimation, BACKGROUND_CHOOSE_A_BUCKET_ANIMATION_PATH, animationName, false);
        bucketLeft.interactable = false;
        bucketRight.interactable = false;
    }

    private void AnimateChooseBucket()
    {
        DOTween.Sequence()
            .AppendInterval(2.0f)
            .AppendCallback(() =>
            {
                if (isChooseFreeGame)
                {
                    int wildNumber = 10;
                    basketGetFreeGame.gameObject.SetActive(true);
                    basketGetFruitRain.gameObject.SetActive(false);
                    basketGetFreeGame.DOScale(Vector2.one, 0.3f).SetEase(Ease.OutBack);
                    Utility.TweenNumberToNumber(titleBasketTotalFreeSpin, freeSpinLeft, 0, 1.0f);      
                    Utility.TweenNumberToNumber(titleBasketAddedWild, wildNumber, 0, 1.0f);      
                }
                if (isChooseFruitRain)
                {
                    basketGetFreeGame.gameObject.SetActive(false);
                    basketGetFruitRain.gameObject.SetActive(true);
                    titleBasketGetFruitRain.gameObject.SetActive(true);
                    titleBasketGetFruitRain.transform.localScale = Vector2.zero;
                    titleBasketGetFruitRain.transform.DOScale(new Vector2(1.0f, 1.0f), 0.3f).SetEase(Ease.OutBack);
                }
            });

        
        popupChooseABucketAnimation.AnimationState.Complete += delegate
        {
            popupChooseABucketAnimation.gameObject.SetActive(false);
            effectContainer.gameObject.SetActive(false);
            basketGetFreeGame.gameObject.SetActive(false);
            basketGetFruitRain.gameObject.SetActive(false);
            if (isChooseFruitRain)
            {
                SetupFruitRainGame(false);
            }
            else if (isChooseFreeGame)
            {
                SetupJuiceFreeGame();
            }
        };
    }
    #endregion

    #region Fruit Rain

    public void HidePopupGetFruitRain()
    {
        GetMatchResult();
        popupGetFruitRain.Hide(false, () =>
            {
                effectContainer.gameObject.SetActive(false);
                NextTween();
            }
        );
    }

    private void CreateHolderPackageView()
    {
        SetDarkAllItems(isBackgroundDark: false);
        for (int i = 0; i < slotColumnList.Count; i++)
        {
            SlotJuicyItem resultItem = (SlotJuicyItem)slotColumnList[i].ResultItem;
            for (int j = 0; j < resultItem.ValuePackageList.Length; j++)
            {
                int index = j;
                if (resultItem.GetFinishView()[index] >= 13) //id phai la gio va dang trong freespin
                {
                    CreateHoldPackage(index * 5 + i, resultItem.ImageList[index].gameObject, resultItem.GetFinishView()[index] > 13);
                }
            }
        }
    }

    public void CreateHoldPackage(int index, GameObject itemTemplate, bool isJackpotPackage = false)
    {
        Transform itemContainer = holdPackageContainer.transform.GetChild(index);
        itemContainer.gameObject.SetActive(true);
        if (itemContainer.transform.childCount > 0) return;
        GameObject itemPackage = Instantiate(itemTemplate, itemContainer);

        Vector2 posInWorld = itemTemplate.GetComponent<RectTransform>().position;
        // itemPackage.transform.localPosition = itemContainer.InverseTransformPoint(posInWorld);
        float x = 0f;
        float y = 0f;
        // Y theo hàng
        if (index < 5)
            y = -15f;
        else if (index > 9)
            y = 15f;

        // X theo cột
        switch (index % 5)
        {
            case 0: x = 10f; break;
            case 1: x = 5f;  break;
            case 2: x = 0f;  break;
            case 3: x = -5f;  break;
            case 4: x = -10f; break;
        }
        itemPackage.transform.localPosition = new Vector2(x, isJackpotPackage ? y + 10 : y);
        itemPackage.transform.localScale = itemPackage.transform.localScale * new Vector2(0.95f, 0.95f);
        itemPackage.GetComponent<Image>().color = Color.white;
        itemPackage.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
        itemPackage.SetActive(true);
    }

    private void ShowEffectPackageWithIndex(int index, long value)
    {
        Debug.Log("PACKAGE VALUE: " + value);
        totalPackageValue += value;
        Utility.TweenNumberToNumber(totalPackageValueText, totalPackageValue, totalPackageValue - value);
        effectPackageAnimation.gameObject.SetActive(true);
        Utility.PlayAnimationByPath(effectPackageAnimation, BACKGROUND_MONEY_PACKAGE_ANIMATION_PATH, index.ToString(), false);
        effectPackageAnimation.AnimationState.Complete += delegate
        {
            // NextTween();
        };
    }

    private void ShowTotalMoneyPackage()
    {
        totalMoneyPackageAnimation.gameObject.SetActive(true);
        totalPackageValueText.text = "0";
        totalMoneyPackageAnimation.gameObject.SetActive(true);
        Utility.PlayAnimationByPath(totalMoneyPackageAnimation, BACKGROUND_MONEY_PACKAGE_ANIMATION_PATH, "box_chip", true);
  
        // transform.Find("bgTable/NodeJackPotNumber").gameObject.SetActive(false);
    }

    private void ShowPackageResult()
    {
        backgroundFreeSpinLeftAnimation.gameObject.SetActive(false);
        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < spinList.Count; i++)
        {
            if (spinList[i].WinAmount <= 0) continue;
            int index = i;
            sequence
                .AppendCallback(() =>
                {
                    ShowEffectPackageWithIndex(index + 1, spinList[index].WinAmount);
                })
                .AppendInterval(1.0f);
        }
        sequence.OnComplete(() =>
        {
            if (winJackpot == WinJackpot.Grand)
            {
                tweenQueue.Enqueue(() => ShowJackpotAnimation());
            }
            tweenQueue.Enqueue(() =>
            {
                currentChipWin = totalChipWinByGame;
                SetWinType(currentChipWin);
                ShowWinAnimation(winType, false);
            });
            tweenQueue.Enqueue(() => ShowPopupResultPackage());
            foreach (Transform transform in holdPackageContainer)
            {
                transform.gameObject.SetActive(false);
                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }
            }
            backgroundFreeSpinAnimation.gameObject.SetActive(false);
            totalMoneyPackageAnimation.gameObject.SetActive(false);
            spinType = SpinType.NORMAL;
            UpdateGameState(SlotGameState.PREPARE);
            SetLightAllItems();
            NextTween();
        });
    }

    private void ShowJackpotAnimation()
    {
        Debug.Log("jackpotHistory.Grand.ChipsAccum: " + jackpotHistory.Grand.ChipsAccum);
        effectContainer.gameObject.SetActive(true);
        jackpotAnimation.gameObject.SetActive(true);
        jackpotText.gameObject.SetActive(true);
        Utility.TweenNumberToNumber(jackpotText, jackpotHistory.Grand.ChipsAccum + jackpotHistory.Grand.Chips + jpGrandPlayer, 0);
        jackpotAnimation.transform.localScale = new Vector2(0.8f, 0.8f);
        string animationName = winJackpot switch
        {
            WinJackpot.Grand => "JP_grand",
            WinJackpot.Mini => "JP_mini",
            WinJackpot.Major => "JP_major",
            WinJackpot.Minor => "JP_minor",
            _ => "",
        };
        Utility.PlayAnimationByPath(jackpotAnimation, JACKPOT_ANIMPATH, animationName, false);
        DOTween.Sequence()
            .Append(jackpotAnimation.transform.DOScale(new Vector2(1.0f, 1.0f), 0.3f).SetEase(Ease.OutBack))
            .Join(jackpotText.transform.DOScale(new Vector2(1.0f, 1.0f), 0.3f).SetEase(Ease.OutBack))
            .AppendInterval(3f)
            .Append(jackpotAnimation.transform.DOScale(new Vector2(1.5f, 1.5f), 0.3f).SetEase(Ease.InBack))
            .OnComplete(() =>
            {
                effectContainer.gameObject.SetActive(false);
                jackpotAnimation.gameObject.SetActive(false);
                NextTween();
            });
    }
    #endregion
    public override void CheckThirdScatter(int columnIndex)
    {

    }

    protected override void NextTween()
    {
        if (tweenQueue.Count > 0)
        {
            TweenCallback nextTween = tweenQueue.Dequeue();
            DOTween.Sequence().AppendCallback(nextTween);
        }
        // Hết tween = hết show win line
        else
        {
            if (isChooseBasket)
            {
                tweenQueue.Enqueue(() => ShowPopupChooseABucket());
                isChooseBasket = false;
                NextTween();
            }

            Reset();
            // Nếu đang auto spin thì spin tiếp
            if (spinType == SpinType.AUTO || spinType == SpinType.FREE_AUTO)
            {
                HandleSpin();
            }
        }
    }
    
    protected override void Reset()
    {
        if (spinType == SpinType.NORMAL || spinType == SpinType.FREE_NORMAL)
        {
            UpdateGameState(SlotGameState.PREPARE);
        }
        foreach(Sequence sequence in lineOneByOneSequenceList)
        {
            if (sequence.IsActive())
            {
                sequence.Kill();
            }
        }
        foreach (GameObject line in allLinesList)
        {
            if (line.activeSelf)
                linePool.Release(line);
        }
        foreach (GameObject line in lineOneByOneList)
        {
            if (line.activeSelf)
                linePool.Release(line);
        }
        spinList.Clear();
        allLinesList.Clear();
        lineOneByOneList.Clear();
        paylineList.Clear();

        if (!isInFruitRain && !isStartFruitRain)
        {
            Debug.Log("SET LAI ON AI TAM");
            SetLightAllItems();
        }

        SetInfoSessionText("Press SPIN to play");
        ScatterCount = 0;
        paylineInfoContainer.gameObject.SetActive(false);

        // if (isInFreeSpin)
        // {
        //     Debug.Log("SET ACTIVE BACJKGROUND FREE SPIN");
        //     backgroundFreeSpinAnimation.gameObject.SetActive(true);
        //     ShowBackGroundFreeSpin();
        // }
        // else
        // {
        //     backgroundFreeSpinAnimation.gameObject.SetActive(false);
        //     backgroundFreeSpinLeftAnimation.gameObject.SetActive(false);
        // }
    }

    public void UpdateJackpot()
    {
        Debug.Log("INIT JACKPOT");
        if (jackpotHistory != null)
        {
            jpGrandPlayer = jackpotHistory.Grand.ChipsAccum;
            jpMajorPlayer = jackpotHistory.Major.ChipsAccum;

            rateJackpotGrand = jackpotHistory.Grand.Ratio;
            rateJackpotMajor = jackpotHistory.Major.Ratio;
            rateJackpotMinor = jackpotHistory.Minor.Ratio;
            rateJackpotMini = jackpotHistory.Mini.Ratio;

            long valueJPGrandCurrent = jackpotHistory.Grand.Chips + jpGrandPlayer;
            long valueJPMajorCurrent = jackpotHistory.Major.Chips + jpMajorPlayer;
            long valueJPMinorCurrent = jackpotHistory.Minor.Chips;
            long valueJPMiniCurrent = jackpotHistory.Mini.Chips;

            grandJackpotText.SetValue(valueJPGrandCurrent, true, 0.2f);
            majorJackpotText.SetValue(valueJPMajorCurrent, true, 0.2f);
            minorJackpotText.SetValue(valueJPMinorCurrent, true, 0.2f);
            miniJackpotText.SetValue(valueJPMiniCurrent, true, 0.2f);
        }
    }
    
    private IEnumerator DelayNextTween(float timeDelay = 2f)
    {
        yield return new WaitForSeconds(timeDelay);
        NextTween();
    }
}
