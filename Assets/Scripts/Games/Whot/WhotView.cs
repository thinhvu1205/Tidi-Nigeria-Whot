using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Api;
using DG.Tweening;
using Globals;
using Google.Protobuf;
using Nakama;
using Newtonsoft.Json;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameState = Api.GameState;

public class WhotView : GameView
{
    public event Action<OnNextTurnEventArg> OnNextTurn;
    public class OnNextTurnEventArg : EventArgs
    {
        public string playerTurn;
        public WhotCard callCard;
        public int countdown;
        public CardEffect cardEffect;
    }
    [SerializeField] private GameObject whotPlayerPrefab, cardPrefab;
    [SerializeField] private List<Transform> playerPositionsList;
    [SerializeField] private SkeletonGraphic betterLuckNextTimeAnimation, victoryAnimation, matchSymbolAnimation, effectAnimation, lastCardAnimation;
    [SerializeField] private Transform matchResultTransform, yourTurnTransform, suitPickerTransform, effectAnimationParent,
    victoryAnimationParent, loseAnimationParent, matchSymbolAnimationParent, lastCardAnimationParent, deckOfCardParent,
    callCardParent, playAreaParent, playersParent, countdownTransform;
    [SerializeField] private TextMeshProUGUI betText, cardsLeftText, betterLuckNextTimeText, yourTurnText, countdownText;
    [SerializeField] private Image waitImage, victoryImage, deckHighlightImage;
    [SerializeField] WhotSuitPicker suitPicker;

    private readonly List<int[]> spawnOrders = new()
    {
        new int[] { 0 },                       // 1 player 
        new int[] { 0, 2 },                    // 2 players
        new int[] { 0, 1, 3 },                 // 3 players
        new int[] { 0, 1, 2, 3 }               // 4 players
    };
    private const float ANIMATION_TIME = 0.5f;
    private const float ANIMATION_WAIT_ROTATION_SPEED = 270f;
    private const string
        MATCH_SYMBOL_SQUARE_ANIMATION_NAME = "vuong",
        MATCH_SYMBOL_CROSS_ANIMATION_NAME = "thap",
        MATCH_SYMBOL_TRIANGLE_ANIMATION_NAME = "tamgiac",
        MATCH_SYMBOL_CIRCLE_ANIMATION_NAME = "tron",
        MATCH_SYMBOL_STAR_ANIMATION_NAME = "sao",
        EFFECT_HOLD_ON_ANIMATION_NAME = "hold",
        EFFECT_GENERAL_MARKET_ANIMATION_NAME = "general",
        LAST_CARD_EFFECT_ANIMATION_NAME = "1",
        LOSE_ANIMATION_NAME = "better",
        VICTORY_ANIMATION_NAME = "victory";
    public WhotCard CallCard { get; private set; }
    private List<WhotPlayer> playersList;
    private List<WhotCard> initialCardsList;
    private List<BalanceUpdate> balanceUpdates = new();
    private GameState gameState = GameState.Preparing;
    private CardEffect currentEffect = CardEffect.EffectNone;
    private WhotPlayerHand playerHand;
    private Tween waitRotationTween;
    private bool hasDealtCards = false;
    private bool isAutoPlay = false;
    private bool isRejoinTable = false;

    protected override void Awake()
    {
        base.Awake();
        WhotHandler whotHandler = new(this);
        GameManager.Instance.SetGameHandler(whotHandler);

        HandleResetGame();
    }

    private void Update()
    {
        // Khi chạm vào màn hình thì gửi lên trạng thái active để server ko kick người chơi ra khỏi bàn
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            DataSender.SendMatchState((long)OpCodeRequest.OpcodeUserInteractCards, new byte[0]);
        }   
    }

    public void HandleResetGame()
    {
        DOTween.KillAll(true);
        playersList = new();
        initialCardsList = new();
        playerHand = GetComponent<WhotPlayerHand>();
        suitPicker.OnSuitPicked += WhotSuitPicker_OnSuitPicked;
        // playAreaParent.gameObject.SetActive(false);
        // playersParent.gameObject.SetActive(false);
        suitPicker.gameObject.SetActive(false);
        deckHighlightImage.gameObject.SetActive(false);
        matchResultTransform.gameObject.SetActive(false);
        countdownTransform.gameObject.SetActive(false);
        hasDealtCards = false;
        isRejoinTable = false;
        CallCard = null;
        foreach (Transform child in callCardParent)
        {
            Destroy(child.gameObject);
        }
        // PrepareMockData();
        // StartCoroutine(DealCards());
    }

    #region API Handlers
    public void HandleJoinMatch(IMatch match)
    {
        playersParent.gameObject.SetActive(true);
        string labelJson = match.Label;
        Match data = JsonConvert.DeserializeObject<Match>(labelJson);
        UpdateBetText(data.Bet.MarkUnit);
    }

    // Khi có người chơi join hoặc leave
    public void HandleUpdateTable(UpdateTable data)
    {
        playersParent.gameObject.SetActive(true);
        gameState = data.GameState;
        List<Player> players = data.Players.ToList();
        List<Player> playingPlayers = data.PlayingPlayers.ToList();
        List<Player> joinPlayers = data.JoinPlayers.ToList();
        List<Player> leavePlayers = data.LeavePlayers.ToList();
        string currentPlayerId = User.userMain.userId;
        Player currentPlayer = players.Find((player) => player.Id == currentPlayerId);
        int startIndex = players.IndexOf(currentPlayer);

        isRejoinTable =
            (!(new GameState[] { GameState.Preparing, GameState.Idle, GameState.Matching }).Contains(gameState))
            && joinPlayers.Find((player) => player.Id == currentPlayerId) != null;
        Debug.Log("Is rejoin table: " + isRejoinTable);
        // Order lại List Player sao cho currentPlayer luôn ở đầu
        if (startIndex >= 0)
        {
            List<Player> reordered = new();

            for (int i = 0; i < players.Count; i++)
            {
                int index = (startIndex + i) % players.Count;
                reordered.Add(players[index]);
            }
            players = reordered;
        }

        // Khởi tạo List Player lần đầu
        if (playersList.Count == 0)
        {
            for (int i = 0; i < players.Count; i++)
            {
                Debug.Log("Instantiating player: " + players[i].UserName);
                Player player = players[i];
                int spawnIndex = spawnOrders[players.Count - 1][i];
                WhotPlayer whotPlayer = Instantiate(whotPlayerPrefab, playerPositionsList[spawnIndex]).GetComponent<WhotPlayer>();
                whotPlayer.SetPlayerInfo(
                    player.Id,
                    player.AvatarId,
                    player.UserName,
                    player.Wallet
                );
                whotPlayer.transform.localPosition = Vector3.zero;
                whotPlayer.SetWhotGame(this);
                whotPlayer.HideCardsLeft();

                if (player.Id == currentPlayerId)
                {
                    whotPlayer.isCurrentPlayer = true;
                }
                else
                {
                    whotPlayer.isCurrentPlayer = false;
                }
                if (playingPlayers.Find(playingPlayer => playingPlayer.Id == player.Id) != null)
                {
                    whotPlayer.isPlaying = true;
                }
                else
                {
                    whotPlayer.isPlaying = false;
                    whotPlayer.HideCardsLeft();
                }
                playersList.Add(whotPlayer);
            }
            AdjustPlayerLayout();
        }
        else if (joinPlayers.Count > 0)
        {
            // Có người chơi mới join vào
            for (int i = 0; i < joinPlayers.Count; i++)
            {
                Player joinPlayer = joinPlayers[i];
                Player player = players.Find((p) => p.Id == joinPlayer.Id);
                if (playersList.Find((p) => p.playerId == player.Id) == null)
                {
                    Debug.Log("Joining player: " + joinPlayers[i].UserName);

                    // Nếu người chơi là người chơi mới vào thì cho vào slot còn trống 
                    // int spawnIndex = spawnOrders[players.Count - 1][players.Count - 1];
                    int index;
                    WhotPlayer whotPlayer = Instantiate(whotPlayerPrefab, GetEmptyPlayerSlot(out index)).GetComponent<WhotPlayer>();
                    whotPlayer.SetPlayerInfo(
                        player.Id,
                        player.AvatarId,
                        player.UserName,
                        player.Wallet
                    );
                    whotPlayer.transform.localPosition = Vector3.zero;
                    whotPlayer.SetWhotGame(this);
                    whotPlayer.HideCardsLeft();
                    if (player.Id == currentPlayerId)
                    {
                        whotPlayer.isCurrentPlayer = true;
                    }
                    else
                    {
                        whotPlayer.isCurrentPlayer = false;
                    }
                    if (playingPlayers.Find(playingPlayer => playingPlayer.Id == player.Id) != null)
                    {
                        whotPlayer.isPlaying = true;
                    }
                    else
                    {
                        whotPlayer.isPlaying = false;
                        whotPlayer.HideCardsLeft();
                    }
                    playersList.Insert(index, whotPlayer);
                }
            }

        }
        else if (leavePlayers.Count > 0)
        {
            // Có người chơi rời khỏi bàn
            foreach (Player leavePlayer in leavePlayers)
            {
                WhotPlayer whotPlayer = playersList.Find((p) => p.playerId == leavePlayer.Id);
                int index = playersList.IndexOf(whotPlayer);
                if (whotPlayer != null)
                {
                    Debug.Log("Removing player: " + whotPlayer.GetPlayerName());
                    foreach (Transform child in playerPositionsList[spawnOrders[playersList.Count - 1][index]])
                    {
                        Destroy(child.gameObject);
                    }
                    playersList.Remove(whotPlayer);
                }
            }
        }
    }

    // Gọi khi bài trên tay của người chơi hiện tại thay đổi
    public void HandleUpdateDeal(UpdateDeal data)
    {
        playAreaParent.gameObject.SetActive(true);
        List<Card> presenceCard = data.PresenceCard.Cards.ToList();

        if (isRejoinTable)
        {
            foreach(Card card in presenceCard)
            {
                WhotCard whotCard = Instantiate(cardPrefab, playerHand.GetCardsParent()).GetComponent<WhotCard>();
                whotCard.transform.localPosition = Vector3.zero;
                whotCard.transform.localScale = Vector3.one;
                whotCard.SetInfo(card.Suit, card.Rank);
                whotCard.OnCardSelected += playerHand.WhotCard_OnCardSelected;
                playerHand.cardsInHand.Add(whotCard);
            }
            playerHand.SortCards();
            playerHand.SpreadCards();
            Card callCard = data.TopCard;
            CallCard = Instantiate(cardPrefab).GetComponent<WhotCard>();
            CallCard.SetInfo(callCard.Suit, callCard.Rank);
            AnimateChooseCallCard();
            isRejoinTable = false;
            hasDealtCards = true;
            return;
        }
        if (!hasDealtCards)
            {
                Debug.Log("DEALING CARDS");
                // Lần đầu thì chia bài
                Card callCard = data.TopCard;
                CallCard = Instantiate(cardPrefab).GetComponent<WhotCard>();
                CallCard.SetInfo(callCard.Suit, callCard.Rank);
                foreach (Card card in presenceCard)
                {
                    WhotCard whotCard = Instantiate(cardPrefab).GetComponent<WhotCard>();
                    whotCard.SetInfo(card.Suit, card.Rank);
                    initialCardsList.Add(whotCard);
                }
                StartCoroutine(DealCards());
            }
            else
            {
                // Ko phải lần đầu thì là bốc bài, lá bài mới sẽ là các phần tử cuối cùng của mảng
                int diff = presenceCard.Count - playerHand.cardsInHand.Count;
                if (diff > 0)
                {
                    List<Card> newCards = presenceCard.Skip(presenceCard.Count - diff).ToList();
                    Sequence sequence = DOTween.Sequence();
                    foreach (Card drawnCard in newCards)
                    {
                        sequence
                            .AppendCallback(() =>
                            {
                                WhotCard card = Instantiate(cardPrefab).GetComponent<WhotCard>();
                                card.SetInfo(drawnCard.Suit, drawnCard.Rank);
                                DrawACard(card, playerHand.GetCardsParent(), true);
                            })
                            .AppendInterval(0.2f);
                    }
                }
            }
    }

    public void HandleUpdateGameState(UpdateGameState data)
    {

        gameState = data.State;

        switch (gameState)
        {
            case GameState.Preparing:

                // HandleResetGame();
                countdownTransform.gameObject.SetActive(true);
                countdownText.text = data.CountDown.ToString();
                foreach (WhotPlayer player in playersList)
                {
                    player.isPlaying = true;
                    player.Reset();
                }
                hasDealtCards = false;
                playerHand.Reset();
                break;
            case GameState.Play:
                countdownTransform.gameObject.SetActive(false);
                SetActivePlayArea();
                break;
            case GameState.Matching:
                break;
            case GameState.Idle:
                break;
            case GameState.Reward:
                break;
            case GameState.Finish:
                // Handle finish logic
                break;
            default:
                Debug.LogWarning("Unhandled game state: " + gameState);
                break;
        }
    }

    public void HandleUpdateTurn(UpdateTurn data)
    {
        Debug.Log("Handling update turn for player: " + data.UserId);
        Debug.Log("Current player: " + GetCurrentPlayer().playerId);
        if (data.UserId == GetCurrentPlayer().playerId)
        {
            ShowYourTurn();
            AnimateHighlightDeck();
        }
        else
        {
            HideYourTurn();
            playerHand.EndTurn();
        }
        OnNextTurn?.Invoke(new OnNextTurnEventArg
        {
            playerTurn = data.UserId,
            callCard = CallCard,
            countdown = (int)data.Countdown,
            cardEffect = currentEffect
        });
    }

    public void HandleUpdateCardState(UpdateCardState data)
    {
        playAreaParent.gameObject.SetActive(true);

        currentEffect = data.Effect;
        isAutoPlay = data.IsAutoPlay;
        List<KeyValuePair<string, int>> playerCardsCount = data.PlayerCardCounts.ToList();
        switch (data.Event)
        {
            case CardEvent.Play:
                HandleCardEffect(data);

                if (data.UserId == GetCurrentPlayer().playerId)
                {
                    // Khi người chơi hiện tại đánh 1 lá bài
                    WhotCard playedCard = playerHand.cardsInHand.Find(card =>
                    {
                        bool sameRank = card.GetCardRank() == data.TopCard.Rank;
                        bool isWhot = data.TopCard.Rank == CardRank.Rank20;
                        bool sameSuit = card.GetCardSuit() == data.TopCard.Suit;

                        if (isAutoPlay)
                        {
                            return sameRank && (isWhot || sameSuit);
                        }
                        else
                        {
                            return sameRank && (isWhot || sameSuit) && card.GetIsSelected();
                        }
                    });
                    // Nếu TopCard.Rank == 20, thì chỉ cần check Rank, bỏ qua Suit. Nếu TopCard.Rank != 20, thì phải match cả Rank và Suit.
                    if (playedCard == null) return;
                    playerHand.PlayACard(playedCard);
                }
                else
                {
                    WhotPlayer player = playersList.Find(p => p.playerId == data.UserId);
                    if (!Instantiate(cardPrefab, player.GetPlayedCardParent()).TryGetComponent<WhotCard>(out var playedCard)) return;
                    playedCard.SetInfo(data.TopCard.Suit, data.TopCard.Rank);
                    if (data.Effect == CardEffect.ChoiceShapeGhost)
                    {
                        // Khi người chơi khác chọn chất mới
                        CallCard = playedCard;
                        Destroy(playedCard.gameObject);
                    }
                    else
                    {
                        // Khi người chơi khác đánh 1 lá bài
                        player.PlayACard(playedCard);
                    }
                }
                if (data.Effect != CardEffect.ChoiceShapeGhost)
                {
                    UpdateCardsCount(data.DeckCount, playerCardsCount);
                }
                break;
            case CardEvent.Draw:
                WhotCard drawnCard = Instantiate(cardPrefab).GetComponent<WhotCard>();
                drawnCard.SetInfo(data.TopCard.Suit, data.TopCard.Rank);
                if (data.UserId != GetCurrentPlayer().playerId)
                {
                    // Khi người chơi khác rút bài
                    WhotPlayer player = GetPlayerByID(data.UserId);
                    // player.pickPenalty = data.PickPenalty;
                    if (data.PickPenalty > 0)
                    {
                        // Khi người chơi khác rút bài do bị phạt +2, +3
                        player.AnimatePlusText(data.PickPenalty);
                        AnimateDrawMultipleCards(player, data.PickPenalty, "", drawnCard);
                    }
                    else
                    {
                        // Khi người chơi khác rút bài bình thường
                        AnimateDrawMultipleCards(player, 1, "", drawnCard);
                    }
                }
                UpdateCardsCount(data.DeckCount, playerCardsCount);
                break;
            default:
                // Khi vào bàn đang dang chơi dở 
                if (data.TopCard == null) return;
                CallCard = Instantiate(cardPrefab).GetComponent<WhotCard>();
                CallCard.SetInfo(data.TopCard.Suit, data.TopCard.Rank);
                AnimateChooseCallCard();
                UpdateCardsCount(data.DeckCount, playerCardsCount);
                break;
        }
    }

    public void HandleUpdateWallet(BalanceResult data)
    {
        balanceUpdates = data.Updates.ToList();
    }

    public void HandleUpdateFinish(UpdateFinish data)
    {
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendInterval(1.2f)
            .AppendCallback(() =>
            {
                playAreaParent.gameObject.SetActive(false);
                HideYourTurn();
                foreach (WhotPlayer player in playersList)
                {
                    if (!player.isPlaying) continue;
                    player.StopCountDown();
                }
                AnimateShowResult(data.Results.ToList());
            });
    }
    #endregion

    #region UI 
    public void SetActivePlayersParent()
    {
        playersParent.gameObject.SetActive(true);
    }

    public void SetActivePlayArea()
    {
        playAreaParent.gameObject.SetActive(true);
    }
    private void UpdateCardsCount(int deckCount, List<KeyValuePair<string, int>> playerCardsCount)
    {
        cardsLeftText.text = deckCount.ToString();
        foreach (var pair in playerCardsCount)
        {
            WhotPlayer player = playersList.Find(p => p.playerId == pair.Key);
            if (player != null)
            {
                player.UpdateCardsLeftVisual(pair.Value);
            }
        }
    }

    private void ShowYourTurn()
    {
        yourTurnTransform.gameObject.SetActive(true);

    }

    private void HideYourTurn()
    {
        UpdateYourTurnText("Your turn");
        yourTurnTransform.gameObject.SetActive(false);
    }

    private void UpdateBetText(float bet)
    {
        betText.text = "Bet: " + bet.ToString();
    }

    private void UpdateYourTurnText(string text)
    {
        yourTurnText.text = text;
    }
    #endregion

    #region Card Actions
    public void OnDrawACard()
    {
        if (!yourTurnTransform.gameObject.activeSelf) return;
        DataSender.SendMatchState((long)OpCodeRequest.DrawCard, new byte[0]);
        playerHand.EndTurn();
    }

    public IEnumerator DealCards()
    {
        Debug.Log("TAI SAO KO CHIA BAI??");
        int length = playersList.Count;
        int cardsPerPlayer = initialCardsList.Count;
        for (int i = 0; i < length * cardsPerPlayer; i++)
        {
            int index = i % length;
            WhotPlayer player = playersList[index];
            if (!player.isPlaying) continue;
            yield return new WaitForSeconds(0.6f / (length * cardsPerPlayer));

            GameObject card = Instantiate(cardPrefab, GetDeckOfCardParent());
            WhotCard whotCard = card.GetComponent<WhotCard>();
            whotCard.SetFaceDown();
            if (player.isCurrentPlayer)
            {
                whotCard.transform.SetParent(playerHand.GetCardsParent());
                AnimateDealACard(whotCard, playerHand.GetCardsParent(), player, i / length);
            }
            else
            {
                AnimateDealACard(whotCard, player.GetDealedCardParent(), player, i / length);
            }
        }
        AnimateChooseCallCard();
    }

    public void DrawACard(WhotCard card, Transform targetPosition, bool isCurrentPlayer)
    {
        Debug.Log("Drawing a card: " + card.GetCardRank() + " of " + card.GetCardSuit() + " for " + (isCurrentPlayer ? "current player" : "other player"));
        GameObject cardInstance = Instantiate(cardPrefab, GetDeckOfCardParent());
        cardInstance.transform.localPosition = Vector3.zero;
        cardInstance.transform.localScale = Vector3.one;
        cardInstance.transform.SetParent(targetPosition);
        WhotCard whotCard = cardInstance.GetComponent<WhotCard>();
        whotCard.SetFaceDown();
        whotCard.SetInfo(card.GetCardSuit(), card.GetCardRank());
        if (isCurrentPlayer)
        {
            whotCard.OnCardSelected += playerHand.WhotCard_OnCardSelected;
            AnimateCurrentPlayerDrawACard(whotCard);
            playerHand.cardsInHand.Add(whotCard);
        }
        else
        {
            AnimateOtherPlayerDrawACard(whotCard, targetPosition);
        }
        Destroy(card.gameObject);
    }

    public void PlayACard(WhotCard card, Transform startingPosition)
    {
        GameObject cardInstance = Instantiate(cardPrefab, startingPosition);
        cardInstance.transform.localPosition = Vector3.zero;
        cardInstance.transform.localScale = Vector3.one;
        cardInstance.transform.SetParent(callCardParent);
        WhotCard whotCard = cardInstance.GetComponent<WhotCard>();
        whotCard.SetInfo(card.GetCardSuit(), card.GetCardRank());
        whotCard.SetSelectable(false);
        CallCard = whotCard;
        AnimatePlayACard(whotCard);
        Destroy(card.gameObject);
    }
    #endregion

    #region Card Effects
    private void HandleCardEffect(UpdateCardState data)
    {
        Debug.Log("Handling card effect: " + data.Effect);
        WhotPlayer player = playersList.Find(p => p.playerId == data.UserId);
        WhotPlayer targetPlayer = playersList.Find(p => p.playerId == data.TargetUserId);
        bool isCurrentPlayer = player.isCurrentPlayer;
        switch (currentEffect)
        {
            case CardEffect.Whot when isCurrentPlayer:
                AnimateOpenSuitPicker();
                break;
            case CardEffect.Whot when !isCurrentPlayer:
                AnimateWait();
                break;
            case CardEffect.ChoiceShapeGhost:
                OnSuitPicked(data.TopCard.Suit);
                break;
            case CardEffect.HoldOn:
                HandleHoldOnEffect(player);
                break;
            case CardEffect.PickTwo:
                HandlePick2Effect(targetPlayer);
                if (targetPlayer.playerId == GetCurrentPlayer().playerId)
                {
                    UpdateYourTurnText("Pick 2");
                }   
                break;
            case CardEffect.PickThree:
                HandlePick3Effect(targetPlayer);
                if (targetPlayer.playerId == GetCurrentPlayer().playerId)
                {
                    UpdateYourTurnText("Pick 3");
                }
                break;
            case CardEffect.Suspension:
                HandleSuspensionEffect(targetPlayer);
                break;
            case CardEffect.GeneralMarket:
                HandleGeneralMarketEffect(player);
                break;
            default:
                break;
        }
    }

    private void HandleHoldOnEffect(WhotPlayer activePlayer)
    {
        foreach (WhotPlayer player in playersList)
        {
            if (player.playerId != activePlayer.playerId && player.isPlaying)
            {
                player.AnimateShowHoldOn();
                if (!player.isCurrentPlayer)
                {
                    player.UpdateEffectNoti("Hold On");
                }
            }
        }
        AnimateHoldOn();
    }

    private void HandlePick2Effect(WhotPlayer targetPlayer)
    {
        AnimateDrawMultipleCards(targetPlayer, 2, "Pick 2");
    }

    private void HandlePick3Effect(WhotPlayer targetPlayer)
    {
        AnimateDrawMultipleCards(targetPlayer, 3, "Pick 3");
    }

    private void HandleSuspensionEffect(WhotPlayer targetPlayer)
    {
        targetPlayer.AnimateShowSuspension();
        targetPlayer.UpdateEffectNoti("Suspension");
    }
    private void HandleGeneralMarketEffect(WhotPlayer activePlayer)
    {
        foreach (WhotPlayer player in playersList)
        {
            if (player.playerId != activePlayer.playerId && player.isPlaying)
            {
                player.AnimateShowSuspension();
                player.AnimatePlusText(1);
                AnimateDrawMultipleCards(player, 1);
            }
        }
        AnimateGeneralMarket();
    }
    #endregion

    #region Animations
    private void AnimateChooseCallCard()
    {
        WhotCard newCallCard = Instantiate(cardPrefab, GetDeckOfCardParent()).GetComponent<WhotCard>();
        newCallCard.transform.localPosition = Vector3.zero;
        newCallCard.transform.localScale = Vector3.one;
        newCallCard.SetInfo(CallCard.GetCardSuit(), CallCard.GetCardRank());
        newCallCard.transform.DOScale(new Vector2(0.01f, 1f), ANIMATION_TIME / 2f).OnComplete(() =>
        {
            newCallCard.SetFaceUp();
            newCallCard.transform.DOScale(1f, ANIMATION_TIME / 2f).SetEase(Ease.InOutCubic);
        });
        Quaternion newRotation = Quaternion.Euler(0, 10, 0);
        newCallCard.transform.DOLocalRotate(newRotation.eulerAngles, ANIMATION_TIME / 5).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            newRotation = Quaternion.Euler(0, -10, 0);
            newCallCard.transform.localRotation = newRotation;
            newCallCard.transform.DOLocalRotate(Vector3.zero, ANIMATION_TIME * 4 / 5).SetEase(Ease.InOutCubic);
        });
        newCallCard.transform.DOMove(GetCallCardParent().position, ANIMATION_TIME).SetEase(Ease.InOutCubic);
    }

    private void AnimateDrawMultipleCards(WhotPlayer targetPlayer, int numberOfCards, string effectName = "", WhotCard drawnCard = null)
    {
        Debug.Log("Animating draw " + numberOfCards + " cards for player: " + targetPlayer.GetPlayerName());
        if (effectName != "" && targetPlayer.playerId != GetCurrentPlayer().playerId)
        {
            targetPlayer.UpdateEffectNoti(effectName);
        }   
        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < numberOfCards; i++)
        {
            sequence
                .AppendCallback(() =>
                {
                    DrawACard(drawnCard, targetPlayer.GetDealedCardParent(), false);
                })
                .AppendInterval(0.35f);
        }
    }
    private void AnimateGeneralMarket()
    {
        effectAnimationParent.gameObject.SetActive(true);
        Utility.PlayAnimation(effectAnimation, EFFECT_GENERAL_MARKET_ANIMATION_NAME, false);
        effectAnimation.AnimationState.Complete += delegate
        {
            effectAnimationParent.gameObject.SetActive(false);
        };
    }

    private void AnimateHoldOn()
    {
        effectAnimationParent.gameObject.SetActive(true);
        Utility.PlayAnimation(effectAnimation, EFFECT_HOLD_ON_ANIMATION_NAME, false);
        effectAnimation.AnimationState.Complete += delegate
        {
            effectAnimationParent.gameObject.SetActive(false);
        };
    }

    private void AnimateWait()
    {
        waitImage.gameObject.SetActive(true);
        float duration = 360f / ANIMATION_WAIT_ROTATION_SPEED; 

        waitRotationTween = waitImage.transform
            .DORotate(new Vector3(0, 0, -360), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    public void StopWaitAnimation()
    {
        if (waitRotationTween != null && waitRotationTween.IsActive())
        {
            waitRotationTween.Kill();
            waitImage.gameObject.SetActive(false);
        }
    }

    public void AnimateLastCardEffect()
    {
        lastCardAnimationParent.gameObject.SetActive(true);
        Utility.PlayAnimation(lastCardAnimation, LAST_CARD_EFFECT_ANIMATION_NAME, false);
        lastCardAnimation.AnimationState.Complete += delegate
        {
            lastCardAnimationParent.gameObject.SetActive(false);
        };
    }

    private void AnimateMatchSymbol(CardSuit suit)
    {
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                matchSymbolAnimationParent.gameObject.SetActive(true);

                string animationName = suit switch
                {
                    CardSuit.SuitSquare => MATCH_SYMBOL_SQUARE_ANIMATION_NAME,
                    CardSuit.SuitCross => MATCH_SYMBOL_CROSS_ANIMATION_NAME,
                    CardSuit.SuitTriangle => MATCH_SYMBOL_TRIANGLE_ANIMATION_NAME,
                    CardSuit.SuitCircle => MATCH_SYMBOL_CIRCLE_ANIMATION_NAME,
                    CardSuit.SuitStar => MATCH_SYMBOL_STAR_ANIMATION_NAME,
                    _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, null)
                };

                Utility.PlayAnimation(matchSymbolAnimation, animationName, false);
                matchSymbolAnimation.AnimationState.Complete += delegate
                {
                    matchSymbolAnimationParent.gameObject.SetActive(false);
                };

            });
    }

    private void AnimateOpenSuitPicker()
    {
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendInterval(0.25f)
            .AppendCallback(() =>
            {
                suitPickerTransform.gameObject.SetActive(true);
            });
    }


    public void AnimateShowResult(List<WhotPlayerResult> result)
    {
        Debug.Log("Animating show result for players: " + string.Join(", ", result.Select(r => r.UserId)));
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                // Lật bài
                foreach (WhotPlayer player in playersList)
                {
                    List<Card> cards = result.Find(p => p.UserId == player.playerId)?.RemainingCards.ToList();
                    if (player.playerId == GetCurrentPlayer().playerId)
                    {
                        playerHand.AnimateShowRemainingCards(cards);
                    }
                    else
                    {
                        player.AnimateShowRemainingCards(cards);
                    }
                }
            })
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                // Show điểm
                foreach (WhotPlayer player in playersList)
                {
                    long totalPoints = result.Find(p => p.UserId == player.playerId)?.TotalPoints ?? 0;
                    player.isWinner = result.Find(p => p.UserId == player.playerId)?.IsWinner ?? false;
                    if (player.playerId == GetCurrentPlayer().playerId)
                    {
                        playerHand.DisplayScore(totalPoints);
                    }
                    else
                    {
                        player.DisplayScore(totalPoints);
                    }
                }
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                if (GetCurrentPlayer().isPlaying)
                {
                    playerHand.HideRemainingCards();
                    if (GetCurrentPlayer().isWinner)
                    {
                        AnimateVictory(result);
                    }
                    else
                    {
                        AnimateBetterLuckNextTime(result);
                    }
                }
            });    
    }

    public void AnimateVictory(List<WhotPlayerResult> result)
    {
        victoryAnimationParent.gameObject.SetActive(true);
        StopWaitAnimation();
        playAreaParent.gameObject.SetActive(false);
        foreach (WhotPlayer player in playersList)
        {
            if (!player.isCurrentPlayer)
            {
                player.HideRemainingCards();
            }
        }
        victoryImage.transform.localPosition = Vector3.zero;
        victoryImage.transform.localScale = Vector3.zero;
        victoryImage.transform.DOScale(Vector3.one, ANIMATION_TIME).SetEase(Ease.OutBack);
        Utility.PlayAnimation(victoryAnimation, VICTORY_ANIMATION_NAME, false);
        victoryAnimation.AnimationState.Complete += delegate
        {
            victoryAnimationParent.gameObject.SetActive(false);
            Sequence chipSequence = DOTween.Sequence();
            foreach (WhotPlayer player in playersList)
            {
                if (!player.isPlaying) continue;
                BalanceUpdate playerWallet = balanceUpdates.Find((playerWallet) => playerWallet.UserId == player.playerId);
                if (!player.isCurrentPlayer)
                {
                    player.AnimateChipTransfer(GetCurrentPlayer().GetPlayedCardParent(), playerWallet);
                }
                else
                {
                    chipSequence.AppendInterval(2f);
                    player.AnimateAddChipText(playerWallet.AmountChipAdd);
                }
            }
            chipSequence.AppendInterval(2.5f);
            chipSequence.OnComplete(() =>
            {
                HandleShowMatchResult(result, balanceUpdates, true);
            });
        };

    }

    public void AnimateBetterLuckNextTime(List<WhotPlayerResult> result)
    {
        loseAnimationParent.gameObject.SetActive(true);
        StopWaitAnimation();
        playAreaParent.gameObject.SetActive(false);

        foreach (WhotPlayer player in playersList)
        {
            if (!player.isCurrentPlayer)
            {
                player.HideRemainingCards();
            }
        }

        betterLuckNextTimeText.transform.localPosition = Vector3.zero;
        betterLuckNextTimeText.transform.localScale = Vector3.zero;
        betterLuckNextTimeText.transform.DOScale(Vector3.one, ANIMATION_TIME).SetEase(Ease.OutBack);
        Utility.PlayAnimation(betterLuckNextTimeAnimation, LOSE_ANIMATION_NAME, false);
        betterLuckNextTimeAnimation.AnimationState.Complete += delegate
        {
            loseAnimationParent.gameObject.SetActive(false);

            // Animation transfer chips from other players to the winner
            Sequence chipSequence = DOTween.Sequence();
            foreach (WhotPlayer player in playersList)
            {
                if (!player.isPlaying) continue;
                BalanceUpdate playerWallet = balanceUpdates.Find((playerWallet) => playerWallet.UserId == player.playerId);
                if (!player.isWinner)
                {
                    player.AnimateChipTransfer(GetWinner().GetPlayedCardParent(), playerWallet);
                }
            }
            chipSequence.AppendInterval(2.5f);
            chipSequence.OnComplete(() =>
            {
                HandleShowMatchResult(result, balanceUpdates, false);
            });
        };
    }
    private void AnimateDealACard(WhotCard card, Transform targetTransform, WhotPlayer player, int index = 0)
    {
        Sequence sequence = DOTween.Sequence();
        sequence
            .Join(card.transform.DOMove(targetTransform.position, ANIMATION_TIME).SetEase(Ease.InOutCubic))
            .Join(card.transform.DOScale(0.9f, ANIMATION_TIME))
            .OnComplete(() =>
            {
                WhotCard whotCard = initialCardsList[index];

                if (player.isCurrentPlayer)
                {
                    whotCard.transform.SetParent(playerHand.GetCardsParent(), worldPositionStays: false);
                    whotCard.transform.localPosition = playerHand.GetNewCardPosition(whotCard);
                    whotCard.transform.localScale = Vector3.one;
                    whotCard.OnCardSelected += playerHand.WhotCard_OnCardSelected;
                    playerHand.cardsInHand.Add(whotCard);
                    playerHand.SpreadCards();

                    if (index == initialCardsList.Count - 1)
                    {
                        playerHand.AnimateSortCards();
                    }
                    hasDealtCards = true;
                }
                else
                {
                    player.UpdateCardsLeftVisual(index + 1, true);
                }

                Destroy(card.gameObject);
            });
    }

    private void AnimateCurrentPlayerDrawACard(WhotCard card)
    {
        card.transform.DOScale(new Vector2(0.01f, 1f), ANIMATION_TIME / 2f).OnComplete(() =>
        {
            card.SetFaceUp();
            card.transform.DOScale(1f, ANIMATION_TIME / 2f).SetEase(Ease.InOutCubic);
        });
        Quaternion newRotation = Quaternion.Euler(0, 10, 0);
        card.transform.DOLocalRotate(newRotation.eulerAngles, ANIMATION_TIME / 5).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            newRotation = Quaternion.Euler(0, -10, 0);
            card.transform.localRotation = newRotation;
            card.transform.DOLocalRotate(Vector3.zero, ANIMATION_TIME * 4 / 5).SetEase(Ease.InOutCubic);
        });
        card.transform.DOLocalMove(playerHand.GetNewCardPosition(card), ANIMATION_TIME).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            playerHand.SortCards();
            playerHand.SpreadCards();
        });
    }

    private void AnimateOtherPlayerDrawACard(WhotCard card, Transform targetPosition)
    {
        card.SetFaceDown();
        card.transform.localScale = Vector3.one * 0.6f;
        card.transform.DOMove(targetPosition.position, ANIMATION_TIME).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            Destroy(card.gameObject);
        });
    }

    private void AnimatePlayACard(WhotCard card)
    {
        card.SetFaceDown();
        card.transform.DOScale(new Vector2(0.01f, 1f), ANIMATION_TIME / 2f).OnComplete(() =>
        {
            card.SetFaceUp();
            card.transform.DOScale(1f, ANIMATION_TIME / 2f).SetEase(Ease.InOutCubic);
        });
        Quaternion newRotation = Quaternion.Euler(0, 10, 0);
        card.transform.DOLocalRotate(newRotation.eulerAngles, ANIMATION_TIME / 5).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            newRotation = Quaternion.Euler(0, -10, 0);
            card.transform.localRotation = newRotation;
            card.transform.DOLocalRotate(Vector3.zero, ANIMATION_TIME * 4 / 5).SetEase(Ease.InOutCubic);
        });
        card.transform.DOMove(callCardParent.position, ANIMATION_TIME).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            playerHand.SortCards();
            playerHand.SpreadCards();
        });
    }

    private void AnimateHighlightDeck()
    {
        CanvasGroup scoreCanvasGroup = deckHighlightImage.GetComponent<CanvasGroup>();
        scoreCanvasGroup.DOKill();
        scoreCanvasGroup.alpha = 0f;
        deckHighlightImage.gameObject.SetActive(true);
        scoreCanvasGroup.DOFade(1f, ANIMATION_TIME / 2);
    }

    public void AnimateHideDeckHighlight()
    {
        CanvasGroup scoreCanvasGroup = deckHighlightImage.GetComponent<CanvasGroup>();
        scoreCanvasGroup.DOFade(0f, ANIMATION_TIME / 2).OnComplete(() =>
        {
            deckHighlightImage.gameObject.SetActive(false);
        });
    }
    #endregion

    #region Events
    private void OnSuitPicked(CardSuit cardSuit)
    {
        if (suitPickerTransform.gameObject.activeSelf)
        {
            suitPickerTransform.gameObject.SetActive(false);
        }
        StopWaitAnimation();
        CallCard.SetSuit(cardSuit);
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendCallback(() =>
            {
                AnimateMatchSymbol(cardSuit);
            });
    }
    private void WhotSuitPicker_OnSuitPicked(CardSuit cardSuit)
    {
        CallCard.SetSuit(cardSuit);
        Card cardObject = new()
        {
            Suit = cardSuit,
            Rank = CardRank.Rank20
        };
        DataSender.SendMatchState((long)OpCodeRequest.CallWhot, cardObject.ToByteArray());
    }
    #endregion

    private void HandleShowMatchResult(List<WhotPlayerResult> result, List<BalanceUpdate> balanceUpdates, bool isVictory)
    {
        matchResultTransform.gameObject.SetActive(true);
        WhotMatchResult matchResult = matchResultTransform.GetComponent<WhotMatchResult>();
        matchResult.SetInfo(this, playersList, result, balanceUpdates, isVictory);
    }

    #region Getters
    public Transform GetCallCardParent() => callCardParent;
    public Transform GetDeckOfCardParent() => deckOfCardParent;
    public WhotPlayer GetCurrentPlayer() => playersList.Find(player => player.isCurrentPlayer);
    public WhotPlayer GetPlayerByID(string id) => playersList.Find(player => player.playerId == id);
    private WhotPlayer GetWinner() => playersList.Find(player => player.isWinner);
    private Transform GetEmptyPlayerSlot(out int index)
    {
        for (int i = 0; i < playerPositionsList.Count; i++)
        {
            if (playerPositionsList[i].childCount == 0)
            {
                index = i;
                return playerPositionsList[i];
            }
        }

        index = -1;
        return null;
    }
    #endregion

    public void OnQuitMatch()
    {
        if (new GameState[] { GameState.Idle, GameState.Matching, GameState.Preparing }.Contains(gameState))
        {
            NetworkManager.INSTANCE.LeaveMatch();
            Destroy(gameObject);
        }
    }
    private void AdjustPlayerLayout()
    {
        for (int i = 0; i < playerPositionsList.Count; i++)
        {
            Transform playerPosition = playerPositionsList[i];
            WhotPlayer player = playerPosition.GetComponentInChildren<WhotPlayer>();
            if (player != null)
            {
                PlayerLayout playerLayout = player.GetComponent<PlayerLayout>();
                switch (i)
                {
                    case 0:
                        playerLayout.SetLayout(PlayerLayout.EPlayerLayout.Top);
                        break;
                    case 1:
                        playerLayout.SetLayout(PlayerLayout.EPlayerLayout.Left);
                        break;
                    case 2:
                        playerLayout.SetLayout(PlayerLayout.EPlayerLayout.Top);
                        break;
                    case 3:
                        playerLayout.SetLayout(PlayerLayout.EPlayerLayout.Right);
                        break;

                }
            }
        }
    }
}
