using System;
using Proto;
using TMPro;
using UnityEngine;

public class FriendSendChipView : BaseView
{
    [SerializeField] private TextMeshProUGUI textRecipient, textTransferLeft;
    private FriendsView friendsView;
    private FriendItem friendItem;

    public void Setup(FriendsView friendsView, FriendItem friendItem)
    { 
        this.friendsView = friendsView;
        this.friendItem = friendItem;
        textRecipient.text = $"{friendItem.Username} - ID: {friendItem.Sid}";
    }

    public override void Hide(bool isDestroy = true, Action onCompleteCallback = null, bool notDeactive = false)
    {
        base.Hide(false, onCompleteCallback, notDeactive);
    }
}