using System;
using System.Collections.Generic;
using Proto;
using TMPro;
using UnityEngine;

public class FriendSendGiftView : BaseView
{
    [SerializeField] private FriendSendGiftItem friendSendGiftPrefab;
    [SerializeField] private Transform parent;
    [SerializeField] private TextMeshProUGUI textRecipient;

    public override void Hide(bool isDestroy = true, Action onCompleteCallback = null, bool notDeactive = false)
    {
        base.Hide(false, onCompleteCallback, notDeactive);
    }

    public void Setup(List<FriendGiftItem> listFriendGift)
    { 
        foreach(FriendGiftItem friendGiftItem in listFriendGift)
        {
            FriendSendGiftItem friendSendGiftItem = Instantiate(friendSendGiftPrefab, parent);
            friendSendGiftItem.SetInfo(friendGiftItem);
        }
    }

    public void SetTextRecipientInfo(FriendItem friendItem)
    {
        textRecipient.text = $"{friendItem.Username} - ID: {friendItem.Sid}";
    }

    public void OnClickMarkAllAsRead()
    {
        
    }
}
