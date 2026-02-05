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
    [SerializeField] private ScrollRect scrollRect;
    public string nextCursor = "";
    public string prevCursor = "";
    private bool isAtTop = true;
    private bool isAtBottom = true;
    private bool ignoreScrollEvent;
    private ChatWorldPresenter chatWorldPresenter;
    private List<PoolInfo> listPoolInfo = new();

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
        scrollRect.onValueChanged.AddListener(OnScroll);
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();
        NetworkManager.INSTANCE.OnMessageWorldReceived -= NetworkManager_OnMessageReceived;
        scrollRect.onValueChanged.RemoveListener(OnScroll);
    }

    public void OnScroll(Vector2 pos)
    {
        if (ignoreScrollEvent) return;
        // Scroll lên đầu
        if (scrollRect.verticalNormalizedPosition >= 0.99f)
        {
            if (!isAtTop)
            {
                isAtTop = true;
                OnReachTop();
                return;
            }
        }
        else 
        {
            isAtTop = false;
        }

        if (scrollRect.verticalNormalizedPosition <= 0.01f)
        {
            if (!isAtBottom)
            {
                isAtBottom = true;
                OnReachBottom();
                return;
            }
        }
        else 
        {
            isAtBottom = false;
        }
    }

    private void OnReachTop()
    {
        if (!string.IsNullOrEmpty(nextCursor) && nextCursor != prevCursor)
        {
            _ = GetHistory(nextCursor);  
        }
    }
    private void OnReachBottom()
    {
        // if (!string.IsNullOrEmpty(prevCursor) && nextCursor != prevCursor)
        // {
        //     _ = GetHistory(prevCursor);  
        // }
    }

    // private async UniTask GetHistory(string cursor = "")
    // {
    //     ignoreScrollEvent = true;
    //     isAtTop = true;
    //     listMessage = await chatWorldPresenter.GetWorldChatHistory(cursor);
    //     if (listMessage.Count > 0)
    //     {
    //         // listPoolInfo.Clear(); // Clear list hiện tại
    //         foreach(IApiChannelMessage message in listMessage)
    //         {
    //             ChatPayload chatPayload = ConvertToChatPayload(message);
    //             listPoolInfo.Insert(0, new PoolInfo { Data = chatPayload });
    //         }
    //         verticalPoolGroup.SetControlInfo(listPoolInfo, listPoolInfo.Count - 1);
    //         // verticalPoolGroup.ScrollToLast(0);
    //     }
    //     // scrollRect.verticalNormalizedPosition = 0.1f;

    // // Đợi layout ổn định rồi mới mở lại
    //     await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
    //     ignoreScrollEvent = false;
    // }

    private async UniTask GetHistory(string cursor = "")
    {
        ignoreScrollEvent = true;
        UIManager.Instance.ShowProgressing();
        var olderMessages = await chatWorldPresenter.GetWorldChatHistory(cursor);
        if (olderMessages == null || olderMessages.Count == 0)
        {
            ignoreScrollEvent = false;
            return;
        }

        // Ghi lại thông tin trước khi thêm (có thể không cần nữa, nhưng giữ để debug)
        float contentYBefore = verticalPoolGroup._DataSR.content.localPosition.y;
        float viewportHeight = verticalPoolGroup._DataSR.viewport.rect.height;

        // Thêm vào ĐẦU danh sách (tin cũ hơn ở trên)
        int oldCount = listPoolInfo.Count;
        List<PoolInfo> listPoolTemp = new();
        foreach (IApiChannelMessage message in olderMessages)
        {
            ChatPayload payload = ConvertToChatPayload(message);
            listPoolTemp.Add(new PoolInfo { Data = payload });
        }
        listPoolInfo.InsertRange(0, listPoolTemp);

        // Cập nhật control info
        verticalPoolGroup.SetControlInfo(listPoolInfo, listPoolInfo.Count - 1);

        // Đợi layout tính height + vị trí LocalY mới
        await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
        await UniTask.Yield(PlayerLoopTiming.PostLateUpdate); // an toàn hơn

        // ───────────────────────────────────────────────
        // TÍNH VỊ TRÍ ĐỂ BOTTOM CỦA BATCH MỚI NẰM Ở BOTTOM VIEWPORT
        // ───────────────────────────────────────────────

        // Index của tin nhắn cuối cùng trong batch vừa thêm (sau khi InsertRange(0,...))
        int lastAddedIndex = olderMessages.Count - 1;

        // Lấy PoolInfo của item đó
        PoolInfo lastAddedInfo = listPoolInfo[lastAddedIndex];

        // LocalYBot của item cuối batch mới (đây là điểm dưới cùng của item đó)
        float targetBottomY = lastAddedInfo.LocalYBot;
        // Để item này nằm sát bottom viewport → content phải dịch sao cho:
        // content.localPosition.y + targetBottomY = -viewportHeight
        // → content.localPosition.y = -viewportHeight - targetBottomY
        float targetY = -viewportHeight - targetBottomY;

        // Optional: dịch lên một chút để có khoảng trống đẹp (ví dụ 20-50px)
        targetY += 240f;  // điều chỉnh theo cảm giác, có thể để 0 hoặc 20-80

        // Clamp để không vượt giới hạn
        float maxY = 1000f;
        float contentHeight = verticalPoolGroup._DataSR.content.sizeDelta.y;
        float minY = -(contentHeight - viewportHeight);
        // targetY = Mathf.Clamp(targetY, minY, maxY);

        // Áp dụng vị trí
        verticalPoolGroup._DataSR.content.localPosition = new Vector2(0, targetY);

        // Nếu muốn animate mượt (tùy chọn)
        // verticalPoolGroup._DataSR.content.DOLocalMoveY(targetY, 0.25f).SetEase(Ease.OutQuad);

        ignoreScrollEvent = false;
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
