using System.Collections;
using System.Collections.Generic;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [SerializeField] private Image imageBestDeal, imageCoin, imageBackground;
    [SerializeField] private TextMeshProUGUI textChip, textPercent, textPrice, textChipPerUnit;
    [SerializeField] private Button buttonBuy;
    [SerializeField] private List<Sprite> listSpriteCoin, listSpriteBackground;

    public void SetInfo(Deal deal, int index, bool isBestDeal)
    {
        imageBestDeal.gameObject.SetActive(isBestDeal);
        imageCoin.sprite = listSpriteCoin[index];
        imageBackground.sprite = isBestDeal ? listSpriteBackground[0] : listSpriteBackground[1];

        textChip.text = deal.AmountChips.ToString();
        textPercent.text = $"+{deal.Percent}%";
        textPrice.text = deal.Price;
        textChipPerUnit.text = $"1$ = ${deal.ChipPerUnit} Chips";
    }
}
