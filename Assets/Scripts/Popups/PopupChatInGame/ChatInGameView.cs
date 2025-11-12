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
    [SerializeField] private TextMeshProUGUI textAccountChip;
    [SerializeField] private VerticalPoolGroup verticalPoolGroup;
    private List<ChatData> _PoolData = new();
    private ChatInGamePresenter chatInGamePresenter;


    protected override void Awake()
    {
        base.Awake();
        chatInGamePresenter = new ChatInGamePresenter();
        chatInGamePresenter.Init(this);
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

    protected override void OnEnable()
    {
        base.OnEnable();
        NetworkManager.INSTANCE.OnMessageTableReceived += NetworkManager_OnMessageTableReceived;
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();
        NetworkManager.INSTANCE.OnMessageTableReceived -= NetworkManager_OnMessageTableReceived;
    }

    private void NetworkManager_OnMessageTableReceived(IApiChannelMessage message)
    {
        var payload = JsonUtility.FromJson<ChatPayload>(message.Content);
        if (!string.IsNullOrEmpty(payload.Content))
        {
            bool isCurrentPlayer = message.SenderId == User.userProfile.UserId;
            ChatInGameItem chatItem = Instantiate(messagePrefab, messageContentParent);
            Debug.Log("MESSAGE CONTENT: " + message.CreateTime);
            chatItem.SetInfo(message, isCurrentPlayer);
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
}
public class ChatData
{
    public int Id;
    public string Content;
    public bool IsUser;
    public bool IsSelect;
}