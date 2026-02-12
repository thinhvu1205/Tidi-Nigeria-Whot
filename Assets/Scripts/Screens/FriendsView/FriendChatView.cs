using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Globals;
using Nakama;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendChatView : BaseView
{
    [SerializeField] private FriendChatTabItem friendChatTabItemPrefab;
    [SerializeField] private FriendChatItem friendChatItemPrefab;
    [SerializeField] private VerticalPool verticalPoolGroup;
    [SerializeField] private Transform friendTabContainer, friendChatContainer;
    [SerializeField] private Button btnSendMessage;
    [SerializeField] private TMP_InputField messageTMP;
    private FriendsView friendView;
    private List<PoolInfo> listPoolInfo = new();
    private string selectedTabId;

    protected override void Start()
    {
        base.Start();
        Debug.Log("START");
  
        btnSendMessage.onClick.AddListener(()=>
        {
            Debug.Log("Sending message");
            _ = NetworkManager.INSTANCE.SendMessageDirectChat(messageTMP.text);
        });
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        NetworkManager.INSTANCE.OnMessageDirectReceived += NetworkManager_OnMessageReceived;
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();
        NetworkManager.INSTANCE.OnMessageDirectReceived -= NetworkManager_OnMessageReceived;
        btnSendMessage.onClick.RemoveAllListeners();
    }
    
    public override void Hide(bool isDestroy = true, Action onCompleteCallback = null, bool notDeactive = false)
    {
        base.Hide(false, onCompleteCallback, notDeactive);
    }

    private void NetworkManager_OnMessageReceived(IApiChannelMessage message)
    {
        ChatPayload chatPayload = ConvertToChatPayload(message);
        if (!string.IsNullOrEmpty(chatPayload.Content))
        {
            listPoolInfo.Add(new PoolInfo { Data = chatPayload });
            verticalPoolGroup.SetControlInfo(listPoolInfo, listPoolInfo.Count - 1);
            // verticalPoolGroup.ScrollToLast(0);
            // chatWorldItem.SetInfo(message, isCurrentPlayer);
        }
    }

    public async Task Setup(FriendsView friendView, FriendItem itemView)
    {
        this.friendView = friendView;
        await friendView.FriendPresenter.JoinDirectChat(itemView.UserId.ToString());
        selectedTabId = itemView.Sid.ToString();
        await SetupListRecentConversation();
        await SetupListMessage();

        verticalPoolGroup.SetApplyDataCb((go, data, index) =>
        {
            FriendChatItem chatItem = go.GetComponent<FriendChatItem>();
            chatItem.SetInfo((ChatPayload)data.Data, (cellW, cellH) =>
            {
                data.SetCellWidth(verticalPoolGroup.GetComponent<RectTransform>().rect.width);
                data.SetCellHeight(cellH + 40);
            });
            // chatItem.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, containerWidth); 
            RectTransform childRect = chatItem.GetComponent<RectTransform>();
            childRect.anchorMin = new Vector2(0, childRect.anchorMin.y);
            childRect.anchorMax = new Vector2(1, childRect.anchorMax.y);
            childRect.offsetMin = new Vector2(0, childRect.offsetMin.y);
            childRect.offsetMax = new Vector2(0, childRect.offsetMax.y);
        }, true);
    }

    private async Task SetupListRecentConversation()
    {
        ListRecentConversationsResponse listRecentConversationsResponse = await friendView.FriendPresenter.GetListRecentConversations();
        Debug.Log("listRecentConversationsResponse: " + listRecentConversationsResponse);
        List<ConversationEntry> conversationEntries = listRecentConversationsResponse.Conversations.ToList();
        foreach(Transform child in friendTabContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < conversationEntries.Count; i++)
        {
            int index = i;
            ConversationEntry conversation = conversationEntries[i];
            FriendChatTabItem friendChatTabItem = Instantiate(friendChatTabItemPrefab, friendTabContainer).GetComponent<FriendChatTabItem>();
            friendChatTabItem.Setup(conversation, index == 0, conversation.Sid.ToString() == selectedTabId);
        }
    }

    private async Task SetupListMessage()
    {
        List<IApiChannelMessage> messageList = await friendView.FriendPresenter.GetDirectChatHistory();
        Debug.Log("messageList: " + messageList);
        List<PoolInfo> listPoolTemp = new();
        foreach (IApiChannelMessage message in messageList)
        {
            Debug.Log("Message: " + message.Content);
            ChatPayload payload = ConvertToChatPayload(message);
            listPoolTemp.Add(new PoolInfo { Data = payload });
        }
        listPoolInfo.InsertRange(0, listPoolTemp);
        Debug.Log("LIST POOL INFO: " + listPoolInfo.Count);
        verticalPoolGroup.SetControlInfo(listPoolInfo, listPoolInfo.Count - 1);

    }

    private ChatPayload ConvertToChatPayload(IApiChannelMessage message)
    {
        ContentData data = JsonUtility.FromJson<ContentData>(message.Content);
        ChatPayload chatPayload = new()
        {
            ID = message.SenderId,
            Name = message.Username,
            Time = Utility.ConvertISOToHHMMDDMMYYYY(message.CreateTime),
            Content = data.content,
            Avatar = data.sender_profile.avt,
            Vip = data.sender_profile.vip_level
        };
        return chatPayload;
    }
    
}
