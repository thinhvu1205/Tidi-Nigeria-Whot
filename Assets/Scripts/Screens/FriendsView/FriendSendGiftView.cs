using System;
using UnityEngine;

public class FriendSendGiftView : BaseView
{
    [SerializeField] private FriendSendGiftItem friendSendGiftPrefab;
    [SerializeField] private Transform arent;

    public override void Hide(bool isDestroy = true, Action onCompleteCallback = null, bool notDeactive = false)
    {
        base.Hide(false, onCompleteCallback, notDeactive);
    }

    public void OnClickMarkAllAsRead()
    {
        
    }
}
