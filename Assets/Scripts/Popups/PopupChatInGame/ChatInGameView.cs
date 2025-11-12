using System.Collections;
using System.Collections.Generic;
using GIKCore.Pool;
using Globals;
using Nakama;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatInGameView : BaseView
{
    [SerializeField] private ChatInGameItem messagePrefab;
    [SerializeField] private Transform messageContentParent;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private VerticalPoolGroup verticalPoolGroup;
    private List<IApiChannelMessage> listMessage = new();
    private List<ChatPayload> listChatPayload = new();
    private ChatInGamePresenter chatInGamePresenter;


    protected override void Awake()
    {
        base.Awake();
        chatInGamePresenter = new ChatInGamePresenter();
        chatInGamePresenter.Init(this);
        chatInputField.characterLimit = 200;
        // verticalPoolGroup.SetCellDataCallback<ChatData>((go, data, index) =>
        // {
        //     ChatInGameItem dataCIGI = go.GetComponent<ChatInGameItem>();
        //     dataCIGI.SetInfo(data);
        // });
        // ChatData a1 = new();
        // ChatData a2 = new();
        // ChatData a3 = new();
        // ChatData a4 = new();
        // _PoolData.Clear();
        // _PoolData.Add(a1);
        // _PoolData.Add(a2);
        // _PoolData.Add(a3);
        // _PoolData.Add(a4);
        // verticalPoolGroup.SetAdapter(_PoolData);
        // verticalPoolGroup.ReloadDataToVisibleCell();
    }

    protected override void Start()
    {
        base.Start();


    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();
        NetworkManager.INSTANCE.OnMessageTableReceived -= NetworkManager_OnMessageTableReceived;
    }

    public void Init()
    {
        verticalPoolGroup.SetCellDataCallback<ChatPayload>((go, data, index) =>
        {
            ChatItem chatItem = go.GetComponent<ChatItem>();
            chatItem.SetInfo(data, index);
            // chatItem.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, containerWidth); 
            RectTransform childRect = chatItem.GetComponent<RectTransform>();
            childRect.anchorMin = new Vector2(0, childRect.anchorMin.y);
            childRect.anchorMax = new Vector2(1, childRect.anchorMax.y);
            childRect.offsetMin = new Vector2(0, childRect.offsetMin.y);
            childRect.offsetMax = new Vector2(0, childRect.offsetMax.y);
        });
        NetworkManager.INSTANCE.OnMessageTableReceived += NetworkManager_OnMessageTableReceived;
        
    }

    private void NetworkManager_OnMessageTableReceived(IApiChannelMessage message)
    {
        ChatPayload chatPayload = ConvertToChatPayload(message);
        if (!string.IsNullOrEmpty(chatPayload.Content))
        {
            listChatPayload.Add(chatPayload);
            verticalPoolGroup.SetAdapter(listChatPayload, false);
            verticalPoolGroup.ScrollToLast(0);
            // chatWorldItem.SetInfo(message, isCurrentPlayer);
        }
    }

    public void OnClickSendMessage()
    {
        if (!string.IsNullOrEmpty(chatInputField.text))
        {
            _ = chatInGamePresenter.SendMessage(chatInputField.text);
            chatInputField.text = "";
        }
    }

    private ChatPayload ConvertToChatPayload(IApiChannelMessage message)
    {
        ContentData data = JsonUtility.FromJson<ContentData>(message.Content);
        ChatPayload chatPayload = new()
        {
            Name = message.Username,
            Time = Utility.ConvertISOToHHMM(message.CreateTime),
            Content = data.content,
            // Avatar = message.
        };
        return chatPayload;
    }

    public override void OnClickCloseButton()
    {
        Hide(false);
    }
}
