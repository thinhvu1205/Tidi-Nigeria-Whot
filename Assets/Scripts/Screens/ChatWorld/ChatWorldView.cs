using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GIKCore.Pool;
using Globals;
using Nakama;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatWorldView : BaseView
{
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private TextMeshProUGUI textAccountChip;
    [SerializeField] private TextMeshProUGUI textChip;
    [SerializeField] private VerticalPoolGroup verticalPoolGroup;
    private ChatWorldPresenter chatWorldPresenter;
    private List<IApiChannelMessage> listMessage = new();
    private List<ChatPayload> listChatPayload = new();
    private float containerWidth;

    protected override void Awake()
    {
        base.Awake();
        chatWorldPresenter = new ChatWorldPresenter();
        chatWorldPresenter.Init(this);
        chatInputField.characterLimit = 200;
    }

    protected override void Start()
    {
        base.Start();
        containerWidth = Screen.width - 230f;
        _ = GetHistory();

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

    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateProfileData();
        NetworkManager.INSTANCE.OnMessageWorldReceived += NetworkManager_OnMessageReceived;
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();
        NetworkManager.INSTANCE.OnMessageWorldReceived -= NetworkManager_OnMessageReceived;
    }

    private async UniTask GetHistory()
    {
        listMessage = await chatWorldPresenter.GetWorldChatHistory();
        Debug.Log("HISTORY RESULT: " + listMessage);
        if (listMessage.Count > 0)
        {
            listChatPayload.Clear(); // Clear list hiện tại
            foreach(IApiChannelMessage message in listMessage)
            {
                ChatPayload chatPayload = ConvertToChatPayload(message);
                listChatPayload.Add(chatPayload); // Add tất cả items từ list gốc
            }
            verticalPoolGroup.SetAdapter(listChatPayload);
            verticalPoolGroup.ScrollToLast(0);
        }
    }

    private void NetworkManager_OnMessageReceived(IApiChannelMessage message)
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
            _ = chatWorldPresenter.SendMessage(chatInputField.text);
            chatInputField.text = "";
        }
    }

    public void UpdateProfileData()
    {
        if (User.userProfile != null)
        {
            textChip.text = Utility.FormatNumber(User.userProfile.AccountChip);
        }
    }

    private ChatPayload ConvertToChatPayload(IApiChannelMessage message)
    {
        ContentData data = JsonUtility.FromJson<ContentData>(message.Content);
        Debug.Log("dataaaa: " + data.content.ToString());
        ChatPayload chatPayload = new()
        {
            Name = message.Username,
            Time = Utility.ConvertISOToHHMMDDMMYYYY(message.CreateTime),
            Content = data.content
        };
        return chatPayload;
    }

}

public struct ChatPayload
{
    public int GameID;
    public int Type;
    public string Name;
    public string Content;
    public int Vip;
    public int Avatar;
    public int ID;
    public int FaceID;
    public string Time;
    public bool IsAudio;
}

public struct ContentData
{
    public string content;
}
