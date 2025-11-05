using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto;
using DG.Tweening;
using Globals;
using Google.Protobuf;
using Nakama;
using Newtonsoft.Json;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;
public class BaseSlotView : BaseGameView
{
    protected enum WinType
    {
        BIG_WIN,
        MEGA_WIN,
        HUGE_WIN,
        FREE_SPIN,
        FIVE_OF_A_KIND,
        SCATTER,
        NONE
    }

    protected enum StateWin
    {
        WIN,
        TOTAL_WIN,
        LAST_WIN
    }
    [SerializeField] protected Button maxBetButton, plusBetButton, minusBetButton;
    [SerializeField] protected TextMeshProUGUI betAmountText, betInfoSessionText, betStateText, stateWinText, freeSpinLeftText, bigWinText, chipWinText, currentChipText, paylineText,
    numLineLeftText, numLineRightText;
    [SerializeField] protected Image spinBackgroundImage, chipImage, spinButton, stateWinImage, betStateImage;
    [SerializeField] protected List<Sprite> stateWinSpriteList, itemSpriteList, betStateSpriteList;
    [SerializeField] protected Transform lineContainer, effectContainer, paylineInfoContainer, paylineIconContainer, columnContainer, coinParent;
    [SerializeField] protected GameObject rulePrefab, linePrefab, coinPrefab, columnPrefab;
    [SerializeField] protected SkeletonGraphic backgroundFreeSpinAnimation, thirdScatterAnimation, buttonSpinAnimation, animationEffect, backgroundFreeSpinLeftAnimation;

    [Header("Constants")]
    protected readonly string[] colorsList = new string[]
    {
        "#69C4C9", "#067048", "#25A0F0", "#6AF28E", "#003CC3",
        "#1DC42C", "#6C58B1", "#97B158", "#0F0098", "#6700BE",
        "#920E48", "#F277E2", "#BC8B15", "#AC6456", "#E17512",
        "#E8C500", "#F0E915", "#FD93A1", "#C735D4", "#FF0C04",
        "#69C4C9", "#067048", "#25A0F0", "#6AF28E", "#003CC3",
        "#1DC42C", "#6C58B1", "#97B158", "#0F0098", "#6700BE",
        "#920E48", "#F277E2", "#BC8B15", "#AC6456", "#E17512",
        "#E8C500", "#F0E915", "#FD93A1", "#C735D4", "#FF0C04",
        "#69C4C9", "#067048", "#25A0F0", "#6AF28E", "#003CC3",
        "#1DC42C", "#6C58B1", "#97B158", "#0F0098", "#6700BE"
    };
    protected virtual Dictionary<SiXiangSymbol, int> SymbolDictionary => new();
    protected virtual List<int[]> PaylineIdList => new List<int[]>
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
    protected virtual Vector2 RECT_SIZE => new(200, 170);
    protected virtual string SOUND_BACKGROUND_ANIMATION_PATH => Sound.IN_GAME_COMMON;
    protected virtual float AUTO_SPIN_HOLD_DURATION => 1.3f;
    protected virtual int ThirdScatterIndex => 3;
    protected virtual string BIG_WIN_ANIMATION_PATH => "SlotSpine/Noel/big_megawinNoel/skeleton_SkeletonData";
    protected virtual string MEGA_WIN_ANIMATION_PATH => "SlotSpine/Noel/big_megawinNoel/skeleton_SkeletonData";
    protected virtual string HUGE_WIN_ANIMATION_PATH => "SlotSpine/";
    protected virtual string FIVE_OF_A_KIND_ANIMATION_PATH => "SlotSpine/FiveOfAKind/skeleton_SkeletonData";
    protected virtual string FREE_SPIN_ANIMATION_PATH => "SlotSpine/freespin/skeleton_SkeletonData";
    protected virtual string BACKGROUND_FREE_SPIN_ANIMATION_PATH => "SlotSpine/freespin/vienBg/skeleton_SkeletonData";
    protected virtual string BIG_WIN_ANIMATION_NAME => "big";
    protected virtual string MEGA_WIN_ANIMATION_NAME => "mega";
    protected virtual string HUGE_WIN_ANIMATION_NAME => "hugethai";
    protected virtual string FIVE_OF_A_KIND_ANIMATION_NAME => "animation";
    protected virtual string FREE_SPIN_ANIMATION_NAME => "eng";
    protected virtual string BACKGROUND_FREE_SPIN_ANIMATION_NAME => "animation";
    public override bool CanLeaveTable => gameState != SlotGameState.SPINNING;

    [Header("Game State")]
    protected SpinType spinType = SpinType.NORMAL;
    protected SlotGameState gameState = SlotGameState.JOIN_GAME;
    protected WinType winType = WinType.NONE;

    [Header(" Object Pools ")]
    protected UnityEngine.Pool.ObjectPool<Image> coinPool;
    protected UnityEngine.Pool.ObjectPool<GameObject> linePool;

    [Header("Game Data")]
    protected List<long> betLevelList = new();
    protected List<Payline> paylineList = new();
    protected List<SlotColumn> slotColumnList = new();
    protected List<GameObject> allLinesList = new();
    protected List<GameObject> lineOneByOneList = new();
    protected Queue<TweenCallback> tweenQueue = new();
    protected List<Sequence> lineOneByOneSequenceList = new();
    [SerializeField] protected long playerWallet, playerWalletAfter, lastBetLevel, currentBetLevel, lastChipWin = 0, currentChipWin = 0, totalChipWinByGame = 0, lastTotalChipWinByGame = 0;
    protected int totalLineWin = 0, freeSpinLeft = 0;
    public int ScatterCount { get; set; } = 0;
    public bool IsSpinning { get; set; } = false;
    protected bool isHoldingSpin, hasGotFreeSpin, isInFreeSpin, isLastFreeSpin, hasSetupStartView = false, isClickMaxBet = false, isGetMatchResult = false;
    protected float holdingSpinTime = 0;

    protected override void Awake()
    {
        base.Awake();
        Init();
        InitColumns();
        UpdateSpinButtonUI();
        SetSpinAnimation(spinType);
        SoundManager.Instance.PlayMusicInGame(SOUND_BACKGROUND_ANIMATION_PATH);
    }

    protected override void Update()
    {
        if (gameState == SlotGameState.SPINNING || gameState == SlotGameState.SHOWING_RESULT) return;
        HandleHoldingSpin();
    }

    protected override void OnDestroy()
    {
    }

    #region API Handlers
    public override void HandleUpdateTable(IMatchState matchState)
    {
        base.HandleUpdateTable(matchState);
        SlotDesk data = SlotDesk.Parser.ParseFrom(matchState.State);
        winType = data.BigWin switch
        {
            BigWin.Big => WinType.BIG_WIN,
            BigWin.Mega => WinType.MEGA_WIN,
            _ => WinType.NONE,
        };
        Debug.Log("Slot : " +data.ToString());
        if (!hasSetupStartView)
        {
            SetupStartView(data);
        }
        else
        {
            if (!IsSpinning || isGetMatchResult)
            {
                isGetMatchResult = false;
                return;
            }
            OnStartSpin();
        }

        UpdateColumnView(data);
        UpdateReward(data);
        SetWinType(data.GameReward.ChipsWin);     
        if (!hasSetupStartView) hasSetupStartView = true;
    }

    protected void GetMatchResult()
    {
        isGetMatchResult = true;
        DataSender.SendMatchState((long)OpCodeRequest.InfoTable, new byte[0]);
    }
    #endregion

    #region Spin Actions
    protected void HandleSpin()
    {
        Debug.Log("HANDLE SPIN");
        isClickMaxBet = false;
        if (!hasSetupStartView || IsSpinning) return;
        if (!CheckEnoughBalance())
        {
            return;
        }
        InfoBet infoBet = new()
        {
            Chips = currentBetLevel,
        };
        DataSender.SendMatchState((long)OpCodeRequest.Spin, infoBet.ToByteArray());
        UpdateGameState(SlotGameState.SPINNING);
        IsSpinning = true;
    }

    protected void OnStartSpin()
    {
        Debug.Log("START SPIN");
        if (!isInFreeSpin)
        {
            // Nếu đang ko Free Spin thì trừ tiền
            long updatedWallet = playerWallet - currentBetLevel;
            SetCurrentChipValue(updatedWallet);
            SetInfoSessionText($"Playing {PaylineIdList.Count} lines. Good luck!");
            UpdateStateWinUI(StateWin.LAST_WIN); // Last win
        }
        else
        {
            // Nếu đang Free Spin thì hiện Free Spin left
            // UpdateTotalChipWinValue();
        }
        UpdateGameState(SlotGameState.SPINNING);
        IsSpinning = true;
        foreach (SlotColumn column in slotColumnList)
        {
            column.StartSpin(spinType);
        }
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.WHEEL);
    }

    public virtual void OnStopSpin()
    {
        if (isInFreeSpin)
        {
            ShowBackGroundFreeSpin();
        }
        // ShowWinAnimation(WinType.MEGA_WIN);
        IsSpinning = false;
 

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

        if (isLastFreeSpin)
        {
            // if (totalChipWinByGame > PaylineIdList.Count * currentBetLevel) winType = WinType.BIG_WIN;
            // if (totalChipWinByGame > 50 * currentBetLevel) winType = WinType.MEGA_WIN;
            if (winType == WinType.NONE && totalChipWinByGame > 0)
            {
                AnimateCoinsFly();
            }
            if (totalChipWinByGame > lastTotalChipWinByGame)
            {
                UpdateTotalChipWinValue();
            }
            isLastFreeSpin = false;
            spinType = SpinType.NORMAL;
            UpdateGameState(SlotGameState.PREPARE);
            HideBackgroundFreeSpin();
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

        // else
        // {
        //     ShowBackGroundFreeSpin();
        // }

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

    public void OnColumnStop(int columnIndex)
    {
        if (slotColumnList[columnIndex].IsShowingThirdScatter)
        {
            slotColumnList[columnIndex].IsShowingThirdScatter = false;
            HideThirdScatterColumn();
        }
    }

    public virtual void CheckThirdScatter(int columnIndex)
    {
        Debug.Log("COLUMN INDEX: " + columnIndex);
        if (columnIndex == ThirdScatterIndex || isInFreeSpin) return;
        // int nextColumnIndex = columnIndex + 1;
        if (ScatterCount == 2)
        {
            foreach (SlotColumn column in slotColumnList)
            {
                if (column.IsSpinning)
                {
                    column.ExtraTime += 2f;
                }
            }
            slotColumnList[ThirdScatterIndex].IsShowingThirdScatter = true;
            ShowThirdScatterColumn(ThirdScatterIndex);
        }
    }
    #endregion

    #region Buttons
    // giữ nút spin
    public void OnTriggerDownSpinButton()
    {
        holdingSpinTime = 0f;
        isHoldingSpin = true;
    }

    // thả nút spin
    public void OnTriggerUpSpinButton()
    {
        isHoldingSpin = false;
        if (!hasSetupStartView) return;
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.CLICK);
        if (holdingSpinTime < AUTO_SPIN_HOLD_DURATION)
        {
            // Nếu spintype đang là normal hoặc Free Normal thì bấm sẽ bắt đầu quay
            if (spinType == SpinType.NORMAL || spinType == SpinType.FREE_NORMAL)
            {
                // Free Spin thì khi bắt đầu quay sẽ sang trạng thái AUTO luôn
                if (spinType == SpinType.FREE_NORMAL)
                {
                    spinType = SpinType.FREE_AUTO;
                }
                switch (gameState)
                {
                    case SlotGameState.PREPARE:
                    case SlotGameState.JOIN_GAME:
                        HandleSpin();
                        break;
                    case SlotGameState.SHOWING_RESULT:
                        tweenQueue.Clear();
                        NextTween();
                        break;
                }

            }
            // Nếu SpinType là Auto thì bấm sẽ stop và chuyển về Normal
            // Nếu SpinType là FreeAuto thì ko bấm dc
            else if (spinType == SpinType.AUTO)
            {
                switch (gameState)
                {
                    case SlotGameState.SPINNING:
                    case SlotGameState.SHOWING_RESULT:
                        spinType = SpinType.NORMAL;
                        UpdateSpinButtonUI();
                        break;

                }
                // tweenQueue.Clear();
                // NextTween();
                // spinType = SpinType.NORMAL;
            }
        }

    }
    protected void HandleHoldingSpin()
    {
        // Ko Auto thì mới hold dc
        if (isHoldingSpin && spinType != SpinType.AUTO && spinType != SpinType.FREE_AUTO && gameState != SlotGameState.SHOWING_RESULT)
        {
            holdingSpinTime += Time.deltaTime;
            if (holdingSpinTime > AUTO_SPIN_HOLD_DURATION)
            {
                // if (agPlayer < totalListBetRoom[currentMarkBet])
                // {
                //     lbInfoSession.text = Config.getTextConfig("msg_warrning_send");
                //     return;
                // }
                if (spinType == SpinType.NORMAL)
                    spinType = SpinType.AUTO;
                else
                    spinType = SpinType.FREE_AUTO;
                HandleSpin();
                UpdateSpinButtonUI();
                isHoldingSpin = false;
            }
        }
    }

    public virtual void OnClickPlusBetButton()
    {
        if (gameState == SlotGameState.SPINNING || gameState == SlotGameState.SHOWING_RESULT || !hasSetupStartView)
        {
            return; 
        }
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.CLICK);
        lastBetLevel = currentBetLevel;
        currentBetLevel = betLevelList.Find(bet => bet > currentBetLevel);
        if (currentBetLevel == 0)
        {
            currentBetLevel = betLevelList[0];
        }
        SetCurrentBetText(currentBetLevel);
        SetCurrentBetImage(currentBetLevel);
    }

    public virtual void OnClickMinusBetButton()
    {
        if (gameState == SlotGameState.SPINNING || gameState == SlotGameState.SHOWING_RESULT || !hasSetupStartView)
        {
            return; 
        }

        SoundManager.Instance.PlayEffectFromPath(SoundSlot.CLICK);
        lastBetLevel = currentBetLevel;
        currentBetLevel = betLevelList.FindLast(bet => bet < currentBetLevel);
        if (currentBetLevel == 0)
        {
            currentBetLevel = betLevelList[^1];
        }
        SetCurrentBetText(currentBetLevel);
        SetCurrentBetImage(currentBetLevel);
    }

    public virtual void OnClickMaxBetButton()
    {
        if (gameState == SlotGameState.SPINNING || gameState == SlotGameState.SHOWING_RESULT || isClickMaxBet || !hasSetupStartView)
        {
            return;
        }

        SoundManager.Instance.PlayEffectFromPath(SoundSlot.CLICK);
        lastBetLevel = currentBetLevel;
        currentBetLevel = betLevelList[^1];
        SetCurrentBetText(currentBetLevel);
        SetCurrentBetImage(currentBetLevel);
        if (lastBetLevel == currentBetLevel && !isInFreeSpin)
        {
            isClickMaxBet = true;
            HandleSpin();
        }
    }

    public void OnClickShopButton()
    {
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.CLICK);
    }

    public void OnClickMenuButton()
    {
        UIManager.Instance.OpenGroupMenu();
    }

    public override void OpenRule()
    {
        Instantiate(rulePrefab, transform);
    }

    #endregion


    #region Win Effects
    protected void ShowAllWinLines()
    {
        if (gameObject == null) return;
        bool isCurrentlyAutoSpin = spinType == SpinType.AUTO;
        if (isCurrentlyAutoSpin)
        {
            UpdateChipWinValue();
        }
        else if (spinType == SpinType.FREE_AUTO || spinType == SpinType.FREE_NORMAL)
        {
            UpdateTotalChipWinValue();
        }
        betInfoSessionText.gameObject.SetActive(false);
        SetDarkAllItems();
        // Draw Line và lưu vào listLine
        foreach (Payline payline in paylineList)
        {
            int[] lineWinID = GetPaylineWithID(payline.Id);
            ColorUtility.TryParseHtmlString(colorsList[payline.Id % colorsList.Length], out Color colorLine);
            List<Vector2> listPosition = new();
            for (int j = 0; j < lineWinID.Length; j++)
            {
                Vector2 positionItem = slotColumnList[j].GetItemPositionAtIndex(lineWinID[j]);
                Vector2 positionInContainer = lineContainer.transform.InverseTransformPoint(positionItem);
                listPosition.Add(positionInContainer);
            }
            DrawLines(listPosition, colorLine);
        }

        // Show Line từ listLine
        int totalLines = allLinesList.Count;
        Sequence sequence = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDestroy);;

        // Hiện line lên, mỗi line cách nhau 0.1s
        for (int i = 0; i < totalLines; i++)
        {
            GameObject line = allLinesList[i];
            sequence
                .AppendCallback(() => line.SetActive(true))
                .AppendInterval(0.1f);
        }

        // Sau 1.5s thì ẩn hết line đi và show tween tiếp theo
        sequence
            .AppendInterval(1.5f)
            .OnComplete(() =>
            {
                foreach (GameObject line in allLinesList)
                {
                    linePool.Release(line);
                }
                if (isCurrentlyAutoSpin && winType == WinType.NONE)
                {
                    AnimateCoinsFly();
                }
                SetLightAllItems();
                NextTween();
            });
    }

    protected void ShowWinLineOneByOne()
    {
        if (gameObject == null) return;
        // // Nếu đang FREE SPIN -> Update totalChipWinByGame 
        // if (spinType == SpinType.FREE_NORMAL || spinType == SpinType.FREE_AUTO)
        // {
        //     UpdateTotalChipWinValue();
        // }
        // Nếu Spin thường thì update currentChipWin, nếu Auto thường thì ko update vì đã update ở ShowAllWinLines
        if (spinType == SpinType.NORMAL)
        {
            if (currentChipWin > 0 && !isLastFreeSpin)
            {
                UpdateChipWinValue();
                AnimateCoinsFly();
            }
        }
        UpdateGameState(SlotGameState.SHOWING_RESULT);
        for (int i = 0; i < paylineList.Count; i++)
        {

            int index = i;
            Payline payline = paylineList[i];
            // List<int> lineWinID = getPaylineWithID(lineId);
            int[] lineWinID = GetPaylineWithID(payline.Id);
            ColorUtility.TryParseHtmlString(colorsList[payline.Id % colorsList.Length], out Color colorLine);

            Sequence sequence = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDestroy);;
            lineOneByOneSequenceList.Add(sequence);
            sequence
                .AppendInterval(2.0f * index)
                .AppendCallback(() =>
                {
                    if (sequence == null || !sequence.IsActive())
                        return;
                    else
                    {
                        SoundManager.Instance.PlayEffectFromPath(SoundSlot.SHOW_LINE);
                        SetDarkAllItems();
                        DrawRectangularAndConnectingLines(lineWinID, payline.Indices[0] % 5, payline.NumOccur, colorLine);
                        ShowPaylinesInfo(index);
                    }

                })
                .AppendInterval(2.0f)
                .AppendCallback(() =>
                {
                    foreach (GameObject line in lineOneByOneList)
                    {
                        linePool.Release(line);
                    }
                    lineOneByOneList.Clear();
                })
                .AppendCallback(() =>
                {
                    if (index == paylineList.Count - 1)
                    {
                        if (spinType == SpinType.NORMAL)
                        {
                            SetLightAllItems();
                        }
                        NextTween();
                    }
                });
        }
    }

    protected void ShowPaylinesInfo(int index)
    {
        foreach (Transform child in paylineIconContainer)
        {
            child.gameObject.SetActive(false);
        }
        paylineText.text = "";

        paylineInfoContainer.gameObject.SetActive(true);
        Payline payline = paylineList[index];
        int paylineIconId = 0;
        if (SymbolDictionary.TryGetValue(payline.Symbol, out int mappedValue))
        {
            paylineIconId = mappedValue;
        }
        for (int i = 0; i < payline.NumOccur; i++)
        {
            Image paylineIcon = paylineIconContainer.GetChild(i).GetComponent<Image>();
            paylineIcon.sprite = itemSpriteList[paylineIconId];
            paylineIcon.gameObject.SetActive(true);
            paylineIcon.SetNativeSize();
        }
        paylineText.text = $"Win {payline.Chips} chips";
    }

    protected void ShowWinScatter()
    {
        // Nếu đang ko Free Spin (tính cả Fruit Rain) thì có hiệu ứng tiền bay và Update tiền thưởng ngay lập tức
        // if (!isInFreeSpin)
        // {
        //     AnimateCoinsFly();
        //     SetCurrentChipValue(playerWalletAfter);
        //     UpdateChipWinValue();
        // }
        // else
        // {
        //     UpdateTotalChipWinValue();
        // }
        List<int> scatterColumnIds = new();
        slotColumnList.ForEach(arr =>
        {
            if (arr.ResultItem.GetFinishView().Contains(12))
            {
                scatterColumnIds.Add(slotColumnList.IndexOf(arr));
            }
        });
        foreach (int id in scatterColumnIds)
        {
            slotColumnList[id].ResultItem.ShowScatterAnimation();
        }   
        DOTween.Sequence().AppendInterval(3.5f).AppendCallback(() =>
        {
            SetLightAllItems();
            if (paylineList.Count == 0 && currentChipWin > 0)
            {
                if (isInFreeSpin)
                {
                    UpdateTotalChipWinValue();
                }
                else
                {
                    UpdateChipWinValue();
                    if (!isLastFreeSpin)
                    {
                        AnimateCoinsFly();
                    }
                }
            }
            NextTween();
        }).SetLink(gameObject, LinkBehaviour.KillOnDestroy);;
    }

    protected virtual void ShowWinAnimation(WinType winType)
    {
        float delay = 5.5f;
        effectContainer.gameObject.SetActive(true);
        animationEffect.gameObject.SetActive(true);

        switch (winType)
        {
            case WinType.BIG_WIN:
                delay = 3f;
                SoundManager.Instance.PlayEffectFromPath(SoundSlot.BIG_WIN);
                bigWinText.transform.parent.gameObject.SetActive(true);
                bigWinText.gameObject.SetActive(true);
                Utility.TweenNumberToNumber(bigWinText, totalChipWinByGame, 0, delay - 1);
                // animationEffect.transform.localScale = new Vector2(0.9f, 0.9f);
                animationEffect.transform.localScale = new Vector2(1f, 1f);
                // animationEffect.transform.localPosition = new Vector2(0, -70);
                Utility.PlayAnimationByPath(animationEffect, BIG_WIN_ANIMATION_PATH, BIG_WIN_ANIMATION_NAME, false);
                break;
            case WinType.MEGA_WIN:
                delay = 5.5f;
                SoundManager.Instance.PlayEffectFromPath(SoundSlot.MEGA_WIN);
                bigWinText.transform.parent.gameObject.SetActive(true);
                bigWinText.gameObject.SetActive(true);
                Utility.TweenNumberToNumber(bigWinText, totalChipWinByGame, 0, delay - 1);
                // animationEffect.transform.localScale = new Vector2(0.9f, 0.9f);
                animationEffect.transform.localScale = new Vector2(1f, 1f);
                // animationEffect.transform.localPosition = new Vector2(0, -70);
                Debug.Log("Show big win out of mega win");
                Utility.PlayAnimationByPath(animationEffect, BIG_WIN_ANIMATION_PATH, BIG_WIN_ANIMATION_NAME, false);
                DOVirtual.DelayedCall(1.2f, () =>
                {
                Debug.Log("Show mega win out of mega win");
                    Utility.PlayAnimationByPath(animationEffect, MEGA_WIN_ANIMATION_PATH, MEGA_WIN_ANIMATION_NAME, false);
                    animationEffect.AnimationState.Complete += delegate
                    {
                        effectContainer.gameObject.SetActive(false);
                        if (new WinType[] { WinType.BIG_WIN, WinType.HUGE_WIN, WinType.MEGA_WIN }.Contains(winType))
                        {
                            bigWinText.transform.parent.gameObject.SetActive(false);
                            AnimateCoinsFly();
                        }
                        NextTween();
                        effectContainer.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
                    };
                });
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


        if (winType != WinType.MEGA_WIN)
        {
            animationEffect.AnimationState.Complete += delegate
            {
                effectContainer.gameObject.SetActive(false);
                if (new WinType[] { WinType.BIG_WIN, WinType.HUGE_WIN, WinType.MEGA_WIN }.Contains(winType))
                {
                    bigWinText.transform.parent.gameObject.SetActive(false);
                    AnimateCoinsFly();
                }
                NextTween();
                effectContainer.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
            };
        }
    }

    private void ShowThirdScatterColumn(int indexCol)
    {
        thirdScatterAnimation.gameObject.SetActive(true);
        thirdScatterAnimation.transform.localPosition = new Vector2(thirdScatterAnimation.transform.parent.InverseTransformPoint(slotColumnList[indexCol].transform.position).x, thirdScatterAnimation.transform.localPosition.y);
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.NEAR_FREESPIN_START);
    }

    private void HideThirdScatterColumn()
    {
        thirdScatterAnimation.gameObject.SetActive(false);
    }

    private void HideWinAnimation()
    {

    }

    protected virtual bool CheckWinScatter()
    {
        int numberScatter = slotColumnList.FindAll(col => col.ResultItem.GetFinishView().Contains(12)).Count;
        if (numberScatter >= 3) hasGotFreeSpin = true;
        return numberScatter >= 2;
    }

    protected bool CheckFiveOfAKind()
    {
        bool isFiveOfAKind = false;
        foreach (Payline payline in paylineList)
        {
            if (payline.NumOccur == 5)
            {
                isFiveOfAKind = true;
                break;
            }
        }
        return isFiveOfAKind;
    }
    #endregion

    #region Draw Lines
    private void DrawLines(List<Vector2> positionList, Color colorLine)
    {
        Vector2 startPosition = new(positionList[0].x - 90, positionList[0].y);
        Vector2 lastPosition = new(positionList[^1].x + 90, positionList[^1].y);
        positionList.Insert(0, startPosition);
        positionList.Add(lastPosition);
        GameObject line = linePool.Get();

        RectTransform rectTransform = line.GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0);
        line.SetActive(false);
        allLinesList.Add(line);
        LineController lineController = line.GetComponent<LineController>();
        lineController.DrawLine(positionList, colorLine);
    }

    protected void DrawLineBetween2Points(List<Vector2> listPos, Color colorLine)
    {
        GameObject line = linePool.Get();
        line.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        lineOneByOneList.Add(line);
        LineController lineController = line.GetComponent<LineController>();
        lineController.DrawLine(listPos, colorLine);
    }

    protected void DrawSquare(Vector2 startPos, Color colorLine)
    {
        GameObject lineRect = linePool.Get();
        RectTransform rectTransform = lineRect.GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0);
        LineController lineController = lineRect.GetComponent<LineController>();
        lineController.DrawRect(startPos, RECT_SIZE, colorLine);
        lineOneByOneList.Add(lineRect);
    }

    protected virtual void DrawRectangularAndConnectingLines(int[] lineWinID, int startIndex, int matchedItemCount, Color colorLine)
    {
        // lineWinID: { 0, 1, 0, 1, 0 }
        List<Vector2> itemPositions = new();

        // Bước 1: Thu thập vị trí & id của các item trên line
        for (int colIndex = 0; colIndex < lineWinID.Length; colIndex++)
        {
            // Lấy vị trí item theo world position
            Vector2 worldPos = slotColumnList[colIndex].GetItemPositionAtIndex(lineWinID[colIndex]);

            // Chuyển về local position trong container
            Vector2 localPos = lineContainer.transform.InverseTransformPoint(worldPos);
            itemPositions.Add(localPos);
        }

        // Bước 2: Highlight các item thắng
        for (int colIndex = startIndex; colIndex < startIndex + matchedItemCount; colIndex++)
        {
            int itemIndex = lineWinID[colIndex];
            slotColumnList[colIndex].SetLightItemAtIndex(itemIndex);
            slotColumnList[colIndex].SetAnimationForItemAtIndex(itemIndex);
        }

        // Bước 3: Vẽ line highlight
        List<Vector2> remainingLinePoints = new();
        List<Vector2> startingLinePoints = new();

        for (int i = 0; i < itemPositions.Count; i++)
        {
            Vector2 currentPosition = itemPositions[i];

            // Vẽ hình vuông nếu còn trong số lượng item
            if (i >= startIndex && i < matchedItemCount + startIndex)
            {
                DrawSquare(currentPosition, colorLine);
            }

            // Vẽ đường nối tiếp nếu cần
            if (i >= startIndex && i < itemPositions.Count - 1)
            {
                Vector2 nextPos = itemPositions[i + 1];
                if (Mathf.Abs(nextPos.y - currentPosition.y) > 50 && i < startIndex + matchedItemCount - 1)
                {
                    List<Vector2> listPos = new();
                    Vector2 firstIntersectPos = GetIntersectPoint(itemPositions[i], itemPositions[i + 1]);
                    Vector2 nextIntersectPos = GetIntersectPoint(itemPositions[i + 1], itemPositions[i]);
                    listPos.Add(firstIntersectPos);
                    listPos.Add(nextIntersectPos);
                    DrawLineBetween2Points(listPos, colorLine);
                }
            }

            // Tính toán các điểm bắt đầu của line còn lại

            if (i >= startIndex + matchedItemCount) // bắt đầu sau hình vuông cuối cùng
            {
                if (remainingLinePoints.Count == 0 && i > 0)
                {
                    Vector2 previousPos = itemPositions[i - 1];
                    Vector2 startPosLineRemain;
                    if (Mathf.Abs(previousPos.y - currentPosition.y) < 100)
                    {
                        startPosLineRemain = new Vector2(previousPos.x + RECT_SIZE.x / 2, previousPos.y);
                    }
                    else
                    {
                        bool isTwoItemSpace = Mathf.Abs(currentPosition.y - previousPos.y) > 200;
                        if (previousPos.y < currentPosition.y)
                        {
                            startPosLineRemain = isTwoItemSpace ? new Vector2(previousPos.x, previousPos.y + RECT_SIZE.y / 2) : new Vector2(previousPos.x + RECT_SIZE.x / 2, previousPos.y + RECT_SIZE.y / 2);
                        }
                        else
                        {
                            startPosLineRemain = isTwoItemSpace ? new Vector2(previousPos.x, previousPos.y - RECT_SIZE.y / 2) : new Vector2(previousPos.x + RECT_SIZE.x / 2, previousPos.y - RECT_SIZE.y / 2);
                        }
                    }
                    remainingLinePoints.Add(startPosLineRemain);
                }
                remainingLinePoints.Add(currentPosition);
            }

            // Nếu startIndex != 0
            if (i < startIndex)
            {
                startingLinePoints.Add(currentPosition);
                if (i == 0)
                {
                    Vector2 startPosition = new(startingLinePoints[0].x - 90, startingLinePoints[0].y);
                    startingLinePoints.Insert(0, startPosition);
                }
            }

            if (i == startIndex - 1)
            {
                List<Vector2> listPos = new();
                Vector2 firstIntersectPos = itemPositions[i];
                // Vector2 nextIntersectPos = GetIntersectPoint(itemPositions[i + 1], itemPositions[i]);
                Vector2 nextIntersectPos = itemPositions[i + 1];
                nextIntersectPos = new Vector2(nextIntersectPos.x - RECT_SIZE.x / 2, nextIntersectPos.y);
                listPos.Add(firstIntersectPos);
                listPos.Add(nextIntersectPos);
                DrawLineBetween2Points(listPos, colorLine);
            }
        }

        // Thêm đoạn line cuối từ icon cuối ra mép phải
        Vector2 lastPos = itemPositions[^1];
        Vector2 endRemainingLine = new Vector2(lastPos.x + RECT_SIZE.x / 2, lastPos.y);
        remainingLinePoints.Add(endRemainingLine);

 

        // Vẽ line còn lại
        DrawLineBetween2Points(remainingLinePoints, colorLine);
        DrawLineBetween2Points(startingLinePoints, colorLine);

    }

    protected Vector2 GetIntersectPoint(Vector2 vector1, Vector2 vector2)
    {

        int delta = vector1.y > vector2.y ? -1 : 1;
        Vector2 crossPoint = new(vector1.x - RECT_SIZE.x / 2, vector1.y + RECT_SIZE.y / 2 * delta);
        if (Mathf.Abs(vector1.y - vector2.y) < 1)
        {
            crossPoint = new Vector2(vector1.x + RECT_SIZE.x / 2, vector1.y + RECT_SIZE.y / 2);
        }
        return new(crossPoint.x + RECT_SIZE.x / 2, crossPoint.y);
    }

    protected int[] GetPaylineWithID(int id)
    {
        return PaylineIdList[id - 1];
    }
    #endregion

    #region UI
    protected void UpdateSpinButtonUI()
    {
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

    protected virtual void UpdateStateWinUI(StateWin stateWin)
    {
        bool isUsingStateImage = stateWinImage.gameObject.activeSelf;
        switch (stateWin)
        {
            case StateWin.WIN when isUsingStateImage:
                stateWinImage.sprite = stateWinSpriteList[0];
                break;
            case StateWin.WIN when !isUsingStateImage:
                stateWinText.text = "Win";
                break;
            case StateWin.TOTAL_WIN when isUsingStateImage:
                stateWinImage.sprite = stateWinSpriteList[1];
                break;
            case StateWin.TOTAL_WIN when !isUsingStateImage:
                stateWinText.text = "Total Win";
                break;
            case StateWin.LAST_WIN when isUsingStateImage:
                stateWinImage.sprite = stateWinSpriteList[2];
                break;
            case StateWin.LAST_WIN when !isUsingStateImage:
                stateWinText.text = "Last Win";
                break;
        }
    }

    protected virtual void SetSpinAnimation(SpinType type)
    {
        buttonSpinAnimation.startingAnimation = type switch
        {
            SpinType.NORMAL => "autospin",
            SpinType.FREE_NORMAL or SpinType.FREE_AUTO => "freespin",
            SpinType.AUTO => "stop",
            _ => "autospin"
        };
    }

    protected virtual void ShowBackGroundFreeSpin()
    {
        Debug.Log("ABCDEF");
        if (backgroundFreeSpinAnimation != null)
        {
            backgroundFreeSpinAnimation.gameObject.SetActive(true);
            // if (Config.curGameId == (int)GAMEID.SLOT_INCA)
            // {
            //     backgroundFreeSpinAnimation.transform.localScale = Vector2.one * 1.4f;
            // }
            Utility.PlayAnimationByPath(backgroundFreeSpinAnimation, BACKGROUND_FREE_SPIN_ANIMATION_PATH, BACKGROUND_FREE_SPIN_ANIMATION_NAME, true);
        }

        if (backgroundFreeSpinLeftAnimation != null)
        {
            backgroundFreeSpinLeftAnimation.gameObject.SetActive(true);
        }

        freeSpinLeftText.text = $"Freespin left: {freeSpinLeft}";
    }

    protected void HideBackgroundFreeSpin()
    {
         if (backgroundFreeSpinAnimation != null)
        {
            backgroundFreeSpinAnimation.gameObject.SetActive(false);
        }

        if (backgroundFreeSpinLeftAnimation != null)
        {
            backgroundFreeSpinLeftAnimation.gameObject.SetActive(false);
        }
    }

    protected void SetInfoSessionText(string text)
    {
        betInfoSessionText.gameObject.SetActive(true);
        betInfoSessionText.text = text;
    }

    protected void SetBetStateText(string text)
    {
        betStateText.gameObject.SetActive(true);
        betStateText.text = text;
    }

    protected void SetBetStateImage(string text)
    {
        betStateImage.gameObject.SetActive(true);
        betStateImage.sprite = text == "Max bet" ? betStateSpriteList[1] : betStateSpriteList[0];
    }

    protected void SetCurrentBetText(long betLevel)
    {
        if (betStateText == null) return;
        betAmountText.text = Utility.FormatNumber(betLevel);
        if (betLevel == betLevelList[^1])
        {
            SetBetStateText("Max bet");
        }
        else
        {
            SetBetStateText("Bet");
        }
    }

    protected void SetCurrentBetImage(long betLevel)
    {
        if (betStateImage == null) return;
        betAmountText.text = Utility.FormatNumber(betLevel);
        if (betLevel == betLevelList[^1])
        {
            SetBetStateImage("Max bet");
        }
        else
        {
            SetBetStateImage("Bet");
        }
    }

    protected void UpdateChipWinValue()
    {
        UpdateStateWinUI(StateWin.WIN);
        Utility.TweenNumberToNumberScale1(chipWinText, currentChipWin, lastChipWin, 0.5f, false);
    }
    protected void UpdateTotalChipWinValue()
    {
        UpdateStateWinUI(StateWin.TOTAL_WIN);
        Utility.TweenNumberToNumberScale1(chipWinText, totalChipWinByGame, lastTotalChipWinByGame, 0.5f, false);
    }

    protected void SetCurrentChipValue(long value)
    {
        Debug.Log("CURRENT CHIP VALUE: " + value);
        Utility.TweenNumberToNumberScale1(currentChipText, value, playerWallet, 0.5f, false);
        playerWallet = value;
    }

    protected void SetupStartView(SlotDesk data)
    {
        betLevelList = data.BetLevels.ToList();
        currentBetLevel = data.ChipsMcb;
        lastBetLevel = currentBetLevel;
        SetInfoSessionText("Press SPIN to play");
        SetCurrentBetText(currentBetLevel);
        SetCurrentBetImage(currentBetLevel);
        SetCurrentChipValue(data.GameReward.BalanceChipsWalletAfter);

        if (data.GameConfig != null)
        {
            freeSpinLeft = (int)data.GameConfig.NumFreeSpin;
            isLastFreeSpin = data.GameConfig.NumFreeSpin <= 0;
        }
        lastTotalChipWinByGame = totalChipWinByGame;
        totalChipWinByGame = data.GameReward.TotalChipsWinByGame;
        if (freeSpinLeft > 0)
        {
            UpdateTotalChipWinValue();
            ShowBackGroundFreeSpin();
            spinType = SpinType.FREE_NORMAL;
            UpdateSpinButtonUI();
        }
    }

    protected void UpdateColumnView(SlotDesk data)
    {
        List<SiXiangSymbol> listSymbols = data.Matrix.Lists.ToList();
        List<SpinSymbol> spinList = data.Matrix.SpinLists.ToList();
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

            if (hasSetupStartView)
            {
                column.SetFinishView(columnArray);
                paylineList = data.Paylines.ToList();

            }
            else
            {
                column.SetStartView(columnArray);
            }
        }
    }

    protected void UpdateReward(SlotDesk data)
    {
        if (data.GameConfig != null)
        {
            freeSpinLeft = (int)data.GameConfig.NumFreeSpin;
            if (data.GameConfig.NumFreeSpin <= 0)
            {
                backgroundFreeSpinAnimation.gameObject.SetActive(false);
                isLastFreeSpin = true;
            }
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
    
        if (data.GameReward.UpdateWallet)
        {
            lastChipWin = currentChipWin;
            currentChipWin = data.GameReward.ChipsWin;
            totalChipWinByGame = data.GameReward.TotalChipsWinByGame;
            playerWalletAfter = data.GameReward.BalanceChipsWalletAfter;
        }
    }
    #endregion

    #region Effects
    protected void AnimateCoinsFly(int totalCoins = 5, float timeInterval = 0.05f)
    {
        Debug.Log("AnimateCoinsFly");
        Sequence sequence = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDestroy);;
        for (int i = 0; i < totalCoins; i++)
        {
            int index = i;
            sequence
                .AppendInterval(i * timeInterval)
                .AppendCallback(() =>
                {
                    Image coin = coinPool.Get();
                    coin.gameObject.SetActive(true);
                    // coin.GetComponent<Animator>().Play("Idle");
                    AnimateCoinFly(coin, chipWinText.transform, chipImage.transform);
                });
        }
        sequence.OnComplete(() =>
        {
            SetCurrentChipValue(playerWalletAfter);
        });
    }
    protected void AnimateCoinFly(Image coin, Transform from, Transform to)
    {
        coin.transform.position = from.position;
        coin.transform.DOJump(to.position, 1, 1, 2f).SetEase(Ease.InOutCubic);
        Color fadedColor = coin.color;
        fadedColor.a = .2f;
        coin.color = fadedColor;
        coin.DOFade(1, .75f);

        DOTween.Sequence()
            .AppendInterval(.5f)
            .Append(coin.transform.DOScale(2, 0.25f))
            .AppendInterval(0.15f)
            .Append(coin.transform.DOScale(0.75f, 0.25f))//0.85
            .AppendInterval(0.6f)
            .Append(coin.DOFade(0, .25f)).AppendCallback(() =>
            {
                coinPool.Release(coin);
            }).SetLink(gameObject, LinkBehaviour.KillOnDestroy);;
    }
    #endregion

    #region Helpers

    private void Init()
    {
        paylineInfoContainer.gameObject.SetActive(false);
        if (numLineLeftText != null) numLineLeftText.text = PaylineIdList.Count.ToString();
        if (numLineRightText != null) numLineRightText.text = PaylineIdList.Count.ToString();
        coinPool = new UnityEngine.Pool.ObjectPool<Image>(
            createFunc: () =>
            {
                var coin = Instantiate(coinPrefab, coinParent).GetComponent<Image>();
                coin.gameObject.SetActive(false); // bắt đầu ẩn
                return coin;
            },
            actionOnGet: (coin) =>
            {
                coin.gameObject.SetActive(true);
                coin.transform.localScale = Vector3.one;
                coin.color = new Color(coin.color.r, coin.color.g, coin.color.b, 0f);
            },
            actionOnRelease: (coin) =>
            {
                coin.gameObject.SetActive(false);
            },
            actionOnDestroy: (coin) =>
            {
                Destroy(coin.gameObject);
            },
            collectionCheck: false,  // không cần check trùng (cho nhanh)
            defaultCapacity: 5,     // số lượng khởi tạo
            maxSize: 10             // tối đa object trong pool
        );
        linePool = new UnityEngine.Pool.ObjectPool<GameObject>(
            createFunc: () =>
            {
                var line = Instantiate(linePrefab, lineContainer);
                line.SetActive(false); // bắt đầu ẩn
                return line;
            },
            actionOnGet: (line) =>
            {
                line.SetActive(true);
                line.transform.localScale = Vector3.one;
            },
            actionOnRelease: (line) =>
            {
                line.SetActive(false);
            },
            actionOnDestroy: (line) =>
            {
                Destroy(line);
            },
            defaultCapacity: 1,     // số lượng khởi tạo
            maxSize: 400             // tối đa object trong pool
        );
    }


    private void InitColumns()
    {
        for (int i = 0; i < 5; i++)
        {
            SlotColumn column = Instantiate(columnPrefab, columnContainer).GetComponent<SlotColumn>();
            column.SetInfo(this, i);
            // column.SetRandomSprite();
            slotColumnList.Add(column);
        }
    }

    protected virtual void NextTween()
    {
        Debug.Log("Next tween");
        if (tweenQueue.Count > 0)
        {
            TweenCallback nextTween = tweenQueue.Dequeue();
            DOTween.Sequence().AppendCallback(nextTween).SetLink(gameObject, LinkBehaviour.KillOnDestroy);;
        }
        // Hết tween = hết show win line
        else
        {
            if (hasGotFreeSpin)
            {
                if (spinType == SpinType.NORMAL || spinType == SpinType.AUTO)
                {
                    spinType = SpinType.FREE_NORMAL;
                }
                ShowBackGroundFreeSpin();
                UpdateStateWinUI(StateWin.TOTAL_WIN);
                chipWinText.text = "0";
                freeSpinLeftText.text = "9";
                hasGotFreeSpin = false;
                isInFreeSpin = true;

            }
            Reset();
            // Nếu đang auto spin thì spin tiếp
            if (spinType == SpinType.AUTO || spinType == SpinType.FREE_AUTO)
            {
                HandleSpin();
            }
        }
    }

    protected virtual void SetWinType(long winAmount)
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
        else if (winAmount > 20 * currentBetLevel)
        {
            winType = WinType.BIG_WIN;
        }
    }

    protected void SetDarkAllItems(bool isBackgroundDark = true)
    {
        if (isBackgroundDark)
        {
            spinBackgroundImage.color = Color.gray;
        }
        foreach (SlotColumn column in slotColumnList)
        {
            column.SetDarkAllItems();
        }
    }

    protected void SetLightAllItems()
    {
        Debug.Log("Set light all items");
        spinBackgroundImage.color = Color.white;
        foreach (SlotColumn column in slotColumnList)
        {
            column.SetLightAllItems();
        }
    }

    protected void UpdateGameState(SlotGameState gameState)
    {
        this.gameState = gameState;
        UpdateSpinButtonUI();
    }

    protected bool CheckEnoughBalance()
    {
        if (playerWallet < currentBetLevel && (spinType == SpinType.NORMAL || spinType == SpinType.AUTO))
        {
            spinType = SpinType.NORMAL;
            UpdateSpinButtonUI();
            string message = "Not enough chips to spin.";
            UIManager.Instance.ShowConfirmDialog(message, () => UIManager.Instance.OpenShop(), null, "Get More Chips");
            return false;
        }
        return true;
    }

    protected virtual void Reset()
    {
        Debug.Log("RESET");
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
        allLinesList.Clear();
        lineOneByOneList.Clear();
        paylineList.Clear();
        SetLightAllItems();

        foreach (SlotColumn column in slotColumnList)
        {
            column.ExtraTime = 0f;
        }
        SetInfoSessionText("Press SPIN to play");
        ScatterCount = 0;
        paylineInfoContainer.gameObject.SetActive(false);

        if (isInFreeSpin)
        {
            ShowBackGroundFreeSpin();
            freeSpinLeftText.gameObject.SetActive(true);
        }
        else
        {
            backgroundFreeSpinAnimation.gameObject.SetActive(false);
            backgroundFreeSpinLeftAnimation.gameObject.SetActive(false);
            freeSpinLeftText.gameObject.SetActive(false);
        }

        if (hasGotFreeSpin)
        {
            freeSpinLeftText.gameObject.SetActive(true);
        }

        if (isLastFreeSpin)
        {
            freeSpinLeftText.gameObject.SetActive(false);
        }
        // isFiveOfaKind = false;
        // isWinScatter = false;
        // listActionHandleSpin.Clear();
        // slotViews.Clear();
        // countScatter = 0;

            // if (finishData != null)
            // {
            //     if (isInFreeSpin && !isFreeSpin)
            //     {
            //         countTotalAgFreespin = 0;
            //     }
            // }
            // setStateBtnSpin();
            // if (freespinLeft > 0)
            // {
            //     if (Config.curGameId == (int)GAMEID.SLOTTARZAN)
            //     {
            //         lbFreespinLeft.gameObject.SetActive(true);
            //         lbFreespinLeft.text = freespinLeft.ToString();
            //     }
            //     else
            //     {
            //         animFreespinNum.gameObject.SetActive(true);
            //         lbFreespinLeft.text = Config.getTextConfig("txt_freespinRM") + ": " + freespinLeft;
            //     }
            // }
            // else
            // {
            //     if (animBgFreeSpin != null)
            //     {
            //         animBgFreeSpin.gameObject.SetActive(false);
            //     }
            //     if (Config.curGameId != (int)GAMEID.SLOTTARZAN)
            //     {
            //         animFreespinNum.gameObject.SetActive(false);
            //     }
            //     else
            //     {
            //         lbFreespinLeft.gameObject.SetActive(false);
            //     }
            // }
    }

    #endregion

    public SpinType GetSpinType() => spinType;
}
