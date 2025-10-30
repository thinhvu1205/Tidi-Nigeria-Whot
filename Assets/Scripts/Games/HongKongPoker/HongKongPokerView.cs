using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Games.Card;
using Globals;
using Nakama;
using Proto;
using Spine.Unity;
using TMPro;
using UnityEngine;
using GameState = Proto.GameState;

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
    [SerializeField] private GameObject cardPrefab;
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

    private UnityEngine.Pool.ObjectPool<HongKongPokerBoxBet> boxBetPool;
    private UnityEngine.Pool.ObjectPool<HongKongPokerChip> chipPool;
    private UnityEngine.Pool.ObjectPool<CardModel> cardPool;
    private GameState gameState = GameState.Preparing;

    protected override void Awake()
    {
        base.Awake();
        InitPool();
        InitPlayers();
        InitPlayerCards();
        buttonBetContainer.SetValues(10000, 2000, 4000);
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
    }

    public override void HandleUpdateUserInTable(IMatchState matchState)
    {
        base.HandleUpdateUserInTable(matchState);
        var updateTable = UpdateTable.Parser.ParseFrom(matchState.State);
        Debug.Log($"[HK Poker] Update User In Table: Players={updateTable.Players.Count}");
        
        // Update player positions (rearrange to put local player at index 0)
        UpdatePosUserTable(updateTable, isRearrange: true);
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
                break;
            case GameState.Play:
                HandleStartGame();
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
        Debug.Log("Update Turn: " + data);
    }

    public override void HandleFinish(IMatchState matchState)
    {
        base.HandleFinish(matchState);
        var data = UpdateFinish.Parser.ParseFrom(matchState.State);
        Debug.Log("Update Finish: " + data);
    }
    public override void HandleUpdateWallet(IMatchState matchState)
    {
        base.HandleUpdateWallet(matchState);
        var data = BalanceResult.Parser.ParseFrom(matchState.State);
        Debug.Log("Update Wallet: " + data);
    }

    // public override void HandleUpdateKickOffTheTable(IMatchState matchState)
    // {
    //     Destroy(gameObject);
    // }

    #region API Handlers

    public override void HandleUpdatePlayerAction(IMatchState matchState)
    {
        base.HandleUpdatePlayerAction(matchState);
        var data = HKUpdatePlayerAction.Parser.ParseFrom(matchState.State);
        Debug.Log($"[HK Poker] Player Action: User={data.UserId}, Action={data.Action}, Amount={data.Amount}, Pot={data.NewPot}");
        
        // Get player index
        int playerIndex = GetPlayerIndex(data.UserId);
        if (playerIndex < 0) return;
        
        // Update pot display
        pot.SetValue((int)data.NewPot);
        
        // Show action text on player
        string actionText = GetActionText(data.Action);
        ShowPlayerAction(playerIndex, data.Action, (int)data.Amount);
        
        // Animate chips to pot
        if (data.Amount > 0)
        {
            PlayerGiveChipsToBoxbet(playerIndex);
        }
        
        // Update betting state UI
        if (data.BettingState != null)
        {
            UpdateBettingStateUI(data.BettingState);
        }
    }

    public override void HandleUpdateNewRound(IMatchState matchState)
    {
        base.HandleUpdateNewRound(matchState);
        var data = HKUpdateNewRound.Parser.ParseFrom(matchState.State);
        Debug.Log($"[HK Poker] New Round: Round={data.Round}");
        
        // Clear previous round UI
        ClearRoundUI();
        
        // Deal new cards
        if (data.PlayerCards != null && data.PlayerCards.Count > 0)
        {
            foreach (var playerCards in data.PlayerCards)
            {
                int playerIndex = GetPlayerIndex(playerCards.UserId);
                if (playerIndex < 0) continue;
                
                // Deal face-up cards
                foreach (var card in playerCards.FaceUpCards)
                {
                    DealACard(card, playerIndex, 0.25f);
                }
            }
        }
        
        // Update UI for new round
        UpdateRoundUI(data.Round);
        
        // Highlight first bettor
        if (!string.IsNullOrEmpty(data.FirstBettor))
        {
            int firstBettorIndex = GetPlayerIndex(data.FirstBettor);
            HighlightPlayer(firstBettorIndex);
        }
    }

    public override void HandleUpdateCardSwap(IMatchState matchState)
    {
        base.HandleUpdateCardSwap(matchState);
        var data = HKUpdateCardSwap.Parser.ParseFrom(matchState.State);
        Debug.Log($"[HK Poker] Card Swap: User={data.UserId}, Swapped={data.Swapped}");
        
        int playerIndex = GetPlayerIndex(data.UserId);
        if (playerIndex < 0) return;
        
        if (data.Swapped)
        {
            // Show swap animation
            ShowSwapAnimation(playerIndex, data.NewFaceUpCard);
        }
        else
        {
            // Show "Keep" text
            // ShowPlayerAction(playerIndex, "KEEP", 0);
        }
    }

    public override void HandleUpdateShowdown(IMatchState matchState)
    {
        base.HandleUpdateShowdown(matchState);
        var data = HKUpdateShowdown.Parser.ParseFrom(matchState.State);
        Debug.Log($"[HK Poker] Showdown: Winners={string.Join(", ", data.Winners)}");
        
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
                ShowHandRank(playerIndex, result.HandName);
                
                // Show winnings
                if (result.Winnings > 0)
                {
                    ShowWinAnimation(playerIndex, (int)result.Winnings);
                }
            }
        }
        
        // Distribute pot to winners
        if (data.Winners != null && data.Winners.Count > 0)
        {
            DOTween.Sequence()
                .AppendInterval(2f)
                .AppendCallback(() =>
                {
                    foreach (var winnerId in data.Winners)
                    {
                        int winnerIndex = GetPlayerIndex(winnerId);
                        if (winnerIndex >= 0)
                        {
                            DealerGiveChipsToPlayer(winnerIndex);
                        }
                    }
                });
        }
    }

    #endregion

    #region Helper 

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

    private string GetActionText(HKPokerAction action)
    {
        return action switch
        {
            HKPokerAction.HkActionFold => "FOLD",
            HKPokerAction.HkActionCheck => "CHECK",
            HKPokerAction.HkActionCall => "CALL",
            HKPokerAction.HkActionRaise => "RAISE",
            HKPokerAction.HkActionBet => "BET",
            HKPokerAction.HkActionAllIn => "ALL-IN",
            _ => ""
        };
    }

    private void ShowPlayerAction(int playerIndex, HKPokerAction action, int amount)
    {
        // TODO: Show action text above player
        // Example: Show text with fade-in/fade-out animation
        Debug.Log($"Player {playerIndex} action: {action} {(amount > 0 ? amount.ToString() : "")}");
        
        // Show in BoxBet if available
        if (playerIndex >= 0 && playerIndex < listBoxBet.Count)
        {
            var boxBet = listBoxBet[playerIndex];
            if (boxBet != null)
            {
                // Update box bet display with action
                boxBet.SetInfo(action, playerIndex, amount);
            }
        }
    }

    private void UpdateBettingStateUI(HKBettingState bettingState)
    {
        // Update button container with current betting state
        if (buttonBetContainer != null)
        {
            // Enable/disable buttons based on available actions
            // Update min/max bet values
            buttonBetContainer.SetValues(
                (int)bettingState.CurrentBet,
                (int)bettingState.MinRaise,
                (int)bettingState.CurrentBet + (int)bettingState.MinRaise
            );
        }
    }

    private void ClearRoundUI()
    {
        // Clear previous round displays
        Debug.Log("Clearing round UI");
    }

    private void UpdateRoundUI(HKPokerRound round)
    {
        // Update UI based on current round
        Debug.Log($"Update round UI: {round}");
        
        // Show round indicator
        // Enable/disable card swap button for Round 4
        if (round == HKPokerRound.HkRound4Card)
        {
            buttonChangeCardContainer?.gameObject.SetActive(true);
        }
    }

    private void HighlightPlayer(int playerIndex)
    {
        // Highlight current player turn
        Debug.Log($"Highlight player {playerIndex}");
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
        
        // Show all 5 cards face-up
        var playerCards = listPlayerCards[playerIndex];
        for (int i = 0; i < cards.Count && i < playerCards.Count; i++)
        {
            playerCards[i].ShowCard();
        }
    }

    private void ShowHandRank(int playerIndex, string handName)
    {
        // Show hand rank name above player's cards
        Debug.Log($"Player {playerIndex} hand: {handName}");
        // TODO: Show UI text with hand rank
    }

    private void ShowWinAnimation(int playerIndex, int winnings)
    {
        // Show win animation and amount
        Debug.Log($"Player {playerIndex} wins: {winnings}");
        
        // Get player from rearrangedPlayers
        if (playerIndex >= 0 && playerIndex < rearrangedPlayers.Count)
        {
            var player = rearrangedPlayers[playerIndex];
            if (userIdToView.TryGetValue(player.Id, out var playerView))
            {
                // Play win effect on player view
                // playerView.SetEffectWin();
                // playerView.EffectFlyMoney(winnings);
                
                Debug.Log($"Win effect for {player.UserName}: {winnings} chips");
            }
        }
    }

    private void DealerGiveChipsToPlayer(int playerIndex)
    {
        // Animate chips from dealer to player
        Debug.Log($"Dealer gives chips to player {playerIndex}");
        // TODO: Implement chip animation from dealer to player
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
                    cardPool.Release(card);
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
                boxBetPool.Release(boxBet);
                listBoxBet[playerIndex] = null;
                Debug.Log($"[HK Poker] Removed box bet for player {leavePlayerId} at index {playerIndex}");
            }
        }
    }

    #endregion

    #endregion

    #region Button
    public void OnClickSwap()
    {

    }

    public void OnClickCancel()
    {

    }

    public void OnClickSendTip()
    {

    }

    #endregion

    #region Card Actions
    private void DealACard(Card pokerCard, int playerIndex, float delay = 0f, bool isAnimate = true)
    {
        List<CardModel> playerCards = listPlayerCards[playerIndex];
        Vector2 basePosition = listCardPosition[playerIndex];
        Vector2 targetPosition;

        bool isLeftTable = !(basePosition.x > 0);

        CardModel card = cardPool.Get();
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
            if (pokerCard.Rank != CardRank.RankUnspecified && pokerCard.Suit != CardSuit.SuitUnspecified)
            {
                // SoundManager.Instance.PlaySound("sound_cardFlipBJ");
                // playSound(SOUND_GAME.CARD_FLIP_2);
                card.SetData((int)pokerCard.Rank, (int)pokerCard.Suit);
                card.ShowCard();
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
        }
        playerCards.Add(card);
    }

    private void ReturnAllCardsToDealer()
    {
        int zIndex = 0;
        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < listPlayerCards.Count; i++)
        {
            List<CardModel> currentPlayerCards = listPlayerCards[i];
            for (int j = currentPlayerCards.Count - 1; j >= 0; j--)
            {
                CardModel card = currentPlayerCards[j];
                currentPlayerCards.RemoveAt(j);
                sequence
                    .AppendCallback(() =>
                    {
                        ReturnACard(card, zIndex);
                    })
                    .AppendInterval(0.1f);
                zIndex++;
            }
        }
        sequence
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                foreach (Transform child in cardContainer.transform)
                {
                    child.gameObject.SetActive(false);
                }
            });
        // await UniTask.Delay((delay + 700) / 1000);
    }

    private void ReturnACard(CardModel card, int zIndex)
    {
        card.transform.SetSiblingIndex(50 + zIndex);
        card.transform.DOMove(new Vector2(dealer.transform.position.x, dealer.transform.position.y - 40), 0.4f)
            .SetEase(Ease.OutCubic);
        card.transform.DOScale(new Vector3(0.04f, 0.4f, 1f), 0.15f)
            .OnComplete(() => card.transform.DOScale(new Vector3(0.4f, 0.4f, 1f), 0.15f));
        card.transform.DOLocalRotate(new Vector3(0, -15, 0), 0.15f)
            .OnComplete(() =>
            {
                card.HideCard();
                card.transform.localRotation = Quaternion.Euler(0, 15, 0);
                card.transform.DOLocalRotate(Vector3.zero, 0.15f);
            });


        // await UniTask.Delay(TimeSpan.FromSeconds(0.6f));
    }
    #endregion

    #region Boxbet Actions
    private void ShowBoxBet(HKPokerAction status, int index, int chip)
    {
        HongKongPokerBoxBet boxbet = boxBetPool.Get();
        listBoxBet[index] = boxbet;
        boxbet.transform.localPosition = listBoxBetPosition[index];
        boxbet.SetInfo(status, index, chip);
    }
    #endregion

    #region Chip Actions
    private void PlayerGiveChipsToBoxbet(int index)
    {
        int valueBoxBet = listBoxBet[index].Chip;
        // if ((int)data["chipStack"] - preNextStack != 0)
        // {
        //     currentPlayer.playerView.effectFlyMoney(-valueBoxBet);
        // }

        int numberChip = 0;
        if (valueBoxBet > 0)
        {
            numberChip = Mathf.Clamp((int)Mathf.Ceil((float)valueBoxBet / MarkUnit), 1, 4);
        }

        Vector2 targetPosition = listBoxBetPosition[index];
        Vector2 startPosition = listCardPosition[index];

        for (int i = 0; i < numberChip; i++)
        {
            HongKongPokerChip chip = chipPool.Get();
            chip.transform.localScale = Vector3.one * 0.4f;
            chip.transform.localPosition = startPosition;

            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(i * 0.1f).AppendCallback(() => chip.MoveToBoxBet(targetPosition));
                
        }
    }

    private void BoxBetGiveChipsToDealer(int index)
    {
        if (listBoxBet[index].Chip > 0)
        {
            for (int i = 0; i < 4; i++)
            {
                HongKongPokerChip chip = chipPool.Get();
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

    private void DealerGiveChipsToPlayer()
    {
        HongKongPokerChip chip = chipPool.Get();
        chip.transform.position = pot.transform.position;

        Vector2 vtemp = new Vector2(0, 115);
        chip.MoveToPlayer(vtemp, listCardPosition[0], 3000);
    }
    #endregion

    #region Animation
    private IEnumerator AnimateArrowSwapLoop()
    {
        for (int i = 0; i < 10; i++)
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

    private void HandleStartGame()
    {
        Debug.Log("handle start game");
        textCountDown.gameObject.SetActive(false);
        Utility.PlayAnimation(animationStart, "startgame", false);
    }

    private void InitPool()
    {
        chipPool = new UnityEngine.Pool.ObjectPool<HongKongPokerChip>(
            createFunc: () =>
            {
                var chip = Instantiate(chipPrefab, chipContainer);
                chip.gameObject.SetActive(false); // bắt đầu ẩn
                return chip.GetComponent<HongKongPokerChip>();
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
                Destroy(chip);
            },
            defaultCapacity: 1,     // số lượng khởi tạo
            maxSize: 50             // tối đa object trong pool
        );
        boxBetPool = new UnityEngine.Pool.ObjectPool<HongKongPokerBoxBet>(
            createFunc: () =>
            {
                var boxBet = Instantiate(boxBetPrefab, boxBetContainer);
                boxBet.gameObject.SetActive(false); // bắt đầu ẩn
                return boxBet.GetComponent<HongKongPokerBoxBet>();
            },
            actionOnGet: (boxBet) =>
            {
                boxBet.gameObject.SetActive(true);
                boxBet.transform.localScale = Vector3.one;
            },
            actionOnRelease: (boxBet) =>
            {
                boxBet.gameObject.SetActive(false);
            },
            actionOnDestroy: (boxBet) =>
            {
                Destroy(boxBet);
            },
            defaultCapacity: 1,     // số lượng khởi tạo
            maxSize: 50             // tối đa object trong pool
        );
        cardPool = new UnityEngine.Pool.ObjectPool<CardModel>(
            createFunc: () =>
            {
                var card = Instantiate(cardPrefab, cardContainer);
                card.SetActive(false); // bắt đầu ẩn
                return card.GetComponent<CardModel>();
            },
            actionOnGet: (card) =>
            {
                card.gameObject.SetActive(true);
                card.transform.localScale = new Vector3(0.45f, 0.45f, 1);
                card.transform.localRotation = Quaternion.Euler(0, 0, 0);
            },
            actionOnRelease: (card) =>
            {
                card.gameObject.SetActive(false);
            },
            actionOnDestroy: (card) =>
            {
                Destroy(card);
            },
            defaultCapacity: 1,     // số lượng khởi tạo
            maxSize: 50             // tối đa object trong pool
        );
    }

    private void InitPlayers()
    {

    }

    private void InitPlayerCards()
    {
        // CardModel card1 = new();
        // card1.SetData(1, 1);
        // CardModel card2 = new();
        // card1.SetData(2, 2);
        // CardModel card3 = new();
        // card1.SetData(3, 3);
        // CardModel card4 = new();
        // card1.SetData(4, 4);
        // List<CardModel> cardModels = new() {card1, card2, card3, card4};
        // listPlayerCards[0] = cardModels;

    }
}
