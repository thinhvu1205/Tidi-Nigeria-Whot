using System;
using System.Collections;
using System.Collections.Generic;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FreeChipItem : MonoBehaviour
{
    public event EventHandler<OnItemClickedEventArgs> OnItemClicked;
    public class OnItemClickedEventArgs : EventArgs
    {
        public FreeChip freeChip;
    }
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI textTitle, textContent, textTime;
    [SerializeField] private Button buttonClaim;
    [SerializeField] private Sprite[] listSpriteMoney; // 0: Coin, 1: GCode
    [SerializeField] private Sprite[] listSpriteButton; // 0: green, 1: red

    private FreeChip freeChip;

    public void SetInfo(FreeChip freeChip)
    {
        this.freeChip = freeChip;
        textTitle.text = freeChip.Title;
        textContent.text = freeChip.Content;
        buttonClaim.interactable = freeChip.Claimable;
        buttonClaim.GetComponent<Image>().sprite = freeChip.Claimable ? listSpriteButton[0] : listSpriteButton[1];
    }

    public void OnClick()
    {
        OnItemClicked?.Invoke(this, new OnItemClickedEventArgs
        {
            freeChip = freeChip
        });
    }
}
