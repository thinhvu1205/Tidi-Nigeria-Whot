using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Api;
using DG.Tweening;
using Globals;
using Google.Protobuf;
using Nakama;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class SlotJuicyView : BaseSlotView
{
    [SerializeField] private Transform holdPackageContainer;
    [SerializeField] private TextNumberControl grandJackpotText, majorJackpotText, minorJackpotText, miniJackpotText;
    [SerializeField] private TextMeshProUGUI totalPackageValueText, packageValueWinText, jackpotText;
    [SerializeField] private SkeletonGraphic popupChooseABucketAnimation, popupResultPackageAnimation, jackpotAnimation, totalMoneyPackageAnimation, effectPackageAnimation;
    [SerializeField] private DialogView popupGetFruitRain;
    [SerializeField] private Material grandJackpotMaterial, majorJackpotMaterial, minorJackpotMaterial, miniJackpotMaterial;
    [SerializeField] private Button bucketLeft, bucketRight;
    protected override Vector2 RECT_SIZE => new(140f, 135f);
    protected override string BIG_WIN_ANIMATION_PATH => "SlotSpine/JuicyGarden/big_megawinJuicy/skeleton_SkeletonData";
    protected override string MEGA_WIN_ANIMATION_PATH => "GameView/SlotSpine/JuicyGarden/big_megawinJuicy/skeleton_SkeletonData";
    protected override string FREE_SPIN_ANIMATION_PATH => "GameView/SlotSpine/JuicyGarden/AnimBox/skeleton_SkeletonData";
    protected override string BACKGROUND_FREE_SPIN_ANIMATION_PATH => "SlotSpine/JuicyGarden/BgFreeSpin/skeleton_SkeletonData";
    private const string BACKGROUND_MONEY_PACKAGE_ANIMATION_PATH = "SlotSpine/JuicyGarden/EffectPackage/skeleton_SkeletonData";
    private const string BACKGROUND_CHOOSE_A_BUCKET_ANIMATION_PATH = "SlotSpine/JuicyGarden/AnimBox/skeleton_SkeletonData";
    private const string RESULT_BONUSGAME_ANIMPATH = "SlotSpine/JuicyGarden/EndGame/skeleton_SkeletonData";
    private const string JACKPOT_ANIMPATH = "SlotSpine/JuicyGarden/Jackpot/skeleton_SkeletonData";
    private List<SpinSymbol> spinList;
    private WinJackpot winJackpot;
    private JackpotHistory jackpotHistory;
    private SiXiangGame currentGame = SiXiangGame.Normal, nextGame = SiXiangGame.Normal;
    private long rateJackpotGrand = 0, rateJackpotMajor = 0, rateJackpotMinor = 0, rateJackpotMini = 0, jpGrandPlayer = 0, jpMajorPlayer = 0, totalPackageValue = 0;
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
    protected override List<int[]> PaylineIdList => new List<int[]>
    {
        new int[] {1, 1, 1, 1, 1},
        new int[] {0, 0, 0, 0, 0},
        new int[] {2, 2, 2, 2, 2},
        new int[] {0, 1, 2, 1, 0},
        new int[] {2, 1, 0, 1, 2},
        new int[] {0, 0, 1, 2, 2},
        new int[] {2, 2, 1, 0, 0},
        new int[] {1, 0, 1, 2, 1},
        new int[] {1, 2, 1, 0, 1},
        new int[] {1, 0, 0, 1, 0},
        new int[] {1, 2, 2, 1, 2},
        new int[] {0, 1, 0, 0, 1},
        new int[] {2, 1, 2, 2, 1},
        new int[] {0, 2, 0, 2, 0},
        new int[] {2, 0, 2, 0, 2},
        new int[] {1, 0, 2, 0, 1},
        new int[] {1, 2, 0, 2, 1},
        new int[] {0, 1, 1, 1, 0},
        new int[] {2, 1, 1, 1, 2},
        new int[] {0, 2, 2, 2, 0},
    };

    public override void HandleUpdateTable(IMatchState matchState)
    {
        SlotDesk data = SlotDesk.Parser.ParseFrom(matchState.State);
                Debug.Log("Slot : " +data.ToString());
        if (!hasSpinned)
        {

        }
        else
        {
            if (!IsSpinning) return;
            OnStartSpin();
        }
        // Update UI cho các item
        List<SiXiangSymbol> listSymbols = data.Matrix.Lists.ToList();
        spinList = data.Matrix.SpinLists.ToList();
        currentGame = data.CurrentSixiangGame;
        nextGame = data.NextSixiangGame;
        jackpotHistory = data.WinJpHistory;
        int totalCol = data.Matrix.Cols;

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

            SetPackageValue(column, valuePackageColumnArray);

            // Gán chung trước
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

            isInFreeSpin = freeSpinLeft > 0;

            // Xử lý winType chung
            winType = data.BigWin switch
            {
                BigWin.Big => WinType.BIG_WIN,
                BigWin.Mega => WinType.MEGA_WIN,
                _ => WinType.NONE,
            };
            winJackpot = data.WinJp;

            // Logic riêng
            if (hasSpinned)
            {
                column.SetFinishView(columnArray);
                paylineList = data.Paylines.ToList();
            }
            else
            {
                InitJackpot();
                column.SetStartView(columnArray);

                betLevelList = data.BetLevels.ToList();
                currentBetLevel = data.ChipsMcb;
                SetInfoSessionText("Press SPIN to play");
                SetCurrentBetText(currentBetLevel);
                SetCurrentChipValue(data.GameReward.BalanceChipsWalletAfter);

                if (currentGame == SiXiangGame.JuiceFruitRain)
                {
                    spinType = SpinType.FREE_NORMAL;
                    column.ShowPackageValue();
                    ShowBackGroundFreeSpin();
                    CreateHolderPackageView();
                    UpdateSpinButtonUI();
                }
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

        hasSpinned = true;
    }

    public override void OnStopSpin()
    {
        IsSpinning = false;
        InitJackpot();
        if (isLastFreeSpin)
        {
            if (totalChipWinByGame > PaylineIdList.Count * currentBetLevel) winType = WinType.BIG_WIN;
            if (totalChipWinByGame > 50 * currentBetLevel) winType = WinType.MEGA_WIN;
            if (totalChipWinByGame > 0)
            {
                AnimateCoinsFly();
            }
            isLastFreeSpin = false;
            UpdateGameState(SlotGameState.PREPARE);
            spinType = SpinType.NORMAL;
        }

        // Từ Minigame chuyển sang Normal Game
        // if (CheckNormalGame())
        // {
        //    
        // }
        
        // Đang từ Normal game chuyển sang Fruit Rain
        if (CheckStartFruitRain())
        {
            backgroundFreeSpinAnimation.gameObject.SetActive(true);
            freeSpinLeft = 3;
            ShowBackGroundFreeSpin();
            CreateHolderPackageView();
            tweenQueue.Enqueue(() => ShowPopupGetFruitRain());

            // Fruit Rain sẽ autospin cho đến khi hết Fruit Rain
            if (spinType == SpinType.NORMAL || spinType == SpinType.AUTO)
            {
                spinType = SpinType.FREE_AUTO;
            }
            UpdateSpinButtonUI();
            UpdateStateWinUI(StateWin.TOTAL_WIN);
            chipWinText.text = "0";
            NextTween();
            return;
        }

        if (CheckEndFruitRain())
        {
            totalPackageValue = 0;
            ShowTotalMoneyPackage();
            tweenQueue.Enqueue(() => ShowPackageResult());
            tweenQueue.Enqueue(() => ShowJackpotAnimation());
        }
        else
        {
            // Nếu đang là Fruit Rain thì giữ lại các item là giỏ
            if (freeSpinLeft > 0)
            {
                CreateHolderPackageView();
            }
        }

        ///------------------CHECK SHOW WIN SCATTER--------------------//
        if (CheckWinScatter())
        {
            tweenQueue.Enqueue(() => ShowWinScatter());
        }

        ///------------------CHECK SHOW FIVE OF A KIND--------------------///
        if (CheckFiveOfAKind())
        {
            tweenQueue.Enqueue(() => ShowWinAnimation(WinType.FIVE_OF_A_KIND));
        }

        ///------------------CHECK SHOW FREESPIN--------------------//
        if (hasGotFreeSpin)
        {
            // Nếu đang quay thường hoặc quay auto mà đc freespin -> dừng lại
            tweenQueue.Enqueue(() => ShowWinAnimation(WinType.FREE_SPIN));
        }
        
        ///------------------CHECK SHOW ALL LINE--------------------///
        if (paylineList.Count > 0)
        {
            tweenQueue.Enqueue(() => ShowAllWinLines());
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
        if (paylineList.Count > 0)
        {
            if (spinType == SpinType.NORMAL)
            {
                // if (freespinLeft == 0) listActionHandleSpin.Add(acShowOneWinLine);
                tweenQueue.Enqueue(() => ShowWinLineOneByOne());
            }
            else if (spinType == SpinType.AUTO || spinType == SpinType.FREE_AUTO)
            {
                if (paylineList.Count == 1) tweenQueue.Enqueue(() => ShowWinLineOneByOne());
                // if (!isInFreeSpin) listActionHandleSpin.Add(acShowAnimChipBay);
            }
        }

        NextTween();

    }

    protected override void SetPackageValue(SlotColumn column, long[] packageValues)
    {
        column.SetPackageValue(packageValues);
    }

    #region Buttons

    public override void OnClickMaxBetButton()
    {
        base.OnClickMaxBetButton();
        OnBetLevelChanged();
    }

    public override void OnClickMinusBetButton()
    {
        base.OnClickMinusBetButton();
        OnBetLevelChanged();
    }

    public override void OnClickPlusBetButton()
    {
        base.OnClickPlusBetButton();
        OnBetLevelChanged();
    }

    public void OnClickClosePopupResult()
    {
        effectContainer.gameObject.SetActive(false);
        popupResultPackageAnimation.gameObject.SetActive(false);
    }

    private void OnBetLevelChanged()
    {
        InfoBet infoBet = new()
        {
            Chips = currentBetLevel,
        };
        DataSender.SendMatchState((long)OpCodeRequest.Bet, infoBet.ToByteArray());
    }
    #endregion

    #region Popups
    private void ShowPopupGetFruitRain()
    {
        effectContainer.gameObject.SetActive(true);
        popupGetFruitRain.gameObject.SetActive(true);
        popupGetFruitRain.Show();
        if (spinType == SpinType.AUTO || spinType == SpinType.FREE_AUTO)
        {
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
    }

    private void ShowPopupChooseABucket()
    {
        effectContainer.gameObject.SetActive(true);
        popupChooseABucketAnimation.gameObject.SetActive(true);
        Utility.PlayAnimationByPath(popupChooseABucketAnimation, BACKGROUND_CHOOSE_A_BUCKET_ANIMATION_PATH, "thung", true);
    }

    private void ShowPopupResultPackage()
    {
        effectContainer.gameObject.SetActive(true);
        popupResultPackageAnimation.gameObject.SetActive(true);
        packageValueWinText.text = totalPackageValue.ToString();
        Utility.PlayAnimationByPath(popupResultPackageAnimation, RESULT_BONUSGAME_ANIMPATH, "eng", true);
        popupResultPackageAnimation.transform.localScale = new Vector2(0.8f, 0.8f);
        popupResultPackageAnimation.transform.DOScale(new Vector2(1.0f, 1.0f), 0.3f).SetEase(Ease.OutBack);
    }
    #endregion

    #region Choose A Bucket
    public void ChooseBucket(int type)
    {
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

    }
    #endregion

    #region Fruit Rain
    private bool CheckStartFruitRain()
    {
        return currentGame != SiXiangGame.JuiceFruitRain && nextGame == SiXiangGame.JuiceFruitRain;
    }

    private bool CheckEndFruitRain()
    {
        return currentGame == SiXiangGame.JuiceFruitRain && nextGame != SiXiangGame.JuiceFruitRain;
    }

    private bool CheckNormalGame()
    {
        return currentGame != SiXiangGame.Normal && nextGame == SiXiangGame.Normal;
    }


    public void HidePopupGetFruitRain()
    {
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
                    CreateHoldPackage(index * 5 + i, resultItem.ImageList[index].gameObject);
                }
            }
        }
    }

    public void CreateHoldPackage(int index, GameObject itemTemplate)
    {
        Transform itemContainer = holdPackageContainer.transform.GetChild(index);
        itemContainer.gameObject.SetActive(true);
        if (itemContainer.transform.childCount > 0) return;
        GameObject itemPackage = Instantiate(itemTemplate, itemContainer);

        Vector2 posInWorld = itemTemplate.GetComponent<RectTransform>().position;
        itemPackage.transform.localPosition = itemContainer.InverseTransformPoint(posInWorld);
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
        Utility.PlayAnimationByPath(effectPackageAnimation, BACKGROUND_MONEY_PACKAGE_ANIMATION_PATH, (index + 1).ToString(), false);
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
        Debug.Log("SHOW PACKAGE RESULT");
        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < spinList.Count; i++)
        {
            if (spinList[i].WinAmount <= 0) continue;
            int index = i;
            sequence
                .AppendCallback(() =>
                {
                    Debug.Log("ShowEffectPackageWithIndex: " + index + " - " + spinList[index].WinAmount);
                    ShowEffectPackageWithIndex(index + 1, spinList[index].WinAmount);
                })
                .AppendInterval(1.0f);
        }
        sequence.OnComplete(() =>
        {
            ShowPopupResultPackage();
            foreach (Transform transform in holdPackageContainer)
            {
                transform.gameObject.SetActive(false);
                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }
            }
            backgroundFreeSpinAnimation.gameObject.SetActive(false);
            backgroundFreeSpinLeftAnimation.gameObject.SetActive(false);
            totalMoneyPackageAnimation.gameObject.SetActive(false);
            spinType = SpinType.NORMAL;
            UpdateGameState(SlotGameState.PREPARE);
            SetLightAllItems();
            NextTween();
        });
    }

    private void ShowJackpotAnimation()
    {
        effectContainer.gameObject.SetActive(true);
        jackpotAnimation.gameObject.SetActive(true);
        jackpotText.gameObject.SetActive(true);
        Utility.TweenNumberToNumber(jackpotText, totalChipWinByGame, 0);
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
            .AppendInterval(2.5f)
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
        if (lineOneByOneSequence.IsActive())
        {
            lineOneByOneSequence.Complete(true);
            // lineOneByOneSequence.Pause();
            lineOneByOneSequence.Kill(true);
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

    public void InitJackpot()
    {
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
}
