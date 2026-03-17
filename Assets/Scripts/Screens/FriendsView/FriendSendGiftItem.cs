using System;
using Globals;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class FriendSendGiftItem : MonoBehaviour
{
    public event Action<FriendGiftItem> OnClick;
    [SerializeField] private TextMeshProUGUI textPrice, textIntimacyPoint;
    [SerializeField] private Image imageGift, imageBackground;
    [SerializeField] private Sprite[] listSpriteGift;
    [SerializeField] private Button button;

    public void SetInfo(FriendGiftItem item)
    {
        button.onClick.AddListener(() => OnClick?.Invoke(item)); 
        textPrice.text = Utility.FormatNumber(item.ChipPrice);
        textIntimacyPoint.text = $"IP +{item.IpGain}";
        imageGift.sprite = listSpriteGift[item.ItemId - 1];
        imageGift.preserveAspect = true;
        imageBackground.color = item.VipUnlock <= User.UserAccount.Profile.Vip ? Color.white : Color.gray;
        imageGift.color = item.VipUnlock <= User.UserAccount.Profile.Vip ? Color.white : Color.gray;
        textPrice.color = item.VipUnlock <= User.UserAccount.Profile.Vip ? Color.white : Color.gray;
    }
}
