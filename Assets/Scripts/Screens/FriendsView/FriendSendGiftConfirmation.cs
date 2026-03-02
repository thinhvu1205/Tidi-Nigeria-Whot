using System;
using System.Collections.Generic;
using System.Text;
using Globals;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendSendGiftConfirmation : BaseView
{
    [SerializeField] private TextMeshProUGUI textDesc;
    [SerializeField] private Button buttonCancel, buttonOk;
    [SerializeField] private TextMeshProUGUI textPrice, textIntimacyPoint;
    [SerializeField] private Image imageGift;
    [SerializeField] private Sprite[] listSpriteGift;
    private FriendsView friendsView;
    private FriendItem friendItem;
    private FriendGiftItem friendGiftItem;

    public void Setup(FriendsView friendsView, FriendItem friendItem, FriendGiftItem friendGiftItem)
    {
        this.friendsView = friendsView;
        this.friendItem = friendItem;
        this.friendGiftItem = friendGiftItem;
        SetInfo(friendItem, friendGiftItem);
    }

    protected override void Awake()
    {
        base.Awake();
        buttonOk.onClick.AddListener(() => OnClickOk());
        buttonCancel.onClick.AddListener(() => Hide());
    }

    public void SetInfo(FriendItem friendItem, FriendGiftItem friendGiftItem)
    {
        textPrice.text = Utility.FormatNumber(friendGiftItem.ChipPrice);
        textIntimacyPoint.text = $"IP +{friendGiftItem.IpGain}";
        imageGift.sprite = listSpriteGift[friendGiftItem.ItemId - 1];
        imageGift.preserveAspect = true;
        textDesc.text = $"You are sending gift to {friendItem.Username}, ID: {friendItem.Sid} in your Friend List.\nPlease check and confirm your action.";
    }

    public void OnClickOk()
    {
        friendsView.ConfirmSendGift(friendItem.UserId, friendGiftItem.ItemId);
        Hide();

    }

    public override void Hide(bool isDestroy = true, Action onCompleteCallback = null, bool notDeactive = false)
    {
        base.Hide(false, onCompleteCallback, notDeactive);
    }
}