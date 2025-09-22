using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Pool;
using DG.Tweening;
using Games.Card;
using Globals;
using Google.Protobuf;
using Nakama;
using Proto;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameState = Proto.GameState;

public class BlackjackView : BaseDiceGameView
{
    [Header(" Transforms ")]
    [SerializeField] private Transform boxBetContainer;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private Transform chipContainer;
    [SerializeField] private Transform bankerCardsContainer;
    [SerializeField] private Transform buttonBetContainer;
    [SerializeField] private Transform dealCardContainer;
    [SerializeField] private Transform returnCardContainer;
    [SerializeField] private Transform effectContainer;

    [Header(" Game Objects ")]
    [SerializeField] private GameObject countdownContainer;
    [SerializeField] private GameObject buttonRebet;
    [SerializeField] private GameObject buttonDoubleBet;
    [SerializeField] private GameObject buttonDeal;
    [SerializeField] private GameObject buttonClear;
    [SerializeField] private GameObject rulePrefab;
    [SerializeField] private GameObject phaseBet;
    [SerializeField] private GameObject phasePlay;

    [Header(" Buttons ")]
    [SerializeField] private BlackjackButtonAction buttonDouble;
    [SerializeField] private BlackjackButtonAction buttonSplit;
    [SerializeField] private BlackjackButtonAction buttonHit;
    [SerializeField] private BlackjackButtonAction buttonStand;

    [Header(" Texts ")]
    [SerializeField] private TextMeshProUGUI textCountdown;
    [SerializeField] private TextMeshProUGUI textDealValue;
    [SerializeField] private TextMeshProUGUI textClearValue;

    [Header(" Images ")]
    [SerializeField] private Image imageCountdown;

    [Header(" Prefabs ")]
    [SerializeField] private CardModel cardPrefab;
    [SerializeField] private BlackjackChip chipPrefab;
    [SerializeField] private BlackjackBoxBet boxBetPrefab;
    [SerializeField] private BlackjackInsurance insurance;
    [SerializeField] private BlackjackBoxBet bankerBoxBet;

    [Header(" Animations ")]
    [SerializeField] private SkeletonGraphic animationWinBlackjack;

    [Header(" Object Pool")]
    protected UnityEngine.Pool.ObjectPool<CardModel> cardPool;
    protected UnityEngine.Pool.ObjectPool<BlackjackChip> chipPool;

    [Header(" Lists ")]
    [SerializeField] private Vector2[] listBoxBetPosition;
    [SerializeField] private BlackjackChipBet[] listChipBet;
    private readonly Vector2[] listInsuranceChipPosition = new Vector2[]
    {
        new(0, 30),
        new(-248, 181),
        new(248, 181),
    };
    private Coroutine countdownCoroutine;
    private BlackjackBoxBet currentPlayerBoxBet;


    [Header(" Constants ")]
    protected readonly Dictionary<string, BlackjackBoxBet> userIdToBoxBetView = new();
    private const float DEAL_CARD_ANIMATION_TIME = 0.5f;

    private List<long> listValueChipBets = new();
    private List<Card> listBankerCard = new();
    private int currentChipIndex = 0, lastChipIndex = -1, playerReceiveCardCount = 0;
    private BlackjackActionCode nextActionCode = BlackjackActionCode.BlackjackActionUnspecified; // 1: Split, 2: Double, 3: Hit, 4: Stand
    [SerializeField] private long totalBetValue = 0, currentBetValue = 0, lastBetValue = 0;
    private bool hasDealtCardsForPlayers = false; // Check xem đã chia bài cho các player chưa
    private bool hasDealtCardsForBanker = false; // Check xem đã chia bài cho banker chưa
    private bool isCurrentPlayerTurn = false; // Check xem có phải lượt của người chơi hiện tại không
    private bool isRebet = false; // Check xem có phải người chơi hiện tại đang rebet không
    [SerializeField] private bool isCurrentPlayerFinished = false; // Check xem người chơi hiện tại đã xong ván chưa (được Blackjack/Busted)
    private bool isCountingDown = false;

    protected override void Awake()
    {
        base.Awake();
        InitPool();
        insurance.SetInfo(this);
        phaseBet.gameObject.SetActive(false);
        phasePlay.gameObject.SetActive(false);

    }

    protected override void Update()
    {
        // Khi chạm vào màn hình thì gửi lên trạng thái active để server ko kick người chơi ra khỏi bàn
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            DataSender.SendMatchState((long)OpCodeRequest.OpcodeUserInteractCards, new byte[0]);
        }
    }

    #region API Handlers
    public override void LoadInfoMatch(Match match)
    {
        base.LoadInfoMatch(match);
        SetInfoBet();

        // string labelJson = match.Label;
        // Match data = JsonConvert.DeserializeObject<Match>(labelJson);

    }

    public override void HandleUpdateUserInTable(IMatchState matchState)
    {
        base.HandleUpdateUserInTable(matchState);
        var updateTable = UpdateTable.Parser.ParseFrom(matchState.State);
        Debug.Log("HandleUpdateUserInTable " + updateTable);
        UpdatePosUserTable(updateTable, true);
        
        // if (rearrangedPlayers.Count >= 2)
        // {
        //     Player secondPlayer = rearrangedPlayers[2];
        //     BasePlayerView playerView = userIdToView.GetValueOrDefault(secondPlayer.Id);
        //     playerView.SetPositionInfoRightPlayer();

        // }

    }

    public override void HandleUpdateTable(IMatchState matchState)
    {
        BlackjackUpdateDesk data = BlackjackUpdateDesk.Parser.ParseFrom(matchState.State);
        Debug.Log("Update table: " + data.ToString());
        string playerId = data.Bet?.UserId;
        string currentPlayerId = User.userProfile.UserId;
        // Khi có người chơi đặt cược thì tiến hành trừ tiền của người chơi đó
        if (data.Bet?.Balance?.AmountChipAdd != 0)
        {
            if (playerId != null && userIdToView.TryGetValue(playerId, out var playerView))
            {
                playerView.AnimateFlyMoney(data.Bet.Balance.AmountChipAdd);
                playerView.SetCurrentChip(data.Bet.Balance.AmountChipCurrent);
                // Rebet
                if (isRebet && data.Bet?.Balance.AmoutChipBet > 0)
                {
                    totalBetValue = data.Bet.Balance.AmoutChipBet;
                    OnClickButtonChip(lastChipIndex);
                }

                // Player Khác đặt cược
                if (playerId != currentPlayerId)
                {
                    BlackjackChip chip = chipPool.Get();
                    BlackjackBoxBet boxbet = userIdToBoxBetView.GetValueOrDefault(playerId);
                    boxbet.HasBet = true;
                    int chipIndex = GetChipIndex((int)data.Bet.Balance.AmoutChipBet);
                    chip.transform.SetParent(playerView.GetAvatarTransform().parent, true);
                    chip.SetInfo(chipIndex, playerView.GetAvatarPosition());
                    chip.transform.localScale = Vector2.one * 0.8f;
                    boxbet.HideAnimationWaiting();
                    AnimateMoveChip(chip, boxbet.transform.position, () =>
                    {
                        boxbet.SetBetValue(chipIndex, data.Bet.Balance.AmoutChipBet, data.Bet.Balance.AmoutChipBet);
                    });
                }
            }
        }

        if (data.IsNewTurn)
        {
            isCurrentPlayerTurn = data.InTurn == User.userProfile.UserId;

            // Nếu đã chọn action khi đang ở lượt người chơi khác thì khi đến lượt sẽ thực hiện action đó ngay
            if (isCurrentPlayerTurn && nextActionCode != BlackjackActionCode.BlackjackActionUnspecified)
            {
                HandleClickButtonAction(nextActionCode);
                nextActionCode = BlackjackActionCode.BlackjackActionUnspecified;
            }
            // Set Turn hiện tại
            if (userIdToView.TryGetValue(data.InTurn, out var view))
            {
                view.SetCurrentTurn(true, 10);
            }

            // Cập nhật các nút hành động
            if (data.Actions != null && !isCurrentPlayerFinished)
            {
                buttonBetContainer.gameObject.SetActive(true);
                List<BlackjackActionCode> actions = data.Actions.Actions.ToList();
                buttonDouble.gameObject.SetActive(actions.Contains(BlackjackActionCode.BlackjackActionDouble));
                buttonSplit.gameObject.SetActive(actions.Contains(BlackjackActionCode.BlackjackActionSplit));
                buttonHit.gameObject.SetActive(actions.Contains(BlackjackActionCode.BlackjackActionHit));
                buttonStand.gameObject.SetActive(actions.Contains(BlackjackActionCode.BlackjackActionStay));
            }
            if (data.InTurn != User.userProfile.UserId && !isCurrentPlayerFinished)
            {
                buttonBetContainer.gameObject.SetActive(true);
                buttonDouble.gameObject.SetActive(true);
                // buttonSplit.gameObject.SetActive(true);
                buttonHit.gameObject.SetActive(true);
                buttonStand.gameObject.SetActive(true);
            }
            UpdateButtonActionVisual(isCurrentPlayerTurn);

        }

        // Nhà cái có 1 lá Át, hiện popup bảo hiểm
        if (data.IsInsuranceTurnEnter && !isCurrentPlayerFinished)
        {
            insurance.Show();
        }

        // Đặt cược bảo hiểm
        if (data.Bet != null && data.Bet.Insurance > 0)
        {
            Player player = playingPlayers.Find(p => p.Id == data.Bet.UserId);
            BasePlayerView playerView = userIdToView.GetValueOrDefault(data.Bet.UserId);
            BlackjackChip chip = chipPool.Get();
            chip.transform.SetParent(playerView.transform, true);
            chip.SetInfo(5, playerView.GetAvatarPosition(), data.Bet.Insurance);
            chip.transform.localScale = Vector2.one * 0.8f;
            AnimateMoveInsuranceChip(chip, listInsuranceChipPosition[playingPlayers.IndexOf(player)]);
            thisPlayer.AnimateFlyMoney(-data.Bet.Insurance);

        }

        // Tách bài khi có 2 lá cùng Rank
        if (data.IsSplitHand)
        {
            BlackjackBoxBet boxBet = userIdToBoxBetView.GetValueOrDefault(data.Hand.UserId);
            boxBet.SplitBoxBet(data.Hand.First, data.Hand.Second);

        }
    }

    public override void HandleUpdateDeal(IMatchState matchState)
    {
        var data = BlackjackUpdateDeal.Parser.ParseFrom(matchState.State);
        Debug.Log("Update deal: " + data.ToString());

        if (data.IsBanker)
        {
            // BANKER
            if (data.NewCards.Count > 0)
            {
                // Lật lá thứ 2 (lá úp)
                if (data.IsRevealBankerHiddenCard)
                {
                    Card flippedCard = data.NewCards[0];
                    CardModel flippedCardModel = bankerBoxBet.listCardModel[1];
                    BlackjackHand bankerHand = data.Hand.First;
                    listBankerCard[1] = flippedCard;
                    flippedCardModel.SetData((int)flippedCard.Rank, (int)flippedCard.Suit);
                    flippedCardModel.HideCard();
                    AnimateFlipCard(flippedCardModel);
                    bankerBoxBet.ShowScore(bankerHand.Point, bankerHand.MinPoint, bankerHand.MaxPoint, bankerHand.Type);
                    bankerBoxBet.StopHighlightCards();
                    return;
                }

                // Chia 2 lá đầu tiên (1 là ngửa, 1 lá úp)
                if (!hasDealtCardsForBanker)
                {
                    listBankerCard.AddRange(data.NewCards);
                    StartCoroutine(DealCardsForBanker());
                    hasDealtCardsForBanker = true;
                }
                // Bốc 1 lá mới (lá 3, 4, 5...)
                else
                {
                    Card newCard = data.NewCards[0];
                    CardModel newCardModel = InitCard();
                    BlackjackHand bankerHand = data.Hand.First;
                    newCardModel.SetData((int)newCard.Rank, (int)newCard.Suit);
                    newCardModel.transform.SetParent(bankerCardsContainer);
                    newCardModel.HideCard();
                    bankerBoxBet.listCardModel.Add(newCardModel);
                    AnimateDealACard(newCardModel, bankerCardsContainer.position, true, null, bankerBoxBet.SpreadCards);
                    bankerBoxBet.ShowScore(bankerHand.Point, bankerHand.MinPoint, bankerHand.MaxPoint, bankerHand.Type);

                }
            }
        }
        else
        // PLAYER
        {
            if (data.NewCards.Count > 0)
            {
                BlackjackBoxBet boxBet = userIdToBoxBetView.GetValueOrDefault(data.UserId);
                foreach (Card card in data.NewCards)
                {
                    CardModel cardModel = InitCard();
                    cardModel.SetData((int)card.Rank, (int)card.Suit);
                    cardModel.HideCard();
                    cardModel.transform.SetParent(boxBet.GetCardPosition);
                    boxBet.listCardModel.Add(cardModel);
                }

                if (data.UserId == User.userProfile.UserId &&
                      (data.Hand.First.Type == BlackjackHandType.Blackjack ||
                      data.Hand.Second.Type == BlackjackHandType.Blackjack))
                {
                    isCurrentPlayerFinished = true;
                }

                // Chia bài cho player
                if (!hasDealtCardsForPlayers)
                {
                    playerReceiveCardCount++;
                    currentPlayerBoxBet.HideImageChip();

                    if (playerReceiveCardCount == playingPlayers.Count)
                    {
                        StartCoroutine(DealCardsForPlayers());
                        hasDealtCardsForPlayers = true;
                    }
                }
                else
                {

                    // Player bốc 1 lá mới
                    CardModel newCardModel = boxBet.listCardModel[^1];
                    BlackjackHand playerHand = data.Hand.First;
                    // newCardModel.SetData((int)newCard.Rank, (int)newCard.Suit);
                    // newCardModel.transform.SetParent(boxBet.GetCardPosition);
                    // newCardModel.HideCard();

                    // Box bài 1
                    if (data.HandN0 == BlackjackHandN0.BlackjackHand1St)
                    {
                        AnimateDealACard(newCardModel, boxBet.GetNewCardPosition(), true, null, () =>
                        {
                            boxBet.SpreadCards();
                        });
                    }
                    // Box bài 2
                    else
                    {
                        AnimateDealACard(newCardModel, boxBet.SecondBoxBet.GetNewCardPosition(), true, null, () =>
                        {
                            boxBet.SecondBoxBet.SpreadCards();
                        });
                    }
                    CheckCurrentPlayerFinish(data);
                    DOVirtual.DelayedCall(1.2f, () =>
                    {
                        // Show điểm và box 1
                        if (data.Hand.First != null && data.HandN0 == BlackjackHandN0.BlackjackHand1St)
                        {
                            boxBet.ShowScore(data.Hand.First.Point, data.Hand.First.MinPoint, data.Hand.First.MaxPoint, data.Hand.First.Type);
                        }

                        // Show điểm và box 2
                        if (data.Hand.Second != null && data.HandN0 == BlackjackHandN0.BlackjackHand2Nd)
                        {
                            boxBet.SecondBoxBet.ShowScore(data.Hand.Second.Point, data.Hand.Second.MinPoint, data.Hand.Second.MaxPoint, data.Hand.Second.Type);
                        }

                        // Nếu thisPlayer ăn được Blackjack thì hiện animation Blackjack
                        if (data.UserId == User.userProfile.UserId &&
                            (data.Hand.First.Type == BlackjackHandType.Blackjack ||
                            data.Hand.Second.Type == BlackjackHandType.Blackjack))
                        {
                            effectContainer.gameObject.SetActive(true);
                            animationWinBlackjack.gameObject.SetActive(true);
                            animationWinBlackjack.AnimationState.Complete += delegate
                            {
                                effectContainer.gameObject.SetActive(false);
                                animationWinBlackjack.gameObject.SetActive(false);
                            };
                        }
                    });
                }

                // Check xem player đã chơi hết lượt chưa (Busted/Blackjack)

            }
        }
    }

    public override void HandleUpdateGameState(IMatchState matchState)
    {
        UpdateGameState data = UpdateGameState.Parser.ParseFrom(matchState.State);
        switch (data.State)
        {
            case GameState.Idle:
                Debug.Log("HandleUpdateGameState Idle " + data.ToString());
                break;
            case GameState.Matching:
                Debug.Log("HandleUpdateGameState Matching " + data.ToString());
                break;
            case GameState.Preparing:
                if (!isCountingDown)
                {
                    isCountingDown = true;
                    countdownContainer.SetActive(true);
                    phaseBet.gameObject.SetActive(true);
                    phasePlay.gameObject.SetActive(false);
                    ResetGame();
                    StartCountDownBetTime((int)data.CountDown);
                }
                ShowAllPlayersLoading();
                Debug.Log("HandleUpdateGameState Preparing " + data.ToString());
                break;
            case GameState.Play:
                isCountingDown = false;
                currentPlayerBoxBet.SetBetValue(currentChipIndex, currentBetValue, totalBetValue);
                countdownContainer.SetActive(false);
                phaseBet.gameObject.SetActive(false);
                phasePlay.gameObject.SetActive(true);
                HideAllImageChip();
                HideAllPlayersLoading();
                break;
            case GameState.Reward:
                Debug.Log("HandleUpdateGameState Reward " + data.ToString());
                break;
            case GameState.Finish:
                phasePlay.gameObject.SetActive(false);
                Debug.Log("HandleUpdateGameState Finish " + data.ToString());
                break;

        }
    }

    public override void HandleUpdateTurn(IMatchState matchState)
    {
        var data = UpdateTurn.Parser.ParseFrom(matchState.State);
        Debug.Log("Update turn: " + data.ToString());
    }

    public override void HandleUpdateWallet(IMatchState matchState)
    {
        var data = BalanceResult.Parser.ParseFrom(matchState.State);
        Debug.Log("Update wallet: " + data.ToString());
        foreach (BalanceUpdate update in data.Updates)
        {
            BasePlayerView playerView = userIdToView.GetValueOrDefault(update.UserId);
            if (playerView == null) continue;
            playerView.AnimateFlyMoney(update.AmountChipAdd);
        }
    }

    public override void HandleFinish(IMatchState matchState)
    {
        var data = BlackjackUpdateFinish.Parser.ParseFrom(matchState.State);
        Debug.Log("Update finish: " + data.ToString());
        buttonBetContainer.gameObject.SetActive(false);
        DOVirtual.DelayedCall(2.5f, () =>
        {
            foreach (Player player in rearrangedPlayers)
            {
                if (userIdToView.TryGetValue(player.Id, out var playerView))
                {
                    playerView.SetCurrentTurn(false);
                }
            }
            foreach (BlackjackPLayerBetResult result in data.BetResults)
            {
                if (userIdToBoxBetView.TryGetValue(result.UserId, out var boxBet))
                {
                    boxBet.HideScore();
                }
                if (userIdToView.TryGetValue(result.UserId, out var playerView))
                {
                    if (result.First.IsWin == 1)
                    {
                        playerView.SetEffectWin("animation", false);
                    }
                    else if (result.First.IsWin == -1)
                    {
                        playerView.SetEffectLose("lose", false);
                    }
                    else
                    {
                        playerView.SetEffectDraw("animation", false);
                    }
                }
            }
            bankerBoxBet.HideScore();

            AnimateReturnAllCards();
        });
    }

    public override void HandleUpdateKickOffTheTable(IMatchState matchState)
    {
        Destroy(gameObject);
    }
    #endregion

    private void StartCountDownBetTime(int time)
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);
        countdownCoroutine = StartCoroutine(CountdownRoutine(time));
    }

    private IEnumerator CountdownRoutine(int startTime)
    {
        countdownContainer.SetActive(true);
        float timeLeft = startTime;
        imageCountdown.fillAmount = 1f;

        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            imageCountdown.fillAmount = timeLeft / 12;
            textCountdown.text = Mathf.CeilToInt(timeLeft).ToString();
            yield return null;
        }

        // Hết thời gian
        countdownContainer.SetActive(false);
    }

    #region Buttons
    /// --------- PHASE BET ------------ ///
    public void OnClickButtonChip(int index)
    {

        currentBetValue = listValueChipBets[index];
        currentChipIndex = index;


        buttonRebet.SetActive(false);
        textDealValue.text = Utility.FormatMoney(currentBetValue, true);
        textClearValue.text = Utility.FormatMoney(currentBetValue, true);

        // Chip
        foreach (BlackjackChipBet chipBet in listChipBet)
        {
            chipBet.OnUnselect();
        }
        BlackjackChipBet selectedChipBet = listChipBet[index];
        selectedChipBet.OnSelect();
        BlackjackChip chip = chipPool.Get();
        chip.transform.SetParent(selectedChipBet.transform.parent, true);
        chip.SetInfo(index, selectedChipBet.transform.localPosition);
        chip.transform.localScale = Vector2.one * 0.8f;
        if (isRebet)
        {
            currentBetValue = 0;
            AnimateMoveChip(chip, currentPlayerBoxBet.transform.position, () =>
            {
                currentPlayerBoxBet.SetBetValue(index, totalBetValue, totalBetValue);
                OnClickButtonDeal();
            });
        }
        else
        {
            buttonDeal.SetActive(true);
            buttonClear.SetActive(true);
            AnimateMoveChip(chip, currentPlayerBoxBet.transform.position, () =>
            {
                currentPlayerBoxBet.SetBetValue(index, currentBetValue, totalBetValue + currentBetValue);
            });
        }

    }

    public void OnClickButtonDeal()
    {
        lastChipIndex = currentChipIndex;
        totalBetValue += currentBetValue;
        BlackjackBet bet = new BlackjackBet
        {
            Chips = totalBetValue,
            Code = BlackjackBetCode.BlackjackBetNormal
        };

        if (!isRebet)
        {
            DataSender.SendMatchState((long)OpCodeRequest.Bet, bet.ToByteArray());
            textDealValue.text = currentBetValue.ToString();
            textClearValue.text = currentBetValue.ToString();
            currentPlayerBoxBet.SetBetValue(lastChipIndex, totalBetValue, totalBetValue);
        }
        isRebet = false;
        currentBetValue = 0;

        buttonDoubleBet.SetActive(true);
        buttonDeal.SetActive(false);
        buttonClear.SetActive(false);

    }

    public void OnClickButtonClear()
    {
        currentBetValue = 0;
        textDealValue.text = currentBetValue.ToString();
        textClearValue.text = currentBetValue.ToString();

        buttonDeal.SetActive(false);
        buttonClear.SetActive(false);
        currentPlayerBoxBet.SetBetValue(lastChipIndex, totalBetValue, totalBetValue);

        BlackjackChip chip = chipPool.Get();
        chip.transform.SetParent(currentPlayerBoxBet.transform.parent, true);
        chip.SetInfo(currentChipIndex, currentPlayerBoxBet.transform.localPosition);
        chip.transform.localScale = Vector2.one * 0.8f;
        AnimateClearChip(chip);
    }

    public void OnClickButtonDoubleBet()
    {
        BlackjackBet bet = new BlackjackBet
        {
            Chips = totalBetValue,
            Code = BlackjackBetCode.BlackjackBetDouble
        };
        buttonDoubleBet.SetActive(false);
        totalBetValue *= 2;
        currentPlayerBoxBet.SetBetValue(lastChipIndex, totalBetValue, totalBetValue);

        DataSender.SendMatchState((long)OpCodeRequest.Bet, bet.ToByteArray());
    }

    public void OnClickButtonRebet()
    {
        isRebet = true;
        BlackjackBet bet = new BlackjackBet
        {
            // Chips = lastBetValue,
            Code = BlackjackBetCode.BlackjackBetRebet
        };
        buttonRebet.SetActive(false);
        buttonDeal.SetActive(false);
        buttonClear.SetActive(false);
        DataSender.SendMatchState((long)OpCodeRequest.Bet, bet.ToByteArray());
    }

    /// --------- PHASE PLAY ------------ ///

    public void OnClickButtonSplit()
    {
        if (isCurrentPlayerTurn)
        {
            HandleClickButtonAction(BlackjackActionCode.BlackjackActionSplit);
        }
        else
        {
            ResetAllButtonAction();
            buttonSplit.OnClickCheckBox();
            nextActionCode = BlackjackActionCode.BlackjackActionSplit;
        }
        BlackjackBoxBet boxBet = userIdToBoxBetView.GetValueOrDefault(User.userProfile.UserId);
        boxBet.SplitBoxBet();
    }

    public void OnClickButtonDouble()
    {
        if (isCurrentPlayerTurn)
        {
            HandleClickButtonAction(BlackjackActionCode.BlackjackActionDouble);
        }
        else
        {
            ResetAllButtonAction();
            buttonDouble.OnClickCheckBox();
            nextActionCode = BlackjackActionCode.BlackjackActionDouble;
        }
    }

    public void OnClickButtonHit()
    {
        if (isCurrentPlayerTurn)
        {
            HandleClickButtonAction(BlackjackActionCode.BlackjackActionHit);
        }
        else
        {
            ResetAllButtonAction();
            buttonHit.OnClickCheckBox();
            nextActionCode = BlackjackActionCode.BlackjackActionHit;
        }

    }

    public void OnClickButtonStand()
    {
        if (isCurrentPlayerTurn)
        {
            HandleClickButtonAction(BlackjackActionCode.BlackjackActionStay);
        }
        else
        {
            ResetAllButtonAction();
            buttonStand.OnClickCheckBox();
            nextActionCode = BlackjackActionCode.BlackjackActionStay;
        }
        buttonBetContainer.gameObject.SetActive(false);
    }

    public void OnClickMenuButton()
    {
        UIManager.Instance.OpenGroupMenu();
    }

    public override void OpenRule()
    {
        Instantiate(rulePrefab, transform);
    }

    private void HandleClickButtonAction(BlackjackActionCode code)
    {
        BlackjackAction blackjackAction = new()
        {
            Code = code
        };
        DataSender.SendMatchState((long)OpCodeRequest.DeclareCards, blackjackAction.ToByteArray());
    }

    private void UpdateButtonActionVisual(bool isCurrentPlayerTurn)
    {
        if (isCurrentPlayerTurn)
        {
            buttonDouble.HideCheckBox();
            buttonHit.HideCheckBox();
            buttonSplit.HideCheckBox();
            buttonStand.HideCheckBox();
            ResetAllButtonAction();
        }
        else
        {
            buttonDouble.ShowCheckBox();
            buttonHit.ShowCheckBox();
            buttonSplit.ShowCheckBox();
            buttonStand.ShowCheckBox();
        }

    }

    private void ResetAllButtonAction()
    {
        buttonDouble.Reset();
        buttonHit.Reset();
        buttonSplit.Reset();
        buttonStand.Reset();

    }
    #endregion

    #region Cards
    private IEnumerator DealCardsForPlayers()
    {
        int length = rearrangedPlayers.Count;
        for (int i = 0; i < length * 2; i++)
        {

            int indexInRound = i % length;
            int round = i / length; // vòng chia thứ mấy
            Player player = rearrangedPlayers[indexInRound];
            BlackjackBoxBet boxCard = userIdToBoxBetView.GetValueOrDefault(player.Id);
            if (boxCard.listCardModel.Count == 0) continue;
            CardModel cardModel = boxCard.listCardModel[round];
            AnimateDealACard(cardModel, boxCard.GetCardPosition.position, true, boxCard, boxCard.SpreadCards);

            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator DealCardsForBanker()
    {
        for (int i = 0; i < listBankerCard.Count; i++)
        {
            Card card = listBankerCard[i];
            CardModel cardModel = InitCard();
            cardModel.HideCard();
            // cardModel.transform.SetParent(dealCardContainer);
            cardModel.transform.SetParent(bankerCardsContainer);

            bool isShow = card.Rank != CardRank.RankUnspecified && card.Suit != CardSuit.SuitUnspecified;
            if (isShow)
            {
                cardModel.SetData((int)card.Rank, (int)card.Suit);
                cardModel.HideCard();
                AnimateDealACard(cardModel, bankerCardsContainer.position, true, null, bankerBoxBet.SpreadCards);
            }
            else
            {
                AnimateDealACard(cardModel, bankerCardsContainer.position, false, null, bankerBoxBet.SpreadCards);
            }
            bankerBoxBet.listCardModel.Add(cardModel);
            yield return new WaitForSeconds(0.2f);
        }
        if (bankerBoxBet.listCardModel[0].GetRank() == (int)CardRank.RankA)
        {
            bankerBoxBet.AnimateHighlightCards();
        }
    }

    #endregion

    #region Card Animations
    private void AnimateDealACard(CardModel cardModel, Vector2 targetPosition, bool isShow, BlackjackBoxBet boxBet = null, Action callback = null)
    {
        Sequence sequence = DOTween.Sequence();
        sequence
            .Join(cardModel.transform.DOMove(targetPosition, DEAL_CARD_ANIMATION_TIME).SetEase(Ease.InOutCubic))
            .Join(cardModel.transform.DOScale(0.48f, DEAL_CARD_ANIMATION_TIME))
            .AppendCallback(() =>
            {
                if (isShow)
                {
                    cardModel.transform.DOScale(new Vector2(0.01f, 0.5f), DEAL_CARD_ANIMATION_TIME / 2f).OnComplete(() =>
                    {
                        cardModel.ShowCard();
                        cardModel.transform.DOScale(0.5f, DEAL_CARD_ANIMATION_TIME / 2f).SetEase(Ease.InOutCubic);
                    });
                    Quaternion newRotation = Quaternion.Euler(0, 10, 0);
                    cardModel.transform.DOLocalRotate(newRotation.eulerAngles, DEAL_CARD_ANIMATION_TIME / 5).SetEase(Ease.InOutCubic).OnComplete(() =>
                    {
                        newRotation = Quaternion.Euler(0, -10, 0);
                        cardModel.transform.localRotation = newRotation;
                        cardModel.transform.DOLocalRotate(Vector3.zero, DEAL_CARD_ANIMATION_TIME * 4 / 5).SetEase(Ease.InOutCubic);
                    });
                }
                callback?.Invoke();

            });
    }

    private void AnimateFlipCard(CardModel cardModel)
    {
        cardModel.transform.DOScale(new Vector2(0.01f, 1f), DEAL_CARD_ANIMATION_TIME / 2f).OnComplete(() =>
        {
            cardModel.ShowCard();
            cardModel.transform.DOScale(0.5f, DEAL_CARD_ANIMATION_TIME / 2f).SetEase(Ease.InOutCubic);
        });
        Quaternion newRotation = Quaternion.Euler(0, 10, 0);
        cardModel.transform.DOLocalRotate(newRotation.eulerAngles, DEAL_CARD_ANIMATION_TIME / 5).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            newRotation = Quaternion.Euler(0, -10, 0);
            cardModel.transform.localRotation = newRotation;
            cardModel.transform.DOLocalRotate(Vector3.zero, DEAL_CARD_ANIMATION_TIME * 4 / 5).SetEase(Ease.InOutCubic);
        });
    }

    private void AnimateReturnAllCards()
    {
        Sequence seq = DOTween.Sequence();

        float delayBetweenCards = 0.15f;
        int cardIndex = 0;

        // Gom cả player và banker vào chung một list để xử lý
        List<BlackjackBoxBet> allBoxBets = new List<BlackjackBoxBet>();
        foreach (Player player in rearrangedPlayers)
        {
            allBoxBets.Add(userIdToBoxBetView.GetValueOrDefault(player.Id));
            allBoxBets.Add(userIdToBoxBetView.GetValueOrDefault(player.Id).SecondBoxBet);
        }
        allBoxBets.Add(bankerBoxBet);

        // --- FLIP CARDS ---
        foreach (BlackjackBoxBet boxBet in allBoxBets)
        {
            foreach (CardModel card in boxBet.listCardModel)
            {
                float startTime = cardIndex * delayBetweenCards;
                card.transform.SetSiblingIndex(50 + cardIndex);

                // Scale down (để lật)
                seq.Insert(startTime, card.transform.DOScaleX(0.01f, DEAL_CARD_ANIMATION_TIME / 2f));

                // Callback để đổi mặt lá bài
                seq.InsertCallback(startTime + DEAL_CARD_ANIMATION_TIME / 2f, () =>
                {
                    card.HideCard();
                });

                // Scale back
                seq.Insert(startTime + DEAL_CARD_ANIMATION_TIME / 2f,
                    card.transform.DOScale(0.5f, DEAL_CARD_ANIMATION_TIME / 2f).SetEase(Ease.InOutCubic));

                // Rotate effect
                seq.Insert(startTime,
                    card.transform.DOLocalRotate(new Vector3(0, 10, 0), DEAL_CARD_ANIMATION_TIME / 5).SetEase(Ease.InOutCubic));

                cardIndex++;
            }
        }

        // --- RETURN CARDS ---
        foreach (BlackjackBoxBet boxBet in allBoxBets)
        {
            foreach (CardModel card in boxBet.listCardModel)
            {
                float startTime = cardIndex * delayBetweenCards;

                // Đặt parent về return container
                seq.InsertCallback(startTime, () =>
                {
                    card.transform.SetParent(cardContainer, true);
                });

                // Move + Scale song song
                seq.Insert(startTime + 0.5f,
                    card.transform.DOLocalMove(returnCardContainer.localPosition, 0.5f)
                        .SetEase(Ease.OutCubic)
                        .OnComplete(() =>
                        {
                            cardPool.Release(card);
                        })
                );

                seq.Insert(startTime + 0.5f,
                    card.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InCubic) // scale về 0
                );

                cardIndex++;


            }
        }
        seq.OnComplete(() =>
        {
            foreach (Player player in rearrangedPlayers)
            {
                if (userIdToBoxBetView.TryGetValue(player.Id, out var boxBet))
                {
                    boxBet.Reset();
                }
            }
        });
    }
    #endregion

    #region Chip Animations
    private void AnimateMoveChip(BlackjackChip chip, Vector2 position, Action callback = null)
    {
        chip.transform
            .DOMove(position, 0.3f)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                // Vector2 randomPosition = new Vector2(
                //     targetPosition.x + UnityEngine.Random.Range(-30, 30),
                //     targetPosition.y + UnityEngine.Random.Range(-8, 8)
                // );

                // chip.transform.DOLocalJump(randomPosition, 20, 1, 0.2f);
                chipPool.Release(chip);
                callback?.Invoke();
            });
    }

    private void AnimateMoveInsuranceChip(BlackjackChip chip, Vector2 targetPosition)
    {
        chip.transform.SetParent(chipContainer, true);
        chip.transform
            .DOLocalMove(targetPosition, 0.3f)
            .SetEase(Ease.InSine);

    }

    private void AnimateClearChip(BlackjackChip chip)
    {
        if (!chip.gameObject.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            canvasGroup = chip.gameObject.AddComponent<CanvasGroup>();
        }

        chip.transform.DOScale(Vector3.one, 0.5f);
        chip.transform.DOLocalMove(new Vector3(0, 15, 0), 0.4f);
        canvasGroup.DOFade(0, 1f).OnComplete(() =>
        {
            canvasGroup.alpha = 1f;
            chipPool.Release(chip);
        });

    }

    private void ShowAllPlayersLoading()
    {
        foreach (Player player in rearrangedPlayers)
        {
            if (player.Id == thisPlayer.id) continue;
            if (userIdToBoxBetView.TryGetValue(player.Id, out var boxBet))
            {
                if (!boxBet.HasBet)
                {
                    boxBet.ShowAnimationWaiting();
                }
            }
        }
    }
    private void HideAllPlayersLoading()
    {
        foreach (Player player in rearrangedPlayers)
        {
            if (player.Id == thisPlayer.id) continue;
            if (userIdToBoxBetView.TryGetValue(player.Id, out var boxBet))
            {
                boxBet.HideAnimationWaiting();
            }
        }
    }
    #endregion

    #region Helpers
    private int GetChipIndex(int value)
    {
        for (int i = 0; i < listValueChipBets.Count; i++)
        {
            if (value <= listValueChipBets[i])
            {
                return i;
            }
        }
        return 4;
    }

    private void HideAllImageChip()
    {
        foreach (var boxBet in userIdToBoxBetView.Values)
        {
            boxBet.HideImageChip();
        }
    }

    private void CheckCurrentPlayerFinish(BlackjackUpdateDeal data)
    {
        var finalHandTypes = new HashSet<BlackjackHandType>
        {
            BlackjackHandType.Blackjack,
            BlackjackHandType.Busted,
            BlackjackHandType._21P
        };
        bool isFirstHandFinal = finalHandTypes.Contains(data.Hand.First.Type);
        bool isSecondHandOk = data.Hand.Second.Type == BlackjackHandType.Unspecified || finalHandTypes.Contains(data.Hand.Second.Type);
        if (data.UserId == User.userProfile.UserId && isFirstHandFinal && isSecondHandOk)
        {
            isCurrentPlayerFinished = true;
            buttonDouble.gameObject.SetActive(false);
            buttonSplit.gameObject.SetActive(false);
            buttonHit.gameObject.SetActive(false);
            buttonStand.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Setups

    protected override void CreatePlayerView(Player player, Vector2 anchoredPos)
    {
        base.CreatePlayerView(player, anchoredPos);
        string localUserId = User.userProfile.UserId;
        Debug.Log("CREATE PLAYER VIEW: " + player.UserName);
        if (!userIdToBoxBetView.TryGetValue(player.Id, out var boxBetView) || boxBetView == null)
        {
            int playerIndex = rearrangedPlayers.IndexOf(player);
            boxBetView = Instantiate(boxBetPrefab, boxBetContainer).GetComponent<BlackjackBoxBet>();
            userIdToBoxBetView[player.Id] = boxBetView;
            boxBetView.transform.localPosition = listBoxBetPosition[rearrangedPlayers.IndexOf(player)];
            boxBetView.gameObject.SetActive(true);
            boxBetView.SetInfo(playerIndex, listBoxBetPosition[playerIndex]);
            // boxBetView.SetInfo(this, 1);
        }
        if (userIdToBoxBetView.TryGetValue(localUserId, out BlackjackBoxBet boxBet))
        {
            currentPlayerBoxBet = boxBet;
        }
    }

    public CardModel InitCard()
    {
        CardModel cardModel = cardPool.Get();
        cardModel.transform.SetParent(dealCardContainer);
        cardModel.transform.localPosition = Vector3.zero;
        cardModel.transform.localScale = Vector3.zero;
        cardModel.gameObject.SetActive(true);
        return cardModel;
    }

    private void SetInfoBet()
    {
        listValueChipBets = new List<long> { MarkUnit, MarkUnit * 5, MarkUnit * 10, MarkUnit * 50, MarkUnit * 100 };
        for (int i = 0; i < 5; i++)
        {
            listChipBet[i].textValue.text = Utility.FormatMoney(listValueChipBets[i], true);
        }
    }

    private void InitPool()
    {
        cardPool = new UnityEngine.Pool.ObjectPool<CardModel>(
            createFunc: () =>
            {
                var card = Instantiate(cardPrefab, cardContainer).GetComponent<CardModel>();
                card.gameObject.SetActive(false);
                return card;
            },
            actionOnGet: (card) =>
            {
                card.gameObject.SetActive(true);
                card.transform.localScale = Vector3.one;
            },
            actionOnRelease: (card) =>
            {
                card.gameObject.SetActive(false);
            },
            actionOnDestroy: (card) =>
            {
                Destroy(card.gameObject);
            },
            collectionCheck: false,
            defaultCapacity: 20,
            maxSize: 50
        );
        chipPool = new UnityEngine.Pool.ObjectPool<BlackjackChip>(
            createFunc: () =>
            {
                var chip = Instantiate(chipPrefab, chipContainer).GetComponent<BlackjackChip>();
                chip.gameObject.SetActive(false);
                return chip;
            },
            actionOnGet: (chip) =>
            {
                chip.gameObject.SetActive(true);
                chip.transform.localScale = Vector3.one;
            },
            actionOnRelease: (chip) =>
            {
                chip.gameObject.SetActive(false);
            },
            actionOnDestroy: (chip) =>
            {
                Destroy(chip.gameObject);
            },
            collectionCheck: false,
            defaultCapacity: 20,
            maxSize: 50
        );
    }

    private void ResetGame()
    {
        lastBetValue = totalBetValue;
        currentBetValue = totalBetValue = playerReceiveCardCount = 0;
        currentChipIndex = -1;
        hasDealtCardsForBanker = hasDealtCardsForPlayers = isCurrentPlayerFinished = isRebet = false;

        listBankerCard.Clear();

        buttonBetContainer.gameObject.SetActive(false);
        boxBetContainer.gameObject.SetActive(true);
        buttonClear.SetActive(false);
        buttonDeal.SetActive(false);
        buttonDoubleBet.SetActive(false);
        buttonRebet.SetActive(lastBetValue > 0);

        bankerBoxBet.Reset();
        foreach (Player player in rearrangedPlayers)
        {
            if (userIdToBoxBetView.TryGetValue(player.Id, out var boxBet))
            {
                boxBet.Reset();
            }
        }

        foreach (Transform child in chipContainer)
        {
            if (child.TryGetComponent<BlackjackChip>(out var chip))
            {
                Destroy(chip.gameObject);
            }
        }

        foreach (BlackjackChipBet chipBet in listChipBet)
        {
            chipBet.OnUnselect();
        }

        // foreach (Transform child in chipContainer)
        // {
        //     if (child.gameObject.activeSelf)
        //     {
        //         chipPool.Release(child.GetComponent<BlackjackChip>());
        //     }
        // }
    }

    protected override void RemovePlayerBoxBet(string leavePlayerId)
    {
        base.RemovePlayerBoxBet(leavePlayerId);
        if (userIdToBoxBetView.TryGetValue(leavePlayerId, out var view))
        {
            Destroy(view.gameObject);
            userIdToBoxBetView.Remove(leavePlayerId);
        }
    }

    #endregion
}
