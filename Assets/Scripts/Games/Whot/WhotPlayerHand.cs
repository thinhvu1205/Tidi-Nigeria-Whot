using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Api;
using DG.Tweening;
using Globals;
using Google.Protobuf;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WhotPlayerHand : MonoBehaviour
{
    [SerializeField] private Transform cardsParent, scoreParent, playerHandParent, remainingCardsParent;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private WhotSuitPicker suitPicker;
    [HideInInspector] public List<WhotCard> cardsInHand = new();
    private WhotView whotGame;
    private const float ANIMATION_TIME = 0.35f;
    private const float CARD_SCALE = 0.86f;
    private const float SCORE_IMAGE_OFFSET = 120f;

    private float REMAINING_CARD_SPACING = CARD_SCALE / 2 * 100f;

    private float CARD_SPACING = 56f;

    public Transform GetCardsParent() => cardsParent;

    private void Awake()
    {
        whotGame = GetComponent<WhotView>();
        whotGame.OnNextTurn += WhotGame_OnNextTurn;

        scoreParent.gameObject.SetActive(false);
    }

    public void PlayACard(WhotCard card)
    {
        card.SetSelectable(false);
        whotGame.PlayACard(card, GetCardsParent());
        EndTurn();
        cardsInHand.Remove(card);
        Destroy(card.gameObject);
        if (cardsInHand.Count == 0)
        {
            whotGame.AnimateLastCardEffect();
        }
        SpreadCards();
    }

    #region Animations

    public void AnimateSortCards()
    {
        Sequence sequence = DOTween.Sequence();
        sequence
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                // Gom tất cả bài về giữa
                foreach (var card in cardsInHand)
                {
                    card.transform.DOLocalMove(GetHandPosition(), 0.25f);
                }
            })
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                SortCards();
                SpreadCards();
            })
            .AppendInterval(0.5f);
    }

    public void AnimateShowRemainingCards(List<Card> cards)
    {
        playerHandParent.gameObject.SetActive(false);
        remainingCardsParent.gameObject.SetActive(true);
        if (cards.Count >= 10)
        {
            REMAINING_CARD_SPACING = CARD_SCALE / 2 * 75;
        }
        float totalWidth = (cards.Count - 1) * REMAINING_CARD_SPACING;
        float startX = -totalWidth / 2f;
        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            WhotCard whotCard = Instantiate(cardPrefab, remainingCardsParent).GetComponent<WhotCard>();
            whotCard.SetInfo(card.Suit, card.Rank);
            whotCard.SetSelectable(false);
            whotCard.transform.localScale = Vector3.one * CARD_SCALE;

            CanvasGroup cardCanvasGroup = whotCard.GetComponent<CanvasGroup>();
            cardCanvasGroup.alpha = 0f;

            Vector3 offset = new Vector3(-20f, 0f, 0f);
            Vector3 targetPos = new Vector3(startX + i * REMAINING_CARD_SPACING, 0f, 0f);

            whotCard.transform.localPosition = targetPos + offset;

            // Animate move & fade
            whotCard.transform.DOLocalMove(targetPos, ANIMATION_TIME).SetEase(Ease.OutCubic).SetDelay(i * 0.05f);
            cardCanvasGroup.DOFade(1f, ANIMATION_TIME / 2).SetDelay(i * 0.1f);

        }
        scoreParent.transform.localPosition = new Vector3(startX + totalWidth + SCORE_IMAGE_OFFSET, 0f, 0f);

    }

    public void SortCards()
    {
        cardsInHand = cardsInHand.OrderBy(c => GetSortValue(c)).ToList();
    }

    public void SpreadCards()
    {
        if (cardsInHand.Count > 14)
        {
            CARD_SPACING = 44f; // Giảm khoảng cách nếu có quá nhiều thẻ
        }
        else
        {
            CARD_SPACING = 56f; // Khoảng cách bình thường
        }
        float totalWidth = (cardsInHand.Count - 1) * CARD_SPACING;
        float startX = -totalWidth / 2f;
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            WhotCard card = cardsInHand[i];
            Vector2 targetPos = new Vector2(startX + i * CARD_SPACING, GetHandPosition().y);
            card.SetLocalPosition(targetPos);
            card.transform.SetSiblingIndex(i);
            card.transform.DOLocalMove(targetPos, 0.1f);
        }
    }

    public void DisplayScore(long totalPoints)
    {
        if (totalPoints > 0)
        {
            CanvasGroup scoreCanvasGroup = scoreParent.GetComponent<CanvasGroup>();
            scoreCanvasGroup.alpha = 0f;
            float totalWidth = (cardsInHand.Count - 1) * CARD_SPACING;
            float startX = -totalWidth / 2f;
            scoreText.text = totalPoints.ToString();
            scoreParent.gameObject.SetActive(true);
            scoreParent.localPosition = new Vector2(startX + totalWidth + 120f, scoreParent.localPosition.y);
            scoreCanvasGroup.DOFade(1f, ANIMATION_TIME / 2);
        }
    }
    
    private void SetCardsToNormal()
    {
        foreach (var card in cardsInHand)
        {
            card.SetNormal();
            card.SetSelectable(false);
        }
    }

    #endregion

    #region Events
    public void WhotGame_OnNextTurn(WhotView.OnNextTurnEventArg e)
    {
        Debug.Log("CALL CARD: " + e.callCard.GetCardRank() + " - " + e.callCard.GetCardSuit());
        if (e.playerTurn == whotGame.GetCurrentPlayer().playerId)
        {
            WhotCard callCard = e.callCard;
            CardEffect cardEffect = e.cardEffect;
            foreach (WhotCard card in cardsInHand.ToList())
            {
                // Debug.Log("Card: " + card.GetCardRank() + " - " + card.GetCardSuit());
                if (callCard.GetCardRank() == CardRank.Rank2 && cardEffect != CardEffect.EffectNone)
                {
                    if (card.GetCardRank() == CardRank.Rank2)
                    {
                        card.SetSelectable(true);
                        card.SetHighLight();
                    }
                    else
                    {
                        card.SetSelectable(false);
                        card.SetDark();
                    }
                    continue;
                }

                if (callCard.GetCardRank() == CardRank.Rank5 && cardEffect != CardEffect.EffectNone)
                {
                    if (card.GetCardRank() == CardRank.Rank5)
                    {
                        card.SetSelectable(true);
                        card.SetHighLight();
                    }
                    else
                    {
                        card.SetSelectable(false);
                        card.SetDark();
                    }
                    continue;
                }

                if (card.GetCardRank() == CardRank.Rank20
                )
                {
                    card.SetSelectable(true);
                    card.SetHighLight();
                    continue;
                }

                if (card.GetCardSuit() == callCard.GetCardSuit() ||
                    card.GetCardRank() == callCard.GetCardRank()
                )
                {
                    card.SetSelectable(true);
                    card.SetHighLight();
                }
                else
                {
                    card.SetSelectable(false);
                    card.SetDark();
                }

            }
        }
    }

    public void WhotCard_OnCardSelected(object sender, WhotCard.OnCardSelectedEventArg e)
    {
        WhotCard selectedCard = sender as WhotCard;
        foreach (var card in cardsInHand.ToList())
        {
            if (card != selectedCard)
            {
                card.Unselect();
            }
            else
            {
                if (e.isSelected)
                {
                    Card cardObject = new()
                    {
                        Suit = card.GetCardSuit(),
                        Rank = card.GetCardRank()
                    };
                    DataSender.SendMatchState((long)OpCodeRequest.PlayCard, cardObject.ToByteArray());
                }
            }
        }
    }
    #endregion

    public void EndTurn()
    {
        foreach (var card in cardsInHand)
        {
            card.Unselect();
        }
        whotGame.GetCurrentPlayer().StopCountDown();
        whotGame.AnimateHideDeckHighlight();
        SetCardsToNormal();
    }

    public void Reset()
    {
        foreach (var card in cardsInHand)
        {
            Destroy(card.gameObject);
        }
        cardsInHand.Clear();
        scoreParent.gameObject.SetActive(false);
    }

    public void HideRemainingCards()
    {
        remainingCardsParent.gameObject.SetActive(false);
    }
    #region Helpers

    private int GetSortValue(WhotCard card)
    {
        int suitOrder = Constants.WhotSuitSortOrder.TryGetValue(card.GetCardSuit(), out var order) ? order : 999;
        int rank = (int)card.GetCardRank();
        return suitOrder * 100 + rank;
    }

    private Vector2 GetHandPosition()
    {
        return new Vector2(0f, 0f);
    }

    public Vector2 GetCardPosition(WhotCard card)
    {
 
        float totalWidth = (cardsInHand.Count - 1) * CARD_SPACING;
        float startX = -totalWidth / 2f;

        int newIndex = cardsInHand.IndexOf(card);
        Vector2 targetPos = new Vector2(startX + newIndex * CARD_SPACING, GetHandPosition().y);

        return targetPos;
    }

    public Vector2 GetNewCardPosition(WhotCard newCard)
    {
        List<WhotCard> sortedCards = new List<WhotCard>(cardsInHand)
        {
            newCard
        };

        sortedCards = sortedCards.OrderBy(c => GetSortValue(c)).ToList();

        float totalWidth = (sortedCards.Count - 1) * CARD_SPACING;
        float startX = -totalWidth / 2f;

        int newIndex = sortedCards.IndexOf(newCard);
        Vector2 targetPos = new Vector2(startX + newIndex * CARD_SPACING, GetHandPosition().y);

        return targetPos;
    }
    #endregion

    
}
