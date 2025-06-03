using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Api;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WhotPlayerHand : MonoBehaviour
{
    [SerializeField] private Transform cardsParent;
    [SerializeField] private GameObject cardPrefab;
    private WhotGame whotGame;
    private const float CARD_SPACING = 56f;
    public List<WhotCard> cardsInHand = new();
    public Transform GetCardsParent() => cardsParent;

    private readonly Dictionary<CardSuit, int> SuitSortOrder = new()
    {
        { CardSuit.SuitCircle, 0 },
        { CardSuit.SuitTriangle, 1 },
        { CardSuit.SuitCross, 2 },
        { CardSuit.SuitStar, 3 },
        { CardSuit.SuitSquare, 4 },
        { CardSuit.SuitUnspecified, 5 }, // Whot
    };

    private void Awake()
    {
        whotGame = GetComponent<WhotGame>();
        AddCards();
        SpreadCards();
        SortCardsAnimation();
    }

    #region Animations

    private void SortCardsAnimation()
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
    #endregion

    #region Helpers

    private int GetSortValue(WhotCard card)
    {
        int suitOrder = SuitSortOrder.TryGetValue(card.GetCardSuit(), out var order) ? order : 999;
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

    private void AddCards()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject cardObject = Instantiate(cardPrefab, cardsParent);
            if (cardObject.TryGetComponent<WhotCard>(out var card))
            {
                cardsInHand.Add(card);
                card.OnCardSelected += WhotCard_OnCardSelected;

                card.SetInfo(
                    (CardSuit)Random.Range(1, 5),
                    (CardRank)Random.Range(1, 4)
                );
            }
        }
    }

    #endregion

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
                    whotGame.PlayCard(card);
                    Destroy(card.gameObject);
                    SpreadCards();
                }
            }
        }
    }
}
