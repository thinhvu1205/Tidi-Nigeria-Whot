using System;
using Globals;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class FriendSendGiftItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textPrice, textIntimacyPoint;
    [SerializeField] private Image imageGift, imageBackground;
    [SerializeField] private Sprite[] listSpriteGift;

    public void SetInfo(FriendGiftItem item)
    {
        textPrice.text = Utility.FormatNumber(item.ChipPrice);
        textIntimacyPoint.text = $"IP +{item.IpGain}";
        imageGift.sprite = listSpriteGift[item.ItemId - 1];
        imageGift.preserveAspect = true;
        imageBackground.color = item.VipUnlock <= User.userProfile.VipLevel ? Color.white : Color.gray;
        imageGift.color = item.VipUnlock <= User.userProfile.VipLevel ? Color.white : Color.gray;
        textPrice.color = item.VipUnlock <= User.userProfile.VipLevel ? Color.white : Color.gray;
    }
}
