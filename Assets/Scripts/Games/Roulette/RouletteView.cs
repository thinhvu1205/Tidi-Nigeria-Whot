using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Globals;
using Google.Protobuf;
using Nakama;
using Proto;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class RouletteView : BaseDiceGameView
{
    [SerializeField] private RouletteOptionBet[] listBetOptions;
    [SerializeField] private RouletteButtonBet[] listBetButtons;
    [SerializeField] private Image imageSpin, imageBall, imagePopupHistory, imageButtonRebet, imageButtonDouble;
    [SerializeField]
    private Image button1stDozenActive, button2ndDozenActive, button3rdDozenActive, button1To18Active,
    button19To36Active, buttonRedActive, buttonBlackActive, buttonOddActive, buttonEvenActive, button1stLineActive, button2ndLineActive, button3rdLineActive;
    [SerializeField]
    private TextMeshProUGUI textResult, textNumWin, textNumLose, textPercentRed, textPercentBlack,
    textClearValue, textDealValue, textMoney, textDeal;
    [SerializeField] private TextNumberControl textCoinValue;
    [SerializeField] private GameObject chipPrefab;
    [SerializeField] private Button buttonDouble, buttonDeal, buttonClear, buttonHistory, buttonCloseHistory, buttonRebet, buttonSpin;
    [SerializeField] private SkeletonGraphic animationResult, animationWinLose;
    [SerializeField] private RectTransform transformTabResult, transformButtonMenu, tableBet, tableSpin;
    [SerializeField] private RouletteHistory resultHistoryPrefab;
    [SerializeField] private Transform resultHistoryParent, resultHistoryPopupParent, chipContainer, effectContainer;
    [SerializeField] private RoulettePlayerView player;
    private readonly Vector2[] listPositionBallEnd = new Vector2[]
    {
        new Vector2(-112, 152),
        new Vector2(-28, -200),
        new Vector2(76, 179),
        new Vector2(-165, 115),
        new Vector2(8, 196),
        new Vector2(102, -178),
        new Vector2(176, 91),
        new Vector2(-203, -15),
        new Vector2(179, -104),
        new Vector2(-151, -138),
        new Vector2(133, -157),
        new Vector2(199, -39),
        new Vector2(-200, 50),
        new Vector2(202, 28),
        new Vector2(-93, -181),
        new Vector2(-53, 189),
        new Vector2(40, -202),
        new Vector2(134, 145),
        new Vector2(-186, -82),
        new Vector2(-21, 193),
        new Vector2(-61, -196),
        new Vector2(47, 196),
        new Vector2(-167, -112),
        new Vector2(158, -132),
        new Vector2(76, -194),
        new Vector2(108, 165),
        new Vector2(-141, 138),
        new Vector2(191, 62),
        new Vector2(-203, 17),
        new Vector2(-198, -52),
        new Vector2(188, -74),
        new Vector2(-123, -165),
        new Vector2(-91, 180),
        new Vector2(7, -207),
        new Vector2(160, 118),
        new Vector2(-183, 83),
        new Vector2(204, -10)
    };

    private int result, currentBetIndex;
    [SerializeField] private long totalBetValue;
    [SerializeField] private long currentBetValue;
    private RouletteOptionBet resultOption, selectedOption;
    private readonly List<BetData> listDataBet = new();
    private readonly List<BetData> listDataRebet = new();
    private readonly List<RouletteBet> listRouletteBet = new(); // Data gửi lên server
    private Dictionary<int, long> playersBet = new();
    private readonly List<RouletteHistory> listResultHistory = new();
    private UnityEngine.Pool.ObjectPool<RouletteChip> chipPool;
    private List<long> coefficients = new();
    [SerializeField] private long chipWin, chipAfter, playerWallet;
    [SerializeField] private bool isRebet = false, isConfirmRebet = false, isShowingResult = false, canClick = true;
    [SerializeField] private long markUnit, maxBetValue;

    protected override void Awake()
    {
        base.Awake();
        InitPool();
        playerWallet = User.userProfile.AccountChip;
    }

    protected override void Start()
    {
        base.Start();
        foreach (var option in listBetOptions)
        {
            option.OnTriggerDown += RouletteOptionBet_OnTriggerDown;
            option.OnTriggerUp += RouletteOptionBet_OnTriggerUp;
        }
        
    }

    public override void HandleUpdateUserInTable(IMatchState matchState)
    {
        base.HandleUpdateUserInTable(matchState);
        var updateTable = UpdateTable.Parser.ParseFrom(matchState.State);
        Debug.Log("HandleUpdateUserInTable " + updateTable);
        player.SetData(updateTable.Players[0]);
        playerWallet = long.Parse(updateTable.Players[0].Wallet);
        // UpdateListPlayer(updateTable.Players.ToList());
    }

    public override void HandleUpdateWallet(IMatchState matchState)
    {
        base.HandleUpdateWallet(matchState);
        var data = BalanceResult.Parser.ParseFrom(matchState.State);
        Debug.Log("Update wallet: " + data.ToString());
        if (data.Updates.Count == 0) return;
        playerWallet = data.Updates[0].AmountChipCurrent;
        hasBet = true;
        if (data.Updates[0].AmountChipBefore <= data.Updates[0].AmountChipCurrent)
        {
            // Nhận tiền thắng sau khi quay
            chipWin = data.Updates[0].AmountChipAdd;
            chipAfter = data.Updates[0].AmountChipCurrent;
        } else
        {
            // Đặt cược
            player.AnimateFlyMoney(-data.Updates[0].AmoutChipBet);
            player.SetCurrentChip(data.Updates[0].AmountChipCurrent);
            chipWin = 0;
        }

    }

    public override void HandleUpdateTable(IMatchState matchState)
    {
        base.HandleUpdateTable(matchState);
        var data = RouletteUpdateDesk.Parser.ParseFrom(matchState.State);
        Debug.Log("Update table: " + data.ToString());
        if (data.BetLevels.Count > 0)
        {
            markUnit = (long)data.BetLevels.ToList()[0];
            maxBetValue = markUnit * 100;
            coefficients = new List<long>() { markUnit, markUnit * 5, markUnit * 10, markUnit * 50, markUnit * 100};
            InitButtonBet();
        }

        if (data.IsUpdateDeskCell)
        {
            
        }

        // if (data.Actions.Actions.Count > 0)
        // {
        //     result = data.Actions.Actions[0];
        // }
    }

    public override void HandleFinish(IMatchState matchState)
    {
        base.HandleFinish(matchState);
        var data = RouletteGameFinish.Parser.ParseFrom(matchState.State);
        Debug.Log("Update finish: " + data.ToString());
        result = data.WinningNumber;
        OnSpin();
        // if (data.Actions.Actions.Count > 0)
        // {
        //     result = data.Actions.Actions[0];
        // }
    }

    #region Events
    private void RouletteOptionBet_OnTriggerDown(int id)
    {
        if (isShowingResult) return;
        if (currentBetValue + coefficients[currentBetIndex] > playerWallet)
        {
            UIManager.Instance.ShowToast("You do not have enough chips!", 2, transform);
            return;
        }
        if (totalBetValue + currentBetValue + coefficients[currentBetIndex] > maxBetValue)
        {
            UIManager.Instance.ShowToast("You can only bet at most " + Utility.FormatNumber(maxBetValue) + " chips!", 2, transform);
            return;
        }
        if (Constants.RouletteNumberDictionary.TryGetValue(id, out int[] values))
        {
            Debug.Log("ID PRESSED: " + id);
            Debug.Log("Values: " + string.Join(", ", values));
            foreach (RouletteOptionBet item in listBetOptions)
            {
                bool isSelected = item.Id == id;
                bool isInValues = values.Contains(item.Id);

                if (isSelected)
                    selectedOption = item;

                if ((isSelected || isInValues) && item.Id < 49)
                    Utility.SetAlpha100(item.HighlightImage);
            }
        }
    }

    private void RouletteOptionBet_OnTriggerUp(int id)
    {
        if (isShowingResult) return;
        if (currentBetValue + coefficients[currentBetIndex] > playerWallet)
        {
            return;
        }
        if (totalBetValue + currentBetValue + coefficients[currentBetIndex] > maxBetValue)
        {
            return;
        }

        Debug.Log("SELECTED BET: " + selectedOption.transform.position);
       
        SoundManager.Instance.PlayEffectFromPath(SoundRoulette.chipAdd);
        DOVirtual.DelayedCall(0.1f, () =>
        {
            foreach (RouletteOptionBet item in listBetOptions)
            {
                Utility.SetAlpha0(item.HighlightImage);
            }
        });
        RouletteChip chip = chipPool.Get();
        chip.transform.SetParent(selectedOption.transform);
        chip.SetInfo(currentBetIndex, coefficients[currentBetIndex]);
        selectedOption.AddChip(chip);

        currentBetValue += coefficients[currentBetIndex];
        UpdateTotalBetUI(totalBetValue + currentBetValue);

        if (Constants.RouletteNumberDictionary.TryGetValue(id, out int[] values))
        {
            listDataBet.Add(new BetData(id, currentBetIndex, values, coefficients[currentBetIndex]));

        }
        
        if (playersBet.TryGetValue(id, out long current))
        {
            playersBet[id] = current + coefficients[currentBetIndex];   // đã có -> cộng thêm
        }
        else
        {
            playersBet[id] = coefficients[currentBetIndex];             // chưa có -> add mới
        }
        UpdateTotalDealValueUI();
        UpdateButtonBetInteractivity();
    }
    #endregion

    #region Button Click
    public void OnClickSpin()
    {
        Debug.Log("BEFORE CLICK");
        if (isShowingResult || !canClick) return;
        canClick = false;
        if (totalBetValue > 0 && totalBetValue < markUnit)
        {
            UIManager.Instance.ShowToast("You must bet at least " + Utility.FormatNumber(markUnit) + " chips!", 2, transform);
            canClick = true;
            return;
        }
        Debug.Log("CLICK");
        SoundManager.Instance.PlayEffectFromPath(Sound.CLICK);
        DataSender.SendMatchState((long)OpCodeRequest.Spin, new byte[0]);
    }

    public void OnSpin()
    {
        resultOption = listBetOptions.FirstOrDefault(option => option.Id == result);
        buttonSpin.interactable = false;
        // ClickButtonClear();
        // playSound(SOUND_GAME.CLICK);
        // for (int i = 0; i < listBetOptions.Count; i++)
        // {
        //     listBetOptions[i].buttonBetOption.interactable = false;
        // }
        buttonHistory.interactable = false;
        tableBet.DOAnchorPosX(1280, 1)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                RotateSpinAndBall();
            });
        tableSpin.DOAnchorPosX(0, 1).SetEase(Ease.InOutQuad);
        transformButtonMenu.DOAnchorPosX(240, 0.5f);
        transformTabResult.DOAnchorPosX(-232, 0.75f).SetEase(Ease.InOutQuad);
        // SocketSend.sendSpinRoulette();
        // if (listDataBetForRebet.Count != 0)
        // {
        //     listDataBetForRebetTemp.Clear();
        //     listDataBetForRebetTemp.AddRange(listDataBetForRebet);
        //     listDataBetForRebet.Clear();
        // }
        if (totalBetValue > 0)
        {
            listDataRebet.Clear();
            listDataRebet.AddRange(listDataBet);
            listDataBet.Clear();
            playersBet.Clear(); 
        }
        else
        {
            // TODO
            Reset();
        }
    }

    public void OnClickButtonBet(int index)
    {
        SoundManager.Instance.PlayEffectFromPath(Sound.CLICK);
        currentBetIndex = index;
        for (int i = 0; i < listBetButtons.Length; i++)
        {
            listBetButtons[i].SetSelected(i == index);
        }
    }

    public void OnClickButtonDeal()
    {
        Debug.Log("TOTAL BET VALUE: " + totalBetValue);
        canClick = true;
        if (totalBetValue + currentBetValue > maxBetValue)
        {
            UIManager.Instance.ShowToast("You must bet at most " + Utility.FormatNumber(maxBetValue) + " chips!", 2, transform);
            return;
        }
        totalBetValue += currentBetValue;
        currentBetValue = 0;

        if (isRebet && !isConfirmRebet)
        {
            isConfirmRebet = true;
        }
        UpdateButtonBetInteractivity();
        SoundManager.Instance.PlayEffectFromPath(Sound.CLICK);
        List<RouletteBet> listBet = new();

        foreach (var (betId, amount) in playersBet)
        {
            Debug.Log($"ID: {betId}  -  Amount: {amount}");
            Constants.RouletteNumberDictionary.TryGetValue(betId, out int[] numberArray);
            Constants.RouletteCellTypeDictionary.TryGetValue(betId, out RouletteBetCell cellType);
            RouletteBet bet = new RouletteBet
            {
                Chips = amount,
                Cell = cellType
            };

            if (numberArray != null)
            {
                bet.Numbers.AddRange(numberArray);
            }
            listBet.Add(bet);
        }

        RoulettePlayerBet roulettePlayerBet = new RoulettePlayerBet
        {
            UserId = User.userProfile.UserId,
        };
        roulettePlayerBet.Bets.AddRange(listBet);
    
        foreach (var betOption in listBetOptions)
        {
            int childCount = betOption.transform.childCount;

            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = betOption.transform.GetChild(i);
                RouletteChip chip = child.GetComponent<RouletteChip>();
                if (chip != null && !chip.IsDealt)
                {
                    chip.IsDealt = true;
                }
            }
        }
        playersBet.Clear();
        UpdateTotalDealValueUI();
        UpdateTotalBetUI(totalBetValue);
    
        DataSender.SendMatchState((long)OpCodeRequest.Bet, roulettePlayerBet.ToByteArray());
    }

    public void OnClickButtonClear()
    {
        isRebet = false;
        SoundManager.Instance.PlayEffectFromPath(Sound.CLICK);
        foreach (RouletteOptionBet betOption in listBetOptions)
        {
            int childCount = betOption.transform.childCount;
            if (childCount == 0) continue;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = betOption.transform.GetChild(i);
                RouletteChip chip = child.GetComponent<RouletteChip>();
                if (chip != null && !chip.IsDealt && chip.gameObject.activeSelf) // Chỉ xóa chip chưa deal
                {
                    betOption.RemoveChip(chip);
                    ClearChip(child);
                    BetData betData = listDataBet.FirstOrDefault(b => b.IdBet == betOption.Id);
                    listDataBet.Remove(betData);

                }
            }
        }
        currentBetValue = 0;
        playersBet.Clear(); 
        UpdateTotalDealValueUI();
        UpdateTotalBetUI(totalBetValue);
        UpdateButtonBetInteractivity();
    }

    public void OnClickButtonRebet()
    {
        isRebet = true;
        SoundManager.Instance.PlayEffectFromPath(Sound.CLICK);
        foreach (BetData data in listDataRebet)
        {
            RouletteOptionBet option = listBetOptions.FirstOrDefault(o => o.Id == data.IdBet);
            if (option != null)
            {
                long totalAmount = data.BetAmount;
                RouletteChip newChip = chipPool.Get();
                newChip.transform.SetParent(option.transform, false);
                newChip.SetInfo(data.BetType, totalAmount);
                option.AddChip(newChip);

                currentBetValue += totalAmount;
                if (Constants.RouletteNumberDictionary.TryGetValue(data.IdBet, out int[] values))
                {
                    listDataBet.Add(new BetData(data.IdBet, data.BetType, values, totalAmount));
                    if (playersBet.TryGetValue(data.IdBet, out long current))
                    {
                        playersBet[data.IdBet] = current + totalAmount;   // đã có -> cộng thêm
                    }
                    else
                    {
                        playersBet[data.IdBet] = totalAmount;             // chưa có -> add mới
                    }
                }
            }
        }
        UpdateTotalDealValueUI();
        UpdateTotalBetUI(totalBetValue + currentBetValue);
        UpdateButtonBetInteractivity();
    }

    public void OnClickButtonDouble()
    {
        SoundManager.Instance.PlayEffectFromPath(Sound.CLICK);
        Debug.Log("OnClickButtonDouble");
        foreach (RouletteOptionBet option in listBetOptions)
        {
            if (option.Chips.Count == 0 || option.Chips.All(chip => chip.IsDealt)) continue;

            long totalAmount = 0;
            for (int i = option.Chips.Count - 1; i >= 0; i--)
            {
                RouletteChip chip = option.Chips[i];
                if (chip != null && !chip.IsDealt)
                {
                    totalAmount += chip.Value;
                    currentBetValue -= chip.Value;
                    option.RemoveChip(chip);
                    BetData betData = listDataBet.FirstOrDefault(b => b.IdBet == option.Id);
                    listDataBet.Remove(betData);

                }
            }

            // Spawn chip mới
            RouletteChip newChip = chipPool.Get();
            newChip.transform.SetParent(option.transform, false);
            newChip.SetInfo(currentBetIndex, totalAmount * 2);
            option.AddChip(newChip);

            currentBetValue += totalAmount * 2;
            if (Constants.RouletteNumberDictionary.TryGetValue(option.Id, out int[] values))
            {
                listDataBet.Add(new BetData(option.Id, currentBetIndex, values, totalAmount * 2));
            }
        }

        foreach (var key in playersBet.Keys.ToList())
        {
            playersBet[key] *= 2;
        }
        UpdateTotalDealValueUI();
        UpdateTotalBetUI(totalBetValue + currentBetValue);
        // playersBet.Clear();
    }

    public void OnClickButtonHistory()
    {
        SoundManager.Instance.PlayEffectFromPath(Sound.CLICK);
        // playSound(SOUND_GAME.CLICK);
        imagePopupHistory.gameObject.SetActive(true);
        foreach (Transform child in resultHistoryPopupParent)
        {
            RouletteHistory history = child.GetComponent<RouletteHistory>();
            history.Animation.gameObject.SetActive(false);
        }
        if (listResultHistory.Count != 0)
        {
            DOVirtual.DelayedCall(0.1f, () =>
            {
                RouletteHistory history = resultHistoryPopupParent.GetChild(0).GetComponent<RouletteHistory>();
                Utility.PlayAnimation(history.Animation, "khung1", true);
            });
        }
        else
        {
            textPercentBlack.text = $"0%";
            textPercentRed.text = $"0%";
        }
    }

    public void OnClickButtonCloseHistory()
    {
        SoundManager.Instance.PlayEffectFromPath(Sound.CLICK);
        imagePopupHistory.gameObject.SetActive(false);
    }
    #endregion

    #region Visuals
    private void UpdateTotalBetUI(long value)
    {
        textCoinValue.SetValue(value);
    }

    private void UpdateTotalDealValueUI()
    {
        textDealValue.text = Utility.FormatMoney(currentBetValue, true);
        textClearValue.text = Utility.FormatMoney(currentBetValue, true);

        if (currentBetValue == 0)
        {
            buttonDeal.interactable = false;
            buttonClear.interactable = false;
        }
        else
        {
            buttonDeal.interactable = true;
            buttonClear.interactable = true;
        }
        long totalRebetAmount = listDataRebet.Sum(data => data.BetAmount);

        buttonDouble.interactable = playersBet.Any() && playerWallet >= 2 * currentBetValue && maxBetValue >= 2 * currentBetValue && maxBetValue >= 2 * totalBetValue;
        imageButtonDouble.color = buttonDouble.interactable ? Color.white : Color.gray;
        buttonRebet.interactable = listDataRebet.Count > 0 && !isRebet && !isConfirmRebet && playerWallet >= totalRebetAmount && !isShowingResult;
        imageButtonRebet.color = buttonRebet.interactable ? Color.white : Color.gray;
    }

    private void ClearChip(Transform chipTransform)
    {
        if (chipTransform.TryGetComponent<RouletteChip>(out var chip))
        {
            if (!chipTransform.gameObject.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup = chipTransform.gameObject.AddComponent<CanvasGroup>();
            }

            chipTransform.DOScale(Vector3.one, 0.5f);
            chipTransform.DOLocalMove(new Vector3(0, 32, 0), 0.5f);
            canvasGroup.DOFade(0, 1f).OnComplete(() =>
            {
                canvasGroup.alpha = 1f;
                chipPool.Release(chip);
            });
        }
    }

    private void ShowTextNumDeal()
    {
        textDeal.transform.localPosition = new Vector3(32, 0, 0);
        textDeal.gameObject.SetActive(true);

        textDeal.text = $"{-currentBetValue}";

        textDeal.transform.DOLocalMoveY(140, 2f)
            .SetEase(Ease.Linear)
            .OnComplete(() => { textDeal.gameObject.SetActive(false); });
    }
    #endregion

    #region Animation Ball
    private void RotateSpinAndBall()
    {
        Vector2 spinCenter = imageSpin.rectTransform.localPosition;
        Vector2 ballCenter = imageBall.rectTransform.localPosition;
        float initialRadius = Vector2.Distance(ballCenter, spinCenter);
        Vector3 startPosBall = new Vector3(296, 0, 0);
        imageBall.transform.localPosition = startPosBall;
        imageSpin.rectTransform.DORotate(new Vector3(0, 0, 100), 2f, RotateMode.FastBeyond360)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                imageBall.rectTransform
                    .DORotate(new Vector3(0, 0, -720), 5f, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear);
                imageBall.rectTransform
                    .DOLocalPath(GetCirclePath(spinCenter, initialRadius, 1080), 5f, PathType.CatmullRom)
                    .SetEase(Ease.Linear)
                    .OnComplete(() => { ReduceRadiusAndSpin(spinCenter, initialRadius, 2, 3f); });

                imageSpin.rectTransform.DORotate(new Vector3(0, 0, -720), 5f, RotateMode.FastBeyond360)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        imageSpin.rectTransform.DORotate(new Vector3(0, 0, -1440), 7f, RotateMode.FastBeyond360)
                            .SetEase(Ease.OutQuad)
                            .OnComplete(() =>
                            {
                                Debug.Log("SpinDone");
                                ShowResultAnimation();
                            });
                    });
            });
    }

    private Vector3[] GetCirclePath(Vector2 center, float radius, float totalDegrees)
    {
        int segments = 100;
        Vector3[] path = new Vector3[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Deg2Rad * (i * (totalDegrees / segments));
            float x = center.x + radius * Mathf.Cos(angle);
            float y = center.y + radius * Mathf.Sin(angle);
            path[i] = new Vector3(x, y, 0);
        }

        return path;
    }

    private void ReduceRadiusAndSpin(Vector2 center, float initialRadius, int numRounds, float duration)
    {
        float totalDegrees = 360f * numRounds;
        float timeStep = duration / numRounds / 10;

        DOTween.To(() => initialRadius, x => initialRadius = x, initialRadius * 0.5f, duration)
            .SetEase(Ease.InOutQuad);

        imageBall.rectTransform.DOLocalPath(GetShrinkingCirclePath(center, initialRadius, totalDegrees), duration,
                PathType.CatmullRom)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                Vector2 targetPos = listPositionBallEnd[result];
                Vector2 direction = (targetPos - Vector2.zero).normalized;
                Vector2 offsetPos = targetPos - direction * 50f;

                imageBall.transform
                    .DOLocalMove(new Vector3(offsetPos.x, offsetPos.y, 0), 0.5f)
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        imageBall.transform.SetParent(imageSpin.transform);
                    });
            });
    }

    private Vector3[] GetShrinkingCirclePath(Vector2 center, float initialRadius, float totalDegrees)
    {
        int segments = 100;
        Vector3[] path = new Vector3[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float radius = Mathf.Lerp(initialRadius, initialRadius * 0.5f, t);
            float angle = Mathf.Deg2Rad * (t * totalDegrees);
            float x = center.x + radius * Mathf.Cos(angle);
            float y = center.y + radius * Mathf.Sin(angle);
            path[i] = new Vector3(x, y, 0);
        }

        return path;
    }
    #endregion

    #region Animation Result
    private void ShowResult()
    {
        DOVirtual.DelayedCall(2f, () =>
        {
            ShowAnimationResult();

            // Delay tiếp 3s sau khi show animation mới restart game
            DOVirtual.DelayedCall(1f, () =>
            {
                if (totalBetValue > 0)
                {
                    SoundManager.Instance.PlayEffectFromPath(Sound.THROW_CHIP);
                    Reset();
                }
                else
                {
                    isShowingResult = false;
                    canClick = true;
                    buttonSpin.interactable = true;
                }
                // playSound(SOUND_GAME.THROW_CHIP);
            });
        });
        HighlightWinningImage(resultOption.HighlightImage);
        if (result == 0) return;
        // Dozen
        var dozenMap = new Dictionary<Func<RouletteOptionBet, bool>, Image>
        {
            { o => o.IsInFirstDozen, button1stDozenActive },
            { o => o.IsInSecondDozen, button2ndDozenActive },
            { o => true,               button3rdDozenActive } // fallback
        };
        HighlightByCondition(resultOption, dozenMap);

        // Line
        var lineMap = new Dictionary<Func<RouletteOptionBet, bool>, Image>
        {
            { o => o.IsInFirstLine, button1stLineActive },
            { o => o.IsInSecondLine, button2ndLineActive },
            { o => true, button3rdLineActive }
        };
        HighlightByCondition(resultOption, lineMap);

        // 1-18 / 19-36
        HighlightWinningImage(resultOption.IsIn1To18 ? button1To18Active : button19To36Active);

        // Red / Black
        HighlightWinningImage(resultOption.IsRed ? buttonRedActive : buttonBlackActive);

        // Even / Odd
        HighlightWinningImage(resultOption.IsEven ? buttonEvenActive : buttonOddActive);

    }

    private void ShowAnimationResult()
    {
        if (!hasBet) return;
        effectContainer.gameObject.SetActive(true);
        string animationName = "win";
        if (chipWin > 0)
        {
            animationName = "win";
            textNumWin.gameObject.SetActive(true);
            textNumLose.gameObject.SetActive(false);
            textNumWin.text = $"+{Utility.FormatNumber(chipWin)}";
            player.AnimateFlyMoney(chipWin);
            player.SetCurrentChip(chipAfter);
            SoundManager.Instance.PlayEffectFromPath(Sound.WIN);

        }
        else
        {
            animationName = "lose";
            textNumWin.gameObject.SetActive(false);
            textNumLose.transform.localPosition = new Vector3(108, 40, 0);
            textNumLose.gameObject.SetActive(true);
            // textNumLose.transform.DOLocalMoveY(10, 0.5f);
            textNumLose.text = $"-{Utility.FormatNumber(totalBetValue)}";
            // playSound(SOUND_GAME.LOSE);
        }
        Utility.PlayAnimation(animationWinLose, animationName, false);
        DOVirtual.DelayedCall(1f, () =>
        {
            effectContainer.gameObject.SetActive(false);
        });
    }

    private void HighlightByCondition(RouletteOptionBet option, Dictionary<Func<RouletteOptionBet, bool>, Image> map)
    {
        foreach (var kv in map)
        {
            if (kv.Key(option))
            {
                HighlightWinningImage(kv.Value);
                break;
            }
        }
    }
    private void HighlightWinningImage(Image image)
    {
        Debug.Log("HIGH LIGHT WINNING IMAGE!");
        Utility.SetAlpha100(image);
        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < 4; i++)
        {
            sequence.AppendCallback(() => Utility.SetAlpha100(image));
            sequence.AppendInterval(0.4f);
            sequence.AppendCallback(() => Utility.SetAlpha0(image));
            sequence.AppendInterval(0.4f);
        }
        sequence.OnComplete(() => Utility.SetAlpha0(image));
    }

    private void ShowResultAnimation()
    {
        isShowingResult = true;
        SoundManager.Instance.PlayEffectFromPath(SoundRoulette.showResult);
        animationResult.gameObject.SetActive(true);
        string animationName = "green";
        if (resultOption.IsRed)
        {
            animationName = "red";
        }
        else if (resultOption.IsBlack)
        {
            animationName = "black";
        }
        // playSound(SOUND_ROULETTE.showResult);
        textResult.gameObject.SetActive(true);
        textResult.text = $"{result}";
        Utility.PlayAnimation(animationResult, animationName, false);
        DOVirtual.DelayedCall(2f, () =>
        {
            buttonHistory.interactable = true;
            // listDataBet.Clear();
            textResult.gameObject.SetActive(false);
            tableBet.DOAnchorPosX(0, 1).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                ShowResult();
                UpdateTotalDealValueUI();
            });
            tableSpin.DOAnchorPosX(-2400, 1f).SetEase(Ease.InOutQuad);
            transformButtonMenu.DOAnchorPosX(60, 0.5f);
            transformTabResult.DOAnchorPosX(-44, 0.75f).SetEase(Ease.InOutQuad);
            
        });

        RouletteHistory resultHistory = Instantiate(resultHistoryPrefab, resultHistoryParent);
        RouletteHistory resultHistoryInPopup = Instantiate(resultHistoryPrefab, resultHistoryPopupParent);
        resultHistoryInPopup.transform.SetAsFirstSibling();
        listResultHistory.Add(resultHistory);
        if (resultOption.IsRed)
        {
            resultHistory.Init(result, 1, false);
            resultHistoryInPopup.Init(result, 1, false);
        }
        else if (resultOption.IsBlack)
        {
            resultHistory.Init(result, 2, false);
            resultHistoryInPopup.Init(result, 2, false);
        }
        else
        {
            resultHistory.Init(result, 0, false);
            resultHistoryInPopup.Init(result, 0, false);
        }

        for (int i = 0; i < resultHistoryParent.childCount; i++)
        {
            Transform child = resultHistoryParent.GetChild(i);
            if (i == resultHistoryParent.childCount - 1)
            {
                child.localScale = Vector3.one;
            }
            else
            {
                child.localScale = new Vector3(0.75f, 0.75f, 1f);
            }
        }

                int nonZeroCount = listResultHistory.Count(history => history.Value != 0);
        int x = listResultHistory.Count(history => history.Value != 0 && history.IsRed);
        float percentRed = nonZeroCount > 0 ? (float)x / nonZeroCount : 0;
        float percentBlack = nonZeroCount > 0 ? 100 - (percentRed * 100) : 0;

        textPercentRed.text = $"{percentRed * 100:0}%";
        textPercentBlack.text = $"{percentBlack:0}%";
    }

    #endregion

    #region Setups
    private void InitPool()
    {
        chipPool = new UnityEngine.Pool.ObjectPool<RouletteChip>(
            createFunc: () =>
            {
                var chip = Instantiate(chipPrefab, chipContainer);
                chip.SetActive(false);
                return chip.GetComponent<RouletteChip>();
            },
            actionOnGet: (chip) =>
            {
                chip.gameObject.SetActive(true);
                chip.transform.localScale = Vector3.one;
                chip.IsDealt = false;
            },
            actionOnRelease: (chip) =>
            {
                chip.gameObject.SetActive(false);
            },
            actionOnDestroy: (chip) =>
            {
                Destroy(chip);
            },
            defaultCapacity: 1,
            maxSize: 1000
        );
    }

    private void InitButtonBet()
    {
        foreach (RouletteButtonBet button in listBetButtons)
        {
            if (Array.IndexOf(listBetButtons, button) >= coefficients.Count)
            {
                button.gameObject.SetActive(false);
                continue;
            }
            Debug.Log("SET INFO");
            button.SetInfo(coefficients[Array.IndexOf(listBetButtons, button)]);
        }
        UpdateButtonBetInteractivity();
        currentBetIndex = 0;
        buttonDeal.interactable = false;
        buttonClear.interactable = false;
        buttonDouble.interactable = false;
        imageButtonDouble.color = Color.gray;
        buttonRebet.interactable = false;
        imageButtonRebet.color = Color.gray;
        OnClickButtonBet(currentBetIndex);
    }

    private void UpdateButtonBetInteractivity()
    {
        foreach (RouletteButtonBet button in listBetButtons)
        {
            int index = Array.IndexOf(listBetButtons, button);
            if (Array.IndexOf(listBetButtons, button) >= coefficients.Count)
            {
                continue;
            }
            if (coefficients[index] > playerWallet || 
                currentBetValue + coefficients[index] > maxBetValue || 
                totalBetValue + coefficients[index] > maxBetValue 
            )
            {
                button.Disable();
            }
            else
            {
                button.Enable();
            }
        }
    }

    private void Reset()
    {
        currentBetValue = 0;
        totalBetValue = 0;
        chipWin = 0;
        chipAfter = 0;
        isRebet = false;
        isConfirmRebet = false;
        hasBet = false;
        isShowingResult = false;
        canClick = true;

        listDataBet.Clear();
        buttonSpin.interactable = true;
        UpdateTotalDealValueUI();
        UpdateTotalBetUI(totalBetValue);
        UpdateButtonBetInteractivity();
        // Xóa chip
        foreach (RouletteOptionBet betOption in listBetOptions)
        {
            int childCount = betOption.transform.childCount;
            if (childCount == 0) continue;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = betOption.transform.GetChild(i);
                RouletteChip chip = child.GetComponent<RouletteChip>();
                if (chip != null && chip.gameObject.activeSelf)
                {
                    betOption.RemoveChip(chip);
                    ClearChip(child);
                }
            }
        }
    }
    #endregion

    [Serializable]
    private struct BetData
    {
        public int IdBet { get; set; }
        public int BetType { get; set; }
        public int[] NumArr { get; set; }
        public long BetAmount { get; set; }

        public BetData(int idBet, int betType, int[] numArr, long betAmount)
        {
            IdBet = idBet;
            BetType = betType;
            NumArr = numArr;
            BetAmount = betAmount;
        }
    }
}
