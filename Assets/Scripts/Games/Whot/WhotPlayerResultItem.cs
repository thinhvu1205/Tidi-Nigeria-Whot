using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WhotPlayerResultItem : MonoBehaviour
{
    [SerializeField] private Image backgroundWinImage, backgroundLoseImage, avatarImage, winnerImage;
    [SerializeField] private TextMeshProUGUI nameText, cashText, scoreText;
    [SerializeField] private GameObject cardLeftPrefab;
    [SerializeField] private Transform cardLeftParent;
    [SerializeField] private Color greenColor, blueColor, lightBlueColor, yellowColor;
    private const float CARD_SCALE = 0.46f;
    private const float CARD_SPACING = CARD_SCALE / 2 * 100f;
    public void SetInfo(WhotPlayer player, string cash, string score, bool isVictory)
    {
        if (player.isCurrentPlayer)
        {
            if (isVictory)
            {
                backgroundWinImage.gameObject.SetActive(true);
                backgroundLoseImage.gameObject.SetActive(false);
                SetTextColor(greenColor);
            }
            else
            {
                backgroundWinImage.gameObject.SetActive(false);
                backgroundLoseImage.gameObject.SetActive(true);
                SetTextColor(blueColor);
            }
        }
        else
        {
            backgroundWinImage.gameObject.SetActive(false);
            backgroundLoseImage.gameObject.SetActive(false);
            if (isVictory)
            {
                SetTextColor(yellowColor);
            }
            else
            {
                SetTextColor(lightBlueColor);
            }
            if (player.isWinner)
            {
                SetTextColor(greenColor);
            }
        }
        avatarImage.sprite = null;
        winnerImage.gameObject.SetActive(player.isWinner);
        nameText.text = player.GetPlayerName();
        cashText.text = cash;
        scoreText.text = score;

        foreach (Transform child in cardLeftParent)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < player.cards.Count; i++)
        {
            WhotCard card = SortedCards(player.cards)[i];
            GameObject cardInstance = Instantiate(cardLeftPrefab, cardLeftParent);
            WhotCard whotCard = cardInstance.GetComponent<WhotCard>();
            whotCard.transform.localScale = Vector3.one * CARD_SCALE;
            whotCard.transform.localPosition = Vector3.zero;
            whotCard.transform.Translate(CARD_SPACING * i, 0f, 0f);
            whotCard.SetInfo(card.GetCardSuit(), card.GetCardRank());
            whotCard.SetSelectable(false);
        }
    }

    private void SetTextColor(Color textColor)
    {
        nameText.color = textColor;
        cashText.color = textColor;
        scoreText.color = textColor;
    }

    private List<WhotCard> SortedCards(List<WhotCard> cards)
    {
        return cards.OrderBy(c => GetSortValue(c)).ToList();
    }

    private int GetSortValue(WhotCard card)
    {
        int suitOrder = Constants.WhotSuitSortOrder.TryGetValue(card.GetCardSuit(), out var order) ? order : 999;
        int rank = (int)card.GetCardRank();
        return suitOrder * 100 + rank;
    }
}
