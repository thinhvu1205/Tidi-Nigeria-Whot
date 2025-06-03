using System;
using System.Collections;
using System.Collections.Generic;
using Api;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WhotCard : MonoBehaviour
{
    public event EventHandler<OnCardSelectedEventArg> OnCardSelected;
    public class OnCardSelectedEventArg : EventArgs
    {
        public bool isSelected;
    }
    [SerializeField] private Image cardImage, cardBackImage, lightImage;
    [SerializeField] private Sprite backSprite;
    private CardSuit suit;
    private CardRank value;
    private Vector2 position;
    private bool isSelected = false;
    private bool isSelectable = true;

    public CardSuit GetCardSuit() => suit;
    public CardRank GetCardRank() => value;
    public Vector2 GetLocalPosition() => position;
    public void SetInfo(CardSuit suit, CardRank value)
    {
        this.suit = suit;
        this.value = value;

        cardImage.sprite = GetSprite();
        cardImage.SetNativeSize();
        cardBackImage.gameObject.SetActive(false);
    }

    public void SetLocalPosition(Vector2 position)
    {
        this.position = position;
    }

    public void SetSelectable(bool isSelectable)
    {
        this.isSelectable = isSelectable;
    }

    public void OnSelect()
    {
        if (!isSelectable) return;
        OnCardSelected?.Invoke(this, new OnCardSelectedEventArg { isSelected = isSelected });
        isSelected = true;
        transform.DOLocalMoveY(position.y + 50f, 0.25f)
            .SetEase(Ease.OutQuad);
    }

    public void Unselect()
    {
        if (!isSelected) return;
        isSelected = false;
        transform.DOLocalMoveY(position.y, 0.25f)
            .SetEase(Ease.OutQuad);
    }

    public void SetHighLight()
    {
        lightImage.gameObject.SetActive(true);
    }

    public void SetDark()
    {
        cardImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Set to dark color
    }

    public void SetFaceUp()
    {
        cardImage.sprite = GetSprite();
    }

    public void SetFaceDown()
    {
        cardImage.sprite = backSprite;
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
