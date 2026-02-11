using System;
using System.Collections.Generic;
using System.Linq;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendChatView : BaseView
{
    [SerializeField] private FriendChatTabItem friendChatTabItemPrefab;
    [SerializeField] private FriendChatItem friendChatItemPrefab;
    [SerializeField] private Transform friendTabContainer, friendChatContainer;
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

    public void Setup(FriendsView friendView, FriendItem itemView)
    {
        SetupListRecentConversation(friendView);
    }

    private async void SetupListRecentConversation(FriendsView friendView)
    {
        ListRecentConversationsResponse listRecentConversationsResponse = await friendView.FriendPresenter.GetListRecentConversations();
        Debug.Log("listRecentConversationsResponse: " + listRecentConversationsResponse);
        List<ConversationEntry> conversationEntries = listRecentConversationsResponse.Conversations.ToList();

        foreach (ConversationEntry conversation in conversationEntries)
        {
            FriendChatTabItem friendChatTabItem = Instantiate(friendChatTabItemPrefab, friendTabContainer).GetComponent<FriendChatTabItem>();

        }
    }
    
}
