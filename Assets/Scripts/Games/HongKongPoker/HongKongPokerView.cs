using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Pool;
using Cysharp.Threading.Tasks;
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
using Random = UnityEngine.Random;

public class HongKongPokerView : BaseDiceGameView
{
    public enum BetStatus
    {
        ALL_IN = 1,
        RAISE = 2,
        CALL = 3,
        CHECK = 4,
        FOLD = 5 
    }
    [SerializeField] private Transform cardContainer, boxBetContainer, arrowSwapContainer, buttonChangeCardContainer, chipContainer, dealer;
    // [SerializeField] private PlayerViewHongKongPoker dealerHkPoker;
    [SerializeField] private HongKongPokerPot pot;
    [SerializeField] private HongKongPokerToggleBetContainer toggleContainer;
    [SerializeField] private HongKongPokerButtonBetContainer buttonBetContainer;
    [SerializeField] private HongKongPokerChip chipPrefab;
    [SerializeField] private HongKongPokerBoxBet boxBetPrefab;
    [SerializeField] private SkeletonGraphic animationStart;
    [SerializeField] private Sprite spriteFrameMask;
    [SerializeField] private Sprite[] listImageWinLose;
    [SerializeField] private TextMeshProUGUI textTipChip, textThanks, textCountDown;
    [SerializeField] private GameObject cardPrefab, sliderContainer;
    private List<HongKongPokerBoxBet> listBoxBet = new() { null, null, null, null, null };
    private List<List<CardModel>> listPlayerCards = new()
    {
        new List<CardModel>(),
        new List<CardModel>(),
        new List<CardModel>(),
        new List<CardModel>(),
        new List<CardModel>()
    };

    private Vector2[] listCardPosition = new Vector2[]
    {
        new(-12, -207),
        new(-404, -49),
        new(-357, 168),
        new(350, 168),
        new(403, -60)
    };

    private Vector2[] listBoxBetPosition = new Vector2[]
    {
        new(10, -105),
        new(-352, -126),
        new(-309, 93),
        new(306, 72),
        new(395, -144)
    };

    private GameState gameState = GameState.Preparing;
    
    // Track previous round bet for each player to detect new bets
    private Dictionary<string, long> previousRoundBets = new Dictionary<string, long>();

    private BalanceResult balanceResult;
    // ✅ Store references to active sequences and coroutines for cleanup
    private Sequence cardSwapSequence;
    private Sequence countdownSequence;
    private Coroutine arrowSwapCoroutine;
    
    protected override void Awake()
    {
        base.Awake();
        InitPool();
        
        // Set view reference in button container
        if (buttonBetContainer != null)
        {
            buttonBetContainer.SetViewReference(this);
        }
        
        // Set view reference in toggle container
        if (toggleContainer != null)
        {
            toggleContainer.SetViewReference(this);
        }
        
    }

    protected override void Update()
    {
        
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        CleanupRound4Animations();
        PoolService.Instance.ClearPool<HongKongPokerChip>(PrefabType.ChipPlayerHkPoker);
        PoolService.Instance.ClearPool<CardModel>(PrefabType.Card);
        PoolService.Instance.ClearPool<HongKongPokerBoxBet>(PrefabType.BoxBetPlayerHkPoker);
        // DestroyImmediate(chatInGameView.gameObject);
    }


    protected override void RequestSyncStateTable()
    {
        base.RequestSyncStateTable();
        DataSender.SendMatchState((long)OpCodeRequest.SyncTable, Array.Empty<byte>());
        // isRejoinTable = true;
        // playerHand.Reset();
    }

    #region API Handlers

    public override void LoadInfoMatch(Match match)
    {
        base.LoadInfoMatch(match);
        Debug.Log($"[HK Poker] Load Info Match: Table ID={match.TableId}, Mark Unit={match.MarkUnit}");
        
        // Initialize game state
        gameState = GameState.Idle;
        
        // Clear previous game state
        ClearAllCards();
        pot.SetValue(0);
        
        // Reset betting containers
        if (buttonBetContainer != null)
        {
            buttonBetContainer.Reset();
            buttonBetContainer.gameObject.SetActive(false);
        }
        
        if (toggleContainer != null)
        {
            toggleContainer.Reset();
            toggleContainer.gameObject.SetActive(false);
        }
        
        // Hide swap container
        if (buttonChangeCardContainer != null)
        {
            buttonChangeCardContainer.gameObject.SetActive(false);
        }
        
        // Hide slider container
        if (sliderContainer != null)
        {
            sliderContainer.SetActive(false);
        }
    }
    
    public override void HandleUpdateTable(IMatchState matchState)
    {
        base.HandleUpdateTable(matchState);
        UpdateTable data = UpdateTable.Parser.ParseFrom(matchState.State);
        Debug.Log($"[HK Poker] Update Table: Players={data.Players.Count}, Join={data.JoinPlayers.Count}, Leave={data.LeavePlayers.Count}");
        
        // Update player list
        UpdatePosUserTable(data);
        
        // Handle players leaving - clear their cards
        foreach (var leavePlayer in data.LeavePlayers)
        {
            ClearPlayerCards(leavePlayer.Id);
        }
    }

    public override void HandleUpdateDeal(IMatchState matchState)
    {
        base.HandleUpdateDeal(matchState);
        var data = UpdateDeal.Parser.ParseFrom(matchState.State);
        Debug.Log("Update Deal: " + data);
   

    }

    public override void HandleUpdateGameState(IMatchState matchState)
    {
        base.HandleUpdateGameState(matchState);
        var data = UpdateGameState.Parser.ParseFrom(matchState.State);
        Debug.Log("Update Game State: " + data);
        gameState = data.State;
        switch (gameState)
        {
            case GameState.Preparing:
                textCountDown.gameObject.SetActive(true);
                textCountDown.text = data.CountDown.ToString();
                // if (data.CountDown == 1)
                // {
                //     _ = HandleStartGame();
                // }

                break;
            case GameState.Play:
                break;
            case GameState.Matching:

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

    public override void HandleUpdateCardState(IMatchState matchState)
    {
        base.HandleUpdateCardState(matchState);
        var data = UpdateCardState.Parser.ParseFrom(matchState.State);
        Debug.Log("Update Card State: " + data);

    }

    public override void HandleUpdateTurn(IMatchState matchState)
    {
        base.HandleUpdateTurn(matchState);
        var data = UpdateTurn.Parser.ParseFrom(matchState.State);
        Debug.Log($"[HK Poker] Update Turn: User={data.UserId}, Countdown={data.Countdown}s");
        
        string myUserId = User.userProfile.UserId;
        int currentPlayerIndex = GetPlayerIndex(data.UserId);
        
        // Highlight current player
        if (currentPlayerIndex >= 0)
        {
            HighlightPlayer(currentPlayerIndex, data.Countdown);
        }
        
        // ✅ Enable betting UI if it's my turn
        if (data.UserId == myUserId)
        {
            // My turn - show button container, hide toggle container
            if (buttonBetContainer != null)
            {
                buttonBetContainer.gameObject.SetActive(true);
            }
            if (toggleContainer != null)
            {
                // ✅ Disable toggles when it's my turn
                toggleContainer.DisableToggles();
                toggleContainer.gameObject.SetActive(false);
                
                // ✅ Note: Toggle validation will be done in HandleBettingState() 
                // where we have access to current AvailableActions
            }
            StartTurnTimer((int)data.Countdown);
        }
        else
        {
            // Not my turn - show toggle container, hide button container
            if (buttonBetContainer != null)
            {
                buttonBetContainer.gameObject.SetActive(false);
            }
            if (toggleContainer != null)
            {
                toggleContainer.gameObject.SetActive(true);
                // ✅ Toggles are already enabled in UpdateTogglesFromAvailableActions()
            }
        }
    }

    public override void HandleBettingState(IMatchState matchState)
    {
        base.HandleBettingState(matchState);
        var data = HKBettingState.Parser.ParseFrom(matchState.State);
        Debug.Log($"[HK Poker] BettingState: {data}");
        
        // Update betting state UI (includes available actions)
        // Don't animate bets when betting state updates - only update state
        UpdateBettingStateUI(data, shouldAnimateBet: false);
        
        // Show/hide UI containers based on turn
        string myUserId = User.userProfile.UserId;
        bool isMyTurn = !string.IsNullOrEmpty(data.CurrentPlayer) && data.CurrentPlayer == myUserId;
        
        if (isMyTurn)
        {
            // My turn - show button container, hide toggle container
            if (buttonBetContainer != null)
            {
                buttonBetContainer.gameObject.SetActive(true);
            }
            if (toggleContainer != null)
            {
                // ✅ Disable toggles when it's my turn
                toggleContainer.DisableToggles();
                toggleContainer.gameObject.SetActive(false);
                
                // ✅ Get current AvailableActions for validation
                HKPlayerAvailableActions currentActions = null;
                if (data.CurrentPlayerActions != null)
                {
                    currentActions = data.CurrentPlayerActions;
                }
                else
                {
                    // Fallback: find my player state
                    foreach (var playerState in data.PlayerStates)
                    {
                        if (playerState.UserId == myUserId && playerState.AvailableActions != null)
                        {
                            currentActions = playerState.AvailableActions;
                            break;
                        }
                    }
                }
                
                // ✅ Validate and send pre-selected toggle action if still valid
                if (currentActions != null)
                {
                    var validatedAction = toggleContainer.ValidateSelectedAction(currentActions);
                    if (validatedAction.HasValue)
                    {
                        // ✅ Action is still valid, auto-send it
                        long actionAmount = 0;
                        if (validatedAction.Value == HKPokerAction.HkActionCall)
                        {
                            // Get call amount from available actions
                            actionAmount = currentActions.CallAmount > 0 ? currentActions.CallAmount : 0;
                        }
                        Debug.Log($"[HK Poker] Auto-sending validated toggle action: {validatedAction.Value}, amount: {actionAmount}");
                        SendPlayerAction(validatedAction.Value, actionAmount);
                        
                        // ✅ Clear toggle selections after sending
                        toggleContainer.ClearSelections();
                    }
                    else
                    {
                        // ✅ Toggle action is no longer valid (e.g., can't check anymore, need to call)
                        // Let user choose action manually from button container
                        Debug.Log("[HK Poker] Toggle action is no longer valid, user must choose action manually");
                        toggleContainer.ClearSelections();
                    }
                }
            }
        }
        else
        {
            // Not my turn - show toggle container, hide button container
            if (buttonBetContainer != null)
            {
                buttonBetContainer.gameObject.SetActive(false);
            }
            if (toggleContainer != null)
            {
                toggleContainer.gameObject.SetActive(true);
                // ✅ Toggles are already enabled in UpdateTogglesFromAvailableActions()
            }
        }
    }
    
    public override void HandleUpdatePlayerAction(IMatchState matchState)
    {
        base.HandleUpdatePlayerAction(matchState);
        var data = HKUpdatePlayerAction.Parser.ParseFrom(matchState.State);
        Debug.Log($"[HK Poker] Player Action: User={data.UserId}, Action={data.Action}, Amount={data.Amount}, Pot={data.NewPot}");
        
        // Get player index
        int playerIndex = GetPlayerIndex(data.UserId);
        if (playerIndex < 0) return;
        
        // Instantiate box-bet on player
        ShowBoxBetPLayer(playerIndex, data.Action, (int)data.Amount);
        
        // Animate chips to box bet
        if (data.Amount > 0)
        {
            PlayerGiveChipsToBoxbet(playerIndex, data.UserId);
        }
        
        // Update betting state UI - animate bet for actions that involve money
        if (data.BettingState != null)
        {
            bool shouldAnimate = data.Action == HKPokerAction.HkActionBet ||
                                 data.Action == HKPokerAction.HkActionRaise ||
                                 data.Action == HKPokerAction.HkActionCall ||
                                 data.Action == HKPokerAction.HkActionAllIn;
            UpdateBettingStateUI(data.BettingState);
        }
    }

    public override void HandleUpdateNewRound(IMatchState matchState)
    {
        base.HandleUpdateNewRound(matchState);
        var data = HKUpdateNewRound.Parser.ParseFrom(matchState.State);
        Debug.Log($"[HK Poker] New Round: {data.Round}, data: {data}");
        
        string myUserId = User.userProfile.UserId;
        bool isSync = data.IsSync;
        
        // Clear previous round UI (only if not sync)
        if (!isSync)
        {
            ClearRoundUI();
        }
        else
        {
            // ✅ Sync: Cleanup old sequences and coroutines before starting new ones
            CleanupRound4Animations();
        }
        
        if (toggleContainer != null)
        {
            toggleContainer.Reset();
        }
        
        pot.SetValue((int) data.PotInfo.TotalPot, 0.5f);
        
        // ✅ Tạo dictionary để lookup nhanh isFolded (O(1) thay vì O(n))
        Dictionary<string, bool> foldedMap = new Dictionary<string, bool>();
        Dictionary<string, bool> swapStateMap = new Dictionary<string, bool>();
        if (data.BettingState != null && data.BettingState.PlayerStates != null)
        {
            foreach (var ps in data.BettingState.PlayerStates)
            {
                foldedMap[ps.UserId] = ps.IsFolded;
                swapStateMap[ps.UserId] = ps.HasSwapped;
            }
        }
        
        // ===== 1. DEAL CARDS =====
        if (data.PlayerCards != null && data.PlayerCards.Count > 0)
        {
            // Chia bài lần đầu
            if (data.Round == HKPokerRound.HkRoundPreFlop)
            {
                foreach (HKPlayerCards playerCards in data.PlayerCards)
                {
                    int playerIndex = GetPlayerIndex(playerCards.UserId);
                    if (playerIndex < 0) continue;

                    bool isMe = playerCards.UserId == myUserId;

                    if (isSync)
                    {
                        // ✅ Sync: Check và chỉ deal cards chưa có
                        // Deal face-down cards
                        if (isMe && playerCards.FaceDownCards != null && playerCards.FaceDownCards.Count > 0)
                        {
                            foreach (var card in playerCards.FaceDownCards)
                            {
                                if (!HasCard(playerIndex, card.Rank, card.Suit))
                                {
                                    DealACard(card, playerIndex, delay: 0f, isFaceUp: false, isAnimate: false, isMe: true);
                                }
                            }
                        }
                        else if (!isMe && playerCards.FaceDownCards != null && playerCards.FaceDownCards.Count > 0)
                        {
                            int faceDownCount = playerCards.FaceDownCards.Count;
                            int currentFaceDownCount = GetFaceDownCardCount(playerIndex);
                            // Deal thêm cards nếu thiếu
                            for (int i = currentFaceDownCount; i < faceDownCount; i++)
                            {
                                ShowCardBack(playerIndex, delay: 0f, isAnimate: false);
                            }
                        }

                        // Deal face-up cards (visible to all)
                        if (playerCards.FaceUpCards != null)
                        {
                            foreach (var card in playerCards.FaceUpCards)
                            {
                                if (!HasCard(playerIndex, card.Rank, card.Suit))
                                {
                                    DealACard(card, playerIndex, delay: 0f, isFaceUp: true, isAnimate: false);
                                }
                            }
                        }
                    }
                    else
                    {
                        // ✅ New round: Animate cards
                        // Deal face-down cards
                        if (isMe && playerCards.FaceDownCards != null && playerCards.FaceDownCards.Count > 0)
                        {
                            foreach (var card in playerCards.FaceDownCards)
                            {
                                DealACard(card, playerIndex, delay: 0.3f, isFaceUp: false, isAnimate: true, isMe: true);
                            }
                        }
                        else if (!isMe && playerCards.FaceDownCards != null && playerCards.FaceDownCards.Count > 0)
                        {
                            int faceDownCount = playerCards.FaceDownCards.Count;
                            for (int i = 0; i < faceDownCount; i++)
                            {
                                ShowCardBack(playerIndex, delay: 0.3f, isAnimate: true);
                            }
                        }

                        // Deal face-up cards (visible to all)
                        foreach (var card in playerCards.FaceUpCards)
                        {
                            DOVirtual.DelayedCall(0.5f, () =>
                            {
                                DealACard(card, playerIndex, delay: 0.5f, isFaceUp: true, isAnimate: true);
                            });
                        }
                    }
                }
            }
            // Ngửa hết bài của người chơi lên show điểm
            else if (data.Round == HKPokerRound.HkRoundShowdown)
            {

            }
            // Bốc 1 lá mới
            else
            {
                
                foreach (HKPlayerCards playerCards in data.PlayerCards)
                {
                    int playerIndex = GetPlayerIndex(playerCards.UserId);
                    if (playerIndex < 0) continue;
                    
                    bool isMe = playerCards.UserId == myUserId;
                    
                    if (foldedMap.TryGetValue(playerCards.UserId, out bool isFolded) && isFolded)
                    {
                        Debug.Log($"[HK Poker] Skipping card deal for folded player: {playerCards.UserId}");
                        continue;
                    }
                    
                    if (isSync) 
                    {
                        // ✅ Sync: Deal all missing cards, not just the last one
                        if (playerCards.AllCards == null || playerCards.AllCards.Count == 0) continue;

                        // Get current cards count
                        int currentCardCount = listPlayerCards[playerIndex].Count;
                        int expectedCardCount = playerCards.AllCards.Count;

                        Debug.Log($"[HK Poker] Sync cards for player {playerCards.UserId}: current={currentCardCount}, expected={expectedCardCount}");
                        bool isRound4 = data.Round == HKPokerRound.HkRound4Card;
                        bool hasSwapped = false;
                    
                        if (isRound4 && swapStateMap.TryGetValue(playerCards.UserId, out var swapState))
                        {
                            hasSwapped = swapState;
                            Debug.Log($"[HK Poker] Round 4 sync for {playerCards.UserId}: hasSwapped={hasSwapped}");
                        }
                        // Deal all missing cards
                        for (int i = currentCardCount; i < expectedCardCount; i++)
                        {
                            Card cardToDeal = playerCards.AllCards[i];
                            if (cardToDeal == null) continue;

                            // Determine if card should be face-up or face-down
                            bool isFaceUp = false;
                            
                            if (isMe)
                            {
                                // For current user: check if card is in face_up_cards or face_down_cards
                                bool inFaceUp = playerCards.FaceUpCards != null && 
                                                playerCards.FaceUpCards.Any(c => c.Rank == cardToDeal.Rank && c.Suit == cardToDeal.Suit);
                                bool inFaceDown = playerCards.FaceDownCards != null && 
                                                  playerCards.FaceDownCards.Any(c => c.Rank == cardToDeal.Rank && c.Suit == cardToDeal.Suit);
                                
                                if (inFaceUp)
                                {
                                    isFaceUp = true;
                                }
                                else if (inFaceDown)
                                {
                                    isFaceUp = false; // Face-down for current user
                                }
                                else
                                {
                                    // Fallback: determine based on round and position
                                    if (isRound4 && i == 3)
                                    {
                                        // Round 4, 4th card: face-down if not swapped yet
                                        isFaceUp = hasSwapped; // If swapped, card is face-up; otherwise face-down
                                    }
                                    else
                                    {
                                        isFaceUp = true; // Other cards are face-up
                                    }
                                }
                            }
                            else
                            {
                                // For other players: check if card is in face_up_cards
                                bool inFaceUp = playerCards.FaceUpCards != null && 
                                                playerCards.FaceUpCards.Any(c => c.Rank == cardToDeal.Rank && c.Suit == cardToDeal.Suit);
                                
                                if (inFaceUp)
                                {
                                    isFaceUp = true;
                                }
                                else
                                {
                                    // Face-down cards for other players show as card backs
                                    isFaceUp = false; // Will show card back
                                }
                            }

                            // Deal the card
                            if (isRound4 && i == 3)
                            {
                                // Round 4, 4th card
                                if (isMe)
                                {
                                    // Current user: face-down if not swapped, face-up if swapped
                                    DealACard(cardToDeal, playerIndex, delay: 0f, isFaceUp: hasSwapped, isAnimate: false, isMe: true);
                                }
                                else
                                {
                                    // Other players: face-down if not swapped, face-up if swapped
                                    if (hasSwapped)
                                    {
                                        DealACard(cardToDeal, playerIndex, delay: 0f, isFaceUp: true, isAnimate: false);
                                    }
                                    else
                                    {
                                        ShowCardBack(playerIndex, delay: 0f, isAnimate: false);
                                    }
                                }
                            }
                            else if (!isMe && !isFaceUp)
                            {
                                // Other players' face-down cards: show card back
                                ShowCardBack(playerIndex, delay: 0f, isAnimate: false);
                            }
                            else
                            {
                                // Face-up cards or current user's cards
                                DealACard(cardToDeal, playerIndex, delay: 0f, isFaceUp: isFaceUp, isAnimate: false, isMe: isMe);
                            }
                        }
                    }
                    else
                    {
                        // ✅ New round: Animate only the new card (last card)
                        if (playerCards.AllCards == null || playerCards.AllCards.Count == 0) continue;
                        
                        Card newCard = playerCards.AllCards[^1];
                        if (newCard == null) continue;

                        // Deal card normally for other players or other rounds
                        if (data.Round == HKPokerRound.HkRound4Card)
                        {
                            DealACard(newCard, playerIndex, delay: 0.25f, isFaceUp: false, isAnimate: true, isMe: isMe);
                        }
                        else
                        {
                            DealACard(newCard, playerIndex, delay: 0.25f, isFaceUp: true, isAnimate: true);
                        }
                    }
                }
            }
        }
        
        // ===== 2. UPDATE BETTING STATE =====
        // Don't animate bets when round changes - only update state
        if (data.BettingState != null)
        {
            UpdateBettingStateUI(data.BettingState, shouldAnimateBet: data.Round != HKPokerRound.HkRoundPreFlop && !isSync);
            // ✅ Sync: Restore box bets from betting state
            if (isSync)
            {
                SyncBoxBetsFromBettingState(data.BettingState);
            }
        }
        
        // ===== 3. HANDLE ROUND 1 SPECIAL (Auto bet 1/2 MCU) =====
        if (data.Round == HKPokerRound.HkRoundPreFlop && !isSync)
        {
            HandleRound1AutoBet(data);
        }
        
        // ===== 4. UPDATE ROUND UI =====
        UpdateRoundUI(data.Round);
        
        
        // ===== 5. HIGHLIGHT FIRST BETTOR =====
        if (!string.IsNullOrEmpty(data.FirstBettor))
        {
            // int firstBettorIndex = GetPlayerIndex(data.FirstBettor);
            // HighlightPlayer(firstBettorIndex);
        }
        
        // ===== 6. HANDLE ROUND 4 CARD SWAP PHASE =====
        if (data.Round == HKPokerRound.HkRound4Card && data.CardSwapCountdown > 0)
        {
            if (foldedMap.TryGetValue(myUserId, out bool isFolded) && isFolded)
            {
                return;
            }
            
            // ✅ Get swap state for current user
            bool hasSwapped = false;
            if (swapStateMap.TryGetValue(myUserId, out var mySwapState))
            {
                hasSwapped = mySwapState;
            }
            // ✅ Sync: Use countdown from server, New round: Start from 8
            int timeStart = data.IsSync ? (int)data.CardSwapCountdown : 8;
            
            // Round 4: Card swap phase - disable action buttons, show swap/keep buttons
            Debug.Log("[HK Poker] Round 4: Card swap phase - disabling action buttons");
            
            // Disable all action buttons during card swap phase
            if (buttonBetContainer != null)
            {
                buttonBetContainer.gameObject.SetActive(false);
            }
            if (toggleContainer != null)
            {
                toggleContainer.gameObject.SetActive(false);
            }
            
            // ✅ Cleanup old sequences before creating new ones
            CleanupRound4Animations();
            
            // Show swap/keep buttons
            cardSwapSequence = DOTween.Sequence();
            cardSwapSequence.AppendCallback(() =>
            {
                if (this == null || hasSwapped) return;
                buttonChangeCardContainer?.gameObject.SetActive(true);
                arrowSwapContainer.gameObject.SetActive(true);
                
                // ✅ Stop old coroutine before starting new one
                if (arrowSwapCoroutine != null)
                {
                    StopCoroutine(arrowSwapCoroutine);
                }
                arrowSwapCoroutine = StartCoroutine(AnimateArrowSwapLoop(timeStart));
            }).AppendInterval((timeStart - 1) * 1.0f).AppendCallback(() =>
            {
                if (!hasSwapped) 
                {
                    OnClickKeepCard();
                }
            });

            // ✅ Show countdown timer
            textCountDown.gameObject.SetActive(true);
            textCountDown.text = timeStart.ToString();
            
            countdownSequence = DOTween.Sequence();
            countdownSequence.AppendInterval(1f).AppendCallback(() =>
            {
                timeStart--;
                if (timeStart > 0)
                {
                    textCountDown.text = timeStart.ToString();
                }
                else
                {
                    textCountDown.gameObject.SetActive(false);
                }
            }).SetLoops(timeStart).OnComplete(() =>
            {
                textCountDown.gameObject.SetActive(false);
            });
        }
    }

    public override void HandleUpdateCardSwap(IMatchState matchState)
    {
        base.HandleUpdateCardSwap(matchState);
        var data = HKUpdateCardSwap.Parser.ParseFrom(matchState.State);
        Debug.Log($"[HK Poker] Card Swap: User={data.UserId}, Swapped={data.Swapped}");
        
        int playerIndex = GetPlayerIndex(data.UserId);
        if (playerIndex < 0) return;
        
        string myUserId = User.userProfile.UserId;
        bool isMe = data.UserId == myUserId;
        List<CardModel> cardPlayer = listPlayerCards[playerIndex];
        
        if (data.Swapped)
        {
            // Swap: face-down card (card 1) becomes face-up, 4th card becomes face-down
            if (isMe)
            {
                // For current user: 
                // 1. Show the old face-down card (card 1) as face-up (it's now the 4th position)
                if (data.NewFaceUpCard != null)
                {
                    // Show the swapped card (old face-down card is now face-up at 4th position)
                    // DealACard(data.NewFaceUpCard, playerIndex, delay: 0.1f, isFaceUp: true);
                    cardPlayer[3].HideShadowCard();
                    int rank = cardPlayer[3].GetRank();
                    int suit = cardPlayer[3].GetSuit();
                    cardPlayer[3].SetData(cardPlayer[0].GetRank(), cardPlayer[0].GetSuit());
                    cardPlayer[0].SetData(rank, suit);
                }
            }
            else
            {
                // For other players: Show swap animation
                ShowSwapAnimation(playerIndex, data.NewFaceUpCard);
            }
        }
        else
        {
            // Keep: Don't swap, show the 4th card for current user (it was hidden before)
            if (isMe)
            {
                // Show the 4th card that was hidden
                Debug.Log("[HK Poker] Current user chose to KEEP card - showing 4th card");
                // DealACard(hiddenFourthCard, playerIndex, delay: 0.1f, isFaceUp: true);
                cardPlayer[3].HideShadowCard();
            }
            else
            {
                // Show "Keep" text for other players
                // ShowPlayerAction(playerIndex, "KEEP", 0);
                ShowSwapAnimation(playerIndex, data.NewFaceUpCard);
            }
        }
    }

   public override async UniTask HandleUpdateShowdown(IMatchState matchState)
    {
        await base.HandleUpdateShowdown(matchState);

        var data = HKUpdateShowdown.Parser.ParseFrom(matchState.State);

        Debug.Log($"[HK Poker] Showdown: {data}");

        ClearRoundUI();
        listBoxBet = new List<HongKongPokerBoxBet>() { null, null, null, null, null };

        pot.SetValue((int)data.PotInfo.TotalPot, 0.5f);


        // Reveal all hands
        if (data.HandResults != null)
        {
            foreach (var result in data.HandResults)
            {
                int playerIndex = GetPlayerIndex(result.UserId);
                if (playerIndex < 0) continue;
                
                // Show all 5 cards
                RevealPlayerHand(playerIndex, result.Cards.ToList());
                
                // Show hand rank
                if (result.Cards.Count == 5)
                {
                    ShowHandRank(playerIndex, (int) result.HandRank);
                }
            }
        }
        

        // Distribute pot to winners (all at once - parallel)
        if (data.Winners is { Count: > 0 })
        {
            DOTween.Sequence()
                .AppendInterval(3f) // Delay before starting chip distribution
                .AppendCallback(() =>
                {
                    float maxChipMoveTime = 0f;
                    
                    // Start chip animations for all winners simultaneously
                    foreach (var winnerId in data.Winners)
                    {
                        var balanceUpdate = balanceResult.Updates.FirstOrDefault(x => x.UserId == winnerId);
                        var playerWinner = userIdToView.GetValueOrDefault(winnerId);
                        
                        if (playerWinner == null || balanceUpdate == null) continue;
                        
                        int winnerIndex = GetPlayerIndex(winnerId);
                        if (winnerIndex < 0) continue;

                        // Set win effect
                        playerWinner.SetEffectWin(isLoop: false);
                        pot.SetValue(0, 0.5f);
                        // Animate chips from dealer to player
                        float chipMoveDuration = DealerGiveChipsToPlayer(winnerIndex, (int)balanceUpdate.AmountChipAdd);
                        
                        // Track max duration (for final cleanup)
                        if (chipMoveDuration > maxChipMoveTime)
                        {
                            maxChipMoveTime = chipMoveDuration;
                        }
                        
                        // Animate money flying after chip animation completes
                        DOTween.Sequence()
                            .AppendInterval(chipMoveDuration)
                            .AppendCallback(() =>
                            {
                                // Show money flying animation for THIS winner
                                playerWinner.AnimateFlyMoney(balanceUpdate.AmountChipAdd, 45);
                            });
                    }
                    
                    // After longest chip animation completes, collect cards and reset pot
                    DOTween.Sequence()
                        .AppendInterval(maxChipMoveTime + 3f) // Wait for longest animation + small buffer
                        .AppendCallback(() =>
                        {
                            AnimateCollectCards().Forget();
                        });
                });
        }
        else
        {
            // No winners (shouldn't happen, but collect cards anyway)
            DOTween.Sequence()
                .AppendInterval(2f)
                .AppendCallback(() =>
                {
                    AnimateCollectCards().Forget();
                    pot.SetValue(0);
                });
        }
    }

    public override void HandleFinish(IMatchState matchState)
    {
        base.HandleFinish(matchState);
        var data = UpdateFinish.Parser.ParseFrom(matchState.State);
        Debug.Log("[HK Poker] Update Finish: " + data);
        // Collect all cards after finish (if not already collected in showdown)
        // This handles single winner case where showdown might not be called
        // DOTween.Sequence()
        //     .AppendInterval(2f)
        //     .AppendCallback(() =>
        //     {
        //         AnimateCollectCards().Forget();
        //     });
    }
    
    public override void HandleUpdateWallet(IMatchState matchState)
    {
        base.HandleUpdateWallet(matchState);
        balanceResult = BalanceResult.Parser.ParseFrom(matchState.State);
        Debug.Log("Update Wallet: " + balanceResult);
    }
    
    #endregion

    #region Helper 

    
    // ✅ Helper function: Check xem card đã tồn tại chưa
    private bool HasCard(int playerIndex, CardRank rank, CardSuit suit)
    {
        if (playerIndex < 0 || playerIndex >= listPlayerCards.Count)
            return false;

        List<CardModel> playerCards = listPlayerCards[playerIndex];
        foreach (var card in playerCards)
        {
            if (card != null && card.GetRank() == (int)rank && card.GetSuit() == (int)suit)
            {
                return true;
            }
        }
        return false;
    }

    // ✅ Helper function: Đếm số face-down cards (card back)
    private int GetFaceDownCardCount(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= listPlayerCards.Count)
            return 0;

        int count = 0;
        List<CardModel> playerCards = listPlayerCards[playerIndex];
        foreach (var card in playerCards)
        {
            if (card != null && !card.IsShow())
            {
                count++;
            }
        }
        return count;
    }
    
    // ✅ Sync box bets from betting state (for rejoin/sync)
    private void SyncBoxBetsFromBettingState(HKBettingState bettingState)
    {
        if (bettingState == null || bettingState.PlayerStates == null)
            return;

        Debug.Log("[HK Poker] Syncing box bets from betting state");

        foreach (var playerState in bettingState.PlayerStates)
        {
            int playerIndex = GetPlayerIndex(playerState.UserId);
            if (playerIndex < 0) continue;

            // Skip if player has no action or no bet
            if (playerState.Action == HKPokerAction.HkActionNone && playerState.CurrentRoundBet == 0)
            {
                continue;
            }

            // Determine action and amount
            HKPokerAction action = playerState.Action;
            int amount = (int)playerState.CurrentRoundBet;

            // Handle Fold action (amount is 0 for fold)
            if (playerState.IsFolded && action == HKPokerAction.HkActionNone)
            {
                action = HKPokerAction.HkActionFold;
                amount = 0;
            }
            // If action is None but has bet, infer action from context
            else if (action == HKPokerAction.HkActionNone && amount > 0)
            {
                // Infer action based on betting state
                long currentBet = bettingState.CurrentBet;
                if (amount == currentBet && currentBet > 0)
                {
                    // Matched current bet - likely Call
                    action = HKPokerAction.HkActionCall;
                }
                else if (amount > currentBet)
                {
                    // Bet more than current - likely Raise
                    action = HKPokerAction.HkActionRaise;
                }
                else if (currentBet == 0 && amount > 0)
                {
                    // First bet in round - likely Bet
                    action = HKPokerAction.HkActionBet;
                }
            }

            // Show box bet (no animation for sync)
            if (action != HKPokerAction.HkActionNone)
            {
                ShowBoxBetPLayer(playerIndex, action, amount, skipAnimation: true);
            }
        }
    }
    
    private int GetPlayerIndex(string userId)
    {
        // Map user ID to visual position index (0-4)
        // rearrangedPlayers is ordered with local player at index 0
        for (int i = 0; i < rearrangedPlayers.Count; i++)
        {
            if (rearrangedPlayers[i].Id == userId)
            {
                return i;
            }
        }
        
        // Fallback: check in userIdToView dictionary
        if (userIdToView.ContainsKey(userId))
        {
            // Find index by checking position
            var view = userIdToView[userId];
            var viewPos = (view.transform as RectTransform)?.anchoredPosition ?? Vector2.zero;
            
            // Match with listPosView to get index
            for (int i = 0; i < listPosView.Count; i++)
            {
                if (Vector2.Distance(viewPos, listPosView[i]) < 1f)
                {
                    return i;
                }
            }
        }
        
        Debug.LogWarning($"[HK Poker] Player {userId} not found in rearrangedPlayers or userIdToView");
        return -1;
    }
    
    private void ShowBoxBetPLayer(int playerIndex, HKPokerAction action, int amount, bool skipAnimation = false)
    {
        Debug.Log($"Player {playerIndex} action: {action} {(amount > 0 ? amount.ToString() : "")}");

        // Show in BoxBet if available
        if (playerIndex >= 0 && playerIndex < listBoxBet.Count)
        {
            var boxBet = listBoxBet[playerIndex];
            if (boxBet != null)
            {
                // Update box bet display with action
                boxBet.gameObject.SetActive(true);
                boxBet.SetInfo(action, playerIndex, amount);
            }
            else
            {
                boxBet = PoolService.Instance.Get<HongKongPokerBoxBet>(PrefabType.BoxBetPlayerHkPoker);
                listBoxBet[playerIndex] = boxBet;
                boxBet.transform.localPosition = listBoxBetPosition[playerIndex];
                boxBet.SetInfo(action, playerIndex, amount);
            }
        }

        // Handle Fold animation (only if not skipping)
        if (!skipAnimation)
        {
            switch(action)
            {
                case HKPokerAction.HkActionFold:
                    Debug.Log("ANIMATE FOLD ACTION");
                    List<CardModel> cards = listPlayerCards[playerIndex];
                    for (int i = 0; i < cards.Count; i++)
                    {
                        StartCoroutine(FoldDown(playerIndex, cards[i], i * 0.1f));
                    }
                    break;
            }
        }
    }

    private void UpdateBettingStateUI(HKBettingState bettingState, bool shouldAnimateBet = false)
    {
        Debug.Log($"[HK Poker] UpdateBettingStateUI: {bettingState}, shouldAnimateBet: {shouldAnimateBet}");
        long currentPlayerStack = 0; // Số chip còn lại trong bàn của người chơi hiện tại
        HKPlayerAvailableActions myAvailableActions = null;
        
        // Update each player's betting info
        foreach (var playerState in bettingState.PlayerStates)
        {
            // BasePlayerView playerView = userIdToView.GetValueOrDefault(playerState.UserId);
            int playerIndex = GetPlayerIndex(playerState.UserId);
            if (playerIndex < 0) continue;
            bool isMe = playerState.UserId == User.userProfile.UserId;
            
            // Update stack display
            long stack = playerState.Stack;
            if (isMe) currentPlayerStack = stack;
            UpdatePlayerStack(playerIndex, stack);
            
            //Move Chip From Box Bet to Dealer After Every Round
            if (shouldAnimateBet){
                MoveChipsBoxBetToDealer(playerIndex);
            }
            
            
            // Get available actions for current player (me)
            if (isMe && playerState.AvailableActions != null)
            {
                myAvailableActions = playerState.AvailableActions;
            }
        }
        
        string myUserId = User.userProfile.UserId;
        bool isMyTurn = !string.IsNullOrEmpty(bettingState.CurrentPlayer) && bettingState.CurrentPlayer == myUserId;
        
        // Update button container for ON TURN players
        if (buttonBetContainer != null && isMyTurn)
        {
            // Use CurrentPlayerActions if available (backward compatibility)
            if (myAvailableActions == null && bettingState.CurrentPlayerActions != null)
            {
                myAvailableActions = bettingState.CurrentPlayerActions;
            }
            
            if (myAvailableActions != null)
            {
                // Update buttons based on available actions from server
                buttonBetContainer.UpdateButtonsFromAvailableActions(
                    myAvailableActions,
                    (int)currentPlayerStack,
                    (int)bettingState.MinRaise,
                    (int)bettingState.CurrentBet
                );
            }
            else
            {
                // Fallback: use old method if no available actions
                long currentBet = bettingState.CurrentBet;
                long minRaise = bettingState.MinRaise;
                buttonBetContainer.SetValues(
                    (int)currentPlayerStack,
                    (int)minRaise,
                    pot.PotValue
                );
            }
        }
        
        // Update toggle container for OFF TURN players
        if (toggleContainer != null && !isMyTurn)
        {
            // Find my player state and get available actions
            HKPlayerAvailableActions myOffTurnActions = null;
            foreach (var playerState in bettingState.PlayerStates)
            {
                if (playerState.UserId == myUserId && playerState.AvailableActions != null)
                {
                    myOffTurnActions = playerState.AvailableActions;
                    break;
                }
            }
            
            if (myOffTurnActions != null)
            {
                toggleContainer.UpdateTogglesFromAvailableActions(myOffTurnActions);
            }
        }
    }

      // ✅ Cleanup function for Round 4 animations
    private void CleanupRound4Animations()
    {
        // Kill DOTween sequences
        if (cardSwapSequence != null && cardSwapSequence.IsActive())
        {
            cardSwapSequence.Kill();
            cardSwapSequence = null;
        }

        if (countdownSequence != null && countdownSequence.IsActive())
        {
            countdownSequence.Kill();
            countdownSequence = null;
        }

        // Stop coroutine
        if (arrowSwapCoroutine != null)
        {
            StopCoroutine(arrowSwapCoroutine);
            arrowSwapCoroutine = null;
        }

        // Kill any DOTween animations on arrowSwapContainer
        if (arrowSwapContainer != null)
        {
            arrowSwapContainer.DOKill();
        }

        // Hide UI elements
        if (buttonChangeCardContainer != null)
        {
            buttonChangeCardContainer.gameObject.SetActive(false);
        }
        if (arrowSwapContainer != null)
        {
            arrowSwapContainer.gameObject.SetActive(false);
        }
        if (textCountDown != null)
        {
            textCountDown.gameObject.SetActive(false);
        }
    }

    private void ClearRoundUI()
    {
        // Clear previous round displays
        Debug.Log("[HK Poker] Clearing round UI");

        // ✅ Cleanup Round 4 animations
        CleanupRound4Animations();

        // Reset previous round bets when starting new round
        previousRoundBets.Clear();

        foreach (var boxBet in listBoxBet)
        {
            if (boxBet != null)
            {
                boxBet.gameObject.SetActive(false);
            }
        }

        foreach (var value in userIdToView)
        {
            value.Value.HideCountDown();
        }

        // Reset betting containers
        if (buttonBetContainer != null)
        {
            buttonBetContainer.Reset();
            buttonBetContainer.gameObject.SetActive(false);
        }

        if (toggleContainer != null)
        {
            toggleContainer.Reset();
            toggleContainer.gameObject.SetActive(false);
        }

        // Hide swap container (will be shown again in Round 4 if needed)
        if (buttonChangeCardContainer != null)
        {
            buttonChangeCardContainer.gameObject.SetActive(false);
        }

        // Hide slider container
        if (sliderContainer != null)
        {
            sliderContainer.SetActive(false);
        }
    }

    private void UpdateRoundUI(HKPokerRound round)
    {
        // Update UI based on current round
        Debug.Log($"Update round UI: {round}");
        
        // Show round indicator
        // Enable/disable card swap button for Round 4
        // if (round == HKPokerRound.HkRound4Card)
        // {
        //     buttonChangeCardContainer?.gameObject.SetActive(true);
        // }
    }

    private void HighlightPlayer(int playerIndex, long timeTurn)
    {
        // Highlight current player turn
        Debug.Log($"Highlight player {playerIndex}");
        
        // TODO: Add visual highlight (glow effect, border, etc.)
        // Example: 
        
        foreach (var player in userIdToView)
        {
            BasePlayerView playerView = player.Value;
            playerView.HideCountDown();
        }
        if (playerIndex >= 0 && playerIndex < rearrangedPlayers.Count)
        {
            var player = rearrangedPlayers[playerIndex];
            if (userIdToView.TryGetValue(player.Id, out var view))
            {
                view.SetCurrentTurn(true, timeTurn, 10);
            }
        }
    }
    
    private void HandleRound1AutoBet(HKUpdateNewRound data)
    {
        // Show auto bet animation for second player in round 1
        if (data.BettingState == null) return;
        
        string firstBettor = data.FirstBettor;
        
        foreach (var playerState in data.BettingState.PlayerStates)
        {
            // Skip first bettor
            if (playerState.UserId == firstBettor) continue;
            
            // Check if player has auto bet (currentRoundBet > 0)
            if (!string.IsNullOrEmpty(playerState.CurrentRoundBet.ToString()))
            {
                long autoBet = long.Parse(playerState.CurrentRoundBet.ToString());
                if (autoBet > 0)
                {
                    int playerIndex = GetPlayerIndex(playerState.UserId);
                    if (playerIndex >= 0)
                    {
                        // Show auto bet animation
                        ShowBoxBet(HKPokerAction.HkActionBet, playerIndex, (int)autoBet);
                        
                        // Animate chips to box bet
                        DOTween.Sequence()
                            .AppendInterval(0.5f)
                            .AppendCallback(() => PlayerGiveChipsToBoxbet(playerIndex, playerState.UserId));
                        
                        // Update previous bet for auto bet player
                        previousRoundBets[playerState.UserId] = autoBet;
                        
                        // Animate fly money for auto bet
                        BasePlayerView playerView = userIdToView.GetValueOrDefault(playerState.UserId);
                        if (playerView != null)
                        {
                            playerView.AnimateFlyMoney(-autoBet, 45);
                        }
                        
                        // Debug.Log($"[HK Poker] Player {playerState.UserId} auto bet {autoBet} (1/2 mark unit)");
                    }
                }
            }
        }
    }
    
    private void EnableBettingUI(HKBettingState bettingState, HKPokerRound round)
    {
        string myUserId = User.userProfile.UserId;
        var myPlayerState = bettingState.PlayerStates.FirstOrDefault(p => p.UserId == myUserId);
        if (myPlayerState == null) return;
        
        bool isFirstBettor = (myUserId == bettingState.FirstBettor);
        bool isRound1 = (round == HKPokerRound.HkRoundPreFlop);
        long currentBet = bettingState.CurrentBet;
        long myRoundBet = !string.IsNullOrEmpty(myPlayerState.CurrentRoundBet.ToString()) 
            ? myPlayerState.CurrentRoundBet 
            : 0;
        long myStack = myPlayerState.Stack;
        long minRaise = bettingState.MinRaise;
        
        // Show betting UI
        buttonBetContainer.gameObject.SetActive(true);
        
        // ⚠️ ROUND 1, FIRST BETTOR: Chỉ BET/FOLD (KHÔNG CHECK)
        if (isRound1 && isFirstBettor && currentBet == 0)
        {
            // Debug.Log("[HK Poker] First bettor round 1 - only BET or FOLD");
            
            // Setup bet values
            buttonBetContainer.SetValues(
                (int)minRaise,      // Min bet = mark unit
                (int)minRaise,      // Min raise
                (int)myStack        // Max
            );
            
            // Enable BET and FOLD only (NO CHECK)
            // buttonBetContainer.SetButtonsEnabled(
            //     fold: true,
            //     check: false,     // ❌ NO CHECK for first bettor round 1
            //     call: false,
            //     bet: true,
            //     raise: false,
            //     allIn: true
            // );
        }
        else if (currentBet == 0)
        {
            // No bet yet - can CHECK or BET
            buttonBetContainer.SetValues(
                (int)minRaise,
                (int)minRaise,
                (int)myStack
            );
            
            // buttonBetContainer.SetButtonsEnabled(
            //     fold: true,
            //     check: true,      // ✅ Can check
            //     call: false,
            //     bet: true,
            //     raise: false,
            //     allIn: true
            // );
        }
        else if (currentBet > myRoundBet)
        {
            // There's a bet - can CALL/RAISE/FOLD
            long amountToCall = currentBet - myRoundBet;
            
            buttonBetContainer.SetValues(
                (int)currentBet,
                (int)minRaise,
                (int)(currentBet + minRaise)
            );
            
            // buttonBetContainer.SetButtonsEnabled(
            //     fold: true,
            //     check: false,
            //     call: true,       // ✅ Can call
            //     bet: false,
            //     raise: myStack > amountToCall,  // Can raise if enough chips
            //     allIn: true
            // );
        }
        else
        {
            // Already matched bet - can CHECK
            buttonBetContainer.SetValues(
                (int)currentBet,
                (int)minRaise,
                (int)(currentBet + minRaise)
            );
            
            // buttonBetContainer.SetButtonsEnabled(
            //     fold: true,
            //     check: true,      // ✅ Can check
            //     call: false,
            //     bet: false,
            //     raise: true,
            //     allIn: true
            // );
        }
        
        // Start turn timer
        StartTurnTimer(10); // 10 seconds
    }
    
    private void StartTurnTimer(int seconds)
    {
        // Start countdown timer
        // Debug.Log($"[HK Poker] Start turn timer: {seconds}s");
        
        // TODO: Implement visual timer
        // Example:
        // StopAllCoroutines();
        // StartCoroutine(TurnTimerCountdown(seconds));
    }
    
    private void UpdatePlayerStack(int playerIndex, long stack)
    {
        // Update player's stack display
        if (playerIndex >= 0 && playerIndex < rearrangedPlayers.Count)
        {
            var player = rearrangedPlayers[playerIndex];
            if (userIdToView.TryGetValue(player.Id, out var view))
            {
                // Update stack text on player view
                view.SetCurrentChip(stack);
                Debug.Log($"Player {playerIndex} stack: {stack}");
            }
        }
    }
    
    private void UpdatePlayerBet(int playerIndex, long bet)
    {
        // Update player's bet display
        Debug.Log($"Player {playerIndex} bet: {bet}");
        
        // Update box bet if exists
        if (playerIndex >= 0 && playerIndex < listBoxBet.Count && listBoxBet[playerIndex] != null)
        {
            // Update box bet chip value
            // listBoxBet[playerIndex].UpdateChip((int)bet);
        }
    }
    
    private void MarkPlayerFolded(int playerIndex)
    {
        // Mark player as folded (grayed out, etc.)
        Debug.Log($"Player {playerIndex} FOLDED");
        
        // TODO: Visual feedback
        // - Gray out cards
        // - Show "FOLD" text
        // - Disable player highlight
    }
    
    private void MarkPlayerAllIn(int playerIndex)
    {
        // Mark player as all-in
        Debug.Log($"Player {playerIndex} ALL-IN");
        
        // TODO: Visual feedback
        // - Show "ALL-IN" badge
        // - Special highlight color
    }

    private void ShowSwapAnimation(int playerIndex, Card newFaceUpCard)
    {
        if (playerIndex < 0 || playerIndex >= listPlayerCards.Count) return;
        
        var playerCards = listPlayerCards[playerIndex];
        if (playerCards.Count >= 4)
        {
            // Animate card flip/swap
            var cardToSwap = playerCards[^1]; // Last card (4th card)
            FoldUp(playerIndex, cardToSwap, newFaceUpCard);
        }
    }

    private void RevealPlayerHand(int playerIndex, List<Card> cards)
    {
        if (playerIndex < 0 || playerIndex >= listPlayerCards.Count) return;

        var playerCards = listPlayerCards[playerIndex];

        var originalScale = playerCards[0].transform.localScale;
        var targetScale = originalScale + Vector3.one * 0.1f;

        for (int i = 0; i < cards.Count && i < playerCards.Count; i++)
        {
            var cardObj = playerCards[i];
            var tr = cardObj.transform;

            cardObj.SetData((int)cards[i].Rank, (int)cards[i].Suit);
            cardObj.ShowCard();
            cardObj.HideShadowCard();

            tr.DOScale(targetScale, 0.5f)
                .OnComplete(() =>
                {
                    tr.DOScale(originalScale, 0.5f);
                });
        }
    }
    
    private void ShowHandRank(int playerIndex, int typeHandRank)
    {
        // Show hand rank name above player's cards
        GameObject imageObject = new GameObject("TextImage");

        Image image = imageObject.AddComponent<Image>();

        imageObject.transform.SetParent(transform, false);
        image.sprite = listImageWinLose[typeHandRank];
        image.preserveAspect = true;
        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(listImageWinLose[typeHandRank].rect.width, listImageWinLose[typeHandRank].rect.height);
        var length = listPlayerCards.Count;

        var cardScale = 0.5f;
        var widthCardNode = (149 * cardScale) / 2;
        var cardBegin = listCardPosition[playerIndex];
        float offsetX = ((length - 2) / 2.0f) * widthCardNode;
        float posX = playerIndex > 2 ? cardBegin.x - offsetX : cardBegin.x + offsetX;
        float posY = cardBegin.y - (200 * cardScale) / 4;

        imageObject.transform.localPosition = new Vector2(posX, posY);

        DOTween.Sequence().AppendInterval(4f).AppendCallback(() =>
        {
            Destroy(imageObject.gameObject);
        });
    }
    
    private void ClearAllCards()
    {
        // Clear all cards from all players
        for (int i = 0; i < listPlayerCards.Count; i++)
        {
            ClearPlayerCardsByIndex(i);
        }
        Debug.Log("[HK Poker] Cleared all cards");
    }
    
    private void ClearPlayerCards(string userId)
    {
        // Clear cards for specific player
        int playerIndex = GetPlayerIndex(userId);
        if (playerIndex >= 0)
        {
            ClearPlayerCardsByIndex(playerIndex);
            Debug.Log($"[HK Poker] Cleared cards for player {userId} at index {playerIndex}");
        }
    }
    
    private void ClearPlayerCardsByIndex(int playerIndex)
    {
        // Return cards to pool
        if (playerIndex >= 0 && playerIndex < listPlayerCards.Count)
        {
            var cards = listPlayerCards[playerIndex];
            foreach (var card in cards)
            {
                if (card != null)
                {
                    PoolService.Instance.Release(PrefabType.Card, card);
                }
            }
            cards.Clear();
        }
    }
    
    protected override void RemovePlayerBoxBet(string leavePlayerId)
    {
        // Remove box bet for leaving player
        int playerIndex = GetPlayerIndex(leavePlayerId);
        if (playerIndex >= 0 && playerIndex < listBoxBet.Count)
        {
            var boxBet = listBoxBet[playerIndex];
            if (boxBet != null)
            {
                PoolService.Instance.Release(PrefabType.BoxBetPlayerHkPoker, boxBet);
                listBoxBet[playerIndex] = null;
                Debug.Log($"[HK Poker] Removed box bet for player {leavePlayerId} at index {playerIndex}");
            }
        }
    }

    #endregion
    
    #region Button Actions
    
    // Card swap buttons (Round 4)
    public void OnClickSwap()
    {
        Debug.Log("[HK Poker] Player chose to SWAP card");
        
        // Send card swap request to server
        SendCardSwapRequest(true);

        // Hide swap UI
        if (buttonChangeCardContainer != null)
        {
            buttonChangeCardContainer.gameObject.SetActive(false);
        }

        arrowSwapContainer.gameObject.SetActive(false);
        arrowSwapContainer.DOKill();

        // Reset betting containers after swap decision
        if (buttonBetContainer != null)
        {
            buttonBetContainer.Reset();
        }

        if (toggleContainer != null)
        {
            toggleContainer.Reset();
        }
    }

    public void OnClickKeepCard()
    {
        Debug.Log("[HK Poker] Player chose to KEEP card");

        // Send card swap request to server
        SendCardSwapRequest(false);

        // Hide swap UI
        if (buttonChangeCardContainer != null)
        {
            buttonChangeCardContainer.gameObject.SetActive(false);
        }

        arrowSwapContainer.gameObject.SetActive(false);
        arrowSwapContainer.DOKill();

        // Reset betting containers after swap decision
        if (buttonBetContainer != null)
        {
            buttonBetContainer.Reset();
        }

        if (toggleContainer != null)
        {
            toggleContainer.Reset();
        }
    }
    // Betting buttons - delegate to button container or handle here
    public void OnClickFold()
    {
        Debug.Log("[HK Poker] Player FOLD");
        SendPlayerAction(HKPokerAction.HkActionFold, 0);
        DisableBettingUI();
    }
    
    public void OnClickCheck()
    {
        Debug.Log("[HK Poker] Player CHECK");
        SendPlayerAction(HKPokerAction.HkActionCheck, 0);
        DisableBettingUI();
    }
    
    public void OnClickCall()
    {
        Debug.Log("[HK Poker] Player CALL");
        SendPlayerAction(HKPokerAction.HkActionCall, 0);
        DisableBettingUI();
    }

    public void OnClickConfirmBet(int amount)
    {
        Debug.Log($"[HK Poker] Player BET {amount}");
        SendPlayerAction(HKPokerAction.HkActionBet, amount);
        DisableBettingUI();
    }
    
    public void OnClickConfirmRaise(int amount)
    {
        Debug.Log($"[HK Poker] Player RAISE {amount}");
        SendPlayerAction(HKPokerAction.HkActionRaise, amount);
        DisableBettingUI();
    }
    
    public void OnClickAllIn()
    {
        Debug.Log("[HK Poker] Player ALL-IN");
        SendPlayerAction(HKPokerAction.HkActionAllIn, 0);
        DisableBettingUI();
    }

    public void OnClickSendTip()
    {
        // Tip functionality
    }

    #endregion
    
    #region Network Requests
    
    private void SendPlayerAction(HKPokerAction action, long amount)
    {
        var request = new HKPlayerActionRequest
        {
            Action = action,
            Amount = amount
        };
        
        DataSender.SendMatchState(
            (long) OpCodeRequest.PlayerAction,
            request.ToByteArray()
        );
        
        Debug.Log($"[HK Poker] Sent action: {action}, amount: {amount}");
    }
    
    private void SendCardSwapRequest(bool swapCard)
    {
        var request = new HKCardSwapRequest
        {
            SwapCard = swapCard
        };
        
        DataSender.SendMatchState(
            (long)Proto.OpCodeRequest.CardSwap,
            request.ToByteArray()
        );
        
        Debug.Log($"[HK Poker] Sent card swap request: {swapCard}");
    }
    
    private void DisableBettingUI()
    {
        if (buttonBetContainer != null)
        {
            buttonBetContainer.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Card Actions
    
    private void ShowCardBack(int playerIndex, float delay = 0f, bool isAnimate = true)
    {
        // Show card back (face-down card for other players)
        List<CardModel> playerCards = listPlayerCards[playerIndex];
        Vector2 basePosition = listCardPosition[playerIndex];
        Vector2 targetPosition;

        bool isLeftTable = !(basePosition.x > 0);
        
        CardModel card = PoolService.Instance.Get<CardModel>(PrefabType.Card);
        card.HideCard(); // Keep it as card back

        if (!isLeftTable)
        {
            card.transform.SetSiblingIndex(20);
        }

        // Calculate position
        float cardWidth = card.GetComponent<RectTransform>().rect.width;
        float offset = cardWidth / 2 * 0.38f * playerCards.Count;

        if (isLeftTable)
        {
            targetPosition = new Vector2(basePosition.x + offset, basePosition.y);
            if (playerCards.Count > 0)
            {
                card.transform.SetSiblingIndex(playerCards[^1].transform.GetSiblingIndex() + 1);
            }
        }
        else
        {
            targetPosition = new Vector2(basePosition.x - offset, basePosition.y);
            if (playerCards.Count > 0)
            {
                card.transform.SetSiblingIndex(playerCards[^1].transform.GetSiblingIndex() - 1);
            }
        }

        if (isAnimate)
        {
            // Animate from dealer to player
            card.transform.localRotation = Quaternion.Euler(0, 0, -90);
            card.transform.localScale = new Vector3(0, 0.45f, 1);

            card.transform
                .DOLocalMove(targetPosition, 0.7f)
                .SetEase(Ease.OutCubic)
                .SetDelay(playerIndex * 0.1f + delay);
            card.transform
                .DOLocalRotate(Vector3.zero, 0.6f)
                .SetEase(Ease.OutCubic)
                .SetDelay(playerIndex * 0.1f + delay);
            card.transform
                .DOScale(new Vector3(0.45f, 0.45f, 1), 0.6f)
                .SetEase(Ease.OutCubic)
                .SetDelay(playerIndex * 0.1f + delay);
        }
        else
        {
            card.transform.localRotation = Quaternion.Euler(0, 0, 0);
            card.transform.localScale = new Vector3(0.45f, 0.45f, 1);
            card.transform.localPosition = targetPosition;
        }
       

        playerCards.Add(card);
    }
    
    private void DealACard(Card pokerCard, int playerIndex, float delay = 0f, bool isFaceUp = true, bool isAnimate = true, bool isMe = false)
    {
        List<CardModel> playerCards = listPlayerCards[playerIndex];
        Vector2 basePosition = listCardPosition[playerIndex];
        Vector2 targetPosition;

        bool isLeftTable = !(basePosition.x > 0);

        CardModel card = PoolService.Instance.Get<CardModel>(PrefabType.Card);
        card.HideCard();

        if (!isLeftTable)
        {
            card.transform.SetSiblingIndex(20);
        }

        // Xác định vị trí đích của lá bài
        float cardWidth = card.GetComponent<RectTransform>().rect.width;
        float offset = cardWidth / 2 * 0.38f * playerCards.Count;

        if (isLeftTable)
        {
            targetPosition = new Vector2(basePosition.x + offset, basePosition.y);
            if (playerCards.Count > 0)
            {
                card.transform.SetSiblingIndex(playerCards[^1].transform.GetSiblingIndex() + 1);
            }
        }
        else
        {
            targetPosition = new Vector2(basePosition.x - offset, basePosition.y);
            if (playerCards.Count > 0)
            {
                card.transform.SetSiblingIndex(playerCards[^1].transform.GetSiblingIndex() - 1);
            }
        }

        // Bài di chuyển từ dealer về tay player
        if (isAnimate)
        {
            if (isFaceUp && pokerCard.Rank != CardRank.RankUnspecified && pokerCard.Suit != CardSuit.SuitUnspecified)
            {
                // Face-up card - show it
                // SoundManager.Instance.PlaySound("sound_cardFlipBJ");
                // playSound(SOUND_GAME.CARD_FLIP_2);
                card.SetData((int)pokerCard.Rank, (int)pokerCard.Suit);
                card.ShowCard();
            }
            else if (!isFaceUp)
            {
                // Face-down card - set data but keep hidden for my view
                // Will show later when needed
                if (isMe)
                {
                    card.SetData((int)pokerCard.Rank, (int)pokerCard.Suit);
                    card.ShowCard();
                    card.ShowShadowCard();
                }
            }

            card.transform.localRotation = Quaternion.Euler(0, 0, -90);
            card.transform.localScale = new Vector3(0, 0.45f, 1);

            card.transform
                .DOLocalMove(targetPosition, 0.7f)
                .SetEase(Ease.OutCubic)
                .SetDelay(playerIndex * 0.1f + delay);
            card.transform
                .DOLocalRotate(Vector3.zero, 0.6f)
                .SetEase(Ease.OutCubic)
                .SetDelay(playerIndex * 0.1f + delay);
            card.transform
                .DOScale(new Vector3(0.45f, 0.45f, 1), 0.6f)
                .SetEase(Ease.OutCubic)
                .SetDelay(playerIndex * 0.1f + delay);
        }
        else
        {
            card.SetData((int)pokerCard.Rank, (int)pokerCard.Suit);
            card.transform.localPosition = targetPosition;
            if (isFaceUp)
            {
                card.ShowCard();
            }
        }
        playerCards.Add(card);
    }
    
    #endregion

    #region Boxbet Actions
    private void ShowBoxBet(HKPokerAction status, int index, int chip)
    {
        HongKongPokerBoxBet boxBet = PoolService.Instance.Get<HongKongPokerBoxBet>(PrefabType.BoxBetPlayerHkPoker);
        listBoxBet[index] = boxBet;
        boxBet.transform.localPosition = listBoxBetPosition[index];
        boxBet.SetInfo(status, index, chip);
    }
    #endregion

    #region Chip Actions
    private void PlayerGiveChipsToBoxbet(int index, string userId)
    {
        // Check if box bet exists
        if (index < 0 || index >= listBoxBet.Count || listBoxBet[index] == null)
        {
            Debug.LogWarning($"[HK Poker] Box bet not found for player {index}");
            return;
        }
        
        int valueBoxBet = listBoxBet[index].Chip;
        BasePlayerView playerView = userIdToView.GetValueOrDefault(userId);
        // if ((int)data["chipStack"] - preNextStack != 0)
        // {
        //     currentPlayer.playerView.effectFlyMoney(-valueBoxBet);
        // }
        playerView.AnimateFlyMoney(-valueBoxBet, 45);
        

        int numberChip = 0;
        if (valueBoxBet > 0)
        {
            numberChip = Mathf.Clamp((int)Mathf.Ceil((float)valueBoxBet / MarkUnit), 1, 4);
        }

        Vector2 targetPosition = listBoxBetPosition[index];
        Vector2 startPosition = listCardPosition[index];

        for (int i = 0; i < numberChip; i++)
        {
            HongKongPokerChip chip = PoolService.Instance.Get<HongKongPokerChip>(PrefabType.ChipPlayerHkPoker);
            chip.transform.localScale = Vector3.one * 0.4f;
            chip.transform.localPosition = startPosition;

            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(i * 0.1f).AppendCallback(() => chip.MoveToBoxBet(targetPosition));
                
        }
    }

    private void MoveChipsBoxBetToDealer(int index)
    {
        if (listBoxBet[index].Chip > 0)
        {
            for (int i = 0; i < 4; i++)
            {
                HongKongPokerChip chip = PoolService.Instance.Get<HongKongPokerChip>(PrefabType.ChipPlayerHkPoker);
                chip.transform.localScale = Vector3.one * 0.4f;
                chip.transform.localPosition = listBoxBetPosition[index];
                Vector3 targetPos = pot.transform.localPosition + new Vector3(
                    Random.Range(-7f, 7f),
                    -40 + Random.Range(-7f, 7f),
                    0
                );
                Vector3 potPosition = pot.transform.position;
                chip.MoveToPot(targetPos, dealer.transform.position);
            }
        }
    }

    private float DealerGiveChipsToPlayer(int playerIndex, int value)
    {
        HongKongPokerChip chip = PoolService.Instance.Get<HongKongPokerChip>(PrefabType.ChipPlayerHkPoker);;
        chip.transform.position = pot.transform.position;
        Vector2 vtemp = new Vector2(0, 115);
        return chip.MoveToPlayer(vtemp, listPosView[playerIndex], value);
    }
    #endregion

    #region Animation
    
    private async UniTask AnimateCollectCards()
    {
        var delay = 0;
        var zView = 0;
        List<UniTask> tasks = new List<UniTask>();
        
        // Collect cards from all players (both showdown and folded)
        for (var i = 0; i < listPlayerCards.Count; i++)
        {
            var playerCards = listPlayerCards[i];
            if (playerCards == null || playerCards.Count == 0) continue;
            
            // Collect cards from this player (from last to first)
            for (var j = playerCards.Count - 1; j >= 0; j--)
            {
                var card = playerCards[j];
                if (card == null) continue;
                
                // Remove from list before animating
                playerCards.RemoveAt(j);
                
                // Add animation task
                tasks.Add(MoveCardsFinish(card, delay, zView));
                zView++;
                delay += 100;
            }
        }

        // Wait for all card animations to complete
        await UniTask.WhenAll(tasks);
        
        // Hide all card game objects
        foreach (Transform child in cardContainer.transform)
        {
            if (child != null)
            {
                child.gameObject.SetActive(false);
            }
        }

        // Clear all player card lists
        for (var i = 0; i < listPlayerCards.Count; i++)
        {
            if (listPlayerCards[i] != null)
            {
                listPlayerCards[i].Clear();
            }
        }

        // Wait a bit more before finishing
        await UniTask.Delay(TimeSpan.FromMilliseconds(delay + 700));
    }
    
    private async UniTask MoveCardsFinish(CardModel card, float delay, int zView)
    {
        if (card == null || card.transform == null) return;
        
        await UniTask.Delay(TimeSpan.FromMilliseconds(delay));
        
        // Set z-order for card
        card.transform.SetSiblingIndex(50 + zView);
        
        // Get dealer position
        Vector3 dealerPosition = dealer != null 
            ? new Vector3(dealer.transform.position.x, dealer.transform.position.y - 40, dealer.transform.position.z)
            : Vector3.zero;
        
        // Animate card moving to dealer
        await UniTask.WhenAll(
            card.transform.DOMove(dealerPosition, 0.4f)
                .SetEase(Ease.OutCubic)
                .ToUniTask(),
            card.transform.DOScale(new Vector3(0.04f, 0.4f, 1f), 0.15f)
                .OnComplete(() => card.transform.DOScale(new Vector3(0.4f, 0.4f, 1f), 0.15f))
                .ToUniTask(),
            card.transform.DOLocalRotate(new Vector3(0, -15, 0), 0.15f)
                .OnComplete(() =>
                {
                    if (card != null)
                    {
                        card.HideCard(); // Hide card (show card back)
                        card.transform.localRotation = Quaternion.Euler(0, 15, 0);
                        card.transform.DOLocalRotate(Vector3.zero, 0.15f);
                    }
                })
                .ToUniTask()
        );
        
        await UniTask.Delay(TimeSpan.FromSeconds(0.6f));
        
        // Return card to pool
        if (card != null)
        {
            // cardPool.Release(card);
            PoolService.Instance.Release(PrefabType.Card, card);
        }
    }
    
    private IEnumerator AnimateArrowSwapLoop(int repeatCount)
    {
        for (int i = 0; i < repeatCount; i++)
        {
            if (arrowSwapContainer == null) yield break;

            Vector2 startPos = new(0, 0);
            Vector2 endPos = new(0, -25);

            yield return arrowSwapContainer.transform.DOLocalMove(startPos, 0.4f).SetEase(Ease.OutCubic).WaitForCompletion();

            yield return new WaitForSeconds(0.05f);

            yield return arrowSwapContainer.transform.DOLocalMove(endPos, 0.15f).SetEase(Ease.OutCubic).WaitForCompletion();

        }
    }

    private IEnumerator FoldDown(int index, CardModel card, float delay)
    {
        float sk1 = (index <= 2) ? -15f : 15f;
        float sk2 = (index <= 2) ? 15f : -15f;
        yield return new WaitForSeconds(delay);

        Sequence foldSequence = DOTween.Sequence();
        foldSequence.Append(card.transform.DOScale(new Vector3(0f, 0.55f, 1f), 0.15f).SetEase(Ease.OutCubic))
            .AppendCallback(() =>
            {
                if (card != null)
                {
                    card.HideCard();
                    card.SetDark(true);
                }
            })
            .Append(card.transform.DOScale(new Vector3(0.45f, 0.45f, 1f), 0.15f).SetEase(Ease.OutCubic));

        Sequence skewSequence = DOTween.Sequence();
        skewSequence.Append(card.transform.DORotate(new Vector3(0, sk1, 0), 0.15f).SetEase(Ease.OutCubic))
            .AppendCallback(() => card.transform.rotation = Quaternion.Euler(0, sk2, 0))
            .Append(card.transform.DORotate(Vector3.zero, 0.15f).SetEase(Ease.OutCubic));
    }

    private void FoldUp(int index, CardModel card, Card pokerCard)
    {
        float sk1 = (index <= 2) ? -15f : 15f;
        float sk2 = (index <= 2) ? 15f : -15f;

        // Scale animation
        Sequence foldSequence = DOTween.Sequence();
        foldSequence.Append(card.transform.DOScale(new Vector3(0f, 0.65f, 1f), 0.2f).SetEase(Ease.OutCubic))
            .AppendCallback(() =>
            {
                if (card != null)
                {
                    card.ShowCard();
                    card.SetData((int)pokerCard.Rank, (int)pokerCard.Suit);
                }
            })
            .Append(card.transform.DOScale(new Vector3(0.45f, 0.45f, 1f), 0.2f).SetEase(Ease.OutCubic));

        // Skew animation (simulated via rotation)
        Sequence skewSequence = DOTween.Sequence();
        skewSequence.Append(card.transform.DORotate(new Vector3(0, sk1, 0), 0.2f).SetEase(Ease.OutCubic))
            .AppendCallback(() => card.transform.rotation = Quaternion.Euler(0, sk2, 0))
            .Append(card.transform.DORotate(Vector3.zero, 0.2f).SetEase(Ease.OutCubic));
    }
    #endregion

    private async UniTask HandleStartGame()
    {
        Debug.Log("[HK Poker] Handle start game");
        textCountDown.gameObject.SetActive(false);
        await UniTask.Delay(TimeSpan.FromSeconds(0.75f));
        Utility.PlayAnimation(animationStart, "startgame", false);
        
        // Reset betting containers when game starts
        if (buttonBetContainer != null)
        {
            buttonBetContainer.Reset();
            buttonBetContainer.gameObject.SetActive(false);
        }
        
        if (toggleContainer != null)
        {
            toggleContainer.Reset();
            toggleContainer.gameObject.SetActive(false);
        }
        
        // Hide swap container
        if (buttonChangeCardContainer != null)
        {
            buttonChangeCardContainer.gameObject.SetActive(false);
        }
        
        // Hide slider container
        if (sliderContainer != null)
        {
            sliderContainer.SetActive(false);
        }
    }

    private void InitPool()
    {
        PoolService.Instance.Register(PrefabType.Card, cardContainer, cardPrefab.GetComponent<CardModel>(), 20, 50, 30);
        PoolService.Instance.Register(PrefabType.ChipPlayerHkPoker, chipContainer, chipPrefab, 20, 30, 15);
        PoolService.Instance.Register(PrefabType.BoxBetPlayerHkPoker, boxBetContainer, boxBetPrefab, 10, 15, 5);
    }
}
