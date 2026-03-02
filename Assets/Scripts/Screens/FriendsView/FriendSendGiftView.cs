using System;
using System.Collections.Generic;
using Proto;
using TMPro;
using UnityEngine;

public class FriendSendGiftView : BaseView
{
    [SerializeField] private FriendSendGiftItem friendSendGiftPrefab;
    [SerializeField] private FriendSendGiftConfirmation friendSendGiftConfirmation;
    [SerializeField] private Transform parent;
    [SerializeField] private TextMeshProUGUI textRecipient;
    private FriendsView friendsView;
    private FriendItem friendItem;

    public override void Hide(bool isDestroy = true, Action onCompleteCallback = null, bool notDeactive = false)
    {
        base.Hide(false, onCompleteCallback, notDeactive);
    }

    public void Setup(FriendsView friendsView, List<FriendGiftItem> listFriendGift)
    { 
        this.friendsView = friendsView;
        foreach(FriendGiftItem friendGiftItem in listFriendGift)
        {
            FriendSendGiftItem friendSendGiftItem = Instantiate(friendSendGiftPrefab, parent);
            friendSendGiftItem.SetInfo(friendGiftItem);
            friendSendGiftItem.OnClick += FriendSendGiftItem_OnClick;
        }
    }

    public void SetTextRecipientInfo(FriendItem friendItem)
    {
        this.friendItem = friendItem;
        textRecipient.text = $"{friendItem.Username} - ID: {friendItem.Sid}";
    }

    public void FriendSendGiftItem_OnClick(FriendGiftItem friendGiftItem)
    {
        friendSendGiftConfirmation.Show();
        friendSendGiftConfirmation.Setup(friendsView, friendItem, friendGiftItem);
    }
}
