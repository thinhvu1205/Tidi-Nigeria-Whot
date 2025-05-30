using System.Collections;
using System.Collections.Generic;
using Api;
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
        InitPlayer();
        InitDrawnCard();
    }

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
        whotCard.SetInfo(CardSuit.SuitCircle, CardRank.Rank3);
    }
}
