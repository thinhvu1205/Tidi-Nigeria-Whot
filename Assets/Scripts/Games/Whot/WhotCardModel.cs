using System;
using Common.Pool;
using DG.Tweening;
using Proto;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class WhotCardModel : MonoBehaviour,IPoolable
{
    public event EventHandler<OnCardSelectedEventArg> OnCardSelected;

    public class OnCardSelectedEventArg : EventArgs
    {
        public bool isSelected;
    }
    [SerializeField] private Image cardImage, cardBackImage, lightImage;
    [SerializeField] private Sprite backSprite;
    private WhotCardSuit suit;
    private WhotCardRank value;
    private Vector2 position;
    private bool isSelected = false;
    private bool isSelectable = true;

    public WhotCardSuit GetCardSuit() => suit;
    public WhotCardRank GetCardRank() => value;
    public bool GetIsSelected() => isSelected;
    public Vector2 GetLocalPosition() => position;
    public void SetInfo(WhotCardSuit suit, WhotCardRank value)
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

    public void SetNormal()
    {
        cardImage.color = Color.white; // Reset to normal color
        lightImage.gameObject.SetActive(false);
    }

    public void SetHighLight()
    {
        lightImage.gameObject.SetActive(true);
        cardImage.color = Color.white;
    }

    public void SetDark()
    {
        cardImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Set to dark color
        lightImage.gameObject.SetActive(false);
    }

    public void SetFaceUp()
    {
        cardImage.sprite = GetSprite();
    }

    public void SetFaceDown()
    {
        cardImage.sprite = backSprite;
    }

    public void SetSuit(WhotCardSuit cardSuit)
    {
        suit = cardSuit;
    }

    private Sprite GetSprite()
    {
        string spriteName = GetSuitName() + "_" + GetRankName();
        Sprite cardSprite;
        if (GetCardRank() == WhotCardRank.WhotRank20)
        {
            return Resources.Load<Sprite>("BundlePack/Images/Games/Whot/Cut/Bai_Whot/Whot_0");
        }
        cardSprite = Resources.Load<Sprite>("BundlePack/Images/Games/Whot/Cut/Bai_Whot/" + spriteName);
        if (cardSprite == null)
        {
            Debug.LogError($"Sprite not found: BundlePack/Images/Games/Whot/Cut/Bai_Whot/{spriteName}");
            return null;
        }
        return cardSprite;
    }

    private string GetSuitName()
    {
        return suit switch
        {
            WhotCardSuit.WhotSuitStar => "sao",
            WhotCardSuit.WhotSuitCircle => "tron",
            WhotCardSuit.WhotSuitCross => "thap",
            WhotCardSuit.WhotSuitSquare => "vuong",
            WhotCardSuit.WhotSuitTriangle => "tamgiac",
            WhotCardSuit.WhotSuitUnspecified => "whot",
            _ => "Unknown",
        };
    }

    private string GetRankName()
    {
        return value switch
        {
            WhotCardRank.WhotRank1 => "1",
            WhotCardRank.WhotRank2 => "2",
            WhotCardRank.WhotRank3 => "3",
            WhotCardRank.WhotRank4 => "4",
            WhotCardRank.WhotRank5 => "5",
            WhotCardRank.WhotRank7 => "7",
            WhotCardRank.WhotRank8 => "8",
            WhotCardRank.WhotRank10 => "10",
            WhotCardRank.WhotRank11 => "11",
            WhotCardRank.WhotRank12 => "12",
            WhotCardRank.WhotRank13 => "13",
            WhotCardRank.WhotRank14 => "14",
            WhotCardRank.WhotRank20 => "0",
            _ => "Unknown"
        };
    }

    public void OnGetFromPool()
    {
        
    }

    public void OnReturnToPool()
    {
        isSelected = false;
        isSelectable = true;
        position = Vector2.zero;
        SetNormal();
        SetFaceDown();
        OnCardSelected = null;
    }
    
}
