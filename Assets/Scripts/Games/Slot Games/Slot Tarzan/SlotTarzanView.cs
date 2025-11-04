using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto;
using DG.Tweening;
using Globals;
using Nakama;
using Spine.Unity;
using TMPro;
using Color = UnityEngine.Color;
using UnityEngine;
using UnityEngine.UI;

public class SlotTarzanView : BaseSlotView
{
    [SerializeField] private SlotTarzanMiniGameView miniGameView;
    [SerializeField]
    private SkeletonGraphic tarzanAnimation, characterAnimation, lightBarAnimation, popupResult, popupMinigame,
    popupResultMinigame, popupFreeSpin, popupResultFreeSpin, diamondAnimation, buttonPopupResult, buttonGetFreeSpin,
    buttonPopupMinigame, buttonPopupResultMinigame, buttonPopupResultFreeSpin;
    [SerializeField]
    private TextMeshProUGUI currentChipBonusText, chipRewardText, diamondNumberText, diamondAmountText,
    freeSpinTurnText, freeSpinMultiplierText, resultFreeSpinRewardText, resultFreeSpinTurnText, resultFreeSpinMultiplierText, resultMinigameRewardText;
    [SerializeField] private Image progressChipBonus;
    [SerializeField] private Button getFreeSpinOKButton, getFreeSpinResultOKButton;
    [SerializeField] private Transform diamondPot, diamondContainer, letterContainer;
    [SerializeField] private GameObject diamondPrefab, letterPrefab;
    [SerializeField] private List<Image> characterList;
    [SerializeField] private List<Sprite> characterActiveList, characterInactiveList;
    protected override Dictionary<SiXiangSymbol, int> SymbolDictionary => new()
    {
        { SiXiangSymbol.K, 0 },
        { SiXiangSymbol.Q, 1 },
        { SiXiangSymbol.J, 2 },
        { SiXiangSymbol.A, 3 },
        { SiXiangSymbol.Snack, 4 },
        { SiXiangSymbol.Jaguar, 5 },
        { SiXiangSymbol.Elephant, 6 },
        { SiXiangSymbol.Gorille, 7 },
        { SiXiangSymbol.Clayton, 8 },
        { SiXiangSymbol.JaneFather, 9 },
        { SiXiangSymbol.Jane, 10 },
        { SiXiangSymbol.Wild, 11 },
        { SiXiangSymbol.FreeSpin, 12 },
        { SiXiangSymbol.Tarzan, 13 },
        { SiXiangSymbol.Diamond, 14 },
        { SiXiangSymbol.LetterJ, 15 },
        { SiXiangSymbol.LetterU, 16 },
        { SiXiangSymbol.LetterN, 17 },
        { SiXiangSymbol.LetterG, 18 },
        { SiXiangSymbol.LetterL, 19 },
        { SiXiangSymbol.LetterE, 20 },
    };

    private readonly Dictionary<SiXiangSymbol, int> letterIndexMap = new()
    {
        { SiXiangSymbol.LetterJ, 0 },
        { SiXiangSymbol.LetterU, 1 },
        { SiXiangSymbol.LetterN, 2 },
        { SiXiangSymbol.LetterG, 3 },
        { SiXiangSymbol.LetterL, 4 },
        { SiXiangSymbol.LetterE, 5 },
    };
    protected override List<int[]> PaylineIdList => new()
    {
        new int[] {1,1,1,1,1},
        new int[] {0,0,0,0,0},
        new int[] {2,2,2,2,2},
        new int[] {0,1,2,1,0},
        new int[] {2,1,0,1,2},
        new int[] {1,0,0,0,1},
        new int[] {1,2,2,2,1},
        new int[] {0,0,1,2,2},
        new int[] {2,2,1,0,0},
        new int[] {1,2,1,0,1},
        new int[] {1,0,1,2,1},
        new int[] {0,1,1,1,0},
        new int[] {2,1,1,1,2},
        new int[] {0,1,0,1,0},
        new int[] {2,1,2,1,2},
        new int[] {1,1,0,1,1},
        new int[] {1,1,2,1,1},
        new int[] {0,0,2,0,0},
        new int[] {2,2,0,2,2},
        new int[] {0,2,2,2,0},
        new int[] {2,0,0,0,2},
        new int[] {1,2,0,2,1},
        new int[] {1,0,2,0,1},
        new int[] {0,2,0,2,0},
        new int[] {2,0,2,0,2},
        new int[] {2,0,1,2,0},
        new int[] {0,2,1,0,2},
        new int[] {0,2,1,2,0},
        new int[] {2,0,1,0,2},
        new int[] {2,1,0,0,1},
        new int[] {0,1,2,2,1},
        new int[] {0,0,2,2,2},
        new int[] {2,2,0,0,0},
        new int[] {1,0,2,1,2},
        new int[] {1,2,0,1,0},
        new int[] {0,1,0,1,2},
        new int[] {2,1,2,1,0},
        new int[] {1,2,2,0,0},
        new int[] {0,0,1,1,2},
        new int[] {2,2,1,1,0},
        new int[] {2,0,0,0,0},
        new int[] {0,2,2,2,2},
        new int[] {2,2,2,2,0},
        new int[] {0,0,0,0,2},
        new int[] {1,0,1,0,1},
        new int[] {1,2,1,2,1},
        new int[] {0,1,2,2,2},
        new int[] {2,1,0,0,0},
        new int[] {0,1,1,1,1},
        new int[] {2,1,1,1,1},
        new int[] {0,0,0,0,1},
        new int[] {0,0,0,1,1},
        new int[] {0,0,0,1,2},
        new int[] {0,0,1,0,0},
        new int[] {0,0,1,1,0},
        new int[] {0,0,1,1,1},
        new int[] {0,0,1,2,1},
        new int[] {0,1,0,0,0},
        new int[] {0,1,0,1,1},
        new int[] {2,0,1,0,1},
        new int[] {0,2,1,2,1},
        new int[] {0,1,0,2,0},
        new int[] {2,1,2,0,2},
        new int[] {0,1,1,1,2},
        new int[] {2,1,1,1,0},
        new int[] {0,1,1,2,2},
        new int[] {1,0,0,0,0},
        new int[] {1,0,0,1,1},
        new int[] {1,0,0,1,2},
        new int[] {0,1,1,0,1},
        new int[] {1,0,1,1,2},
        new int[] {1,0,1,2,2},
        new int[] {1,1,0,0,0},
        new int[] {1,1,0,0,1},
        new int[] {1,1,0,1,2},
        new int[] {0,0,2,1,0},
        new int[] {1,1,1,0,0},
        new int[] {1,1,2,2,2},
        new int[] {2,2,2,2,1},
        new int[] {1,1,1,1,2},
        new int[] {0,1,0,2,1},
        new int[] {1,2,2,2,2},
        new int[] {1,2,1,0,0},
        new int[] {2,2,1,0,1},
        new int[] {2,2,1,1,1},
        new int[] {2,1,2,1,1},
        new int[] {0,0,0,2,0},
        new int[] {2,2,2,0,2},
        new int[] {2,0,2,2,2},
        new int[] {0,2,0,0,0},
        new int[] {2,2,0,1,2},
        new int[] {1,0,1,1,1},
        new int[] {1,2,1,1,1},
        new int[] {1,1,2,1,0},
        new int[] {2,1,2,2,2},
        new int[] {0,2,1,1,1},
        new int[] {2,0,1,1,1},
        new int[] {1,1,1,2,0},
        new int[] {1,1,1,0,2},
        new int[] {1,1,1,1,0},
    };
    protected override string SOUND_BACKGROUND_ANIMATION_PATH => SoundSlot.BG_TARZAN;
    protected override string BACKGROUND_FREE_SPIN_ANIMATION_PATH => "SlotSpine/Tarzan/PopupFreespin/skeleton_SkeletonData";
    protected override string BIG_WIN_ANIMATION_PATH => "SlotSpine/Tarzan/BigWin/skeleton_SkeletonData";
    protected override string MEGA_WIN_ANIMATION_PATH => "SlotSpine/Tarzan/BigWin/skeleton_SkeletonData";
    protected override string HUGE_WIN_ANIMATION_PATH => "SlotSpine/Tarzan/BigWin/skeleton_SkeletonData";
    protected override string BIG_WIN_ANIMATION_NAME => "big";
    protected override string MEGA_WIN_ANIMATION_NAME => "mega";
    protected override string HUGE_WIN_ANIMATION_NAME => "huge";
    protected override Vector2 RECT_SIZE => new Vector2(135, 135);
    private const string TARZAN_ANIMATION_NAME_1 = "du_day1";
    private const string TARZAN_ANIMATION_NAME_2 = "du_day";
    private const string TARZAN_ANIMATION_PATH = "SlotSpine/Tarzan/Model/skeleton_SkeletonData";
    private const string CHARACTER_ANIMATION_PATH = "SlotSpine/Tarzan/JungleCharacter/%letter/skeleton_SkeletonData";
    private const string BUTTON_CONFIRM_ANIMATION_PATH = "SlotSpine/Tarzan/ButtonConfirm/skeleton_SkeletonData";
    protected override int ThirdScatterIndex => 4;
    public long CurrentBetLevel => currentBetLevel;
    private UnityEngine.Pool.ObjectPool<GameObject> diamondPool;
    private readonly List<int> diamondIndexList = new();
    private readonly List<SiXiangSymbol> spinSymbolList = new();
    private int lastDiamondCollect, diamondCollect;
    private long updatedDiamondCollectAmount, diamondCollectChipAmount, diamondPotAmount;
    private bool isWinTarzan, isInMiniGame, isStartMiniGame, isEndMiniGame, isWinDiamondPot;
    private float multiFreeGame;

    protected override void Awake()
    {
        base.Awake();
        diamondPool = new UnityEngine.Pool.ObjectPool<GameObject>(
            createFunc: () =>
            {
                var diamond = Instantiate(diamondPrefab, diamondContainer);
                diamond.SetActive(false);
                return diamond;
            },
            actionOnGet: (diamond) =>
            {
                diamond.SetActive(true);
                diamond.transform.localScale = Vector3.one;
            },
            actionOnRelease: (diamond) =>
            {
                diamond.SetActive(false);
            },
            actionOnDestroy: (diamond) =>
            {
                Destroy(diamond);
            },
            defaultCapacity: 1,
            maxSize: 30
        );
    }
    public override void HandleUpdateTable(IMatchState matchState)
    {
        SlotDesk data = SlotDesk.Parser.ParseFrom(matchState.State);
        Debug.Log("Slot : " + data.ToString());
        diamondIndexList.Clear();
        spinSymbolList.Clear();
        lastDiamondCollect = diamondCollect;
        diamondCollect = data.GameReward.PerlGreenForest;
        diamondCollectChipAmount = updatedDiamondCollectAmount;
        updatedDiamondCollectAmount = data.GameReward.PerlGreenForestChipsCollect;
        diamondPotAmount = data.GameReward.PerlGreenForestChips;
        multiFreeGame = data.GameReward.RatioBonus == 0 ? 1 : data.GameReward.RatioBonus;
        List<SiXiangSymbol> listSymbols = data.Matrix.Lists.ToList();
        List<SiXiangSymbol> spreadListSymbols = data.SpreadMatrix?.Lists.ToList() ?? new();

        int totalCol = data.Matrix.Cols;
        if (isGetMatchResult)
        {
            isGetMatchResult = false;
            // if (data.CurrentSixiangGame == SiXiangGame.TarzanFreespinx9)
            // {
            //     ShowBackGroundFreeSpin();
            // }
            return;
        }

        isInFreeSpin = data.CurrentSixiangGame == SiXiangGame.TarzanFreespinx9;
        hasGotFreeSpin = data.NextSixiangGame == SiXiangGame.TarzanFreespinx9 && data.CurrentSixiangGame != SiXiangGame.TarzanFreespinx9;
        isLastFreeSpin = data.CurrentSixiangGame == SiXiangGame.TarzanFreespinx9 && data.NextSixiangGame == SiXiangGame.Normal;
        isWinTarzan = data.Matrix.Lists.ToList().Contains(SiXiangSymbol.Tarzan);
        isStartMiniGame = data.NextSixiangGame == SiXiangGame.TarzanJungleTreasure && data.CurrentSixiangGame != SiXiangGame.TarzanJungleTreasure;
        isEndMiniGame = data.CurrentSixiangGame == SiXiangGame.TarzanJungleTreasure && (data.NextSixiangGame == SiXiangGame.Normal || data.NextSixiangGame == SiXiangGame.TarzanFreespinx9);
        isInMiniGame = data.CurrentSixiangGame == SiXiangGame.TarzanJungleTreasure;
        isWinDiamondPot = data.GameReward.PerlGreenForestChips > 0;
        freeSpinLeft = (int)data.NumSpinLeft;
        if (isInFreeSpin)
        {
            ShowBackGroundFreeSpin();
        }
        winType = data.BigWin switch
        {
            BigWin.Big => WinType.BIG_WIN,
            BigWin.Mega => WinType.MEGA_WIN,
            _ => WinType.NONE,
        };
        SetWinType(data.GameReward.ChipsWin);     


        // Setup column view
            if (totalCol >= 5 && !isInMiniGame)
            {
                for (int col = 0; col < totalCol; col++)
                {
                    SlotColumn column = slotColumnList[col];
                    int[] columnArray = new int[3];
                    int[] spreadColumnArray = new int[3];

                    for (int row = 0; row < 3; row++)
                    {
                        // Tính index theo layout ngang
                        int index = row * 5 + col;
                        SiXiangSymbol symbol = listSymbols[index];
                        if (symbol == SiXiangSymbol.Diamond)
                        {
                            diamondIndexList.Add(index);
                        }
                        if (SymbolDictionary.TryGetValue(symbol, out int mappedValue))
                        {
                            columnArray[row] = mappedValue;
                        }
                        else
                        {
                            columnArray[row] = -1;
                        }
                        if (spreadListSymbols.Count > 0)
                        {
                            SiXiangSymbol spreadSymbol = spreadListSymbols[index];
                            if (SymbolDictionary.TryGetValue(spreadSymbol, out int spreadValue))
                            {
                                spreadColumnArray[row] = spreadValue;
                            }
                            else
                            {
                                spreadColumnArray[row] = -1;
                            }
                        }
                    }

                    // Khi đã bấm Spin
                    if (hasSetupStartView)
                    {
                        column.SetFinishView(columnArray);
                        column.SetSpreadFinishView(spreadColumnArray);
                        paylineList = data.Paylines.ToList();

                    }
                    // Khi lần đầu vào game -> Setup Views
                    else
                    {
                        column.SetStartView(columnArray);
                    }
                }
            }

        // lastTotalChipWinByGame = totalChipWinByGame;
        if (data.GameConfig != null)
        {
            freeSpinLeft = (int)data.GameConfig.NumFreeSpin;
        }
        else
        {
            freeSpinLeft = (int)data.NumSpinLeft;
        }
        if (hasSetupStartView)
        {
            // Bấm Spin
            if (IsSpinning)
                OnStartSpin();
            if (data.CurrentSixiangGame == SiXiangGame.TarzanJungleTreasure)
            {
                totalChipWinByGame = data.GameReward.TotalChipsWinByGame;
                miniGameView.SetInfo(data);
            }
            ///------------------CHECK END MINIGAME--------------------//
      
            if (data.CurrentSixiangGame == SiXiangGame.TarzanFreespinx9)
            {
                if (data.GameReward.TotalChipsWinByGame > 0)
                {
                    lastTotalChipWinByGame = totalChipWinByGame;
                    totalChipWinByGame = data.GameReward.TotalChipsWinByGame;
                }
            }
        }
        else
        {
            // Lần đầu setup start view
            betLevelList = data.BetLevels.ToList();
            currentBetLevel = data.ChipsMcb;
            SetInfoSessionText("Press SPIN to play");
            SetCurrentBetText(currentBetLevel);
            UpdateDiamondPot();
            SetCurrentChipValue(data.GameReward.BalanceChipsWalletAfter);

            // Lần đầu setup chữ JUNGLE
            foreach (SiXiangSymbol symbol in data.LetterSymbols)
            {
                if (letterIndexMap.TryGetValue(symbol, out int index))
                {
                    characterList[index].sprite = characterActiveList[index];
                }
            }

            // Lần đầu setup Free spin
            if (data.CurrentSixiangGame == SiXiangGame.TarzanFreespinx9)
            {

                totalChipWinByGame = data.GameReward.TotalChipsWinByGame;
                ShowBackGroundFreeSpin();
                UpdateTotalChipWinValue();
                UpdateStateWinUI(StateWin.TOTAL_WIN);
                spinType = SpinType.FREE_NORMAL;
                UpdateSpinButtonUI();
            }

            // Lần đầu setup Mini game Jungle
            if (data.CurrentSixiangGame == SiXiangGame.TarzanJungleTreasure)
            {
                Debug.Log("VAO DAY K?");
                foreach (SlotColumn column in slotColumnList)
                {
                    column.SetRandomFinishView();
                }
                ShowMiniGame();
                miniGameView.SetInfo(data);
            }
        }

        if (isEndMiniGame)
        {
            GetMatchResult();
            tweenQueue.Enqueue(() => ShowPopupResultMiniGame());
            NextTween();
        }


        // JUNGLE LETTERS
            foreach (SpinSymbol spinSymbol in data.SpinSymbols)
            {
                spinSymbolList.Add(spinSymbol.Symbol);
            }

        // Update Reward
        lastChipWin = currentChipWin;
        currentChipWin = data.GameReward.ChipsWin;
        playerWalletAfter = data.GameReward.BalanceChipsWalletAfter;
        if (data.GameReward.UpdateWallet)
        {
        }

        hasSetupStartView = true;
    }

    public override void OnStopSpin()
    {
        IsSpinning = false;
        ///------------------CHECK SHOW CHARACTER--------------------//

        if (spinSymbolList.Count > 0)
        {
            Vector2 posEnd = Vector2.zero;
            foreach (SiXiangSymbol symbol in spinSymbolList)
            {
                if (letterIndexMap.TryGetValue(symbol, out int index))
                {
                    tweenQueue.Enqueue(() => ShowAnimationLetter(posEnd, index));
                }
            }
        }

        ///------------------CHECK SHOW DIAMOND--------------------//
        if (diamondIndexList.Count > 0)
        {
            tweenQueue.Enqueue(() => ShowAnimationDiamond());
        }

        ///------------------CHECK TARZAN WILD--------------------//
        if (isWinTarzan)
        {
            tweenQueue.Enqueue(() => ShowAnimationTarzan());
        }

        ///------------------CHECK SHOW WIN SCATTER--------------------//
        if (CheckWinScatter())
        {
            tweenQueue.Enqueue(() => ShowWinScatter());
        }

        if (isWinDiamondPot)
        {
            tweenQueue.Enqueue(() => ShowPopupDiamond());
        }



        // ///------------------CHECK SHOW FREESPIN--------------------//
        if (hasGotFreeSpin)
        {
            Debug.Log("GET FREE SPIN");
            tweenQueue.Enqueue(() =>
            {
                ShowPopupGetFreeSpin();
            });
        }

        ///------------------CHECK SHOW FIVE OF A KIND--------------------///
        if (CheckFiveOfAKind())
        {
            Debug.Log("FIVE OF A KIND");
            tweenQueue.Enqueue(() => ShowWinAnimation(WinType.FIVE_OF_A_KIND));
        }



        ///------------------CHECK SHOW ALL LINE--------------------///
        if (paylineList.Count > 0)
        {
            tweenQueue.Enqueue(() => ShowAllWinLines());
        }


        if (isLastFreeSpin)
        {
            tweenQueue.Enqueue(() => ShowPopupResultFreeSpin());
            freeSpinLeftText.gameObject.SetActive(false);
        }

        ///------------------CHECK SHOW TYPE WIN--------------------///
        if (!isInFreeSpin || isLastFreeSpin)
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

        ///------------------CHECK WIN MINIGAME--------------------//
        if (isStartMiniGame)
        {
            isStartMiniGame = false;
            Debug.Log("START MINIgAME");
            spinType = SpinType.NORMAL;
            UpdateGameState(SlotGameState.PREPARE);
            tweenQueue.Enqueue(() => ShowPopupMinigame());
        }

        NextTween();
    }

    protected override void ShowWinAnimation(WinType winType)
    {
        effectContainer.gameObject.SetActive(true);
        animationEffect.gameObject.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        switch (winType)
        {
            case WinType.BIG_WIN:
                sequence.AppendInterval(1.2f)
                    .AppendCallback(() =>
                    {
                        bigWinText.transform.parent.gameObject.SetActive(true);
                        bigWinText.gameObject.SetActive(true);
                        Utility.TweenNumberToNumber(bigWinText, currentChipWin, 0, 2.7f);
                    })
                    .AppendInterval(3.3f)
                    .OnComplete(() =>
                    {
                        bigWinText.transform.parent.gameObject.SetActive(false);
                        bigWinText.transform.gameObject.SetActive(false);
                    });
                SoundManager.Instance.PlayEffectFromPath(SoundSlot.BIG_WIN);
                // animationEffect.transform.localScale = new Vector2(0.9f, 0.9f);
                animationEffect.transform.localScale = new Vector2(1f, 1f);
                // animationEffect.transform.localPosition = new Vector2(0, -70);
                Utility.PlayAnimationByPath(animationEffect, BIG_WIN_ANIMATION_PATH, BIG_WIN_ANIMATION_NAME, false);
                break;
            case WinType.MEGA_WIN:
                sequence.AppendInterval(0.7f)
                    .AppendCallback(() =>
                    {
                    bigWinText.transform.parent.gameObject.SetActive(true);
                        Utility.TweenNumberToNumber(bigWinText, currentChipWin, 0, 5f);
                    })
                    .AppendInterval(5f)
                    .OnComplete(() =>
                    {
                        bigWinText.transform.parent.gameObject.SetActive(false);
                        bigWinText.gameObject.SetActive(false);
                    });
                SoundManager.Instance.PlayEffectFromPath(SoundSlot.MEGA_WIN);
                bigWinText.gameObject.SetActive(true);
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
        }


        animationEffect.AnimationState.Complete += delegate
        {
            effectContainer.gameObject.SetActive(false);
            animationEffect.gameObject.SetActive(false);
            if (new WinType[] { WinType.BIG_WIN, WinType.HUGE_WIN, WinType.MEGA_WIN }.Contains(winType))
            {
                AnimateCoinsFly();
            }
            NextTween();
            effectContainer.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
        }; 
    }

    #region Popups
    private void ShowPopupGetFreeSpin()
    {
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.FREESPIN);
        effectContainer.gameObject.SetActive(true);
        popupFreeSpin.gameObject.SetActive(true);
        Utility.PlayAnimation(popupFreeSpin, "yahoo", true);
        freeSpinTurnText.text = "9";

        popupFreeSpin.transform.localScale = new Vector2(0.25f, 0.25f);
        popupFreeSpin.transform.DOScale(new Vector2(1, 1f), 0.3f).SetEase(Ease.OutBack);
        getFreeSpinOKButton.gameObject.SetActive(false);
        Utility.PlayAnimationByPath(buttonGetFreeSpin, BUTTON_CONFIRM_ANIMATION_PATH, "start", true);
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                int number = Random.Range(1, 9);
                freeSpinMultiplierText.text = "x" + number;
            })
            .AppendInterval(0.05f)
            .SetLoops(30)
            .OnComplete(() =>
            {
                freeSpinMultiplierText.text = "x" + multiFreeGame;
                getFreeSpinOKButton.gameObject.SetActive(true);
            });
        freeSpinMultiplierText.transform.DOScale(new Vector2(0.5f, 0.5f), 1.5f).SetEase(Ease.OutBack);
        if (spinType == SpinType.AUTO || spinType == SpinType.FREE_AUTO)
        {
            DOTween.Sequence()
                .AppendInterval(10.0f)
                .AppendCallback(() =>
                {
                    if (popupFreeSpin.gameObject.activeSelf)
                    {
                        HidePopupGetFreeSpin();
                    }
                });
        }
    }

    private void HidePopupGetFreeSpin()
    {
        popupFreeSpin.transform.DOScale(new Vector2(0.25f, 0.25f), 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            popupFreeSpin.gameObject.SetActive(false);
            effectContainer.gameObject.SetActive(false);
            GetMatchResult();
            NextTween();
        });
    }

    private void ShowPopupResultFreeSpin()
    {
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.FREESPIN);
        effectContainer.gameObject.SetActive(true);
        popupResultFreeSpin.gameObject.SetActive(true);
        Utility.PlayAnimation(popupResultFreeSpin, "wonderful", true);
        resultFreeSpinTurnText.text = "9";
        resultFreeSpinMultiplierText.text = "x" + multiFreeGame;
        Utility.TweenNumberToMoney(resultFreeSpinRewardText, totalChipWinByGame, 0, 1.0f, 10000);
        Utility.PlayAnimationByPath(buttonPopupResultFreeSpin, BUTTON_CONFIRM_ANIMATION_PATH, "backtogame", true);

        popupResultFreeSpin.transform.localScale = new Vector2(0.25f, 0.25f);
        popupResultFreeSpin.transform.DOScale(new Vector2(1, 1f), 0.3f).SetEase(Ease.OutBack);

        freeSpinMultiplierText.transform.DOScale(new Vector2(0.5f, 0.5f), 1.5f).SetEase(Ease.OutBack);
        if (spinType == SpinType.AUTO || spinType == SpinType.FREE_AUTO)
        {
            DOTween.Sequence()
                .AppendInterval(10.0f)
                .AppendCallback(() =>
                {
                    // totalChipWinByGame = 0;
                    // lastTotalChipWinByGame = 0;
                    if (popupResultFreeSpin.gameObject.activeSelf)
                    {
                        HidePopupResultFreeSpin();
                    }
                });
        }
    }

    public void HidePopupResultFreeSpin()
    {
        popupResultFreeSpin.transform.DOScale(new Vector2(0.25f, 0.25f), 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            popupResultFreeSpin.gameObject.SetActive(false);
            effectContainer.gameObject.SetActive(false);
            spinType = SpinType.NORMAL;
            UpdateGameState(SlotGameState.PREPARE);
            UpdateTotalChipWinValue();
            isInFreeSpin = false;
            if (totalChipWinByGame > PaylineIdList.Count * currentBetLevel) winType = WinType.BIG_WIN;
            else if (totalChipWinByGame > 50 * currentBetLevel) winType = WinType.MEGA_WIN;
            else if (totalChipWinByGame > 0)
            {
                AnimateCoinsFly();
            }
            NextTween();
        });
    }

    private void ShowPopupMinigame()
    {
        effectContainer.gameObject.SetActive(true);
        popupMinigame.gameObject.SetActive(true);
        Utility.PlayAnimation(popupMinigame, "Eng", true);
        Utility.PlayAnimationByPath(buttonPopupMinigame, BUTTON_CONFIRM_ANIMATION_PATH, "start", true);

        popupMinigame.transform.localScale = new Vector2(0.5f, 0.5f);
        popupMinigame.transform.localPosition = Vector2.zero;
        popupMinigame.transform.DOScale(new Vector2(.4f, .4f), 0.3f).SetEase(Ease.OutBack);
    }

    private void ShowMiniGame()
    {
        miniGameView.Show();
    }

    public void HidePopupMiniGame()
    {
        effectContainer.gameObject.SetActive(false);
        popupMinigame.gameObject.SetActive(false);
        ShowMiniGame();
        NextTween();
    }

    private void ShowPopupResultMiniGame()
    {
        DOVirtual.DelayedCall(1.2f, () =>
        {
            popupResultMinigame.gameObject.SetActive(true);
            effectContainer.gameObject.SetActive(true);
            Utility.PlayAnimation(popupResultMinigame, "Eng", true);
            Utility.PlayAnimationByPath(buttonPopupResultMinigame, BUTTON_CONFIRM_ANIMATION_PATH, "backtogame", true);

            popupResultMinigame.transform.localScale = new Vector2(.8f, .8f);
            popupResultMinigame.transform.DOScale(new Vector2(1.0f, 1.0f), 0.3f).SetEase(Ease.OutBack);

            resultMinigameRewardText.text = Utility.FormatMoney3(totalChipWinByGame, 10000);
        });
    }

    public void HidePopupResultMiniGame()
    {
        popupResultMinigame.transform.DOScale(new Vector2(0.25f, 0.25f), 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            DisableAllCharacters();
            popupResultMinigame.gameObject.SetActive(false);
            effectContainer.gameObject.SetActive(false);
            miniGameView.Hide(false);
            spinType = SpinType.NORMAL;
            UpdateSpinButtonUI();
            UpdateGameState(SlotGameState.PREPARE);
            UpdateTotalChipWinValue();
            if (totalChipWinByGame > PaylineIdList.Count * currentBetLevel) winType = WinType.BIG_WIN;
            else if (totalChipWinByGame > 50 * currentBetLevel) winType = WinType.MEGA_WIN;
            else if (totalChipWinByGame > 0)
            {
                AnimateCoinsFly();
            }
            NextTween();
        });
    }

    private void ShowPopupDiamond()
    {
        popupResult.gameObject.SetActive(true);
        effectContainer.gameObject.SetActive(true);
        Utility.PlayAnimation(popupResult, "getgem", false);
        Utility.PlayAnimationByPath(buttonPopupResult, BUTTON_CONFIRM_ANIMATION_PATH, "backtogame", true);

        popupResult.transform.localScale = new Vector2(.8f, .8f);
        popupResult.transform.DOScale(new Vector2(1.0f, 1.0f), 0.3f).SetEase(Ease.OutBack);

        chipRewardText.text = Utility.FormatMoney3(diamondPotAmount, 10000);
    }

    public void HidePopupDiamond()
    {
        popupResult.transform.DOScale(new Vector2(0.25f, 0.25f), 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            popupResult.gameObject.SetActive(false);
            effectContainer.gameObject.SetActive(false);
            AnimateCoinsFly();
            NextTween();
        });
    }
    #endregion

    #region UI
    private void ShowAnimationLetter(Vector2 position, int index, TweenCallback cb = null)
    {
        List<string> listIndex = new List<string> { "J", "U", "N", "G", "L", "E" };
        string path = CHARACTER_ANIMATION_PATH.Replace("%letter", listIndex[index]);
        GameObject letter = Instantiate(letterPrefab, letterContainer);
        SkeletonGraphic letterAnimation = letter.GetComponent<SkeletonGraphic>();

        Utility.PlayAnimationByPath(letterAnimation, path, "animation", false);
        letterAnimation.transform.localScale = new Vector2(0.5f, 0.5f);
        letterAnimation.transform.localPosition = position;
        Vector2 posEnd = transform.InverseTransformPoint(characterList[index].transform.position);
        letterAnimation.transform.SetParent(transform, false);
        letterAnimation.transform
            .DOLocalMove(posEnd, 1.0f)
            .SetEase(Ease.OutCubic)
            .SetDelay(1.0f)
            .OnComplete(() =>
            {
                characterList[index].sprite = characterActiveList[index];
                if (cb != null)
                {
                    DOTween.Sequence().AppendCallback(cb);
                }
                letterAnimation.gameObject.SetActive(false);
                NextTween();
            });
        letterAnimation.transform.DOScale(new Vector2(0.15f, 0.15f), 1.0f).SetDelay(1.0f).SetEase(Ease.OutCubic);
    }

    private void DisableAllCharacters()
    {
        for (int i = 0; i < characterList.Count; i++)
        {
            characterList[i].sprite = characterInactiveList[i];
        }
    }
    private void ShowAnimationDiamond()
    {
        for (int i = 0; i < diamondIndexList.Count; i++)
        {
            int diamondIndex = diamondIndexList[i];
            int col = diamondIndex % 5;   // column index
            int row = diamondIndex / 5;
            Vector2 positionItem = slotColumnList[col].GetItemPositionAtIndex(row);
            Vector2 positionInContainer = diamondContainer.transform.InverseTransformPoint(positionItem);
            bool isLast = i == diamondIndexList.Count - 1;
            AnimateDiamomd(positionInContainer, isLast);
        }
    }

    private void AnimateDiamomd(Vector2 position, bool isLast)
    {
        GameObject diamond = diamondPool.Get();
        SkeletonGraphic diamondAnimation = diamond.GetComponentInChildren<SkeletonGraphic>();

        diamondAnimation.transform.localScale = new Vector2(1f, 1f);
        Utility.PlayAnimation(diamondAnimation, "animation", false);
        diamond.transform.localPosition = position;
        diamondAnimation.transform.localPosition = new Vector2(453, -971);

        Vector2 posDiamondOnPos = transform.InverseTransformPoint(diamondPot.transform.position);
        diamond.transform.SetParent(transform, false);
        diamond.transform
            .DOLocalMove(posDiamondOnPos, 1.0f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                // if (cb != null)
                // {
                //     float progressChipBonus = (float)getInt(finishData, "numExp") / getInt(finishData, "maxExp");
                //     if (progressChipBonus != 0)
                //     {
                //         //animBuiSang.gameObject.SetActive(true);
                //         //animBuiSang.Initialize(true);
                //         //animBuiSang.AnimationState.SetAnimation(0, "run", false);
                //         //animBuiSang.AnimationState.Complete += delegate
                //         //{
                //         //    animBuiSang.AnimationState.SetAnimation(0, "normal", false);
                //         //};
                //     }
                //     //sprProgressChipBonus.DOFillAmount(progressChipBonus, 0.5f);
                //     sprProgressChipBonus.DOFillAmount(progressChipBonus, 0.5f).SetEase(Ease.InSine).OnUpdate(() =>
                //     {
                //         float posLight = -131 + (242 * sprProgressChipBonus.fillAmount);
                //         animLightBar.transform.localPosition = new Vector2(posLight, 2);
                //         //animLightBar.gameObject.SetActive(posLight > -103);
                //     });
                //     lbDiamondNum.text = getInt(finishData, "numExp").ToString();
                //     Config.tweenNumberToNumber(lbCurrentChipBonus, getInt(finishData, "currentChipBonus"), currentChipBonus, 0.3f);
                //     currentChipBonus = getInt(finishData, "currentChipBonus");
                // }
                diamondPool.Release(diamond);
                if (isLast)
                {
                    UpdateDiamondPot();
                    NextTween();
                }
            });
        diamond.transform.DOScale(new Vector2(0.3f, 0.3f), 1.0f).SetEase(Ease.OutCubic);
    }
    private void ShowAnimationTarzan()
    {
        Debug.Log("SHOW ANIM TARZAN");
        SetDarkAllItems();
        // tarzanAnimation.Skeleton.SetToSetupPose(); // Reset pose
        // tarzanAnimation.AnimationState.ClearTracks(); // Clear animation cũ
        // tarzanAnimation.AnimationState.SetEmptyAnimation(0, 0); 
        tarzanAnimation.gameObject.SetActive(true);
        tarzanAnimation.transform.localPosition = new Vector2(0, 26f);
        Utility.PlayAnimationByPath(tarzanAnimation, TARZAN_ANIMATION_PATH, TARZAN_ANIMATION_NAME_2, false);
        tarzanAnimation.Initialize(true);
        tarzanAnimation.AnimationState.Complete += delegate
        {
            Utility.PlayAnimationByPath(tarzanAnimation, TARZAN_ANIMATION_PATH, TARZAN_ANIMATION_NAME_1, false);
            //tarzanAnimation.transform.localPosition = new Vector2(0, 259);
        };
        DOTween.Sequence()
            .AppendInterval(2.0f)
            .AppendCallback(() =>
            {
                for (int i = 0; i < slotColumnList.Count; i++)
                {
                    SlotTarzanItem itemResult = (SlotTarzanItem)slotColumnList[i].ResultItem;
                    itemResult.TransformToWild();
                }
            })
            .AppendInterval(2.0f)
            .OnComplete(() =>
            {
                tarzanAnimation.gameObject.SetActive(false);
                SetLightAllItems();
                NextTween();
            });
    }
    private void UpdateDiamondPot()
    {
        diamondNumberText.text = diamondCollect.ToString();
        Utility.TweenNumberToNumber(diamondAmountText, updatedDiamondCollectAmount, diamondCollectChipAmount, 0.3f);
        float diamondProgress = diamondCollect / 100f;
        progressChipBonus.DOFillAmount(diamondProgress, 0.5f).SetEase(Ease.InSine).OnUpdate(() =>
        {
            float posLight = -131 + (242 * progressChipBonus.fillAmount);
            lightBarAnimation.transform.localPosition = new Vector2(posLight, 2);
        });
        diamondProgress = updatedDiamondCollectAmount;
    }

    protected override void ShowBackGroundFreeSpin()
    {
        freeSpinLeftText.gameObject.SetActive(true);
        freeSpinLeftText.text = freeSpinLeft == -1 ? "9" : freeSpinLeft.ToString();
    }

    protected override void DrawRectangularAndConnectingLines(int[] lineWinID, int startIndex, int matchedItemCount, UnityEngine.Color colorLine)
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
            if (isWinTarzan)
            {
                slotColumnList[colIndex].SetAnimationWildForItemAtIndex(itemIndex);
            }
            else
            {
                slotColumnList[colIndex].SetAnimationForItemAtIndex(itemIndex);
            }
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
    #endregion

    protected override void SetSpinAnimation(SpinType type)
    {
        buttonSpinAnimation.startingAnimation = type switch
        {
            SpinType.NORMAL => "Spin",
            SpinType.FREE_NORMAL or SpinType.FREE_AUTO => "Freespin",
            SpinType.AUTO => "stop",
            _ => "Spin"
        };
    }
    protected override void UpdateStateWinUI(StateWin stateWin)
    {
        bool isUsingStateImage = stateWinImage.gameObject.activeSelf;
        stateWinImage.transform.localScale = Vector3.one;
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
                stateWinImage.transform.localScale = Vector3.one * 1.4f;
                break;
            case StateWin.TOTAL_WIN when !isUsingStateImage:
                stateWinText.text = "Total Win";
                break;
            case StateWin.LAST_WIN when isUsingStateImage:
                stateWinImage.transform.localScale = Vector3.one * 1.4f;
                stateWinImage.sprite = stateWinSpriteList[2];
                break;
            case StateWin.LAST_WIN when !isUsingStateImage:
                stateWinText.text = "Last Win";
                break;
        }
    }

    protected override void Reset()
    {
        base.Reset();

    }

    public void Speed()
    {
        if (Time.timeScale == 1)
        {
            Time.timeScale = 5;
        }
        else
        {
            Time.timeScale = 1;
        }
    }
}
