using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Globals;
using Nakama;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatWorldView : BaseView
{
    [SerializeField] private ChatWorldItem messagePrefab;
    [SerializeField] private Transform messageContentParent;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private TextMeshProUGUI textAccountChip;
    [SerializeField] private ScrollRect scrollRect;
    private ChatWorldPresenter chatWorldPresenter;
    private List<IApiChannelMessage> listMessage = new();

    protected override void Awake()
    {
        base.Awake();
        chatWorldPresenter = new ChatWorldPresenter();
        chatWorldPresenter.Init(this);
        chatInputField.characterLimit = 200;
        _ = GetHistory();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        NetworkManager.INSTANCE.OnMessageWorldReceived += NetworkManager_OnMessageReceived;
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();
        NetworkManager.INSTANCE.OnMessageWorldReceived -= NetworkManager_OnMessageReceived;
    }

    private void InitMessageUI()
    {

        foreach (Transform child in messageContentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (IApiChannelMessage message in listMessage)
        {
            var payload = JsonUtility.FromJson<ChatPayload>(message.Content);
            if (!string.IsNullOrEmpty(payload.content))
            {
                bool isCurrentPlayer = message.SenderId == User.userProfile.UserId;
                ChatWorldItem chatWorldItem = Instantiate(messagePrefab, messageContentParent);
                Debug.Log("MESSAGE CONTENT: " + message.CreateTime);
                chatWorldItem.SetInfo(message, isCurrentPlayer);
            }
        }
        StartCoroutine(WaitAndScrollToEnd());

    }

    private IEnumerator WaitAndScrollToEnd()
    {
        yield return null; // chờ 1 frame
        scrollRect.verticalNormalizedPosition = 0f; // 0 = cuối, 1 = đầu
    }


    private async UniTask GetHistory()
    {
        listMessage = await chatWorldPresenter.GetWorldChatHistory();
        Debug.Log("HISTORY RESULT: " + listMessage);
        InitMessageUI();
    }

    private void NetworkManager_OnMessageReceived(IApiChannelMessage message)
    {
        var payload = JsonUtility.FromJson<ChatPayload>(message.Content);
        if (!string.IsNullOrEmpty(payload.content))
        {
            bool isCurrentPlayer = message.SenderId == User.userProfile.UserId;
            ChatWorldItem chatWorldItem = Instantiate(messagePrefab, messageContentParent);
            Debug.Log("MESSAGE CONTENT: " + message.CreateTime);
            chatWorldItem.SetInfo(message, isCurrentPlayer);
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

}

public struct ChatPayload
{
    public string content;
}
