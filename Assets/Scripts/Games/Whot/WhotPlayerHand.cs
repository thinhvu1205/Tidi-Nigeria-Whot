using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto;
using Common.Pool;
using DG.Tweening;
using Globals;
using Google.Protobuf;
using TMPro;
using UnityEngine;

public class WhotPlayerHand : MonoBehaviour
{
    [SerializeField] private Transform cardsParent, dealCardsParent, scoreParent, remainingCardsParent;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private TextMeshProUGUI scoreText;
    [HideInInspector] public List<WhotCardModel> cardsInHand, remainingWhotCards = new();
    private WhotView whotGame;
    private const float ANIMATION_TIME = 0.35f;
    private const float CARD_SCALE = 0.86f;
    private float SCORE_IMAGE_OFFSET = 120f;

    private float REMAINING_CARD_SPACING = CARD_SCALE / 2 * 100f;

    private float CARD_SPACING = 56f;

    public Transform GetCardsParent() => cardsParent;
    public Transform GetDealCardsParent() => dealCardsParent;
    public bool isFirstUpdateTurn = false;

    private void Awake()
    {
        whotGame = GetComponent<WhotView>();
        whotGame.OnNextTurn += WhotGame_OnNextTurn;

        scoreParent.gameObject.SetActive(false);
    }

    public void PlayACard(WhotCardModel cardModel)
    {
        cardModel.SetSelectable(false);
        cardsInHand.Remove(cardModel);
        whotGame.PlayACard(cardModel, GetCardsParent());
        EndTurn();
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

    public void AnimateShowRemainingCards(List<WhotCard> cards)
    {
        if (cards == null) return;
        cardsParent.gameObject.SetActive(false);
        remainingCardsParent.gameObject.SetActive(true);
        SortRemainingCards(cards);
        if (cards.Count >= 15)
        {
            REMAINING_CARD_SPACING = CARD_SCALE / 2 * 75;
            SCORE_IMAGE_OFFSET = 50f;
        }
        float totalWidth = (cards.Count - 1) * REMAINING_CARD_SPACING;
        float startX = -totalWidth / 2f;
        for (int i = 0; i < cards.Count; i++)
        {
            WhotCard card = cards[i];
            WhotCardModel whotCardModel = PoolService.Instance.Get<WhotCardModel>(PrefabType.WhotCard);
            whotCardModel.transform.SetParent(remainingCardsParent);
            remainingWhotCards.Add(whotCardModel);
            whotCardModel.SetInfo(card.Suit, card.Rank);
            whotCardModel.SetSelectable(false);
            whotCardModel.transform.localScale = Vector3.one * CARD_SCALE;

            CanvasGroup cardCanvasGroup = whotCardModel.GetComponent<CanvasGroup>();
            cardCanvasGroup.alpha = 0f;

            Vector3 offset = new Vector3(-20f, 0f, 0f);
            Vector3 targetPos = new Vector3(startX + i * REMAINING_CARD_SPACING, 0f, 0f);

            whotCardModel.transform.localPosition = targetPos + offset;

            // Animate move & fade
            whotCardModel.transform.DOLocalMove(targetPos, ANIMATION_TIME).SetEase(Ease.OutCubic).SetDelay(i * 0.05f);
            cardCanvasGroup.DOFade(1f, ANIMATION_TIME / 2).SetDelay(i * 0.1f);

        }
        scoreParent.transform.localPosition = new Vector3(startX + totalWidth + SCORE_IMAGE_OFFSET, 0f, 0f);  
    }

    public void SortCards()
    {
        cardsInHand = cardsInHand.OrderBy(GetSortValue).ToList();
    }

    private void SortRemainingCards(List<WhotCard> cards)
    {
        cards.Sort((a, b) => GetSortValue(a).CompareTo(GetSortValue(b)));
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
            WhotCardModel cardModel = cardsInHand[i];
            Vector2 targetPos = new Vector2(startX + i * CARD_SPACING, GetHandPosition().y);
            cardModel.SetLocalPosition(targetPos);
            cardModel.transform.SetSiblingIndex(i);
            cardModel.transform.DOLocalMove(targetPos, 0.1f);
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

    public void HighlightPlayableCards(WhotCardModel callCardModel)
    {
        foreach (WhotCardModel card in cardsInHand)
        {
            bool isSelectable = false;

            if (callCardModel.GetCardRank() == WhotCardRank.WhotRank2)
            {
                isSelectable = card.GetCardRank() == WhotCardRank.WhotRank2;
            }
            else if (callCardModel.GetCardRank() == WhotCardRank.WhotRank5)
            {
                isSelectable = card.GetCardRank() == WhotCardRank.WhotRank5;
            }
            else if (card.GetCardRank() == WhotCardRank.WhotRank20)
            {
                isSelectable = true;
            }
            else if (card.GetCardSuit() == callCardModel.GetCardSuit() || card.GetCardRank() == callCardModel.GetCardRank())
            {
                isSelectable = true;
            }

            card.SetSelectable(isSelectable);
            if (isSelectable)
                card.SetHighLight();
            else
                card.SetDark();
        }
    }

    #endregion

    #region Events
    public void WhotGame_OnNextTurn(WhotView.OnNextTurnEventArg e)
    {
        if (e.playerTurn == whotGame.GetCurrentPlayer().Id)
        {
            List<WhotCardModel> listCard;
            if (isFirstUpdateTurn)
            {
                listCard = whotGame.GetDealCardsList();
                isFirstUpdateTurn = false;
            }
            else
            {
                listCard = cardsInHand;
            }
            WhotCardModel callCardModel = e.CallCardModel;
            WhotCardEffect cardEffect = e.cardEffect;

            if (cardEffect == WhotCardEffect.Whot)
            {
                foreach (WhotCardModel cardInHand in listCard)
                {
                    cardInHand.SetSelectable(false);
                    cardInHand.SetDark();
                }
                return;
            }
            foreach (WhotCardModel card in listCard)
            {
                bool isSelectable = false;

                if (callCardModel.GetCardRank() == WhotCardRank.WhotRank2 && cardEffect != WhotCardEffect.EffectNone)
                {
                    isSelectable = card.GetCardRank() == WhotCardRank.WhotRank2;
                }
                else if (callCardModel.GetCardRank() == WhotCardRank.WhotRank5 && cardEffect != WhotCardEffect.EffectNone)
                {
                    isSelectable = card.GetCardRank() == WhotCardRank.WhotRank5;
                }
                else if (card.GetCardRank() == WhotCardRank.WhotRank20)
                {
                    isSelectable = true;
                }
                else if (card.GetCardSuit() == callCardModel.GetCardSuit() || card.GetCardRank() == callCardModel.GetCardRank())
                {
                    isSelectable = true;
                }

                card.SetSelectable(isSelectable);
                if (isSelectable)
                    card.SetHighLight();
                else
                    card.SetDark();
            }
        }
    }

    public void WhotCard_OnCardSelected(object sender, WhotCardModel.OnCardSelectedEventArg e)
    {
        WhotCardModel selectedCardModel = sender as WhotCardModel;
        if (selectedCardModel != null)
        {
            foreach (var card in cardsInHand.ToList())
            {
                if (card != selectedCardModel)
                {
                    card.Unselect();
                }
                else
                {
                    if (e.isSelected)
                    {
                        WhotCard cardObject = new()
                        {
                            Suit = card.GetCardSuit(),
                            Rank = card.GetCardRank()
                        };
                        DataSender.SendMatchState((long)OpCodeRequest.PlayCard, cardObject.ToByteArray());
                        foreach (WhotCardModel cardInHand in cardsInHand)
                        {
                            cardInHand.SetSelectable(false);
                            // cardInHand.SetDark();
                        }
                    }
                }
            }
        }
    }
    #endregion

    public void EndTurn()
    {
        foreach (WhotCardModel card in cardsInHand)
        {
            card.Unselect();
        }
        whotGame.GetCurrentPlayer().StopCountDown();
        whotGame.AnimateHideDeckHighlight();
        SetCardsToNormal();
    }

    public void Reset()
    {
        foreach (WhotCardModel card in cardsInHand)
        {
            PoolService.Instance.Release(PrefabType.WhotCard, card);
        }
        foreach (Transform card in GetDealCardsParent())
        {
            // PoolService.Instance.Release(PrefabType.WhotCard, card);
            DestroyImmediate(card.gameObject);
        }
        // foreach (Transform card in remainingWhotCards)
        // {
        //     // PoolService.Instance.Release(PrefabType.WhotCard, card);
        //     DestroyImmediate(card.gameObject);
        // }
        cardsInHand.Clear();
        remainingWhotCards.Clear();
        cardsParent.gameObject.SetActive(true);
        dealCardsParent.gameObject.SetActive(false);
    }

    public void HideRemainingCards()
    {
        remainingCardsParent.gameObject.SetActive(false);
        for (int i = remainingCardsParent.childCount - 1; i >= 1; i--)
        {
            Transform child = remainingCardsParent.GetChild(i);
            DestroyImmediate(child.gameObject);
        }
        scoreParent.gameObject.SetActive(false);
    }
    #region Helpers

    private int GetSortValue(WhotCardModel cardModel)
    {
        int suitOrder = Constants.WhotSuitSortOrder.TryGetValue(cardModel.GetCardSuit(), out var order) ? order : 999;
        int rank = (int)cardModel.GetCardRank();
        return suitOrder * 100 + rank;
    }

    private int GetSortValue(WhotCard card)
    {
        int suitOrder = Constants.WhotSuitSortOrder.TryGetValue(card.Suit, out var order) ? order : 999;
        int rank = (int)card.Rank;
        return suitOrder * 100 + rank;
    }

    private Vector2 GetHandPosition()
    {
        return new Vector2(0f, 0f);
    }

    public Vector2 GetCardPosition(WhotCardModel cardModel)
    {
 
        float totalWidth = (cardsInHand.Count - 1) * CARD_SPACING;
        float startX = -totalWidth / 2f;

        int newIndex = cardsInHand.IndexOf(cardModel);
        Vector2 targetPos = new Vector2(startX + newIndex * CARD_SPACING, GetHandPosition().y);

        return targetPos;
    }

    public Vector2 GetNewCardPosition(WhotCardModel newCardModel)
    {
        List<WhotCardModel> sortedCards = new List<WhotCardModel>(cardsInHand)
        {
            newCardModel
        };

        sortedCards = sortedCards.OrderBy(c => GetSortValue(c)).ToList();

        float totalWidth = (sortedCards.Count - 1) * CARD_SPACING;
        float startX = -totalWidth / 2f;

        int newIndex = sortedCards.IndexOf(newCardModel);
        Vector2 targetPos = new Vector2(startX + newIndex * CARD_SPACING, GetHandPosition().y);

        return targetPos;
    }
    #endregion

    
}
