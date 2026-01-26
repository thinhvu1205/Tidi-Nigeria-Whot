using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Globals;
using Nakama;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatWorldView : BaseView
{
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private TextMeshProUGUI textAccountChip;
    [SerializeField] private TextMeshProUGUI textChip;
    [SerializeField] private VerticalPool verticalPoolGroup;
    private ChatWorldPresenter chatWorldPresenter;
    private List<PoolInfo> listPoolInfo = new();
    private List<IApiChannelMessage> listMessage = new();

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
        _ = GetHistory();

        verticalPoolGroup.SetApplyDataCb((go, data, index) =>
        {
            ChatItem chatItem = go.GetComponent<ChatItem>();
            chatItem.SetInfo((ChatPayload)data.Data, null, true, (cellW, cellH) =>
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
            listPoolInfo.Clear(); // Clear list hiện tại
            foreach(IApiChannelMessage message in listMessage)
            {
                ChatPayload chatPayload = ConvertToChatPayload(message);
                listPoolInfo.Add(new PoolInfo { Data = chatPayload });
            }
            verticalPoolGroup.SetControlInfo(listPoolInfo, listPoolInfo.Count - 1);
            // verticalPoolGroup.ScrollToLast(0);
        }
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

    public void OnClickSendMessage()
    {
        Debug.Log("OnClickSendMessage");
        if (!FeatureManager.IsFeatureAllowed(FeatureName.FeatureChatWorld))
        {
            UIManager.Instance.ShowAlertDialog("Chat World is currently unavailable. Please upgrade to VIP 2 or higher, or try again later.", null, transform);
            return;
        }
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
        Debug.Log("SENDER AVATAR:" + data.sender_profile.avt);
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

public struct ChatPayload
{
    public int GameID;
    public int Type;
    public string Name;
    public string Content;
    public int Vip;
    public string Avatar;
    public string ID;
    public int FaceID;
    public string Time;
    public bool IsAudio;
}

[Serializable]
public class ContentData
{
    public string content;
    public SenderProfile sender_profile;
}

[Serializable]
public class SenderProfile
{
    public string avt;
    public int vip_level;
}
