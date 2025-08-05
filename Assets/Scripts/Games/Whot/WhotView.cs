using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto;
using Common.Pool;
using DG.Tweening;
using Games.Whot;
using Globals;
using Google.Protobuf;
using Nakama;
using Newtonsoft.Json;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using UnityEngine.UI;
using GameState = Proto.GameState;
using PrefabType = Globals.PrefabType;

public class WhotView : BaseGameView
{
    public event Action<OnNextTurnEventArg> OnNextTurn;
    public class OnNextTurnEventArg : EventArgs
    {
        public string playerTurn;
        public WhotCardModel CallCardModel;
        public int countdown;
        public WhotCardEffect cardEffect;
    }
    [SerializeField] private GameObject whotPlayerPrefab;
    [FormerlySerializedAs("whotCardPrefab")] [SerializeField] private WhotCardModel whotCardModelPrefab;
    [SerializeField] private WhotChip chipPrefab;
    [SerializeField] private WhotPlayer[] playersByPosition;
    [SerializeField] private SkeletonGraphic betterLuckNextTimeAnimation, victoryAnimation, matchSymbolAnimation, effectAnimation, lastCardAnimation;
    [SerializeField] private Transform yourTurnTransform, suitPickerTransform, effectAnimationParent,
    victoryAnimationParent, loseAnimationParent, matchSymbolAnimationParent, lastCardAnimationParent, deckOfCardParent,
    callCardParent, playAreaParent, playersParent, countdownTransform, cardPoolParent, chipPoolParent;
    [SerializeField] private TextMeshProUGUI betText, cardsLeftText, betterLuckNextTimeText, yourTurnText, countdownText;
    [SerializeField] private Image waitImage, victoryImage, deckHighlightImage;
    [SerializeField] WhotSuitPicker suitPicker;
    [SerializeField] private WhotMatchResult whotMatchResult;
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
    private WhotCardModel callCardModel, callCardModelLast;
    private List<WhotPlayer> playersList;
    private List<Player> rearrangedPlayersList;
    private List<WhotCardModel> dealCardsList;
    private List<BalanceUpdate> balanceUpdates = new();
    private ObjectPool<WhotCardModel> cardPool;
    // private List<WhotCard> chipPool;
    private GameState gameState = GameState.Preparing;
    private WhotCardEffect currentEffect = WhotCardEffect.EffectNone;
    private WhotPlayerHand playerHand;
    private Tween waitRotationTween;
    private bool hasDealtCards = false;
    private bool hasPreparedNewGame = false;
    private bool isAutoPlay = false;
    private bool isRejoinTable = false;
    private int totalCardsLeft = 54;
    private Action cbShowMatchRs = null;
    public float CurrentMarkUnit { get; private set; }
    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    protected override void Update()
    {
        // Khi chạm vào màn hình thì gửi lên trạng thái active để server ko kick người chơi ra khỏi bàn
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            DataSender.SendMatchState((long)OpCodeRequest.OpcodeUserInteractCards, new byte[0]);
        }
    }

    public void Init()
    {
        DOTween.KillAll(true);
        // cardPool = new ObjectPool<WhotCard>(cardPrefab.GetComponent<WhotCard>(), 20, cardPoolParent);
        PoolService.Instance.Register(PrefabType.WhotCard, cardPoolParent , whotCardModelPrefab, 20, 50, 15);
        PoolService.Instance.Register(PrefabType.ChipPlayerWhot, chipPoolParent , chipPrefab, 15, 50, 10);
        playersList = new();
        rearrangedPlayersList = new();
        dealCardsList = new();
        playerHand = GetComponent<WhotPlayerHand>();
        suitPicker.OnSuitPicked += WhotSuitPicker_OnSuitPicked;
        suitPicker.gameObject.SetActive(false);
        deckHighlightImage.gameObject.SetActive(false);
        whotMatchResult.gameObject.SetActive(false);
        countdownTransform.gameObject.SetActive(false);
        hasDealtCards = false;
        isRejoinTable = false;
        callCardModel = null;
        foreach (Transform child in callCardParent)
        {
            Destroy(child.gameObject);
        }
        foreach (WhotPlayer whotPlayer in playersByPosition)
        {
            whotPlayer.gameObject.SetActive(false);
        }
    }

    #region API Handlers
    public override void LoadInfoMatch(Match match)
    {
        base.LoadInfoMatch(match);
        playersParent.gameObject.SetActive(true);
        // string labelJson = match.Label;
        // Match data = JsonConvert.DeserializeObject<Match>(labelJson);
        CurrentMarkUnit = match.Bet.MarkUnit;
        betText.text = "Bet: " + CurrentMarkUnit;
    }

    // Khi có người chơi join hoặc leave
    public override void HandleUpdateTable(IMatchState matchState)
    {
        var data = UpdateTable.Parser.ParseFrom(matchState.State);
        Debug.Log(data.ToString());
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
            rearrangedPlayersList = new(reordered);
        }

        // Khởi tạo List Player lần đầu
        if (playersList.Count == 0)
        {
            for (int i = 0; i < players.Count; i++)
            {
                Debug.Log("Player count: " + players.Count);
                Debug.Log("Instantiating player: " + players[i].UserName);
                Player player = players[i];
                int spawnIndex = spawnOrders[players.Count - 1][i];
                // WhotPlayer whotPlayer = Instantiate(whotPlayerPrefab, playerPositionsList[spawnIndex]).GetComponent<WhotPlayer>();
                WhotPlayer whotPlayer = playersByPosition[spawnIndex];
                whotPlayer.gameObject.SetActive(true);
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
                    // WhotPlayer whotPlayer = Instantiate(whotPlayerPrefab, GetEmptyPlayerSlot(out int index)).GetComponent<WhotPlayer>();
                    int emptySlotIndex = GetEmptyPlayerSlot();
                    WhotPlayer whotPlayer = playersByPosition[emptySlotIndex];
                    whotPlayer.gameObject.SetActive(true);
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
                    playersList.Insert(emptySlotIndex, whotPlayer);
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
                    // foreach (Transform child in playerPositionsList[spawnOrders[playersList.Count - 1][index]])
                    // {
                    //     Destroy(child.gameObject);
                    // }
                    playersByPosition[spawnOrders[playersList.Count - 1][index]].gameObject.SetActive(false);
                    playersList.Remove(whotPlayer);
                }
            }
        }
    }

    // Gọi khi bài trên tay của người chơi hiện tại thay đổi
    public override void HandleUpdateDeal(IMatchState matchState)
    {
        var data = UpdateDeal.Parser.ParseFrom(matchState.State);
        playAreaParent.gameObject.SetActive(true);
        List<WhotCard> presenceCard = data.PresenceCard.WhotCards.ToList();

        // Kết nối lại bàn cũ
        if (isRejoinTable)
        {
            Debug.Log("Rejoining table, updating player hand and call card");
            // Khởi tạo lại danh sách bài trên tay của người chơi hiện tại
            foreach (WhotCard card in presenceCard)
            {
                WhotCardModel whotCardModel = InitCard(card, playerHand.GetCardsParent());
                playerHand.cardsInHand.Add(whotCardModel);
            }
            playerHand.SortCards();
            playerHand.SpreadCards();

            // Khởi tạo lại call card
            callCardModel = InitCard(data.TopCard, GetDeckOfCardParent());
            AnimateChooseCallCard();
            isRejoinTable = false;
            return;
        }
        if (!hasDealtCards)
        {
            // Lần đầu thì chia bài
            callCardModel = InitCard(data.TopCard, GetDeckOfCardParent());
            // this.callCard.SetInfo(callCard.Suit, callCard.Rank);
            foreach (WhotCard card in presenceCard)
            {
                WhotCardModel whotCardModel = InitCard(card);
                whotCardModel.gameObject.SetActive(false);
                dealCardsList.Add(whotCardModel);
            }
            StartCoroutine(DealCards());
        }
        else
        {
            // Ko phải lần đầu thì là bốc bài, lá bài mới sẽ là các phần tử cuối cùng của mảng
            int diff = presenceCard.Count - playerHand.cardsInHand.Count;
            if (diff > 0)
            {
                List<WhotCard> newCards = presenceCard.Skip(presenceCard.Count - diff).ToList();
                Sequence sequence = DOTween.Sequence();
                foreach (WhotCard drawnCard in newCards)
                {
                    sequence
                        .AppendCallback(() =>
                        {
                            WhotCardModel cardModel = InitCard(drawnCard);
                            DrawACard(cardModel, GetCurrentPlayer(), true);
                        })
                        .AppendInterval(0.2f);
                }
            }
        }
    }

    public override void HandleUpdateGameState(IMatchState matchState)
    {
        var data = UpdateGameState.Parser.ParseFrom(matchState.State);
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
                whotMatchResult.gameObject.SetActive(false);
                hasPreparedNewGame = false;
                break;
            case GameState.Idle:
                break;
            case GameState.Reward:
                Debug.Log("reward : "+ data.ToString());
                if (data.CountDown <= 10)
                {
                    if (cbShowMatchRs != null)
                    {
                        cbShowMatchRs.Invoke();
                        cbShowMatchRs = null;
                    }
                    whotMatchResult.UpdateTimerCountdown((int) data.CountDown);
                }
                break;
            case GameState.Finish:
                break;
            default:
                break;
        }
    }

    public override void HandleUpdateTurn(IMatchState matchState)
    {
        var data = UpdateTurn.Parser.ParseFrom(matchState.State);
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
            CallCardModel = callCardModel,
            countdown = (int)data.Countdown,
            cardEffect = currentEffect
        });
    }

    public override void HandleUpdateCardState(IMatchState matchState)
    {
        var data = UpdateCardState.Parser.ParseFrom(matchState.State);
        playAreaParent.gameObject.SetActive(true);

        currentEffect = data.Effect;
        isAutoPlay = data.IsAutoPlay;
        List<KeyValuePair<string, int>> playerCardsCount = data.PlayerCardCounts.ToList();
        switch (data.Event)
        {
            case WhotCardEvent.WhotEventPlay:
                HandleCardEffect(data);
                if(data.Effect == WhotCardEffect.ChoiceShapeGhost) return;
                if (data.UserId == GetCurrentPlayer().Id)
                {
                    // Khi người chơi hiện tại đánh 1 lá bài
                    WhotCardModel playedCardModel = playerHand.cardsInHand.Find(card =>
                    {
                        bool sameRank = card.GetCardRank() == data.TopCard.Rank;
                        bool isWhot = data.TopCard.Rank == WhotCardRank.WhotRank20;
                        bool sameSuit = card.GetCardSuit() == data.TopCard.Suit;

                        if (isAutoPlay)
                        {
                            return sameRank && (isWhot || sameSuit);
                        }

                        return sameRank && (isWhot || sameSuit) && card.GetIsSelected();
                    });
                    // Nếu TopCard.Rank == 20, thì chỉ cần check Rank, bỏ qua Suit. Nếu TopCard.Rank != 20, thì phải match cả Rank và Suit.
                    if (playedCardModel == null) return;
                    playerHand.PlayACard(playedCardModel);
                }
                else
                {
                    // Khi người chơi khác đánh 1 lá bài
                    WhotPlayer player = GetPlayerByID(data.UserId);
                    WhotCardModel playedCardModel = InitCard(data.TopCard, player.GetPlayedCardParent());
                    playedCardModel.SetInfo(data.TopCard.Suit, data.TopCard.Rank);
                    player.PlayACard(playedCardModel);
                    player.CardsLeft--;
                    player.UpdateCardsLeftVisual();
                }

                // Nếu effect là chọn chất lá Whot thì ko update card count vì server ko trả về
                // if (data.Effect != CardEffect.ChoiceShapeGhost && data.Effect != CardEffect.GeneralMarket)
                // {
                //     UpdateCardsCount(data.DeckCount, playerCardsCount);
                // }
                break;
            case WhotCardEvent.WhotEventDraw:
            Debug.Log("Handling Draw Event: " + data.UserId + ", PickPenalty: " + data.PickPenalty);
                if (data.UserId != GetCurrentPlayer().Id)
                {
                    // Khi người chơi khác rút bài
                    WhotPlayer player = GetPlayerByID(data.UserId);
                    // player.pickPenalty = data.PickPenalty;
                    if (data.PickPenalty > 0)
                    {
                        // Khi người chơi khác rút bài do bị phạt +2, +3
                        player.AnimatePlusText(data.PickPenalty);
                        AnimateDrawMultipleCards(player, data.PickPenalty, data.TopCard);
                    }
                    else
                    {
                        // Khi người chơi khác rút bài bình thường
                        AnimateDrawMultipleCards(player, 1, data.TopCard);
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
                break;
        }
    }

    public override void HandleUpdateWallet(IMatchState matchState)
    {
        var data = BalanceResult.Parser.ParseFrom(matchState.State);
        balanceUpdates = data.Updates.ToList();
    }

    public override void HandleFinish(IMatchState matchState)
    {
        var data = UpdateFinish.Parser.ParseFrom(matchState.State);
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
                AnimateShowResult(data.ResultWhots.ToList());
            });
    }

    public override void HandleUpdateKickOffTheTable(IMatchState matchState)
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

            WhotCardModel whotCardModel = InitCard(GetDeckOfCardParent());
            whotCardModel.SetFaceDown();
            if (player.isCurrentPlayer)
            {
                whotCardModel.transform.SetParent(playerHand.GetCardsParent());
                AnimateDealACard(whotCardModel, playerHand.GetCardsParent(), player, i / length);
            }
            else
            {
                AnimateDealACard(whotCardModel, player.GetDealedCardParent(), player, i / length);
            }
        }
        DecreaseCardsLeft();
        AnimateChooseCallCard();
    }

    public void DrawACard(WhotCardModel cardModel, WhotPlayer player, bool isCurrentPlayer)
    {
        WhotCardModel whotCardModel = InitCard(cardModel, GetDeckOfCardParent());
        whotCardModel.SetFaceDown();
        whotCardModel.SetSelectable(false);
        if (isCurrentPlayer)
        {
            whotCardModel.transform.SetParent(playerHand.GetCardsParent());
            AnimateCurrentPlayerDrawACard(whotCardModel);
            playerHand.cardsInHand.Add(whotCardModel);
        }
        else
        {
            whotCardModel.transform.SetParent(player.GetDealedCardParent());
            AnimateOtherPlayerDrawACard(whotCardModel, player);
        }
        // Destroy(card.gameObject);
        PoolService.Instance.Release(PrefabType.WhotCard, cardModel);
    }

    public void PlayACard(WhotCardModel cardModel, Transform startingPosition)
    {
        WhotCardModel whotCardModel = InitCard(cardModel, startingPosition);
        PoolService.Instance.Release(PrefabType.WhotCard, cardModel);
        whotCardModel.transform.SetParent(GetCallCardParent());
        whotCardModel.SetSelectable(false);
        callCardModel = whotCardModel;
        AnimatePlayACard(whotCardModel);
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
            case WhotCardEffect.Whot when isCurrentPlayer:
                AnimateOpenSuitPicker();
                break;
            case WhotCardEffect.Whot when !isCurrentPlayer:
                AnimateWait();
                break;
            case WhotCardEffect.ChoiceShapeGhost:
                OnSuitPicked(data.TopCard.Suit);
                break;
            case WhotCardEffect.HoldOn:
                HandleHoldOnEffect(player);
                break;
            case WhotCardEffect.PickTwo:
                HandlePick2Effect(targetPlayer);
                if (targetPlayer.Id == GetCurrentPlayer().Id)
                {
                    UpdateYourTurnText("Pick 2");
                }
                break;
            case WhotCardEffect.PickThree:
                HandlePick3Effect(targetPlayer);
                if (targetPlayer.Id == GetCurrentPlayer().Id)
                {
                    UpdateYourTurnText("Pick 3");
                }
                break;
            case WhotCardEffect.Suspension:
                HandleSuspensionEffect(targetPlayer);
                break;
            case WhotCardEffect.GeneralMarket:
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
        // WhotCard newCallCard = InitCard(callCard, GetDeckOfCardParent());
        callCardModel.transform.SetParent(GetCallCardParent());
        callCardModel.SetSelectable(false);
        callCardModel.transform.DOScale(new Vector2(0.01f, 1f), ANIMATION_TIME / 2f).OnComplete(() =>
        {
            callCardModel.SetFaceUp();
            callCardModel.transform.DOScale(1f, ANIMATION_TIME / 2f).SetEase(Ease.InOutCubic);
        });
        Quaternion newRotation = Quaternion.Euler(0, 10, 0);
        callCardModel.transform.DOLocalRotate(newRotation.eulerAngles, ANIMATION_TIME / 5).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            newRotation = Quaternion.Euler(0, -10, 0);
            callCardModel.transform.localRotation = newRotation;
            callCardModel.transform.DOLocalRotate(Vector3.zero, ANIMATION_TIME * 4 / 5).SetEase(Ease.InOutCubic);
        });
        callCardModel.transform.DOLocalMove(Vector3.zero, ANIMATION_TIME).SetEase(Ease.InOutCubic).OnComplete(() => callCardModelLast = callCardModel);
        
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
                        WhotCardModel cardModel = InitCard(drawnCard);
                        DrawACard(cardModel, targetPlayer, false);
                    }
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
            .SetLoops(-1, LoopType.Incremental);
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

    private void AnimateMatchSymbol(WhotCardSuit suit)
    {
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendInterval(0.25f)
            .AppendCallback(() =>
            {
                matchSymbolAnimationParent.gameObject.SetActive(true);

                string animationName = suit switch
                {
                    WhotCardSuit.WhotSuitSquare => MATCH_SYMBOL_SQUARE_ANIMATION_NAME,
                    WhotCardSuit.WhotSuitCross => MATCH_SYMBOL_CROSS_ANIMATION_NAME,
                    WhotCardSuit.WhotSuitTriangle => MATCH_SYMBOL_TRIANGLE_ANIMATION_NAME,
                    WhotCardSuit.WhotSuitCircle => MATCH_SYMBOL_CIRCLE_ANIMATION_NAME,
                    WhotCardSuit.WhotSuitStar => MATCH_SYMBOL_STAR_ANIMATION_NAME,
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
                    r.RemainingCards?.ToList() ?? new List<WhotCard>(),
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
                        if (data.RemainingCards.ToList().Count == 0)
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
                cbShowMatchRs = () =>
                {
                    whotMatchResult.gameObject.SetActive(true);
                    whotMatchResult.SetInfo(this, playersList, result, balanceUpdates, true);
                };
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
                cbShowMatchRs = () =>
                {
                    whotMatchResult.gameObject.SetActive(true);
                    whotMatchResult.SetInfo(this, playersList, result, balanceUpdates, false);
                };
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
    private void AnimateDealACard(WhotCardModel cardModel, Transform targetTransform, WhotPlayer player, int index = 0)
    {
        DecreaseCardsLeft();
        Sequence sequence = DOTween.Sequence();
        sequence
            .Join(cardModel.transform.DOMove(targetTransform.position, ANIMATION_TIME).SetEase(Ease.InOutCubic))
            .Join(cardModel.transform.DOScale(0.9f, ANIMATION_TIME))
            .OnComplete(() =>
            {
                WhotCardModel whotCardModel = dealCardsList[index];

                if (player.isCurrentPlayer)
                {
                    whotCardModel.transform.SetParent(playerHand.GetCardsParent(), worldPositionStays: false);
                    whotCardModel.transform.localPosition = playerHand.GetNewCardPosition(whotCardModel);
                    whotCardModel.transform.localScale = Vector3.one;
                    whotCardModel.gameObject.SetActive(true);
                    playerHand.cardsInHand.Add(whotCardModel);
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

                // Destroy(card.gameObject);
                PoolService.Instance.Release(PrefabType.WhotCard, cardModel);
                
            });
    }

    private void AnimateCurrentPlayerDrawACard(WhotCardModel cardModel)
    {
        DecreaseCardsLeft();
        cardModel.transform.DOScale(new Vector2(0.01f, 1f), ANIMATION_TIME / 2f).OnComplete(() =>
        {
            cardModel.SetFaceUp();
            cardModel.transform.DOScale(1f, ANIMATION_TIME / 2f).SetEase(Ease.InOutCubic);
        });
        Quaternion newRotation = Quaternion.Euler(0, 10, 0);
        cardModel.transform.DOLocalRotate(newRotation.eulerAngles, ANIMATION_TIME / 5).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            newRotation = Quaternion.Euler(0, -10, 0);
            cardModel.transform.localRotation = newRotation;
            cardModel.transform.DOLocalRotate(Vector3.zero, ANIMATION_TIME * 4 / 5).SetEase(Ease.InOutCubic);
        });
        cardModel.transform.DOLocalMove(playerHand.GetNewCardPosition(cardModel), ANIMATION_TIME).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            playerHand.SortCards();
            playerHand.SpreadCards();
        });
    }

    private void AnimateOtherPlayerDrawACard(WhotCardModel cardModel, WhotPlayer player)
    {
        DecreaseCardsLeft();
        cardModel.SetFaceDown();
        cardModel.transform.localScale = Vector3.one * 0.6f;
        cardModel.transform.DOMove(player.GetDealedCardParent().position, ANIMATION_TIME).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            player.CardsLeft++;
            player.UpdateCardsLeftVisual();
            // Destroy(card.gameObject);
            PoolService.Instance.Release(PrefabType.WhotCard, cardModel);
        });
    }

    private void AnimatePlayACard(WhotCardModel cardModel)
    {
        cardModel.SetFaceDown();
        cardModel.transform.DOScale(new Vector2(0.01f, 1f), ANIMATION_TIME / 2f).OnComplete(() =>
        {
            cardModel.SetFaceUp();
            cardModel.transform.DOScale(1f, ANIMATION_TIME / 2f).SetEase(Ease.InOutCubic);
        });
        Quaternion newRotation = Quaternion.Euler(0, 10, 0);
        cardModel.transform.DOLocalRotate(newRotation.eulerAngles, ANIMATION_TIME / 5).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            newRotation = Quaternion.Euler(0, -10, 0);
            cardModel.transform.localRotation = newRotation;
            cardModel.transform.DOLocalRotate(Vector3.zero, ANIMATION_TIME * 4 / 5).SetEase(Ease.InOutCubic);
        });
        cardModel.transform.DOLocalMove(Vector3.zero, ANIMATION_TIME).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            if (callCardModel != callCardModelLast)
            {
                PoolService.Instance.Release(PrefabType.WhotCard, callCardModelLast);
                callCardModelLast = callCardModel;
            }
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
    private void OnSuitPicked(WhotCardSuit cardSuit)
    {
        if (suitPickerTransform.gameObject.activeSelf)
        {
            suitPickerTransform.gameObject.SetActive(false);
        }
        StopWaitAnimation();
        callCardModel.SetSuit(cardSuit);
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendCallback(() =>
            {
                AnimateMatchSymbol(cardSuit);
            });
    }
    private void WhotSuitPicker_OnSuitPicked(WhotCardSuit cardSuit)
    {
        callCardModel.SetSuit(cardSuit);
        WhotCard cardObject = new()
        {
            Suit = cardSuit,
            Rank = WhotCardRank.WhotRank20
        };
        DataSender.SendMatchState((long)OpCodeRequest.CallWhot, cardObject.ToByteArray());
    }
    #endregion
    
    #region Getters
    public Transform GetCallCardParent() => callCardParent;
    public Transform GetDeckOfCardParent() => deckOfCardParent;
    public WhotPlayer GetCurrentPlayer() => playersList.Find(player => player.isCurrentPlayer);
    public WhotPlayer GetPlayerByID(string id) => playersList.Find(player => player.Id == id);
    private WhotPlayer GetWinner() => playersList.Find(player => player.isWinner);
    private int GetEmptyPlayerSlot()
    {
        for (int i = 0; i < playersByPosition.Length; i++)
        {
            if (!playersByPosition[i].gameObject.activeSelf)
            {
                return i;
            }
        }

        return -1;
    }
    #endregion

    public void OnQuitMatch()
    {
        if (new GameState[] { GameState.Idle, GameState.Matching, GameState.Finish }.Contains(gameState) || !GetCurrentPlayer().isPlaying)
        {
            NetworkManager.INSTANCE.LeaveMatch();
            Destroy(gameObject);
        }
    }


    private WhotCardModel InitCard(Transform initTransform)
    {
        // WhotCard whotCard = Instantiate(cardPrefab).GetComponent<WhotCard>();
        // WhotCard whotCard = cardPool.Get();
        WhotCardModel whotCardModel = PoolService.Instance.Get<WhotCardModel>(PrefabType.WhotCard);
        whotCardModel.transform.SetParent(initTransform);
        whotCardModel.transform.localPosition = Vector3.zero;
        whotCardModel.transform.localScale = Vector3.one;
        return whotCardModel;
    }
    
    private WhotCardModel InitCard(WhotCard whotCard)
    {
        // WhotCard whotCard = Instantiate(whotCardPrefab);
        // WhotCard whotCard = cardPool.Get(parent: cardPoolParent);
        // whotCard.transform.SetParent(cardPoolParent, worldPositionStays: false);
        WhotCardModel whotCardModel = PoolService.Instance.Get<WhotCardModel>(PrefabType.WhotCard);
        whotCardModel.transform.localPosition = Vector3.zero;
        whotCardModel.transform.localScale = Vector3.one;
        whotCardModel.SetInfo(whotCard.Suit, whotCard.Rank);
        whotCardModel.OnCardSelected += playerHand.WhotCard_OnCardSelected;
        return whotCardModel;
    }

    private WhotCardModel InitCard(WhotCard whotCard, Transform initTransform)
    {
        // WhotCard whotCard = Instantiate(whotCardPrefab, initTransform);
        // WhotCard whotCard = cardPool.Get(parent: cardPoolParent, position: initTransform.position);
        // whotCard.transform.localPosition = initTransform.position;
        WhotCardModel whotCardModel = PoolService.Instance.Get<WhotCardModel>(PrefabType.WhotCard);
        whotCardModel.transform.SetParent(initTransform);
        whotCardModel.transform.localPosition = Vector3.zero;
        whotCardModel.transform.localScale = Vector3.one;
        whotCardModel.SetInfo(whotCard.Suit, whotCard.Rank);
        whotCardModel.OnCardSelected += playerHand.WhotCard_OnCardSelected;
        return whotCardModel;
    }

    private WhotCardModel InitCard(WhotCardModel cardModel, Transform initTransform)
    {
        // WhotCard whotCard = Instantiate(whotCardPrefab, initTransform);
        // WhotCard whotCard = cardPool.Get(parent: cardPoolParent, position: initTransform.position);
        WhotCardModel whotCardModel = PoolService.Instance.Get<WhotCardModel>(PrefabType.WhotCard);
        whotCardModel.transform.SetParent(initTransform);
        whotCardModel.transform.localPosition = Vector3.zero;
        whotCardModel.transform.localScale = Vector3.one;
        whotCardModel.SetInfo(cardModel.GetCardSuit(), cardModel.GetCardRank());
        whotCardModel.OnCardSelected += playerHand.WhotCard_OnCardSelected;
        return whotCardModel;
    }


    private void PrepareNewGame()
    {
        if (hasPreparedNewGame) return;
        Debug.Log("Preparing new game...");
        RearrangePlayerPosition();
        totalCardsLeft = 54;
        cardsLeftText.text = totalCardsLeft.ToString();
        whotMatchResult.gameObject.SetActive(false);
        countdownTransform.gameObject.SetActive(true);
        dealCardsList.Clear();
        balanceUpdates.Clear();
        hasDealtCards = false;
        callCardModel = null;
        isRejoinTable = false;
        playerHand.Reset();
        // foreach (WhotPlayer player in playersList)
        // {
        //     player.Reset();
        // }
        foreach (Transform card in deckOfCardParent)
        {
            WhotCardModel whotCardModel = card.GetComponent<WhotCardModel>();
            if ( whotCardModel != null)
            {
                // Destroy(card.gameObject);
                PoolService.Instance.Release(PrefabType.WhotCard, whotCardModel);
            }
        }
        foreach (Transform child in callCardParent)
        {
            // Destroy(child.gameObject);
            WhotCardModel whotCardModel = child.GetComponent<WhotCardModel>();
            if ( whotCardModel != null)
            {
                // Destroy(card.gameObject);
                PoolService.Instance.Release(PrefabType.WhotCard, whotCardModel);
            }
        }
        hasPreparedNewGame = true;
    }

    private void RearrangePlayerPosition()
    {
        playersList.Clear();
        foreach (WhotPlayer whotPlayer in playersByPosition)
        {
            whotPlayer.gameObject.SetActive(false);
        }
        string currentPlayerId = User.userMain.userId;
        for (int i = 0; i < rearrangedPlayersList.Count; i++)
        {
            Player player = rearrangedPlayersList[i];
            int spawnIndex = spawnOrders[rearrangedPlayersList.Count - 1][i];
            WhotPlayer whotPlayer = playersByPosition[spawnIndex];
            whotPlayer.gameObject.SetActive(true);
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
            whotPlayer.isPlaying = true;
            playersList.Add(whotPlayer);
        }
    }
    private void AdjustPlayerLayout()
    {
        for (int i = 0; i < playersByPosition.Length; i++)
        {
            int index = i;
            WhotPlayer player = playersByPosition[index];
            if (player != null)
            {
                PlayerLayout playerLayout = player.GetComponent<PlayerLayout>();
                switch (index)
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
        public List<WhotCard> RemainingCards;
        public long TotalPoints;
        public bool IsWinner;

        public PlayerResultData(List<WhotCard> remainingCards, long totalPoints, bool isWinner)
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
