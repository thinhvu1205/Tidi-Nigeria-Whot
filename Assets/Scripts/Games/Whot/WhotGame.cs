using System.Collections;
using System.Collections.Generic;
using Api;
using DG.Tweening;
using Spine.Unity;
using TMPro;
using UnityEngine;

public class WhotGame : MonoBehaviour
{
    [SerializeField] private GameObject whotPlayerPrefab, cardPrefab;
    [SerializeField] private List<Transform> playerPositionsList;
    [SerializeField] private SkeletonGraphic betterLuckNextTimeAnimation, victoryAnimation, matchSymbolAnimation, effectAnimation;
    [SerializeField] private Transform matchResultTransform, yourTurnTransform, wheelTransform, deckOfCardParent, drawnCardParent;
    [SerializeField] private TextMeshProUGUI betText, cardsLeftText;
    private WhotPlayerHand playerHand;

    private const float ANIMATION_TIME = 0.5f;
    private const string
        MATCH_SYMBOL_SQUARE_ANIMATION_NAME = "vuong",
        MATCH_SYMBOL_CROSS_ANIMATION_NAME = "thap",
        MATCH_SYMBOL_TRIANGLE_ANIMATION_NAME = "tamgiac",
        MATCH_SYMBOL_CIRCLE_ANIMATION_NAME = "tron",
        MATCH_SYMBOL_STAR_ANIMATION_NAME = "sao",
        EFFECT_HOLD_ON_ANIMATION_NAME = "hold",
        EFFECT_GENERAL_MARKET_ANIMATION_NAME = "general",
        LOSE_ANIMATION_NAME = "better",
        VICTORY_MARKET_ANIMATION_NAME = "victory";

    private void Awake()
    {
        playerHand = GetComponent<WhotPlayerHand>();
        InitPlayer();
        InitDrawnCard();
    }

    public void DrawCard()
    {
        GameObject card = Instantiate(cardPrefab, deckOfCardParent);
        card.transform.localPosition = Vector3.zero;
        card.transform.localScale = Vector3.one;
        card.transform.SetParent(playerHand.GetCardsParent());
        WhotCard whotCard = card.GetComponent<WhotCard>();
        whotCard.SetInfo(CardSuit.SuitCross, CardRank.Rank1);
        whotCard.SetFaceDown();
        playerHand.cardsInHand.Add(whotCard);

        AnimateCardToHand(whotCard);
    }

    public void PlayCard(WhotCard card)
    {
        Debug.Log($"Play card: {card.GetCardSuit()} - {card.GetCardRank()}");

        GameObject cardInstance = Instantiate(cardPrefab, card.transform);
        cardInstance.transform.localPosition = Vector3.zero;
        cardInstance.transform.localScale = Vector3.one;
        cardInstance.transform.SetParent(drawnCardParent);
        WhotCard whotCard = cardInstance.GetComponent<WhotCard>();
        whotCard.SetInfo(card.GetCardSuit(), card.GetCardRank());
        AnimatePlayCard(whotCard);
    }

    #region Animations
    private void AnimateCardToHand(WhotCard card)
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

    private void AnimatePlayCard(WhotCard card)
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
        card.transform.DOMove(drawnCardParent.position, ANIMATION_TIME).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            playerHand.SortCards();
            playerHand.SpreadCards();
        });
    }
    #endregion

    private void InitPlayer()
    {
        for (int i = 0; i < playerPositionsList.Count; i++)
        {
            Transform transform = playerPositionsList[i];
            GameObject player = Instantiate(whotPlayerPrefab, transform);
            player.transform.localPosition = Vector3.zero;
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

    private void InitDrawnCard()
    {
        GameObject card = Instantiate(cardPrefab, drawnCardParent);
        card.transform.localPosition = Vector3.zero;
        card.transform.localScale = Vector3.one;
        WhotCard whotCard = card.GetComponent<WhotCard>();
        whotCard.SetInfo(CardSuit.SuitUnspecified, CardRank.Rank20);
    }
}
