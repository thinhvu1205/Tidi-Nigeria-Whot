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
using UnityEngine.Pool;
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
    [SerializeField] private Transform[] playerPositionsList;
    [SerializeField] private SkeletonGraphic betterLuckNextTimeAnimation, victoryAnimation, matchSymbolAnimation, effectAnimation, lastCardAnimation;
    [SerializeField] private Transform matchResultTransform, yourTurnTransform, suitPickerTransform, effectAnimationParent,
    victoryAnimationParent, loseAnimationParent, matchSymbolAnimationParent, lastCardAnimationParent, deckOfCardParent,
    callCardParent, playAreaParent, playersParent, countdownTransform;
    [SerializeField] private TextMeshProUGUI betText, cardsLeftText, betterLuckNextTimeText, yourTurnText, countdownText;
    [SerializeField] private Image waitImage, victoryImage, deckHighlightImage;
    [SerializeField] WhotSuitPicker suitPicker;
    private ObjectPool<WhotCard> cardPool;

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
    private WhotCard callCard;
    private List<WhotPlayer> playersList;
    private List<WhotCard> dealCardsList;
    private List<BalanceUpdate> balanceUpdates = new();
    private GameState gameState = GameState.Preparing;
    private CardEffect currentEffect = CardEffect.EffectNone;
    private WhotPlayerHand playerHand;
    private Tween waitRotationTween;
    private bool hasDealtCards = false;
    public bool hasPreparedNewGame { get; set; } = false;
    private bool isAutoPlay = false;
    private bool isRejoinTable = false;
    private int totalCardsLeft = 54;
    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    private void Update()
    {
        // Khi chạm vào màn hình thì gửi lên trạng thái active để server ko kick người chơi ra khỏi bàn
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            DataSender.SendMatchState((long)OpCodeRequest.OpcodeUserInteractCards, new byte[0]);
        }
    }

    public void Init()
    {
        WhotHandler whotHandler = new(this);
        GameManager.Instance.SetGameHandler(whotHandler);
        DOTween.KillAll(true);
        cardPool = new ObjectPool<WhotCard>(
            createFunc: () => Instantiate(cardPrefab).GetComponent<WhotCard>(),
            actionOnGet: card => card.gameObject.SetActive(true),
            actionOnRelease: card => card.gameObject.SetActive(false),
            actionOnDestroy: card => Destroy(card.gameObject)
        );
        playersList = new();
        dealCardsList = new();
        playerHand = GetComponent<WhotPlayerHand>();
        suitPicker.OnSuitPicked += WhotSuitPicker_OnSuitPicked;
        suitPicker.gameObject.SetActive(false);
        deckHighlightImage.gameObject.SetActive(false);
        matchResultTransform.gameObject.SetActive(false);
        countdownTransform.gameObject.SetActive(false);
        hasDealtCards = false;
        isRejoinTable = false;
        callCard = null;
        foreach (Transform child in callCardParent)
        {
            Destroy(child.gameObject);
        }
    }

    #region API Handlers
    public void HandleJoinMatch(IMatch match)
    {
        playersParent.gameObject.SetActive(true);
        string labelJson = match.Label;
        Match data = JsonConvert.DeserializeObject<Match>(labelJson);
        betText.text = "Bet: " + data.Bet.MarkUnit;
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

        // Nếu GameState là Playing hoặc Reward và JoinPlayers có chứa currentPlayer thì là rejoin bàn
        isRejoinTable =
            (!(new GameState[] { GameState.Preparing, GameState.Idle, GameState.Matching }).Contains(gameState))
            && joinPlayers.Find((player) => player.Id == currentPlayerId) != null;
        if (isRejoinTable) hasDealtCards = true;

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
                if (playersList.Find((p) => p.Id == player.Id) == null)
                {
                    Debug.Log("Joining player: " + joinPlayers[i].UserName);

                    // Nếu người chơi là người chơi mới vào thì cho vào slot còn trống 
                    // int spawnIndex = spawnOrders[players.Count - 1][players.Count - 1];
                    WhotPlayer whotPlayer = Instantiate(whotPlayerPrefab, GetEmptyPlayerSlot(out int index)).GetComponent<WhotPlayer>();
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
                WhotPlayer whotPlayer = GetPlayerByID(leavePlayer.Id);
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

        // Kết nối lại bàn cũ
        if (isRejoinTable)
        {
            Debug.Log("Rejoining table, updating player hand and call card");
            // Khởi tạo lại danh sách bài trên tay của người chơi hiện tại
            foreach (Card card in presenceCard)
            {
                WhotCard whotCard = InitCard(card, playerHand.GetCardsParent());
                playerHand.cardsInHand.Add(whotCard);
            }
            playerHand.SortCards();
            playerHand.SpreadCards();

            // Khởi tạo lại call card
            Card callCard = data.TopCard;
            this.callCard = InitCard(callCard);
            AnimateChooseCallCard();
            isRejoinTable = false;
            return;
        }
        if (!hasDealtCards)
        {
            // Lần đầu thì chia bài
            Card callCard = data.TopCard;
            this.callCard = InitCard(callCard);
            // this.callCard.SetInfo(callCard.Suit, callCard.Rank);
            foreach (Card card in presenceCard)
            {
                WhotCard whotCard = InitCard(card);
                dealCardsList.Add(whotCard);
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
                            WhotCard card = InitCard(drawnCard);
                            DrawACard(card, GetCurrentPlayer(), true);
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
                PrepareNewGame();
                countdownText.text = data.CountDown.ToString();
                break;
            case GameState.Play:
                countdownTransform.gameObject.SetActive(false);
                playAreaParent.gameObject.SetActive(true);
                break;
            case GameState.Matching:
                matchResultTransform.gameObject.SetActive(false);
                hasPreparedNewGame = false;
                break;
            case GameState.Idle:
                break;
            case GameState.Reward:
                break;
            case GameState.Finish:
                break;
            default:
                break;
        }
    }

    public void HandleUpdateTurn(UpdateTurn data)
    {
        if (data.UserId == GetCurrentPlayer().Id)
        {
            yourTurnTransform.gameObject.SetActive(true);
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
            callCard = callCard,
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

                if (data.UserId == GetCurrentPlayer().Id)
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
                    WhotPlayer player = GetPlayerByID(data.UserId);
                    // if (!Instantiate(cardPrefab, player.GetPlayedCardParent()).TryGetComponent<WhotCard>(out var playedCard)) return;
                    WhotCard playedCard = InitCard(data.TopCard, player.GetPlayedCardParent());
                    playedCard.SetInfo(data.TopCard.Suit, data.TopCard.Rank);
                    if (data.Effect == CardEffect.ChoiceShapeGhost)
                    {
                        // Khi người chơi khác chọn chất mới
                        callCard = playedCard;
                        Destroy(playedCard.gameObject);
                    }
                    else
                    {
                        // Khi người chơi khác đánh 1 lá bài
                        player.PlayACard(playedCard);
                        player.CardsLeft--;
                        player.UpdateCardsLeftVisual();
                    }
                }

                // Nếu effect là chọn chất lá Whot thì ko update card count vì server ko trả về
                // if (data.Effect != CardEffect.ChoiceShapeGhost && data.Effect != CardEffect.GeneralMarket)
                // {
                //     UpdateCardsCount(data.DeckCount, playerCardsCount);
                // }
                break;
            case CardEvent.Draw:
            Debug.Log("Handling Draw Event: " + data.UserId + ", PickPenalty: " + data.PickPenalty);
                WhotCard drawnCard = InitCard(data.TopCard);
                if (data.UserId != GetCurrentPlayer().Id)
                {
                    // Khi người chơi khác rút bài
                    WhotPlayer player = GetPlayerByID(data.UserId);
                    // player.pickPenalty = data.PickPenalty;
                    if (data.PickPenalty > 0)
                    {
                        // Khi người chơi khác rút bài do bị phạt +2, +3
                        player.AnimatePlusText(data.PickPenalty);
                        AnimateDrawMultipleCards(player, data.PickPenalty, drawnCard);
                    }
                    else
                    {
                        // Khi người chơi khác rút bài bình thường
                        AnimateDrawMultipleCards(player, 1, drawnCard);
                        // UpdateCardsCount(data.DeckCount, playerCardsCount);
                    }
                }
                break;
            default:
                // Khi vào bàn đang dang chơi dở hoặc vào ván mới
                Debug.Log("HAS DEALT CARDS: " + hasDealtCards);
                if (hasDealtCards)
                {
                    Debug.Log("Has dealt cards, updating cards count");
                    UpdateCardsCount(data.DeckCount, playerCardsCount);
                }
                if (data.TopCard == null) return;
                callCard = InitCard(data.TopCard);
                AnimateChooseCallCard();
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

    public void OnUpdateKickOffTable()
    {
        Destroy(gameObject);
    }
    #endregion

    #region UI 
    private void UpdateCardsCount(int deckCount, List<KeyValuePair<string, int>> playerCardsCount)
    {
        totalCardsLeft = deckCount;
        cardsLeftText.text = totalCardsLeft.ToString();
        foreach (var pair in playerCardsCount)
        {
            WhotPlayer player = GetPlayerByID(pair.Key);
            if (player != null)
            {
                player.CardsLeft = pair.Value;
                player.UpdateCardsLeftVisual();
            }
        }
    }

    private void DecreaseCardsLeft()
    {
        if (totalCardsLeft > 0)
        {
            totalCardsLeft--;
        }
        else
        {
            totalCardsLeft = 0;
        }
        cardsLeftText.text = totalCardsLeft.ToString();
    }

    private void HideYourTurn()
    {
        UpdateYourTurnText("Your turn");
        yourTurnTransform.gameObject.SetActive(false);
    }

    private void UpdateYourTurnText(string text)
    {
        yourTurnText.text = text;
    }
    #endregion

    #region Card Actions
    public void OnDrawACard()
    {

        // Khi người chơi hiện tại bấm rút bài
        if (!yourTurnTransform.gameObject.activeSelf) return;
        DataSender.SendMatchState((long)OpCodeRequest.DrawCard, new byte[0]);
        playerHand.EndTurn();
    }

    public IEnumerator DealCards()
    {
        int length = playersList.Count;
        int cardsPerPlayer = dealCardsList.Count;
        for (int i = 0; i < length * cardsPerPlayer; i++)
        {
            int index = i % length;
            WhotPlayer player = playersList[index];
            if (!player.isPlaying) continue;
            yield return new WaitForSeconds(1f / (length * cardsPerPlayer));

            WhotCard whotCard = InitCard(GetDeckOfCardParent());
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
        DecreaseCardsLeft();
        AnimateChooseCallCard();
    }

    public void DrawACard(WhotCard card, WhotPlayer player, bool isCurrentPlayer)
    {
        WhotCard whotCard = InitCard(card, GetDeckOfCardParent());
        whotCard.SetFaceDown();
        if (isCurrentPlayer)
        {
            whotCard.transform.SetParent(playerHand.GetCardsParent());
            AnimateCurrentPlayerDrawACard(whotCard);
            playerHand.cardsInHand.Add(whotCard);
        }
        else
        {
            whotCard.transform.SetParent(player.GetDealedCardParent());
            AnimateOtherPlayerDrawACard(whotCard, player);
        }
        Destroy(card.gameObject);
    }

    public void PlayACard(WhotCard card, Transform startingPosition)
    {
        WhotCard whotCard = InitCard(card, startingPosition);
        whotCard.transform.SetParent(GetCallCardParent());

        whotCard.SetSelectable(false);
        callCard = whotCard;
        AnimatePlayACard(whotCard);
        Destroy(card.gameObject);
    }
    #endregion

    #region Card Effects
    private void HandleCardEffect(UpdateCardState data)
    {
        WhotPlayer player = GetPlayerByID(data.UserId);
        WhotPlayer targetPlayer = GetPlayerByID(data.TargetUserId);
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
                if (targetPlayer.Id == GetCurrentPlayer().Id)
                {
                    UpdateYourTurnText("Pick 2");
                }
                break;
            case CardEffect.PickThree:
                HandlePick3Effect(targetPlayer);
                if (targetPlayer.Id == GetCurrentPlayer().Id)
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
            if (player.Id != activePlayer.Id && player.isPlaying)
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
        if (targetPlayer.Id != GetCurrentPlayer().Id)
        {
            targetPlayer.UpdateEffectNoti("Pick 2");
        }
    }

    private void HandlePick3Effect(WhotPlayer targetPlayer)
    {
        if (targetPlayer.Id != GetCurrentPlayer().Id)
        {
            targetPlayer.UpdateEffectNoti("Pick 3");
        }
    }

    private void HandleSuspensionEffect(WhotPlayer targetPlayer)
    {
        targetPlayer.AnimateShowSuspension();
        if (targetPlayer.Id != GetCurrentPlayer().Id)
        {
            targetPlayer.UpdateEffectNoti("Suspension");
        }
    }
    private void HandleGeneralMarketEffect(WhotPlayer activePlayer)
    {
        foreach (WhotPlayer player in playersList)
        {
            if (player.Id != activePlayer.Id && player.isPlaying)
            {
                player.AnimateShowSuspension();
                player.AnimatePlusText(1);
            }
        }
        AnimateGeneralMarket();
    }
    #endregion

    #region Animations
    private void AnimateChooseCallCard()
    {
        // WhotCard newCallCard = Instantiate(cardPrefab, GetDeckOfCardParent()).GetComponent<WhotCard>();
        // newCallCard.transform.localPosition = Vector3.zero;
        // newCallCard.transform.localScale = Vector3.one;
        // newCallCard.SetInfo(callCard.GetCardSuit(), callCard.GetCardRank());
        WhotCard newCallCard = InitCard(callCard, GetDeckOfCardParent());
        newCallCard.SetSelectable(false);
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

    private void AnimateDrawMultipleCards(WhotPlayer targetPlayer, int numberOfCards, WhotCard drawnCard)
    {
        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < numberOfCards; i++)
        {
            sequence
                .AppendCallback(() =>
                {
                    // Ko Check currentPlayerId vì đã được thêm bài ở UpdateDeal
                    if (targetPlayer.Id != GetCurrentPlayer().Id)
                    {
                        DrawACard(drawnCard, targetPlayer, false);
                    }
                })
                .AppendInterval(0.35f)
                .OnComplete(() => Destroy(drawnCard));
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
            .AppendInterval(0.25f)
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
        Dictionary<string, PlayerResultData> playerResults = result
            .Where(r => !string.IsNullOrEmpty(r.UserId))
            .ToDictionary(
                r => r.UserId,
                r => new PlayerResultData(
                    r.RemainingCards?.ToList() ?? new List<Card>(),
                    r.TotalPoints,
                    r.IsWinner
                )
            );
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                // Lật bài

                foreach (WhotPlayer player in playersList)
                {
                    if (playerResults.TryGetValue(player.Id, out PlayerResultData data))
                    {
                        Debug.Log($"Showing {data.RemainingCards.Count} remaining cards for player {player.GetPlayerName()}");
                        if (data.RemainingCards.ToList() == null || data.RemainingCards.ToList().Count == 0)
                        {
                            Debug.LogWarning($"No remaining cards found for player {player.Id}");
                            continue;
                        }
                        if (player.Id == GetCurrentPlayer().Id)
                        {
                            playerHand.AnimateShowRemainingCards(data.RemainingCards.ToList());
                        }
                        else
                        {
                            player.AnimateShowRemainingCards(data.RemainingCards.ToList());
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"No RESULT found for player {player.Id}");
                    }
                }
            })
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                // Show điểm
                foreach (WhotPlayer player in playersList)
                {
                    if (playerResults.TryGetValue(player.Id, out PlayerResultData data))
                    {
                        if (player.Id == GetCurrentPlayer().Id)
                        {
                            playerHand.DisplayScore(data.TotalPoints);
                        }
                        else
                        {
                            player.DisplayScore(data.TotalPoints);
                        }
                        player.isWinner = data.IsWinner;
                    }

                }
            })
            .AppendInterval(5f)
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
        playAreaParent.gameObject.SetActive(false);
        StopWaitAnimation();
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
            int transferCount = 0;
            int totalTransfers = playersList.Count(p => p.isPlaying && !p.isWinner);
            foreach (WhotPlayer player in playersList)
            {
                if (!player.isPlaying || player.isWinner) continue;
                bool isLast = ++transferCount == totalTransfers;
                player.AnimateChipTransfer(GetWinner(), isLast);
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
        playAreaParent.gameObject.SetActive(false);
        StopWaitAnimation();

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
            int transferCount = 0;
            int totalTransfers = playersList.Count(p => p.isPlaying && !p.isWinner);

            foreach (WhotPlayer player in playersList)
            {
                if (!player.isPlaying || player.isWinner) continue;

                bool isLast = ++transferCount == totalTransfers;
                player.AnimateChipTransfer(GetWinner(), isLast);
            }
            chipSequence.AppendInterval(2.5f);
            chipSequence.OnComplete(() =>
            {
                HandleShowMatchResult(result, balanceUpdates, false);
            });
        };
    }

    public void AnimateAllPlayersAddChip()
    {
        Dictionary<string, BalanceResultData> playerResults = balanceUpdates
            .Where(r => !string.IsNullOrEmpty(r.UserId))
            .ToDictionary(
                r => r.UserId,
                r => new BalanceResultData(
                    r.AmountChipAdd,
                    r.AmountChipCurrent
                )
            );
        foreach (WhotPlayer player in playersList)
        {
            if (player.isPlaying && playerResults.TryGetValue(player.Id, out BalanceResultData data))
            {
                player.AnimateAddChipText(data.AmountChipAdd);
                player.AnimateChipValue(data.AmountChipCurrent);
            }
        }
    }
    private void AnimateDealACard(WhotCard card, Transform targetTransform, WhotPlayer player, int index = 0)
    {
        DecreaseCardsLeft();
        Sequence sequence = DOTween.Sequence();
        sequence
            .Join(card.transform.DOMove(targetTransform.position, ANIMATION_TIME).SetEase(Ease.InOutCubic))
            .Join(card.transform.DOScale(0.9f, ANIMATION_TIME))
            .OnComplete(() =>
            {
                WhotCard whotCard = dealCardsList[index];

                if (player.isCurrentPlayer)
                {
                    whotCard.transform.SetParent(playerHand.GetCardsParent(), worldPositionStays: false);
                    whotCard.transform.localPosition = playerHand.GetNewCardPosition(whotCard);
                    whotCard.transform.localScale = Vector3.one;
                    whotCard.OnCardSelected += playerHand.WhotCard_OnCardSelected;
                    playerHand.cardsInHand.Add(whotCard);
                    playerHand.SpreadCards();

                    if (index == dealCardsList.Count - 1)
                    {
                        playerHand.AnimateSortCards();
                    }
                    hasDealtCards = true;
                }
                else
                {
                    player.CardsLeft = index + 1;
                    player.UpdateCardsLeftVisual(true);
                }

                Destroy(card.gameObject);
            });
    }

    private void AnimateCurrentPlayerDrawACard(WhotCard card)
    {
        DecreaseCardsLeft();
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

    private void AnimateOtherPlayerDrawACard(WhotCard card, WhotPlayer player)
    {
        DecreaseCardsLeft();
        card.SetFaceDown();
        card.transform.localScale = Vector3.one * 0.6f;
        card.transform.DOMove(player.GetDealedCardParent().position, ANIMATION_TIME).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            player.CardsLeft++;
            player.UpdateCardsLeftVisual();
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
        callCard.SetSuit(cardSuit);
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendCallback(() =>
            {
                AnimateMatchSymbol(cardSuit);
            });
    }
    private void WhotSuitPicker_OnSuitPicked(CardSuit cardSuit)
    {
        callCard.SetSuit(cardSuit);
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
    public WhotPlayer GetPlayerByID(string id) => playersList.Find(player => player.Id == id);
    private WhotPlayer GetWinner() => playersList.Find(player => player.isWinner);
    private Transform GetEmptyPlayerSlot(out int index)
    {
        for (int i = 0; i < playerPositionsList.Length; i++)
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
        if (new GameState[] { GameState.Idle, GameState.Matching, GameState.Preparing }.Contains(gameState) || !GetCurrentPlayer().isPlaying)
        {
            NetworkManager.INSTANCE.LeaveMatch();
            Destroy(gameObject);
        }
    }


    private WhotCard InitCard(Transform initTransform)
    {
        WhotCard whotCard = Instantiate(cardPrefab).GetComponent<WhotCard>();
        // WhotCard whotCard = cardPool.Get();
        whotCard.transform.SetParent(initTransform, worldPositionStays: false);
        whotCard.transform.localPosition = Vector3.zero;
        whotCard.transform.localScale = Vector3.one;
        return whotCard;
    }
    private WhotCard InitCard(Card card)
    {
        WhotCard whotCard = Instantiate(cardPrefab).GetComponent<WhotCard>();
        // WhotCard whotCard = cardPool.Get();
        // whotCard.transform.SetParent(GetDeckOfCardParent(), worldPositionStays: false);
        whotCard.transform.localPosition = Vector3.zero;
        whotCard.transform.localScale = Vector3.one;
        whotCard.SetInfo(card.Suit, card.Rank);
        whotCard.OnCardSelected += playerHand.WhotCard_OnCardSelected;
        return whotCard;
    }

    private WhotCard InitCard(Card card, Transform initTransform)
    {
        WhotCard whotCard = Instantiate(cardPrefab, initTransform).GetComponent<WhotCard>();
        // WhotCard whotCard = cardPool.Get();
        whotCard.transform.SetParent(initTransform, worldPositionStays: false);
        whotCard.transform.localPosition = Vector3.zero;
        whotCard.transform.localScale = Vector3.one;
        whotCard.SetInfo(card.Suit, card.Rank);
        whotCard.OnCardSelected += playerHand.WhotCard_OnCardSelected;
        return whotCard;
    }

    private WhotCard InitCard(WhotCard card, Transform initTransform)
    {
        WhotCard whotCard = Instantiate(cardPrefab, initTransform).GetComponent<WhotCard>();
        // WhotCard whotCard = cardPool.Get();
        // whotCard.transform.SetParent(initTransform, worldPositionStays: true);
        whotCard.transform.localPosition = Vector3.zero;
        whotCard.transform.localScale = Vector3.one;
        whotCard.SetInfo(card.GetCardSuit(), card.GetCardRank());
        whotCard.OnCardSelected += playerHand.WhotCard_OnCardSelected;
        return whotCard;
    }

    private void PrepareNewGame()
    {
        if (hasPreparedNewGame) return;
        Debug.Log("Preparing new game...");
        totalCardsLeft = 54;
        cardsLeftText.text = totalCardsLeft.ToString();
        matchResultTransform.gameObject.SetActive(false);
        countdownTransform.gameObject.SetActive(true);
        dealCardsList.Clear();
        balanceUpdates.Clear();
        hasDealtCards = false;
        callCard = null;
        isRejoinTable = false;
        playerHand.Reset();
        foreach (WhotPlayer player in playersList)
        {
            player.Reset();
        }
        foreach (Transform card in deckOfCardParent)
        {
            if (card.GetComponent<WhotCard>() != null)
            {
                Destroy(card.gameObject);
            }
        }
        foreach (Transform child in callCardParent)
        {
            Destroy(child.gameObject);
        }
        hasPreparedNewGame = true;
    }
    private void AdjustPlayerLayout()
    {
        for (int i = 0; i < playerPositionsList.Length; i++)
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

    private struct PlayerResultData
    {
        public List<Card> RemainingCards;
        public long TotalPoints;
        public bool IsWinner;

        public PlayerResultData(List<Card> remainingCards, long totalPoints, bool isWinner)
        {
            RemainingCards = remainingCards;
            TotalPoints = totalPoints;
            IsWinner = isWinner;
        }
    }

    private struct BalanceResultData
    {
        public long AmountChipAdd;
        public long AmountChipCurrent;

        public BalanceResultData(long amountChipAdd, long amountChipCurrent)
        {
            AmountChipAdd = amountChipAdd;
            AmountChipCurrent = amountChipCurrent;
        }
    }
}
