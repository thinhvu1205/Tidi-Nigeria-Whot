using System.Collections;
using System.Collections.Generic;
using Api;
using UnityEngine;
using UnityEngine.UI;

public class WhotCard : MonoBehaviour
{
    [SerializeField] private Image cardImage, cardBackImage;

    private CardSuit suit;
    private CardRank value;

    public void SetInfo(CardSuit suit, CardRank value)
    {
        this.suit = suit;
        this.value = value;

        cardImage.sprite = GetSprite();
        cardImage.SetNativeSize();
        cardBackImage.gameObject.SetActive(false);
    }

    private Sprite GetSprite()
    {
        string spriteName = GetSuitName() + "_" + GetRankName();
        Sprite cardSprite = Resources.Load<Sprite>("Games/Whot/Cut/Bai_Whot/" + spriteName);
        if (cardSprite == null)
        {
            Debug.LogError($"Sprite not found: Games/Whot/Cut/Bai_Whot/{spriteName}");
            return null;
        }
        return cardSprite;
    }

    private string GetSuitName()
    {
        return suit switch
        {
            CardSuit.SuitStar => "sao",
            CardSuit.SuitCircle => "tron",
            CardSuit.SuitCross => "thap",
            CardSuit.SuitSquare => "vuong",
            CardSuit.SuitTriangle => "tamgiac",
            CardSuit.SuitUnspecified => "whot",
            _ => "Unknown",
        };
    }

    private string GetRankName()
    {
        return value switch
        {
            CardRank.Rank1 => "1",
            CardRank.Rank2 => "2",
            CardRank.Rank3 => "3",
            CardRank.Rank4 => "4",
            CardRank.Rank5 => "5",
            CardRank.Rank7 => "7",
            CardRank.Rank8 => "8",
            CardRank.Rank10 => "10",
            CardRank.Rank11 => "11",
            CardRank.Rank12 => "12",
            CardRank.Rank13 => "13",
            CardRank.Rank14 => "14",
            CardRank.Rank20 => "0",
            _ => "Unknown"
        };
    }
}
