using System.Collections;
using System.Collections.Generic;
using Globals;
using Nakama;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

public class ChatWorldItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textName, textTime, textMessage;
    [SerializeField] private Transform imageNarrow;
    private RectTransform rectTransform;
    private const float INITIAL_HEIGHT = 27f;
    private const float MAX_WIDTH = 600f;
    private const float PADDING_WIDTH = 30f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetInfo(IApiChannelMessage message, bool isCurrentPlayer)
    {
        var payload = JsonUtility.FromJson<ChatPayload>(message.Content);
        if (string.IsNullOrEmpty(payload.content))
        {
            Destroy(gameObject);
        }
        textName.text = message.Username;
        textTime.text = Utility.ConvertISOToHHMM(message.CreateTime);
        textMessage.text = payload.content;
        float textWidth = textMessage.preferredWidth;
        float textHeight = textMessage.preferredHeight;
        rectTransform.sizeDelta = new Vector2(textWidth + PADDING_WIDTH, rectTransform.sizeDelta.y);
        if (textWidth + PADDING_WIDTH > MAX_WIDTH)
        {
            rectTransform.sizeDelta = new Vector2(MAX_WIDTH, rectTransform.sizeDelta.y);
            // imageNarrow.position = new Vector2(imageNarrow.position.x, imageNarrow.position.y - 30f);
        }
        Debug.Log("TEXT HEIGHT:" + textHeight);
        // else if (textWidth + 30 < 300)
        // {
        //     rectTransform.sizeDelta = new Vector2(textWidth + 30, rectTransform.sizeDelta.y);
        // }
    }

}


