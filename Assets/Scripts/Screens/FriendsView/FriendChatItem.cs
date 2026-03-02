using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Globals;
using Nakama;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Avatar = Common.Objects.Avatar;

[RequireComponent(typeof(RectTransform))]
public class FriendChatItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textTimeLeft;
    [SerializeField] private TextMeshProUGUI textMessageLeft;
    [SerializeField] private GameObject contentLeft;
    [SerializeField] private GameObject chatContainerLeft;
    [SerializeField] private TextMeshProUGUI textTimeRight;
    [SerializeField] private TextMeshProUGUI textMessageRight;
    [SerializeField] private GameObject contentRight;
    [SerializeField] private GameObject chatContainerRight;
    private RectTransform rectTransform;
    private ChatPayload chatContentData;
    private static ChatItem currentlyPlayingItem;
    private const float PADDING = 12f;
    private const float MIN_WIDTH = 30f;
    private const float DEFAULT_MAX_WIDTH = 350f;
    private const float LOBBY_MAX_WIDTH = 500f;
    private float width, height;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetInfo(ChatPayload data, Action<float, float> onSizeCalculated = null)
    {
        bool isMe = data.ID == User.userProfile.UserId;
        chatContainerLeft.SetActive(!isMe);
        chatContainerRight.SetActive(isMe);
        if (!isMe)
        {
            textTimeLeft.text = data.Time;
            textMessageLeft.text = data.Content;
            AdjustFrameContent(chatContainerLeft, textMessageLeft);
        }
        else
        {
            textTimeRight.text = data.Time;
            textMessageRight.text = data.Content;
            AdjustFrameContent(chatContainerRight, textMessageRight);
        }
        onSizeCalculated?.Invoke(width, height);
    }
    
    private void AdjustFrameContent(GameObject frameContent, TextMeshProUGUI messageText)
    {
        if (messageText == null || frameContent == null) return;

        messageText.textWrappingMode = TextWrappingModes.Normal;
        messageText.ForceMeshUpdate();

        float textWidth = messageText.preferredWidth;

        float maxWidth = LOBBY_MAX_WIDTH;
        float finalWidth = Mathf.Clamp(textWidth + PADDING, MIN_WIDTH, maxWidth);
        messageText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, finalWidth);
        messageText.ForceMeshUpdate();

        float finalHeight = messageText.preferredHeight + PADDING;
        width = finalWidth;
        height = finalHeight;
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

    private void AdjustFrameContent(GameObject frameContent)
    {
        width = 292f;
        height = 55f;
        RectTransform frameRect = frameContent.GetComponent<RectTransform>();
        if (frameRect != null)
        {
            frameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            frameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            
            // height = finalHeight + PADDING;
            // width = finalWidth + PADDING * 6;
        }
    }
    

}


