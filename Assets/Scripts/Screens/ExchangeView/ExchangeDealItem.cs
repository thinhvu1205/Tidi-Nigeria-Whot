using System;
using Globals;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExchangeDealItem : MonoBehaviour
{
    public event Action<long, string> OnClicked;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image imageButton;
    [SerializeField] private Sprite[] listSpriteButton;

    private long chips;
    private string dealId;

    public void SetInfo(Deal deal)
    {
        chips = deal.Chips;
        dealId = deal.Id;
        text.text = Utility.FormatNumber(int.Parse(deal.Price)) + " " + deal.Currency;
        imageButton.sprite = listSpriteButton[0];
    }

    public void OnClick()
    {
        OnClicked?.Invoke(chips, dealId);
    }
}
