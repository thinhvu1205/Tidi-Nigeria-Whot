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
    [SerializeField] private BlackjackBetPhase phaseBet;
    [SerializeField] private BlackjackPlayPhase phasePlay;
    [SerializeField] private BlackjackInsurance insurance;
    [SerializeField] private BlackjackBoxBet bankerBoxBet;

    [Header(" Animations ")]
    [SerializeField] private SkeletonGraphic animationWinBlackjack;

    [Header(" Lists ")]
    [SerializeField] private Vector2[] listBoxBetPosition;
    [SerializeField] private BlackjackChipBet[] listChipBet;
    private Vector2[] listInsuranceChipPosition = new Vector2[]
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
    private List<Card> listCurrentPlayerCard = new();
    private int currentChipIndex = 0, lastChipIndex = -1;
    private BlackjackActionCode nextActionCode = BlackjackActionCode.BlackjackActionUnspecified; // 1: Split, 2: Double, 3: Hit, 4: Stand
    private long totalBetValue = 0, currentBetValue = 0, lastBetValue = 0;
    private bool hasDealtCardsForPlayers = false, hasDealtCardsForBanker = false, isCurrentPlayerTurn = false, isRebet = false;

    protected override void Awake()
    {
        base.Awake();
        insurance.SetInfo(this);
        phaseBet.gameObject.SetActive(false);
        phasePlay.gameObject.SetActive(false);
        PoolService.Instance.Register(PrefabType.Card, cardContainer, cardPrefab, 8, 10, 6);
        PoolService.Instance.Register(PrefabType.ChipPlayerBlackjack, chipContainer, chipPrefab, 20, 30, 15);

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
        UpdatePosUserTable(updateTable);
    }

    public override void HandleUpdateTable(IMatchState matchState)
    {
        BlackjackUpdateDesk data = BlackjackUpdateDesk.Parser.ParseFrom(matchState.State);
        Debug.Log("Update table: " + data.ToString());

        // Khi có người chơi đặt cược thì tiến hành trừ tiền của người chơi đó
        if (data.Bet?.Balance?.AmountChipAdd != 0)
        {
            if (data.Bet?.UserId != null && userIdToView.TryGetValue(data.Bet.UserId, out var playerView))
            {
                playerView.AnimateFlyMoney(data.Bet.Balance.AmountChipAdd);
                playerView.SetCurrentChip(data.Bet.Balance.AmountChipCurrent);

                // Rebet
                if (isRebet && data.Bet?.Balance.AmoutChipBet > 0)
                {
                    OnClickButtonChip(lastChipIndex);
                    isRebet = false;
                }
            }
        }

        // Cập nhật các nút hành động
        if (data.IsNewTurn)
        {
            isCurrentPlayerTurn = data.InTurn == User.userMain.userId;
            if (isCurrentPlayerTurn && nextActionCode != BlackjackActionCode.BlackjackActionUnspecified)
            {
                HandleClickButtonAction(nextActionCode);
            }
            if (userIdToView.TryGetValue(data.InTurn, out var view))
            {
                view.SetCurrentTurn(true, 10);
            }
            if (data.Actions != null)
            {
                buttonBetContainer.gameObject.SetActive(true);
                List<BlackjackActionCode> actions = data.Actions.Actions.ToList();
                buttonDouble.gameObject.SetActive(actions.Contains(BlackjackActionCode.BlackjackActionDouble));
                buttonSplit.gameObject.SetActive(actions.Contains(BlackjackActionCode.BlackjackActionSplit));
                buttonHit.gameObject.SetActive(actions.Contains(BlackjackActionCode.BlackjackActionHit));
                buttonStand.gameObject.SetActive(actions.Contains(BlackjackActionCode.BlackjackActionStay));
            }
            UpdateButtonActionVisual(isCurrentPlayerTurn);

        }

        // Nhà cái có 1 lá Át, hiện popup bảo hiểm
        if (data.IsInsuranceTurnEnter)
        {
            insurance.Show();
        }

        // Đặt cược bảo hiểm
        if (data.Bet != null && data.Bet.Insurance > 0)
        {
            BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
            chip.transform.SetParent(thisPlayer.transform, true);
            chip.SetInfo(5, thisPlayer.GetAvatarPosition(), data.Bet.Insurance);
            chip.transform.localScale = Vector2.one * 0.8f;
            AnimateMoveInsuranceChip(chip, listInsuranceChipPosition[0]);
            thisPlayer.AnimateFlyMoney(-data.Bet.Insurance);

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
                    CardModel newCardModel = InitCard(dealCardContainer);
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
                foreach(Card card in data.NewCards)
                {
                    CardModel cardModel = InitCard(dealCardContainer);
                    cardModel.SetData((int)card.Rank, (int)card.Suit);
                    cardModel.HideCard();
                    cardModel.transform.SetParent(boxBet.GetCardPosition);
                    boxBet.listCardModel.Add(cardModel);
                }

                // Chia bài cho player
                if (!hasDealtCardsForPlayers)
                {
                    currentPlayerBoxBet.HideImageChip();
                    StartCoroutine(DealCardsForPlayers());
                    hasDealtCardsForPlayers = true;
                }
                else
                {
                    CardModel newCardModel = boxBet.listCardModel[^1];
                    BlackjackHand playerHand = data.Hand.First;
                    // newCardModel.SetData((int)newCard.Rank, (int)newCard.Suit);
                    // newCardModel.transform.SetParent(boxBet.GetCardPosition);
                    // newCardModel.HideCard();
                    AnimateDealACard(newCardModel, boxBet.GetNewCardPosition(), true, null, () =>
                    {
                        boxBet.SpreadCards();
                    });
                }
                DOVirtual.DelayedCall(1.2f, () =>
                {
                    if (data.Hand.First != null)
                    {
                        boxBet.ShowScore(data.Hand.First.Point, data.Hand.First.MinPoint, data.Hand.First.MaxPoint, data.Hand.First.Type);
                    }
                    else
                    {
                        boxBet.ShowScore(0, 0, 0);
                    }
                });
            
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
                if (data.CountDown == 12)
                {
                    countdownContainer.SetActive(true);
                    phaseBet.gameObject.SetActive(true);
                    phasePlay.gameObject.SetActive(false);
                    ResetGame();
                    StartCountDownBetTime();
                }
                Debug.Log("HandleUpdateGameState Preparing " + data.ToString());
                break;
            case GameState.Play:
                currentPlayerBoxBet.SetBetValue(currentChipIndex, currentBetValue, totalBetValue);
                currentPlayerBoxBet.HideImageChip();
                countdownContainer.SetActive(false);
                phaseBet.gameObject.SetActive(false);
                phasePlay.gameObject.SetActive(true);

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
        DOVirtual.DelayedCall(2.5f, () =>
        {
            foreach (Player player in players)
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
                    boxBet.ShowResult(result.First.IsWin);
                    boxBet.HideScore();
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

    private void StartCountDownBetTime()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);
        countdownCoroutine = StartCoroutine(CountdownRoutine(12));
    }

    private IEnumerator CountdownRoutine(int startTime)
    {
        countdownContainer.SetActive(true);
        float timeLeft = startTime;
        imageCountdown.fillAmount = 1f;

        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            imageCountdown.fillAmount = timeLeft / startTime;
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
        buttonDeal.SetActive(true);
        buttonClear.SetActive(true);
        textDealValue.text = Utility.FormatMoney(currentBetValue, true);
        textClearValue.text = Utility.FormatMoney(currentBetValue, true);

        // Chip
        foreach (BlackjackChipBet chipBet in listChipBet)
        {
            chipBet.OnUnselect();
        }
        BlackjackChipBet selectedChipBet = listChipBet[index];
        selectedChipBet.OnSelect();

        BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
        chip.transform.SetParent(selectedChipBet.transform.parent, true);
        chip.SetInfo(index, selectedChipBet.transform.localPosition);
        chip.transform.localScale = Vector2.one * 0.8f;
        AnimateMoveChip(chip, () =>
        {
            currentPlayerBoxBet.SetBetValue(index, currentBetValue, totalBetValue + currentBetValue);
        });
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

        DataSender.SendMatchState((long)OpCodeRequest.Bet, bet.ToByteArray());
        currentBetValue = 0;
        textDealValue.text = currentBetValue.ToString();
        textClearValue.text = currentBetValue.ToString();
        currentPlayerBoxBet.SetBetValue(lastChipIndex, totalBetValue, totalBetValue);

        buttonDoubleBet.SetActive(true);
        buttonDeal.SetActive(false);
        buttonClear.SetActive(false);

    }

    public void OnClickButtonClear()
    {
        Debug.Log("ON CLICK CLEAR");
        currentBetValue = 0;
        textDealValue.text = currentBetValue.ToString();
        textClearValue.text = currentBetValue.ToString();

        buttonDeal.SetActive(false);
        buttonClear.SetActive(false);
        currentPlayerBoxBet.SetBetValue(lastChipIndex, totalBetValue, totalBetValue);

        BlackjackChip chip = PoolService.Instance.Get<BlackjackChip>(PrefabType.ChipPlayerBlackjack);
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

        totalBetValue *= 2;
        currentPlayerBoxBet.SetBetValue(lastChipIndex, totalBetValue, totalBetValue);

        DataSender.SendMatchState((long)OpCodeRequest.Bet, bet.ToByteArray());
    }

    public void OnClickButtonRebet()
    {
        isRebet = true;
        BlackjackBet bet = new BlackjackBet
        {
            Chips = lastBetValue,
            Code = BlackjackBetCode.BlackjackBetRebet
        };

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
        // BlackjackBoxBet boxBet = userIdToBoxBetView.GetValueOrDefault(User.userMain.userId);
        // boxBet.SplitBoxBet(0);
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
        int length = players.Count;
        for (int i = 0; i < length * 2; i++)
        {

            int indexInRound = i % length; // 0..playerCount và banker cuối cùng
            int round = i / length; // vòng chia thứ mấy
            Player player = players[indexInRound];
            BlackjackBoxBet boxCard = userIdToBoxBetView.GetValueOrDefault(player.Id);
            CardModel cardModel = boxCard.listCardModel[round];

            if (player.Id == thisPlayer.id)
            {
                AnimateDealACard(cardModel, boxCard.GetCardPosition.position, true, boxCard, boxCard.SpreadCards);
            }
            else
            {
                AnimateDealACard(cardModel, boxCard.GetCardPosition.position, false, boxCard, boxCard.SpreadCards);
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator DealCardsForBanker()
    {
        for (int i = 0; i < listBankerCard.Count; i++)
        {
            Card card = listBankerCard[i];
            CardModel cardModel = InitCard(dealCardContainer);
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
        foreach (Player player in players)
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
                    card.HideCard(); // bạn có thể check isFlipUp nếu muốn
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
                            PoolService.Instance.Release(PrefabType.Card, card);
                        })
                );

                seq.Insert(startTime + 0.5f,
                    card.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InCubic) // scale về 0
                );

                cardIndex++;
            }
        }
    
    }
    #endregion

    #region Chip Animations
    private void AnimateMoveChip(BlackjackChip chip, Action callback = null)
    {
        chip.transform
            .DOMove(currentPlayerBoxBet.transform.position, 0.3f)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                // Vector2 randomPosition = new Vector2(
                //     targetPosition.x + UnityEngine.Random.Range(-30, 30),
                //     targetPosition.y + UnityEngine.Random.Range(-8, 8)
                // );

                // chip.transform.DOLocalJump(randomPosition, 20, 1, 0.2f);
                PoolService.Instance.Release(PrefabType.ChipPlayerBlackjack, chip);
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
            PoolService.Instance.Release(PrefabType.ChipPlayerBlackjack, chip);
        });

    }
    #endregion

    #region Setups

    protected override void CreatePlayerView(Player player, Vector2 anchoredPos)
    {
        base.CreatePlayerView(player, anchoredPos);
        string localUserId = User.userMain.userId;

        if (!userIdToBoxBetView.TryGetValue(player.Id, out var boxBetView) || boxBetView == null)
        {
            boxBetView = Instantiate(boxBetPrefab, boxBetContainer).GetComponent<BlackjackBoxBet>();
            userIdToBoxBetView[player.Id] = boxBetView;
            boxBetView.transform.localPosition = listBoxBetPosition[0];
            boxBetView.gameObject.SetActive(true);
        }
        if (userIdToBoxBetView.TryGetValue(localUserId, out BlackjackBoxBet boxBet))
        {
            currentPlayerBoxBet = boxBet;
        }
    }

    public CardModel InitCard(Transform initTransform)
    {
        CardModel cardModel = PoolService.Instance.Get<CardModel>(PrefabType.Card);
        cardModel.transform.SetParent(initTransform);
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

    private void ResetGame()
    {
        Debug.Log("TOTAL BET VALUE: " + totalBetValue);
        lastBetValue = totalBetValue;
        Debug.Log("LAST BET VALUE: " + lastBetValue);
        currentBetValue = totalBetValue = 0;
        currentChipIndex = -1;
        hasDealtCardsForBanker = hasDealtCardsForPlayers = false;

        listCurrentPlayerCard.Clear();
        listBankerCard.Clear();

        buttonBetContainer.gameObject.SetActive(false);
        boxBetContainer.gameObject.SetActive(true);
        buttonClear.SetActive(false);
        buttonDeal.SetActive(false);
        buttonDoubleBet.SetActive(false);
        buttonRebet.SetActive(lastBetValue > 0);

        bankerBoxBet.Reset();
        foreach (Player player in players)
        {
            if (userIdToBoxBetView.TryGetValue(player.Id, out var boxBet))
            {
                boxBet.Reset();
            }
        }

        foreach (Transform child in chipContainer)
        {
            if (child.gameObject.activeSelf)
            {
                PoolService.Instance.Release(PrefabType.ChipPlayerBlackjack, child.GetComponent<BlackjackChip>());
            }
        }
    }

    #endregion
}
