using System;
using UnityEngine;

public class FriendNotificationView : BaseView
{
    [SerializeField] private FriendNotificationItem friendNotificationPrefab;
    [SerializeField] private Transform friendNotificationParent;

    public override void Hide(bool isDestroy = true, Action onCompleteCallback = null, bool notDeactive = false)
    {
        base.Hide(false, onCompleteCallback, notDeactive);
    }

    public void OnClickMarkAllAsRead()
    {
        
    }
}
