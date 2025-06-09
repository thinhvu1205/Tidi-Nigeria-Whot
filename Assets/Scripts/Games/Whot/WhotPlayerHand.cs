using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Api;
using DG.Tweening;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WhotPlayerHand : MonoBehaviour
{
    [SerializeField] private Transform cardsParent, scoreParent;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private WhotSuitPicker suitPicker;
    [HideInInspector] public List<WhotCard> cardsInHand = new();
    
    private WhotGame whotGame;

    private const float CARD_SPACING = 56f;
    private const float ANIMATION_TIME = 0.35f;

    public Transform GetCardsParent() => cardsParent;

    private void Awake()
    {
        whotGame = GetComponent<WhotGame>();
        whotGame.OnCardPlayed += WhotGame_OnCardPlayed;
        suitPicker.OnSuitPicked += WhotSuitPicker_OnSuitPicked;

        scoreParent.gameObject.SetActive(false);
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
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                SortCards();
                SpreadCards();

            });
    }

    public void SortCards()
    {
        cardsInHand = cardsInHand.OrderBy(c => GetSortValue(c)).ToList();
    }

    public void SpreadCards()
    {
        float totalWidth = (cardsInHand.Count - 1) * CARD_SPACING;
        float startX = -totalWidth / 2f;
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            WhotCard card = cardsInHand[i];
            Vector2 targetPos = new Vector2(startX + i * CARD_SPACING, GetHandPosition().y);
            card.SetLocalPosition(targetPos);
            card.transform.SetSiblingIndex(i);
            card.transform.DOLocalMove(targetPos, 0.25f);
        }
    }

    public void DisplayScore()
    {
        CanvasGroup scoreCanvasGroup = scoreParent.GetComponent<CanvasGroup>();
        scoreCanvasGroup.alpha = 0f;
        float totalWidth = (cardsInHand.Count - 1) * CARD_SPACING;
        float startX = -totalWidth / 2f;
        scoreText.text = "20";
        scoreParent.gameObject.SetActive(true);
        scoreParent.localPosition = new Vector2(startX + totalWidth + 120f, scoreParent.localPosition.y);
        scoreCanvasGroup.DOFade(1f, ANIMATION_TIME / 2);

    }
    #endregion

    #region Events
    public void WhotGame_OnCardPlayed(WhotGame.OnCardPlayedEventArg e)
    {
        foreach (var card in cardsInHand.ToList())
        {
            // Nếu là người chơi hiện tại đánh bài
            if (e.isCurrentPlayer)
            {
                card.SetSelectable(true);
                card.SetNormal();
                continue;
            }
            // Nếu là người chơi khác đánh bài
            if (card.GetCardSuit() == e.lastCard.GetCardSuit() ||
                card.GetCardRank() == e.lastCard.GetCardRank() ||
                card.GetCardRank() == CardRank.Rank20
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

    private void WhotSuitPicker_OnSuitPicked(CardSuit cardSuit)
    {
        foreach (var card in cardsInHand.ToList())
        {
            if (card.GetCardSuit() == cardSuit
                || card.GetCardRank() == CardRank.Rank20
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
                    cardsInHand.Remove(card);
                    card.SetSelectable(false);
                    whotGame.PlayACard(card, card.transform, true);
                    Destroy(card.gameObject);
                    if (cardsInHand.Count == 0)
                    {
                        whotGame.AnimateLastCard();
                        whotGame.AnimateShowRemainingCards(true);
                    }
                    SpreadCards();
                }
            }
        }
    }
    #endregion

    public void Reset()
    {
        foreach (var card in cardsInHand)
        {
            Destroy(card.gameObject);
        }
        cardsInHand.Clear();
        scoreParent.gameObject.SetActive(false);
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
