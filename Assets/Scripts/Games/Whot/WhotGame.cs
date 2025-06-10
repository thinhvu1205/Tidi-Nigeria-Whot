using System;
using System.Collections;
using System.Collections.Generic;
using Api;
using DG.Tweening;
using Globals;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WhotGame : MonoBehaviour
{
    public event Action<OnNextTurnEventArg> OnNextTurn;
    public class OnNextTurnEventArg : EventArgs
    {
        public string playerTurn;
        public WhotCard callCard;
        public CardSuit? cardSuit;
    }
    [SerializeField] private GameObject whotPlayerPrefab, cardPrefab;
    [SerializeField] private List<Transform> playerPositionsList;
    [SerializeField] private SkeletonGraphic betterLuckNextTimeAnimation, victoryAnimation, matchSymbolAnimation, effectAnimation, lastCardAnimation;
    [SerializeField] private Transform matchResultTransform, yourTurnTransform, wheelTransform, effectAnimationParent,
    victoryAnimationParent, loseAnimationParent, matchSymbolAnimationParent, lastCardAnimationParent, deckOfCardParent,
    callCardParent, playAreaParent;
    [SerializeField] private TextMeshProUGUI betText, cardsLeftText, betterLuckNextTimeText;
    [SerializeField] private Image waitImage, victoryImage, deckHighlightImage;
    [SerializeField] WhotSuitPicker suitPicker;
    public WhotCard CallCard { get; private set; }
    public List<string> PlayerIdsList { get; private set; }
    private int currentTurnIndex = 5;
    private List<WhotPlayer> playersList;
    private Stack<WhotCard> deck;
    private List<WhotCard> mockCardsList;

    private WhotPlayerHand playerHand;
    private Tween waitRotationTween;
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
        VICTORY_MARKET_ANIMATION_NAME = "victory";
        

    private void Awake()
    {
        playersList = new();
        mockCardsList = new();
        deck = new();
        PlayerIdsList = new();
        HandleResetGame();
    }

    public void HandleResetGame()
    {
        DOTween.KillAll(true);
        playerHand = GetComponent<WhotPlayerHand>();
        playerHand.Reset();
        playersList.Clear();
        mockCardsList.Clear();
        suitPicker.OnSuitPicked += WhotSuitPicker_OnSuitPicked;
        playAreaParent.gameObject.SetActive(true);
        suitPicker.gameObject.SetActive(false);
        deckHighlightImage.gameObject.SetActive(false);
        CallCard = null;
        foreach (Transform child in callCardParent)
        {
            Destroy(child.gameObject);
        }
        PrepareMockData();
        InitPlayer();
        StartCoroutine(DealCards());
    }
    #region UI 
    private void UpdateCardsLeft()
    {
        cardsLeftText.text = deck.Count.ToString();
    }

    private void ShowYourTurn()
    {
        yourTurnTransform.gameObject.SetActive(true);

    }

    private void HideYourTurn()
    {
        yourTurnTransform.gameObject.SetActive(false);
    }
    #endregion

    public void NextTurn(CardSuit cardSuit = CardSuit.SuitUnspecified)
    {
        // Debug.Log("Next Turn: " + GetCurrentPlayerTurnId());
        if (currentTurnIndex >= playersList.Count - 1)
        {
            currentTurnIndex = 0;
        }
        else
        {
            currentTurnIndex++;
        }
        if (GetCurrentPlayerTurnId() == GetCurrentPlayer().playerId)
        {
            ShowYourTurn();
            AnimateHighlightDeck();
        }
        else
        {
            HideYourTurn();
        }
        OnNextTurn?.Invoke(new OnNextTurnEventArg
        {
            playerTurn = GetCurrentPlayerTurnId(),
            callCard = CallCard,
            cardSuit = cardSuit
        });
    }

    public void SkipTurn()
    {
        if (currentTurnIndex >= playersList.Count - 1)
        {
            currentTurnIndex = 0;
        }
        else
        {
            currentTurnIndex++;
        }
    }

    #region Card Actions
    public void OnDrawACard()
    {
        DrawACard(playerHand.cardsInHand, playerHand.GetCardsParent(), true);
        playerHand.EndTurn();
        NextTurn();
    }

    public IEnumerator DealCards()
    {
        int length = playersList.Count;
        for (int i = 0; i < length * 6; i++)
        {
            int index = i % length;
            WhotPlayer player = playersList[index];
            yield return new WaitForSeconds(0.1f);

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

    public void DrawACard(List<WhotCard> cardsList, Transform targetPosition, bool isCurrentPlayer)
    {
        if (deck.Count == 0)
        {
            AnimateShowRemainingCards();
            return;
        }
        GameObject card = Instantiate(cardPrefab, GetDeckOfCardParent());
        card.transform.localPosition = Vector3.zero;
        card.transform.localScale = Vector3.one;
        card.transform.SetParent(targetPosition);
        WhotCard topCard = deck.Pop();
        WhotCard whotCard = card.GetComponent<WhotCard>();
        whotCard.SetInfo(topCard.GetCardSuit(), topCard.GetCardRank());
        whotCard.SetFaceDown();

        if (isCurrentPlayer)
        {
            whotCard.OnCardSelected += playerHand.WhotCard_OnCardSelected;
            AnimateCurrentPlayerDrawACard(whotCard);
            GetCurrentPlayer().cards.Add(whotCard);
        }
        else
        {
            AnimateOtherPlayerDrawACard(whotCard, targetPosition);
        }
        cardsList.Add(whotCard);
        UpdateCardsLeft();
    }

    public void PlayACard(WhotPlayer player, WhotCard card, Transform startingPosition)
    {
        GameObject cardInstance = Instantiate(cardPrefab, startingPosition);
        cardInstance.transform.localPosition = Vector3.zero;
        cardInstance.transform.localScale = Vector3.one;
        cardInstance.transform.SetParent(callCardParent);
        WhotCard whotCard = cardInstance.GetComponent<WhotCard>();
        whotCard.SetInfo(card.GetCardSuit(), card.GetCardRank());
        whotCard.SetSelectable(false);
        CallCard = whotCard;

        WhotPlayer whotPlayer = playersList.Find(player => player.isCurrentPlayer);
        if (player.isCurrentPlayer)
        {
            whotPlayer.cards.Remove(card);
            whotPlayer.UpdateCardsLeftVisual();

            if (whotPlayer.cards.Count == 0)
            {
                whotPlayer.isWinner = true;
                
            }
        }
        AnimatePlayACard(whotCard);
        HandleCardEffect(player, whotCard);
        if (
            whotCard.GetCardRank() != CardRank.Rank20 &&
            !Constants.WhotUndefensableCards.Contains(whotCard.GetCardRank())
        )
        {
            NextTurn();
        }
    }
    #endregion

    #region Card Effects
    private void HandleCardEffect(WhotPlayer player, WhotCard card)
    {
        if (player.cards.Count == 0)
        {
            return;
        }
        switch (card.GetCardRank())
        {
            case CardRank.Rank20:
                AnimateOpenSuitPickerWheel(player);
                break;
            case CardRank.Rank1:
                HandleHoldOnEffect(player);
                break;
            case CardRank.Rank2:
                HandlePick2Effect(player);
                break;
            case CardRank.Rank5:
                HandlePick3Effect(player);
                break;
            case CardRank.Rank8:
                HandleSuspensionEffect();
                break;
            case CardRank.Rank14:
                HandleGeneralMarketEffect(player);
                break;
            default:
                Debug.LogWarning($"Unhandled card effect for rank: {card.GetCardRank()}");
                break;
        }
    }

    private void HandleHoldOnEffect(WhotPlayer activePlayer)
    {
        foreach (WhotPlayer player in playersList)
        {
            if (player.playerId != activePlayer.playerId)
            {
                SkipTurn();
                player.AnimateShowHoldOn();
                if (player.playerId != activePlayer.playerId)
                {
                    player.UpdateEffectNoti("Hold On");
                }
            }
        }
        NextTurn();
        AnimateHoldOn();
    }

    private void HandlePick2Effect(WhotPlayer activePlayer)
    {
        AnimateDrawMultipleCards(activePlayer, 2, "Pick 2");
    }

    private void HandlePick3Effect(WhotPlayer activePlayer)
    {
        AnimateDrawMultipleCards(activePlayer, 3, "Pick 3");
    }

    private void HandleSuspensionEffect()
    {
        SkipTurn();
        foreach (WhotPlayer player in playersList)
        {
            if (player.playerId == GetCurrentPlayerTurnId())
            {
                player.AnimateShowSuspension();
            }
        }
        NextTurn();
    }
    private void HandleGeneralMarketEffect(WhotPlayer activePlayer)
    {
        foreach (WhotPlayer player in playersList)
        {
            if (player.playerId != activePlayer.playerId)
            {
                player.AnimateShowSuspension();
            }
        }
        AnimateDrawMultipleCards(activePlayer, 1);
        AnimateGeneralMarket();
    }
    #endregion

    #region Animations
    private void AnimateChooseCallCard()
    {
        WhotCard topCard = deck.Pop();
        UpdateCardsLeft();
        CallCard = topCard;
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

    private void AnimateDrawMultipleCards(WhotPlayer activePlayer, int numberOfCards, string effectName = "")
    {
        foreach (WhotPlayer player in playersList)
        {
            if (player.playerId == activePlayer.playerId)
            {
                continue;
            }
            if (!player.isCurrentPlayer)
            {
                Sequence sequence = DOTween.Sequence();
                sequence.JoinCallback(() =>
                {
                    if (effectName != "")
                        player.UpdateEffectNoti(effectName);
                });
                for (int i = 0; i < numberOfCards; i++)
                {
                    sequence
                        .AppendCallback(() => player.DrawACard())
                        .AppendInterval(0.2f);
                }
            }
            else
            {
                Sequence sequence = DOTween.Sequence();
                for (int i = 0; i < numberOfCards; i++)
                {
                    sequence
                        .AppendCallback(() => OnDrawACard())
                        .AppendInterval(0.2f);
                }
            }
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

    public void AnimateLastCard()
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
                    _ => throw new System.ArgumentOutOfRangeException(nameof(suit), suit, null)
                };

                Utility.PlayAnimation(matchSymbolAnimation, animationName, false);
                matchSymbolAnimation.AnimationState.Complete += delegate
                {
                    matchSymbolAnimationParent.gameObject.SetActive(false);
                };

            });
    }

    private void AnimateOpenSuitPickerWheel(WhotPlayer activePlayer)
    {
        if (activePlayer.playerId != GetCurrentPlayer().playerId)
        {
            AnimateWait();
        }
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                wheelTransform.gameObject.SetActive(true);
            });
    }


    public void AnimateShowRemainingCards()
    {
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                foreach (WhotPlayer player in playersList)
                {
                    if (!player.isCurrentPlayer)
                    {
                        player.HideCardsLeft();
                        player.AnimateShowRemainingCards();
                    }
                    else
                    {
                        playerHand.DisplayScore();
                    }
                }
            })
            .AppendInterval(5f)
            .AppendCallback(() =>
            {
                if (GetCurrentPlayer().isWinner)
                {
                    AnimateVictory();
                }
                else
                {
                    AnimateBetterLuckNextTime();
                }

            });
    }
    public void AnimateVictory()
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
        Utility.PlayAnimation(victoryAnimation, VICTORY_MARKET_ANIMATION_NAME, false);
        victoryAnimation.AnimationState.Complete += delegate
        {
            victoryAnimationParent.gameObject.SetActive(false);
            Sequence chipSequence = DOTween.Sequence();
            foreach (WhotPlayer player in playersList)
            {
                if (!player.isCurrentPlayer)
                {

                    player.AnimateChipTransfer(GetCurrentPlayer().GetPlayedCardParent());
                }
            }
            chipSequence.AppendInterval(2.5f);
            chipSequence.OnComplete(() =>
            {
                HandleShowMatchResult(true);
            });
        };
                
    }

    public void AnimateBetterLuckNextTime()
    {
        Debug.Log("Length: " + playersList.Count);
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
                if (!player.isWinner)
                {
                    player.AnimateChipTransfer(GetWinner().GetPlayedCardParent());
                }
            }
            chipSequence.AppendInterval(2.5f);
            chipSequence.OnComplete(() =>
            {
                HandleShowMatchResult(false);
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
                WhotCard whotCard = mockCardsList[index];
                player.cards.Add(whotCard);
                player.UpdateCardsLeftVisual();

                if (player.isCurrentPlayer)
                {
                    whotCard.transform.SetParent(playerHand.GetCardsParent(), worldPositionStays: false);
                    whotCard.transform.localPosition = playerHand.GetNewCardPosition(whotCard);
                    whotCard.transform.localScale = Vector3.one;
                    playerHand.cardsInHand.Add(whotCard);
                    playerHand.SpreadCards();

                    if (index == mockCardsList.Count - 1)
                    {
                        playerHand.AnimateSortCards();
                    }
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
    private void WhotSuitPicker_OnSuitPicked(CardSuit cardSuit)
    {
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendCallback(() =>
            {
                AnimateMatchSymbol(cardSuit);
                StopWaitAnimation();
            })
            .AppendInterval(0.5f)
            .AppendCallback(() => NextTurn(cardSuit));
    }
    #endregion

    private void HandleShowMatchResult(bool isVictory)
    {
        matchResultTransform.gameObject.SetActive(true);
        WhotMatchResult matchResult = matchResultTransform.GetComponent<WhotMatchResult>();
        matchResult.SetInfo(this, playersList, isVictory);
    }

    #region Getters
    public Transform GetCallCardParent() => callCardParent;
    public Transform GetDeckOfCardParent() => deckOfCardParent;
    public WhotPlayer GetCurrentPlayer() => playersList.Find(player => player.isCurrentPlayer);
    private WhotPlayer GetWinner() => playersList.Find(player => player.isWinner);
    public string GetCurrentPlayerTurnId() => PlayerIdsList[currentTurnIndex];
    #endregion

    private void InitPlayer()
    {
        for (int i = 0; i < playersList.Count; i++)
        {
            WhotPlayer player = playersList[i];
            PlayerLayout playerLayout = player.GetComponent<PlayerLayout>();
            switch (i)
            {
                case 0:
                    playerLayout.SetLayout(PlayerLayout.EPlayerLayout.Top);
                    break;
                case 1:
                    playerLayout.SetLayout(PlayerLayout.EPlayerLayout.Top);
                    break;
                case 2:
                    playerLayout.SetLayout(PlayerLayout.EPlayerLayout.Left);
                    break;
                case 3:
                    playerLayout.SetLayout(PlayerLayout.EPlayerLayout.Right);
                    break;

            }
        }
    }

    #region Mock Data
    private void PrepareMockData()
    {
        CardSuit[] suits =
        {
            CardSuit.SuitCircle,
            CardSuit.SuitUnspecified,
            CardSuit.SuitCircle,
            CardSuit.SuitCross,
            CardSuit.SuitCircle,
            CardSuit.SuitCircle
        };
        CardRank[] ranks =
        {
            CardRank.Rank8,
            CardRank.Rank20,
            CardRank.Rank1,
            CardRank.Rank14,
            CardRank.Rank5,
            CardRank.Rank2
        };
        for (int i = 0; i < 6; i++)
        {
            GameObject cardInstance = Instantiate(cardPrefab);
            if (cardInstance.TryGetComponent<WhotCard>(out var card))
            {
                card.OnCardSelected += playerHand.WhotCard_OnCardSelected;

                card.SetInfo(
                    suits[i],
                    ranks[i]
                );
                mockCardsList.Add(card);
                deck.Push(card);
                deck.Push(card);
                deck.Push(card);
                deck.Push(card);
            }
        }

        UpdateCardsLeft();

        for (int i = 0; i < 4; i++)
        {
            foreach (Transform transform in playerPositionsList[i])
            {
                // transform.gameObject.SetActive(false);
                Destroy(transform.gameObject);
            }
            GameObject playerInstance = Instantiate(whotPlayerPrefab, playerPositionsList[i]);
            WhotPlayer player = playerInstance.GetComponent<WhotPlayer>();
            playerInstance.transform.localPosition = Vector3.zero;
            player.SetPlayerInfo(
                (i + 1).ToString(),
                null, // Avatar sprite can be set later
                $"Player {i + 1}",
                1000
            );
            player.SetWhotGame(this);
            player.isWinner = false;
            if (i == 0)
            {
                player.isCurrentPlayer = true;
                player.HideCardsLeft();
            }
            else
            {
                player.isCurrentPlayer = false;
                player.ShowCardsLeft();
            }
            playersList.Add(player);
            PlayerIdsList.Add(player.playerId);
        }
    }
    #endregion
}
