using System;
using System.Collections;
using System.Collections.Generic;
using Api;
using DG.Tweening;
using Globals;
using Spine.Unity;
using TMPro;
using UnityEngine;

public class WhotGame : MonoBehaviour
{
    public event Action<OnCardPlayedEventArg> OnCardPlayed;
    public class OnCardPlayedEventArg : EventArgs
    {
        public bool isCurrentPlayer;
        public WhotCard lastCard;
    }
    [SerializeField] private GameObject whotPlayerPrefab, cardPrefab;
    [SerializeField] private List<Transform> playerPositionsList;
    [SerializeField] private SkeletonGraphic betterLuckNextTimeAnimation, victoryAnimation, matchSymbolAnimation, effectAnimation, lastCardAnimation;
    [SerializeField] private Transform matchResultTransform, yourTurnTransform, wheelTransform, effectAnimationParent,
    victoryAnimationParent, loseAnimationParent, matchSymbolAnimationParent, lastCardAnimationParent, deckOfCardParent, lastCardParent;
    [SerializeField] private TextMeshProUGUI betText, cardsLeftText;
    public WhotCard LastCard { get; private set; }
    private List<WhotPlayer> playersList;
    private List<WhotCard> mockCardsList;

    private WhotPlayerHand playerHand;
    private WhotSuitPicker suitPicker;
    private const float ANIMATION_TIME = 0.5f;
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
        playerHand = GetComponent<WhotPlayerHand>();
        suitPicker = FindFirstObjectByType<WhotSuitPicker>();

        playersList = new();
        mockCardsList = new();

        suitPicker.OnSuitPicked += WhotSuitPicker_OnSuitPicked;
        suitPicker.gameObject.SetActive(false);
        PrepareMockData();
        InitPlayer();
        StartCoroutine(DealCards());
    }

    public void OnDrawCard()
    {
        DrawACard(playerHand.cardsInHand, playerHand.GetCardsParent(), true);
    }

    public IEnumerator DealCards()
    {
        int length = playersList.Count;
        for (int i = 0; i < length * 5; i++)
        {
            int index = i % length;
            WhotPlayer player = playersList[index];
            yield return new WaitForSeconds(0.15f);

            GameObject card = Instantiate(cardPrefab, deckOfCardParent);
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
    }

    public void DrawACard(List<WhotCard> cardsList, Transform targetPosition, bool isCurrentPlayer)
    {
        GameObject card = Instantiate(cardPrefab, deckOfCardParent);
        card.transform.localPosition = Vector3.zero;
        card.transform.localScale = Vector3.one;
        card.transform.SetParent(targetPosition);
        WhotCard whotCard = card.GetComponent<WhotCard>();
        whotCard.SetInfo(CardSuit.SuitCross, CardRank.Rank1);
        whotCard.SetFaceDown();

        if (isCurrentPlayer)
        {
            whotCard.OnCardSelected += playerHand.WhotCard_OnCardSelected;
            AnimateCurrentPlayerDrawACard(whotCard);
        }
        else
        {
            AnimateOtherPlayerDrawACard(whotCard, targetPosition);   
        }
        cardsList.Add(whotCard);
    }

    public void PlayACard(WhotCard card, Transform startingPosition, bool isCurrentPlayer)
    {
        GameObject cardInstance = Instantiate(cardPrefab, startingPosition);
        cardInstance.transform.localPosition = Vector3.zero;
        cardInstance.transform.localScale = Vector3.one;
        cardInstance.transform.SetParent(lastCardParent);
        WhotCard whotCard = cardInstance.GetComponent<WhotCard>();
        whotCard.SetInfo(card.GetCardSuit(), card.GetCardRank());
        whotCard.SetSelectable(false);
        LastCard = whotCard;
        AnimatePlayACard(whotCard);
        HandleCardEffect(whotCard);

        OnCardPlayed?.Invoke(new OnCardPlayedEventArg
        {
            isCurrentPlayer = isCurrentPlayer,
            lastCard = LastCard
        });
    }

    private void HandleCardEffect(WhotCard card)
    {
        switch (card.GetCardRank())
        {
            case CardRank.Rank20:
                AnimateOpenSuitPickerWheel();
                break;
            case CardRank.Rank1:
                AnimateHoldOn();
                break;
            case CardRank.Rank8:
                AnimateSuspension();
                break;
            case CardRank.Rank14:
                HandleGeneralMarketEffect();
                break;
            default:
                Debug.LogWarning($"Unhandled card effect for rank: {card.GetCardRank()}");
                break;
        }
    }
    #region Card Effect
    public void HandleGeneralMarketEffect()
    {
        foreach (WhotPlayer player in playersList)
        {
            if (player.isCurrentPlayer)
            {
                continue;
            }
            else
            {
                player.DrawACard();
            }
        }
        AnimateGeneralMarket();
    }
    #endregion

    #region Animations
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

    private void AnimateSuspension()
    {
        effectAnimationParent.gameObject.SetActive(true);
        Utility.PlayAnimation(effectAnimation, EFFECT_HOLD_ON_ANIMATION_NAME, false);
        effectAnimation.AnimationState.Complete += delegate
        {
            effectAnimationParent.gameObject.SetActive(false);
        };
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

    private void AnimateOpenSuitPickerWheel()
    {
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                wheelTransform.gameObject.SetActive(true);
            });
    }

    private void AnimateDealACard(WhotCard card, Transform targetTransform, WhotPlayer player, int index = 0)
    {
        card.transform
            .DOMove(targetTransform.position, ANIMATION_TIME)
            .SetEase(Ease.InOutCubic)
            .OnComplete(() =>
        {
            if (player.isCurrentPlayer)
            {
                WhotCard card = mockCardsList[index];
                card.transform.SetParent(playerHand.GetCardsParent(), worldPositionStays: false);
                card.transform.localPosition = playerHand.GetNewCardPosition(card);
                card.transform.localScale = Vector3.one;
                playerHand.cardsInHand.Add(card);
                playerHand.SpreadCards();

                if (index == mockCardsList.Count - 1)
                {
                    playerHand.AnimateSortCards();
                }
            }
            else
            {
                WhotCard card = mockCardsList[index];
                player.cards.Add(card);
                player.UpdateCardsLeftVisual();
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
        card.transform.DOMove(lastCardParent.position, ANIMATION_TIME).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            playerHand.SortCards();
            playerHand.SpreadCards();
        });
    }
    #endregion

    #region Events
    private void WhotSuitPicker_OnSuitPicked(CardSuit cardSuit)
    {

        AnimateMatchSymbol(cardSuit);
    }
    #endregion

    public Transform GetLastCardParent() => lastCardParent;
    public Transform GetDeckOfCardParent() => deckOfCardParent;
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
            CardSuit.SuitTriangle,
            CardSuit.SuitCross,
            CardSuit.SuitCircle,
            CardSuit.SuitUnspecified
        };
        CardRank[] ranks =
        {
            CardRank.Rank8,
            CardRank.Rank1,
            CardRank.Rank14,
            CardRank.Rank1,
            CardRank.Rank20
        };
        for (int i = 0; i < 5; i++)
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
            }
        }

        for (int i = 0; i < 4; i++)
        {
            GameObject playerInstance = Instantiate(whotPlayerPrefab, playerPositionsList[i]);
            playerInstance.transform.localPosition = Vector3.zero;
            WhotPlayer player = playerInstance.GetComponent<WhotPlayer>();
            player.SetPlayerInfo(
                null, // Avatar sprite can be set later
                $"Player {i + 1}",
                1000
            );
            player.SetWhotGame(this);
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
        }
    }
    #endregion
}
