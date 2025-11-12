using System.Collections;
using System.Collections.Generic;
using Globals;
using Nakama;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using Avatar = Common.Objects.Avatar;
public class ChatItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textNameLeft;
    [SerializeField] private TextMeshProUGUI textTimeLeft;
    [SerializeField] private TextMeshProUGUI textMessageLeft;
    [SerializeField] private Avatar avatarLeft;
    [SerializeField] private GameObject contentLeft;
    [SerializeField] private GameObject chatContainerLeft;
    [SerializeField] private Transform imageNarrowLeft;
    [SerializeField] private TextMeshProUGUI textNameRight;
    [SerializeField] private TextMeshProUGUI textTimeRight;
    [SerializeField] private TextMeshProUGUI textMessageRight;
    [SerializeField] private Avatar avatarRight;
    [SerializeField] private GameObject contentRight;
    [SerializeField] private GameObject chatContainerRight;
    [SerializeField] private Transform imageNarrowRight;
    private RectTransform rectTransform;
    private const float PADDING = 12f;
    private const float EXTRA_PADDING = 10f;
    private const float MIN_WIDTH = 50f;
    private const float DEFAULT_MAX_WIDTH = 400f;
    private const float LOBBY_MAX_WIDTH = 500f;
    private bool isLobbyChat = true;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetInfo(ChatPayload data, int index = -1)
    {
        // var payload = JsonUtility.FromJson<ChatPayload>(message.Content);
        if (string.IsNullOrEmpty(data.Content))
        {
            Destroy(gameObject);
        }
        bool isMe = data.Name == User.userProfile.UserName;

        contentLeft.SetActive(!isMe);
        contentRight.SetActive(isMe);
        if (!isMe)
        {
            textNameLeft.text = data.Name;
            textTimeLeft.text = data.Time;
            textMessageLeft.text = data.Content;
            AdjustFrameContent(chatContainerLeft, textMessageLeft);
        }
        else
        {
            textNameRight.text = data.Name;
            textTimeRight.text = data.Time;
            textMessageRight.text = data.Content;
            AdjustFrameContent(chatContainerRight, textMessageRight);
        }
        // float textWidth = textMessage.preferredWidth;
        // float textHeight = textMessage.preferredHeight;

        // Debug.Log("TEXT HEIGHT:" + textHeight);
        // else if (textWidth + 30 < 300)
        // {
        //     rectTransform.sizeDelta = new Vector2(textWidth + 30, rectTransform.sizeDelta.y);
        // }
    }
    
    private void AdjustFrameContent(GameObject frameContent, TextMeshProUGUI messageText)
    {
        if (messageText == null || frameContent == null) return;

        messageText.textWrappingMode = TextWrappingModes.Normal;
        messageText.ForceMeshUpdate();

        float textWidth = messageText.preferredWidth;

        float maxWidth = isLobbyChat ? LOBBY_MAX_WIDTH : DEFAULT_MAX_WIDTH;
        float finalWidth = Mathf.Clamp(textWidth + PADDING, MIN_WIDTH, maxWidth);
        messageText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, finalWidth);
        messageText.ForceMeshUpdate();

        float finalHeight = messageText.preferredHeight + PADDING;

        RectTransform frameRect = frameContent.GetComponent<RectTransform>();
        if (frameRect != null)
        {
            frameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, finalWidth + PADDING * 6);
            frameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalHeight + PADDING);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalHeight + PADDING * 3);
            
            // height = finalHeight + PADDING;
            // width = finalWidth + PADDING * 6;
        }
    }

}


