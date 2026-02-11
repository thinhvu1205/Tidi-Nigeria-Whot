using System;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendChatView : BaseView
{
    [SerializeField] private Button btnSendMessage;
    [SerializeField] private TMP_InputField messageTMP;

    protected override void Start()
    {
        base.Start();
        btnSendMessage.onClick.AddListener(()=>
        {
            Debug.Log("Sending message");
            _ = NetworkManager.INSTANCE.SendMessageDirectChat(messageTMP.text);
        });
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        btnSendMessage.onClick.RemoveAllListeners();
    }
    
    public override void Hide(bool isDestroy = true, Action onCompleteCallback = null, bool notDeactive = false)
    {
        base.Hide(false, onCompleteCallback, notDeactive);
    }

    public void Setup(FriendItem itemView)
    {
        
    }
    
}
