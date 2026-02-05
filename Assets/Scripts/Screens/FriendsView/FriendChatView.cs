using System;
using Proto;
using UnityEngine;

public class FriendChatView : BaseView
{
    public override void Hide(bool isDestroy = true, Action onCompleteCallback = null, bool notDeactive = false)
    {
        base.Hide(false, onCompleteCallback, notDeactive);
    }

    public void Setup(FriendListItem item)
    {
        
    }
}
