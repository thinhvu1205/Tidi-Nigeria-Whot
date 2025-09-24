using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto;
using DG.Tweening;
using Globals;
using Nakama;
using Spine.Unity;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SlotTarzanView : BaseSlotView
{
    [SerializeField] SlotTarzanMiniGameView miniGameView;
    [SerializeField]
    SkeletonGraphic tarzanAnimation, characterAnimation, lightBarAnimation, popupResult, popupMinigame,
    popupResultMinigame, popupFreeSpin, popupResultFreeSpin, diamondAnimation;
    [SerializeField]
    TextMeshProUGUI currentChipBonusText, chipRewardText, diamondNumberText, diamondAmountText,
    freeSpinTurnText, freeSpinMultiplierText, resultFreeSpinRewardText, resultFreeSpinTurnText, resultFreeSpinMultiplierText, resultMinigameRewardText;
    [SerializeField] Image progressChipBonus;
    [SerializeField] Button getFreeSpinOKButton, getFreeSpinResultOKButton;
    [SerializeField] Transform diamondPot, diamondContainer, letterContainer;
    [SerializeField] GameObject diamondPrefab, letterPrefab;
    [SerializeField] List<Image> characterList;
    [SerializeField] List<Sprite> characterActiveList;
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
    protected override string BIG_WIN_ANIMATION_NAME => "big";
    protected override string MEGA_WIN_ANIMATION_NAME => "mega";
    protected override string HUGE_WIN_ANIMATION_NAME => "huge";
    protected override Vector2 RECT_SIZE => new Vector2(135, 135);
    private const string TARZAN_ANIMATION_NAME_1 = "du_day1";
    private const string TARZAN_ANIMATION_NAME_2 = "du_day";
    private const string TARZAN_ANIMATION_PATH = "SlotSpine/Tarzan/Model/skeleton_SkeletonData";
    private const string CHARACTER_ANIMATION_PATH = "SlotSpine/Tarzan/JungleCharacter/%letter/skeleton_SkeletonData";
    protected override int ThirdScatterIndex => 4;
    public long CurrentBetLevel => currentBetLevel;
    private UnityEngine.Pool.ObjectPool<GameObject> diamondPool;
    private readonly List<int> diamondIndexList = new();
    private readonly List<SiXiangSymbol> spinSymbolList = new();
    private int diamondCollect;
    private long updatedDiamondCollectAmount, diamondCollectChipAmount;
    private bool isWinTarzan, isInMiniGame, isStartMiniGame, isEndMiniGame;

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
        diamondCollect = data.GameReward.PerlGreenForest;
        diamondCollectChipAmount = updatedDiamondCollectAmount;
        updatedDiamondCollectAmount = data.GameReward.PerlGreenForestChipsCollect;

        List<SiXiangSymbol> listSymbols = data.Matrix.Lists.ToList();
        List<SiXiangSymbol> spreadListSymbols = data.SpreadMatrix?.Lists.ToList() ?? new();

        int totalCol = data.Matrix.Cols;

        isInFreeSpin = data.CurrentSixiangGame == SiXiangGame.TarzanFreespinx9;
        hasGotFreeSpin = data.NextSixiangGame == SiXiangGame.TarzanFreespinx9 && data.CurrentSixiangGame != SiXiangGame.TarzanFreespinx9;
        isLastFreeSpin = data.CurrentSixiangGame == SiXiangGame.TarzanFreespinx9 && data.NextSixiangGame == SiXiangGame.Normal;
        isWinTarzan = data.Matrix.Lists.ToList().Contains(SiXiangSymbol.Tarzan);
        isStartMiniGame = data.NextSixiangGame == SiXiangGame.TarzanJungleTreasure && data.CurrentSixiangGame != SiXiangGame.TarzanJungleTreasure;
        isEndMiniGame = data.CurrentSixiangGame == SiXiangGame.TarzanJungleTreasure && data.NextSixiangGame == SiXiangGame.Normal;
        isInMiniGame = data.CurrentSixiangGame == SiXiangGame.TarzanJungleTreasure;
        Debug.Log("IS END MINI GAME: " + isEndMiniGame);
        winType = data.BigWin switch
        {
            BigWin.Big => WinType.BIG_WIN,
            BigWin.Mega => WinType.MEGA_WIN,
            _ => WinType.NONE,
        };

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

        lastTotalChipWinByGame = totalChipWinByGame;
        totalChipWinByGame = data.GameReward.TotalChipsWinByGame;
        if (data.GameConfig != null)
        {
            freeSpinLeft = (int)data.GameConfig.NumFreeSpin;
        }
        else
        {
            freeSpinLeft = (int)data.NumSpinLeft;
            lastTotalChipWinByGame = 0;
        }
        if (hasSetupStartView)
        {
            // Bấm Spin
            if (IsSpinning)
                OnStartSpin();
            if (data.CurrentSixiangGame == SiXiangGame.TarzanJungleTreasure)
            {
                miniGameView.SetInfo(data);
            }
        }
        else
        {
            betLevelList = data.BetLevels.ToList();
            currentBetLevel = data.ChipsMcb;
            SetInfoSessionText("Press SPIN to play");
            SetCurrentBetText(currentBetLevel);
            UpdateDiamondPot();
            SetCurrentChipValue(data.GameReward.BalanceChipsWalletAfter);
            foreach (SiXiangSymbol symbol in data.LetterSymbols)
            {
                if (letterIndexMap.TryGetValue(symbol, out int index))
                {
                    characterList[index].sprite = characterActiveList[index];
                }
            }
            if (data.CurrentSixiangGame == SiXiangGame.TarzanFreespinx9)
            {
                ShowBackGroundFreeSpin();
                UpdateTotalChipWinValue();
                UpdateStateWinUI(StateWin.TOTAL_WIN);
                spinType = SpinType.FREE_NORMAL;
                UpdateSpinButtonUI();
            }
            else if (data.CurrentSixiangGame == SiXiangGame.TarzanJungleTreasure)
            {
                ShowMiniGame();
                miniGameView.SetInfo(data);
            }
        }

        // JUNGLE LETTERS
        foreach (SpinSymbol spinSymbol in data.SpinSymbols)
        {
            spinSymbolList.Add(spinSymbol.Symbol);
        }

        // Update Reward
        lastChipWin = currentChipWin;
        currentChipWin = data.GameReward.ChipsWin;
        // totalChipWinByGame = data.GameReward.TotalChipsWinByGame;
        totalChipWinByGame = data.GameReward.TotalChipsWinByGame;
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

        ///------------------CHECK SHOW FREESPIN--------------------//
        if (hasGotFreeSpin)
        {
            tweenQueue.Enqueue(() => ShowPopupGetFreeSpin());
        }

        ///------------------CHECK SHOW FIVE OF A KIND--------------------///
        if (CheckFiveOfAKind())
        {
            Debug.Log("FIVE OF A KIND");
            tweenQueue.Enqueue(() => ShowWinAnimation(WinType.FIVE_OF_A_KIND));
        }

        ///------------------CHECK WIN MINIGAME--------------------//
        if (isStartMiniGame)
        {
            Debug.Log("START MINIgAME");
            tweenQueue.Enqueue(() => ShowPopupMinigame());
        }

        ///------------------CHECK END MINIGAME--------------------//
        if (isEndMiniGame)
        {
            Debug.Log("IS END MINI GAME");
            tweenQueue.Enqueue(() => ShowPopupResultMiniGame());
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

    #region Popups
    private void ShowPopupGetFreeSpin()
    {
        SoundManager.Instance.PlayEffectFromPath(SoundSlot.FREESPIN);
        effectContainer.gameObject.SetActive(true);
        popupFreeSpin.gameObject.SetActive(true);
        popupFreeSpin.AnimationState.SetAnimation(0, "yahoo", true);
        freeSpinTurnText.text = "9";

        popupFreeSpin.transform.localScale = new Vector2(0.25f, 0.25f);
        popupFreeSpin.transform.DOScale(new Vector2(1, 1f), 0.3f).SetEase(Ease.OutBack);
        getFreeSpinOKButton.gameObject.SetActive(false);

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
                freeSpinMultiplierText.text = "x" + 50;
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
        resultFreeSpinMultiplierText.text = "x" + "10";
        Utility.TweenNumberToMoney(resultFreeSpinRewardText, totalChipWinByGame, 0, 1.0f, 10000);

        popupResultFreeSpin.transform.localScale = new Vector2(0.25f, 0.25f);
        popupResultFreeSpin.transform.DOScale(new Vector2(1, 1f), 0.3f).SetEase(Ease.OutBack);

        freeSpinMultiplierText.transform.DOScale(new Vector2(0.5f, 0.5f), 1.5f).SetEase(Ease.OutBack);
        if (spinType == SpinType.AUTO || spinType == SpinType.FREE_AUTO)
        {
            DOTween.Sequence()
                .AppendInterval(10.0f)
                .AppendCallback(() =>
                {
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
            if (totalChipWinByGame > PaylineIdList.Count * currentBetLevel) winType = WinType.BIG_WIN;
            if (totalChipWinByGame > 50 * currentBetLevel) winType = WinType.MEGA_WIN;
            if (totalChipWinByGame > 0)
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
        popupResultMinigame.gameObject.SetActive(true);
        effectContainer.gameObject.SetActive(true);
        Utility.PlayAnimation(popupResultMinigame, "Eng", true);
        popupResultMinigame.transform.localScale = new Vector2(.8f, .8f);
        popupResultMinigame.transform.DOScale(new Vector2(1.0f, 1.0f), 0.3f).SetEase(Ease.OutBack);

        resultMinigameRewardText.text = Utility.FormatMoney3(totalChipWinByGame, 10000);
        SetCurrentChipValue(playerWalletAfter);
    }

    public void HidePopupResultMiniGame()
    {
        popupResultMinigame.transform.DOScale(new Vector2(0.25f, 0.25f), 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            popupResultMinigame.gameObject.SetActive(false);
            effectContainer.gameObject.SetActive(false);
            miniGameView.Hide();
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
}
