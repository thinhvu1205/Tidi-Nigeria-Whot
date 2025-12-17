using System;
using System.Collections.Generic;
using System.Linq;
using Proto;
using DG.Tweening;
using Globals;
using Google.Protobuf;
using Nakama;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class BaseSlotSymbolView : BaseGameView
{
    protected enum WinType
    {
        NICE_WIN,
        BIG_WIN,
        MEGA_WIN,
        HUGE_WIN,
        FREE_SPIN,
        FIVE_OF_A_KIND,
        SCATTER,
        NONE
    }

    public enum WinJackpotType
    {
        NONE = 0,
        JACKPOT_MINOR = 1,
        JACKPOT_MAJOR = 2,
        JACKPOT_MEGA = 3,
        JACKPOT_GRAND = 4,
    }

    protected enum StateWin
    {
        WIN,
        TOTAL_WIN,
        LAST_WIN
    }

    [SerializeField] protected SkeletonGraphic animationSpecialWin, animationJackpotWin, animationBackgroundMoney, animationBtnSpin, animationBackground;
    [SerializeField] protected GameObject rulePrefab, coinPrefab, linePrefab, columnPrefab;
    [SerializeField] protected Sprite[] listBetStateSprite, listStateWinSprite, listSymbolSprite;
    [SerializeField] protected Image imageStateBet, imageStateWin, imageBackgroundSpin;
    [SerializeField] protected TextMeshProUGUI textAutoRemain, textInfoSession, textCurrentBet, textStateWin;
    [SerializeField] protected TextNumberControl textSpecialWin, textUserChip, textChipWin, textJackpotWin;
    [SerializeField] protected TextNumberControl[] listTextJackpot;
    [SerializeField] protected Image[] listGemImage;
    [SerializeField] protected Button[] listBuyGemButton;
    [SerializeField] protected Transform columnContainer, lineContainer, autoSpinContainer, effectContainer, coinParent, paylineIconContainer, paylineInfoContainer;
    [SerializeField] protected Button buttonConfirmSpecialWin, buttonConfirmJackpotWin;
    [SerializeField] protected SiXiangDragonPearlView dragonPearlView;


    [Header("Constants")]
    protected List<int[]> listPaylineId = new()
    {
        new int[] {0, 0, 0, 0, 0},
        new int[] {1, 1, 1, 1, 1},
        new int[] {2, 2, 2, 2, 2},
        new int[] {2, 1, 0, 1, 2},
        new int[] {0, 1, 2, 1, 0},
        new int[] {1, 2, 1, 0, 1},
        new int[] {1, 2, 1, 2, 1},
        new int[] {0, 1, 0, 1, 2},
        new int[] {2, 1, 2, 1, 0},
        new int[] {2, 1, 0, 1, 0},
        new int[] {0, 1, 2, 1, 2},
        new int[] {1, 0, 1, 2, 1},
        new int[] {2, 1, 2, 1, 2},
        new int[] {1, 0, 1, 0, 1},
        new int[] {0, 1, 0, 1, 0},
        new int[] {1, 1, 2, 1, 1},
        new int[] {0, 0, 1, 0, 0},
        new int[] {2, 2, 1, 2, 2},
        new int[] {1, 1, 0, 1, 1},
        new int[] {0, 0, 2, 0, 0},
        new int[] {2, 2, 0, 2, 2},
        new int[] {1, 1, 1, 2, 1},
        new int[] {0, 0, 0, 1, 2},
        new int[] {2, 2, 2, 1, 0},
        new int[] {2, 1, 1, 1, 0}
    };

    protected string[] listColor = new string[]
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
        "#1DC42C", "#6C58B1", "#97B158", "#0F0098", "#6700BE",
        "#69C4C9", "#067048", "#25A0F0", "#6AF28E", "#003CC3",
        "#1DC42C", "#6C58B1", "#97B158", "#0F0098", "#6700BE"
    };
    protected virtual string SPECIAL_WIN_ANIMATION_PATH => "";
    protected virtual string SOUND_BACKGROUND_ANIMATION_PATH => Sound.IN_GAME_COMMON;
    [Header("Game State")]
    protected SpinType spinType = SpinType.NORMAL;
    protected SlotGameState gameState = SlotGameState.JOIN_GAME;
    protected WinType winType = WinType.NONE;

    [Header("Game Data")]
    protected List<SlotSymbolColumn> listColumn = new();
    protected List<long> listBetLevel = new();
    protected List<Payline> listPayline = new();
    protected List<GameObject> listLine = new();
    protected List<SiXiangGame> listGem = new();
    protected List<SpinSymbol> listSpinSymbol = new();
    protected Queue<TweenCallback> tweenQueue = new();
    protected bool isHoldingSpin = false, isInSixiangBonus = false, isClickMaxBet = false;
    protected long[] listDataJackpot;
    protected int autoSpinRemain = 0, freeSpinLeft = 0;
    protected float holdingSpinTime = 0;
    protected long lastBetLevel = 0, currentBetLevel = 0, playerChip = 0, winAmount = 0, normalWinAmount = 0, lastWinAmount = 0, currentChipWin = 0, playerWalletAfter = 0, playerWallet = 0,
    totalChipWinByGame = 0, gemPrice = 0;
    protected SiXiangGame currentGame = SiXiangGame.Normal, nextGame = SiXiangGame.Normal;
    protected List<Sequence> listSequenceSymbolOneByOne = new();
    public bool IsSpinning { get; set; } = false;
    public int ScatterCount { get; set; } = 0;

    protected bool hasSetupStartView = false, isInFreeSpin = false, isChooseBonusGame = false, isGetMatchResult = false, isCurrentlyAutoSpin = false, isBetLevelChanged = false, isWinScatter = false;
    protected virtual float AUTO_SPIN_HOLD_DURATION => 1.3f;
    public override bool CanLeaveTable => gameState != SlotGameState.SPINNING && gameState != SlotGameState.SHOWING_RESULT && currentGame == SiXiangGame.Normal;


    [Header(" Object Pools ")]
    protected UnityEngine.Pool.ObjectPool<Image> coinPool;
    protected UnityEngine.Pool.ObjectPool<GameObject> linePool;

    protected virtual Dictionary<SiXiangSymbol, int> SymbolDictionary => new();
    protected virtual Dictionary<SiXiangGame, int> GemDictionary => new();



    protected override void Awake()
    {
        base.Awake();
        Init();
        InitColumns();
        UpdateSpinButtonUI();
        SoundManager.Instance.PlayMusicInGame(SOUND_BACKGROUND_ANIMATION_PATH);

    }

    protected override void Update()
    {
        if (gameState == SlotGameState.SPINNING || gameState == SlotGameState.SHOWING_RESULT || currentGame != SiXiangGame.Normal) return;
        HandleHoldingSpin();
    }

    #region API Handlers
    public override void HandleUpdateTable(IMatchState matchState)
    {
        base.HandleUpdateTable(matchState);
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
            if (IsSpinning && !isBetLevelChanged)
            {
                OnStartSpin();
            }
            else
            {
                Debug.Log("VAO DAY DE");
                isBetLevelChanged = false;
                UpdateJackpot(data);
                UpdateGem();

            }
        }

        if (!hasSetupStartView) hasSetupStartView = true;
    }

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

    private void OnBetLevelChanged()
    {
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

    protected void GetMatchResult()
    {
        Debug.Log("GET MATCH RESULT");
        isGetMatchResult = true;
        DataSender.SendMatchState((long)OpCodeRequest.InfoTable, new byte[0]);
    }

    #endregion

    #region Spin Actions
    protected virtual void OnStartSpin()
    {
        Debug.Log("START SPIN");
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.SPIN_REEL);
        AnimateHideGemButtons();
        if (!isInFreeSpin)
        {
            // Nếu đang ko Free Spin thì trừ tiền
            long updatedWallet = playerWallet - currentBetLevel;
            SetCurrentChipValue(updatedWallet);
            SetInfoSessionText($"Playing {listPaylineId.Count} lines. Good luck!");
            UpdateStateWinUI(StateWin.LAST_WIN);
            if (spinType == SpinType.AUTO)
            {
                if (autoSpinRemain >= 0)
                {
                    autoSpinRemain--;
                    SetAutoSpinRemain();
                }

            }
            else if (spinType == SpinType.FREE_AUTO)
            {
                freeSpinLeft--;
                textAutoRemain.text = freeSpinLeft.ToString();
                if (freeSpinLeft <= 0)
                {
                    spinType = SpinType.FREE_NORMAL;
                    UpdateSpinButtonUI();
                }
            }
        }
        else
        {
            // Nếu đang Free Spin thì hiện Free Spin left
            // ShowBackGroundFreeSpin();
            // UpdateTotalChipWinValue();
        }
        UpdateGameState(SlotGameState.SPINNING);
        IsSpinning = true;
        foreach (SlotSymbolColumn column in listColumn)
        {
            column.StartSpin(spinType);
        }
    }

    public virtual void OnStopSpin()
    {
        IsSpinning = false;
        isCurrentlyAutoSpin = spinType == SpinType.AUTO;

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
        NextTween();
    }

    public virtual void NextTween()
    {
        Debug.Log("NEXT TWEEN");
        if (tweenQueue.Count > 0)
        {
            TweenCallback nextTween = tweenQueue.Dequeue();
            DOTween.Sequence().AppendCallback(nextTween);
        }
        // Hết tween = hết show win line
        else
        {
            DOTween.Sequence()
                .AppendInterval(0.25f)
                .AppendCallback(() =>
                {
                    Reset();
                    // Nếu đang auto spin thì spin tiếp
                    if (spinType == SpinType.AUTO || spinType == SpinType.FREE_AUTO)
                    {
                        if (autoSpinRemain > 0 || isInFreeSpin)
                        {
                            HandleSpin();
                        }
                        // Nếu hết auto spin thì set spinType về NORMAL
                        else
                        {
                            spinType = SpinType.NORMAL;
                        }
                    }
                });
            // if (hasGotFreeSpin)
            // {
            //     if (spinType == SpinType.NORMAL || spinType == SpinType.AUTO)
            //     {
            //         spinType = SpinType.FREE_NORMAL;
            //     }
            //     ShowBackGroundFreeSpin();
            //     UpdateStateWinUI(StateWin.TOTAL_WIN);
            //     chipWinText.text = "0";
            //     freeSpinLeftText.text = "9";
            //     hasGotFreeSpin = false;

            // }

        }
    }
    #endregion
    protected bool CheckWild()
    {
        foreach (SlotSymbolColumn column in listColumn)
        {
            if (column.HasWild())
            {
                return true;
            }
        }
        return false;
    }

    public virtual void CheckThirdScatter(int columnIndex)
    {
        Debug.Log("CheckThirdScatter: " + columnIndex);
        if (columnIndex == 4 || isInFreeSpin) return;
        if (ScatterCount == 2)
        {
            SlotSymbolColumn column = listColumn[^1];
            column.ExtraTime += 3f;
            column.ShowThirdScatter();
            column.UpdateThirdScatterSpeed();
            column.IsShowingThirdScatter = true;
            SoundManager.Instance.PlayEffectFromPath(SoundSlot.SCATTER_SPIN);
        }
    }
    protected bool CheckWinThirdScatter()
    {
        return currentGame == SiXiangGame.Normal && nextGame == SiXiangGame.Bonus;
    }

    protected void HideThirdScatter()
    {
        listColumn[^1].HideThirdScatter();
    }

    #region Win Effects

    protected void ShowAnimationWild()
    {
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendCallback(() =>
            {
                foreach (SlotSymbolColumn column in listColumn)
                {
                    if (column.HasWild())
                    {
                        column.ShowAnimationWild();
                    }
                }
            })
            .AppendInterval(2f)
            .OnComplete(() =>
            {
                // if (spinType == SpinType.NORMAL || spinType == SpinType.AUTO)
                // {
                //     AnimateCoinsFly();
                // }
                if (tweenQueue.Count > 0)
                    NextTween();
            });
    }

    protected void ShowSpreadWild()
    {
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendCallback(() =>
            {
                foreach (SlotSymbolColumn column in listColumn)
                {
                    if (column.HasWild())
                    {
                        column.ShowSpreadWild();
                    }
                }
            })
            .AppendInterval(4f)
            .OnComplete(() =>
            {
                // if ((spinType == SpinType.FREE_AUTO || spinType == SpinType.AUTO) && currentChipWin > 0)
                // {
                //     AnimateCoinsFly();
                // }
                SetLightAllItems();
                NextTween();
            });

    }

    protected void ShowAllWinLines()
    {
        Debug.Log("ShOW ALL WIN LINES");
        paylineIconContainer.gameObject.SetActive(false);

        Debug.Log("IS AUTO SPIN: " + isCurrentlyAutoSpin);
        if (isCurrentlyAutoSpin)
        {
            UpdateChipWinValue();
        }
        else if (spinType == SpinType.FREE_AUTO || spinType == SpinType.FREE_NORMAL)
        {
            UpdateTotalChipWinValue();
        }
        SetDarkAllItems();
        // Draw Line và lưu vào listLine
        foreach (Payline payline in listPayline)
        {
            int[] lineWinID = GetPaylineWithID(payline.Id);
            ColorUtility.TryParseHtmlString(listColor[payline.Id % listColor.Length], out Color colorLine);
            List<Vector2> listPosition = new();
            for (int j = 0; j < lineWinID.Length; j++)
            {
                Vector2 positionItem = listColumn[j].GetItemPositionAtIndex(lineWinID[j]);
                Vector2 positionInContainer = lineContainer.transform.InverseTransformPoint(positionItem);
                listPosition.Add(positionInContainer);
            }
            DrawLines(listPosition, colorLine);
        }

        // Show Line từ listLine
        int totalLines = listLine.Count;
        Sequence sequence = DOTween.Sequence();

        // Hiện line lên, mỗi line cách nhau 0.1s
        for (int i = 0; i < totalLines; i++)
        {
            GameObject line = listLine[i];
            sequence
                .AppendCallback(() => line.SetActive(true))
                .AppendInterval(0.1f);
        }

        // Sau 1.5s thì ẩn hết line đi và show tween tiếp theo
        sequence
            .AppendInterval(1.5f)
            .OnComplete(() =>
            {
                foreach (GameObject line in listLine)
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

    protected void ShowSymbolOneByOne()
    {
        if (spinType == SpinType.NORMAL)
        {
            // if (currentChipWin > lastChipWin)
            // {
            UpdateChipWinValue();
            AnimateCoinsFly();
            // }
        }
        UpdateGameState(SlotGameState.SHOWING_RESULT);
        for (int i = 0; i < listPayline.Count; i++)
        {
            int index = i;
            Payline payline = listPayline[i];
            int[] lineWinID = GetPaylineWithID(payline.Id);

            Sequence sequence = DOTween.Sequence();
            listSequenceSymbolOneByOne.Add(sequence);
            sequence
                .AppendInterval(2f * index)
                .AppendCallback(() =>
                    {
                        if (sequence == null || !sequence.IsActive())
                        {
                            return;
                        }
                        else
                        {
                            SoundManager.Instance.PlayEffectFromPath(SoundSlot.LINE_WIN);
                            SetDarkAllItems();
                            for (int colIndex = 0; colIndex < payline.NumOccur; colIndex++)
                            {
                                int itemIndex = lineWinID[colIndex];
                                listColumn[colIndex].SetAnimationForItemAtIndex(itemIndex);
                            }
                            ShowPaylinesInfo(index);
                        }
                    }
                )
                .AppendInterval(1.5f)
                .OnComplete(() =>
                {
                    if (index == listLine.Count - 1)
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
        paylineIconContainer.gameObject.SetActive(true);
        foreach (Transform child in paylineIconContainer)
        {
            child.gameObject.SetActive(false);
        }
        textInfoSession.text = "";

        paylineInfoContainer.gameObject.SetActive(true);
        Payline payline = listPayline[index];
        int paylineIconId = 0;
        if (SymbolDictionary.TryGetValue(payline.Symbol, out int mappedValue))
        {
            paylineIconId = mappedValue;
        }
        for (int i = 0; i < payline.NumOccur; i++)
        {
            Image paylineIcon = paylineIconContainer.GetChild(i).GetComponent<Image>();
            paylineIcon.sprite = listSymbolSprite[paylineIconId];
            paylineIcon.gameObject.SetActive(true);
            paylineIcon.SetNativeSize();
        }
        textInfoSession.text = $"Win {payline.Chips} chips";
    }

    protected void ShowSpecialWinAnimation(WinType winType, long amount)
    {
        effectContainer.gameObject.SetActive(true);
        animationSpecialWin.transform.parent.gameObject.SetActive(true);
        animationSpecialWin.gameObject.SetActive(true);
        float duration;
        buttonConfirmSpecialWin.transform.localPosition = new Vector2(0, -299);
        textSpecialWin.transform.localPosition = new Vector2(0, -165);
        AudioSource soundBig = SoundManager.Instance.PlayEffectFromPath(SoundSlot.BIGWIN_START);

        textSpecialWin.ResetValue();
        switch (winType)
        {
            case WinType.NICE_WIN:
                Utility.PlayAnimation(animationSpecialWin, "nicewin", false);
                duration = 1f;
                break;
            case WinType.BIG_WIN:
                Utility.PlayAnimation(animationSpecialWin, "bigwin", false);
                duration = 2.5f;
                break;
            case WinType.MEGA_WIN:
                Utility.PlayAnimation(animationSpecialWin, "megawin", false);
                duration = 5f;
                break;
            case WinType.HUGE_WIN:
                Utility.PlayAnimation(animationSpecialWin, "hugewin", false);
                duration = 4f;
                break;
            default:
                return;
        }

        textSpecialWin.SetValue(amount, true, duration, "", () =>
        {
            buttonConfirmSpecialWin.gameObject.SetActive(true);
            if (soundBig != null && soundBig.isPlaying)
            {
                soundBig.Stop();
            }
            SoundManager.Instance.PlayEffectFromPath(SoundSlot.BIGWIN_END);
        });

        DOTween.Sequence()
            .SetId("autoHideSpecialWin")
            .AppendInterval(10.0f)
            .AppendCallback(() =>
            {
                if (animationSpecialWin.gameObject.activeInHierarchy)
                {
                    HideSpecialWinAnimation();
                }
            });
    }

    public void HideSpecialWinAnimation()
    {
        DOTween.Kill("autoHideResultMoney");
        buttonConfirmSpecialWin.gameObject.SetActive(false);
        animationSpecialWin.gameObject.SetActive(false);
        effectContainer.gameObject.SetActive(false);
        AnimateCoinsFly();
        NextTween();
        
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
        listLine.Add(line);
        LineController lineController = line.GetComponent<LineController>();
        lineController.DrawLine(positionList, colorLine);
    }


    protected int[] GetPaylineWithID(int id)
    {
        return listPaylineId[id - 1];
    }
    #endregion

    #region UI
    protected void UpdateSpinButtonUI()
    {
        // Mặc định màu trắng
        animationBtnSpin.color = Color.white;

        // Hàm set animation theo loại spin
        SetSpinAnimation(spinType);

        // Xử lý theo state
        switch (gameState)
        {
            case SlotGameState.SPINNING:

                // Nếu đang spin mà không phải auto, set màu xám
                if (spinType == SpinType.NORMAL || spinType == SpinType.FREE_NORMAL)
                {
                    animationBtnSpin.color = Color.gray;
                }
                break;

            case SlotGameState.SHOWING_RESULT:
                break;

            case SlotGameState.PREPARE:
            case SlotGameState.JOIN_GAME:

                // Nếu hết tiền và không phải free spin => disable
                // if (!IsSpinnable())
                // {
                //     animationBtnSpin.color = Color.gray;
                // }
                break;
        }

        animationBtnSpin.Initialize(true);
    }

    protected virtual void SetSpinAnimation(SpinType type)
    {
        animationBtnSpin.startingAnimation = type switch
        {
            SpinType.NORMAL => "spin",
            SpinType.FREE_NORMAL or SpinType.FREE_AUTO => "freespin",
            SpinType.AUTO => "stop",
            _ => "autospin"
        };
    }


    protected void UpdateGem()
    {
        Debug.Log("UPDATE GEM");
        if (currentGame != SiXiangGame.Normal) return;

        foreach (Image image in listGemImage)
        {
            image.color = Color.gray;
        }

        foreach (Button gemButton in listBuyGemButton)
        {
            gemButton.transform
                .DOLocalMoveX(82, 0.5f)
                .SetEase(Ease.InSine)
                .OnComplete(() => { gemButton.gameObject.SetActive(true); });
        }

        HashSet<int> hideIndexes = new();

        foreach (SiXiangGame game in listGem)
        {
            if (GemDictionary.TryGetValue(game, out int index))
            {
                hideIndexes.Add(index);
            }
        }

        for (int i = 0; i < listBuyGemButton.Length; i++)
        {
            Image image = listGemImage[i];
            Button button = listBuyGemButton[i];
            if (hideIndexes.Any(index => index == i))
            {
                image.color = Color.white;
                DOTween.Kill(button.transform);
                button.transform.DOLocalMoveX(-5, 0.3f).SetEase(Ease.OutSine).OnComplete(() => { button.gameObject.SetActive(false); });
            }
            else
                button.transform
                    .DOLocalMoveX(75, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(0.05f * i)
                    .OnComplete(() =>
                    {
                        button.gameObject.SetActive(true);
                        button.interactable = true;
                    });
        }
        // listGem.ForEach(btn =>
        // {
        //     DOTween.Kill(btn.transform);
        //     btn.transform.DOLocalMoveX(82, 0.5f).SetEase(Ease.InSine).OnComplete(() => { btn.gameObject.SetActive(true); });
        // });
    }

    public void HideAllGemButtons()
    {
        foreach (Button gemButton in listBuyGemButton)
        {
            gemButton.interactable = false;
            gemButton.gameObject.SetActive(false);
        }
    }

    protected void AnimateHideGemButtons()
    {
        foreach (Button gemButton in listBuyGemButton)
        {
            gemButton.interactable = false;
            gemButton.transform
                .DOLocalMoveX(-5, 0.3f)
                .SetEase(Ease.OutSine)
                .OnComplete(() => { gemButton.gameObject.SetActive(false); });
        }
    }
    protected void UpdateJackpot(SlotDesk data)
    {
        // if (currentGame != SiXiangGame.Normal) return;
        JackpotHistory jackpotHistory = data?.WinJpHistory;
        if (jackpotHistory == null) return;
        listDataJackpot = new long[]
        {
            jackpotHistory.Minor.Chips,
            jackpotHistory.Major.Chips,
            jackpotHistory.Mega.Chips,
            jackpotHistory.Grand.Chips
        };
        for (int i = 0; i < listTextJackpot.Length; i++)
        {
            TextNumberControl jackpot = listTextJackpot[i];
            jackpot.SetValue(listDataJackpot[i], true, 0.2f);
        }
    }

    protected void SetAutoSpinRemain()
    {
        textAutoRemain.gameObject.SetActive(autoSpinRemain > 0);
        if (autoSpinRemain > 200)
        {
            textAutoRemain.text = Uri.UnescapeDataString("\u221E");
            textAutoRemain.fontSize = 60;
        }
        else
        {
            textAutoRemain.fontSize = 40;
            textAutoRemain.text = autoSpinRemain.ToString();
        }
    }

    protected void SetInfoSessionText(string text)
    {
        textInfoSession.gameObject.SetActive(true);
        textInfoSession.text = text;
    }

    protected void SetBetStateImage(string text)
    {
        imageStateBet.gameObject.SetActive(true);
        imageStateBet.sprite = text == "Maximun bet" ? listBetStateSprite[1] : listBetStateSprite[0];
    }

    protected void SetCurrentBetImage(long betLevel)
    {
        if (imageStateBet == null) return;
        textCurrentBet.text = Utility.FormatNumber(betLevel);
        if (betLevel == listBetLevel[^1])
        {
            SetBetStateImage("Maximun bet");
        }
        else
        {
            SetBetStateImage("Bet");
        }
    }

    protected void UpdateStateWinUI(StateWin stateWin)
    {
        bool isUsingStateImage = imageStateWin.gameObject.activeSelf;
        switch (stateWin)
        {
            case StateWin.WIN when isUsingStateImage:
                imageStateWin.sprite = listStateWinSprite[0];
                break;
            case StateWin.WIN when !isUsingStateImage:
                textStateWin.text = "Win";
                break;
            case StateWin.TOTAL_WIN when isUsingStateImage:
                imageStateWin.sprite = listStateWinSprite[1];
                break;
            case StateWin.TOTAL_WIN when !isUsingStateImage:
                textStateWin.text = "Total Win";
                break;
            case StateWin.LAST_WIN when isUsingStateImage:
                imageStateWin.sprite = listStateWinSprite[2];
                break;
            case StateWin.LAST_WIN when !isUsingStateImage:
                textStateWin.text = "Last Win";
                break;
        }
    }

    protected void UpdateChipWinValue()
    {
        Debug.Log("UPDATE CHIP WIN VALUE: " + currentChipWin);
        UpdateStateWinUI(StateWin.WIN);
        textChipWin.SetValue(currentChipWin, true, 0.2f);
    }
    public void UpdateTotalChipWinValue()
    {
        UpdateStateWinUI(StateWin.TOTAL_WIN);
        textChipWin.SetValue(totalChipWinByGame, true, 0.2f);
    }

    public void SetCurrentChipValue(long value)
    {
        textUserChip.SetValue(value, true, 0.4f);
        playerWallet = value;
    }

    protected void UpdateGameState(SlotGameState gameState)
    {
        this.gameState = gameState;
        UpdateSpinButtonUI();
    }

    public void ShowBoxAutoSpin()
    {
        if (!autoSpinContainer.gameObject.activeSelf)
        {
            autoSpinContainer.gameObject.SetActive(true);
            autoSpinContainer.transform.DOLocalMoveY(150, 0.2f).SetEase(Ease.OutSine);
        }
    }
    public void HideBoxAutoSpin()
    {
        autoSpinContainer.transform.DOLocalMoveY(-153, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            autoSpinContainer.gameObject.SetActive(false);
        });
    }
    #endregion

    #region Effects
    protected void AnimateCoinsFly(int totalCoins = 7, float timeInterval = 0.02f)
    {
        Debug.Log("ANIMATE COINS FLY");
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.CHIP_REWARD);
        Sequence sequence = DOTween.Sequence();
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
                    AnimateCoinFly(coin, textChipWin.transform, textUserChip.transform);
                });
        }
        sequence.OnComplete(() =>
        {
            SetCurrentChipValue(playerWalletAfter);
            if (tweenQueue.Count > 0 && !isWinScatter)
                NextTween();
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
            .Append(coin.transform.DOScale(0.5f, 0.25f))//0.85
            .AppendInterval(0.6f)
            .Append(coin.DOFade(0, .25f)).AppendCallback(() =>
            {
                coinPool.Release(coin);
            });
    }
    #endregion

    #region Buttons
    // giữ nút spin
    public void OnTriggerDownSpinButton()
    {
        if (!IsButtonInteractable()) return;
        holdingSpinTime = 0f;
        isHoldingSpin = true;
    }

    public void OnTriggerUpSpinButton()
    {
        isHoldingSpin = false;
        if (!IsButtonInteractable() || !hasSetupStartView) return;
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.CLICK);
        if (holdingSpinTime < AUTO_SPIN_HOLD_DURATION)
        {
            HideBoxAutoSpin();
            if (spinType == SpinType.NORMAL || spinType == SpinType.FREE_NORMAL)
            {
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

                // Free Spin thì khi bắt đầu quay sẽ sang trạng thái AUTO luôn
                if (spinType == SpinType.FREE_NORMAL)
                {
                    spinType = SpinType.FREE_AUTO;
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
                        autoSpinRemain = 0;
                        textAutoRemain.gameObject.SetActive(false);
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
        if (isHoldingSpin && spinType != SpinType.AUTO && spinType != SpinType.FREE_AUTO)
        {
            holdingSpinTime += Time.deltaTime;
            if (holdingSpinTime > AUTO_SPIN_HOLD_DURATION)
            {
                ShowBoxAutoSpin();
                isHoldingSpin = false;
            }
        }
    }

    public virtual void OnChooseAutoSpin(int autoSpinCount)
    {
        if (gameState == SlotGameState.SPINNING || gameState == SlotGameState.SHOWING_RESULT)
        {
            return;
        }
        if (autoSpinCount <= 0)
        {
            return;
        }
        if (spinType == SpinType.NORMAL)
            spinType = SpinType.AUTO;
        else
            spinType = SpinType.FREE_AUTO;
        autoSpinRemain = autoSpinCount;
        HandleSpin();
        UpdateSpinButtonUI();
        SetAutoSpinRemain();
        HideBoxAutoSpin();
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.CLICK);
        // SetInfoSessionText($"Auto Spin {autoSpinRemain} times");
    }

    public virtual void OnClickPlusBetButton()
    {
        if (gameState == SlotGameState.SPINNING || gameState == SlotGameState.SHOWING_RESULT || currentGame != SiXiangGame.Normal)
        {
            return;
        }
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.CLICK);
        lastBetLevel = currentBetLevel;
        currentBetLevel = listBetLevel.Find(bet => bet > currentBetLevel);
        if (currentBetLevel == 0)
        {
            currentBetLevel = listBetLevel[0];
        }
        SetCurrentBetImage(currentBetLevel);
        OnBetLevelChanged();
    }

    public virtual void OnClickMinusBetButton()
    {
        if (gameState == SlotGameState.SPINNING || gameState == SlotGameState.SHOWING_RESULT || currentGame != SiXiangGame.Normal)
        {
            return;
        }
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.CLICK);
        lastBetLevel = currentBetLevel;
        currentBetLevel = listBetLevel.FindLast(bet => bet < currentBetLevel);
        if (currentBetLevel == 0)
        {
            currentBetLevel = listBetLevel[^1];
        }
        SetCurrentBetImage(currentBetLevel);
        OnBetLevelChanged();
    }

    public virtual void OnClickMaxBetButton()
    {
        if (gameState == SlotGameState.SPINNING || gameState == SlotGameState.SHOWING_RESULT || currentGame != SiXiangGame.Normal || isClickMaxBet)
        {
            return;
        }
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.CLICK);
        lastBetLevel = currentBetLevel;
        currentBetLevel = listBetLevel[^1];
        SetCurrentBetImage(currentBetLevel);
        if (lastBetLevel == currentBetLevel)
        {
            HandleSpin();
        }
        else
        {
            OnBetLevelChanged();
        }
    }

    public void OnClickBuyGem(int index)
    {
        SiXiangBuyGemsPopup popup = Instantiate(UIManager.Instance.LoadPrefabPopup("PopupBuySixiangGem"), transform).GetComponent<SiXiangBuyGemsPopup>();
        popup.SetInfo(index, gemPrice, playerChip);
    }

    public void OnClickShopButton()
    {
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.CLICK);
    }

    public void OnClickMenuButton()
    {
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.CLICK);
        UIManager.Instance.OpenGroupMenu();
    }

    public override void OpenRule()
    {
        Instantiate(rulePrefab, transform);
    }
    #endregion

    

    #region Helpers
    public void SetDarkAllItems(bool isBackgroundDark = true)
    {
        if (isBackgroundDark)
        {
            imageBackgroundSpin.color = Color.gray;
        }
        foreach (SlotSymbolColumn column in listColumn)
        {
            column.SetDarkAllSymbols();
        }
    }

    public void SetLightAllItems()
    {
        imageBackgroundSpin.color = Color.white;
        foreach (SlotSymbolColumn column in listColumn)
        {
            column.SetLightAllSymbols();
        }
    }

    protected bool CheckEnoughBalance()
    {
        if (playerWallet < currentBetLevel && (spinType == SpinType.NORMAL || spinType == SpinType.AUTO))
        {
            ResetToNormalState();
            HideBoxAutoSpin();
            string message = "Not enough chips to spin.";
            UIManager.Instance.ShowConfirmDialog(message, () => UIManager.Instance.OpenShop(), null, "Get More Chips");
            return false;
        }
        return true;
    }
    #endregion


    #region Setups
    protected void Reset()
    {
        Debug.Log("RESET");
        if (currentGame == SiXiangGame.Normal || nextGame == SiXiangGame.Normal)
        {
            paylineIconContainer.gameObject.SetActive(false);
            effectContainer.gameObject.SetActive(false);
            listLine.Clear();
            listPayline.Clear();
            listSpinSymbol.Clear();
            SetLightAllItems();
            if (autoSpinRemain == 0)
            {
                ResetToNormalState();
            }
            ScatterCount = 0;
        }
        if (spinType == SpinType.NORMAL || spinType == SpinType.FREE_NORMAL)
        {
            UpdateGameState(SlotGameState.PREPARE);
            UpdateGem();

        }
        foreach (GameObject line in listLine)
        {
            if (line.activeSelf)
                linePool.Release(line);
        }

        foreach (SlotSymbolColumn column in listColumn)
        {
            column.Reset();
        }
        foreach (Sequence sequence in listSequenceSymbolOneByOne)
        {
            if (sequence.IsActive())
            {
                sequence.Kill();
            }
        }
     
    }


    protected void ResetToNormalState(bool isEndBonusGame = false)
    {
        spinType = SpinType.NORMAL;
        UpdateGameState(SlotGameState.PREPARE);
        autoSpinRemain = 0;
        SetInfoSessionText("Press SPIN to play");
        SetAutoSpinRemain();
        UpdateSpinButtonUI();
        SetLightAllItems();

        if (isEndBonusGame)
        {
            foreach (SlotSymbolColumn column in listColumn)
            {
                column.UpdateStartViewUI();
            }
        }
    }

    protected virtual void UpdateColumnView(SlotDesk data)
    {
        List<SiXiangSymbol> listSymbols = data.Matrix.Lists.ToList();
        int totalCol = data.Matrix.Cols;
        if (totalCol < 5 || currentGame != SiXiangGame.Normal)
            return;
        for (int col = 0; col < totalCol; col++)
        {
            SlotSymbolColumn column = listColumn[col];
            List<int> columnArray = new();

            for (int row = 0; row < 3; row++)
            {
                int index = row * 5 + col;
                SiXiangSymbol symbol = listSymbols[index];
                if (SymbolDictionary.TryGetValue(symbol, out int mappedValue))
                {
                    columnArray.Add(mappedValue);
                }
            }

            if (hasSetupStartView)
            {
                column.SetFinishView(columnArray);
                listPayline = data.Paylines.ToList();

            }
            else
            {
                column.SetFinishView(columnArray);
                column.UpdateStartViewUI();
            }
        }
    }

    protected void SetupStartView(SlotDesk data)
    {
        listBetLevel = data.BetLevels.ToList();
        currentBetLevel = data.ChipsMcb;
        SetInfoSessionText("Press SPIN to play");
        SetCurrentBetImage(currentBetLevel);

        SetCurrentChipValue(data.GameReward.BalanceChipsWalletAfter);
        if (data.GameConfig != null)
        {
            freeSpinLeft = (int)data.GameConfig.NumFreeSpin;
            // isLastFreeSpin = data.GameConfig.NumFreeSpin <= 0;
            // lastTotalChipWinByGame = totalChipWinByGame;
        }
            totalChipWinByGame = data.GameReward.TotalChipsWinByGame;
        if (freeSpinLeft > 0)
        {
            // ShowBackGroundFreeSpin();
            spinType = SpinType.FREE_NORMAL;
            UpdateSpinButtonUI();
        }
    }

    protected virtual void UpdateReward(SlotDesk data)
    {
        if (currentGame != SiXiangGame.Normal)
        {
            totalChipWinByGame = data.GameReward.TotalChipsWinByGame;
        }
        // if (data.GameConfig != null)
        // {
        //     freeSpinLeft = (int)data.GameConfig.NumFreeSpin;
        //     if (data.GameConfig.NumFreeSpin <= 0)
        //     {
        //         // backgroundfre.gameObject.SetActive(false);
        //         // isLastFreeSpin = true;
        //     }
        //     // lastTotalChipWinByGame = totalChipWinByGame;
        // }
        // else
        // {
        //     // isLastFreeSpin = false;
        //     // lastTotalChipWinByGame = 0;
        // }
        freeSpinLeft = (int)data.NumSpinLeft;
        isInFreeSpin = freeSpinLeft > 0;
        if (isInFreeSpin)
        {
            // ShowBackGroundFreeSpin();
            UpdateSpinButtonUI();
        }
        if (data.GameReward.UpdateWallet)
        {
            // lastChipWin = currentChipWin;
            currentChipWin = data.GameReward.ChipsWin;
            totalChipWinByGame = data.GameReward.TotalChipsWinByGame;
            playerWalletAfter = data.GameReward.BalanceChipsWalletAfter;
        }
    }


    private void Init()
    {
        // if (numLineLeftText != null) numLineLeftText.text = PaylineIdList.Count.ToString();
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
            maxSize: 200             // tối đa object trong pool
        );
    }

    protected void InitColumns()
    {
        listColumn.Clear();
        foreach (Transform child in columnContainer)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < 5; i++)
        {
            SlotSymbolColumn column = Instantiate(columnPrefab, columnContainer).GetComponent<SlotSymbolColumn>();
            column.SetInfo(this, i);
            column.SetRandomView();
            listColumn.Add(column);
        }
    }

    #endregion
    public List<SlotSymbolColumn> ListColumn => listColumn;
    public List<SpinSymbol> ListSpinSymbol => listSpinSymbol;
    public Transform InfoSessionBar => paylineInfoContainer;
    public long[] GetListDataJackpot => listDataJackpot;
    public SpinType GetSpinType() => spinType;
    public SiXiangGame GetCurrentGame() => currentGame;
    public long GetCurrentBetLevel() => currentBetLevel;
    protected bool IsSpinnable() => listBetLevel.Count > 0 && User.userProfile.AccountChip > currentBetLevel;
    protected bool IsButtonInteractable() => new SiXiangGame[] { SiXiangGame.Normal, SiXiangGame.DragonPearl, SiXiangGame.SixangbonusDragonPearl }.Contains(currentGame) || new SiXiangGame[] { SiXiangGame.DragonPearl, SiXiangGame.SixangbonusDragonPearl }.Contains(nextGame);
}

