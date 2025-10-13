using System.Collections;
using System.Collections.Generic;
using Globals;
using Nakama;
using TMPro;
using UnityEngine;

public class ChatInGameItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textName, textTimeOtherPlayer, textMessageOtherPlayer, textTimeCurrentPlayer, textMessageCurrentPlayer;
    [SerializeField] private Transform otherPlayer, currentPlayer, imageNarrowOtherPlayer, imageNarrowCurrentPlayer;
    [SerializeField] private GameObject imageBackgroundOtherPlayer, imageBackgroundCurrentPlayer;
    [SerializeField] private Sprite[] listImageBackground, listImageNarrow; // 0: Other, 1: Mine
    private RectTransform rectTransformCurrentPlayer, rectTransformOtherPlayer;
    private const float INITIAL_HEIGHT = 27f;
    private const float MAX_WIDTH = 400f;
    private const float PADDING_WIDTH = 50f;

    private void Awake()
    {
        rectTransformCurrentPlayer = imageBackgroundCurrentPlayer.GetComponent<RectTransform>();
        rectTransformOtherPlayer = imageBackgroundOtherPlayer.GetComponent<RectTransform>();
    }

    public void SetInfo(IApiChannelMessage message, bool isCurrentPlayer)
    {
        // Current player
        if (true)
        {
            var payload = JsonUtility.FromJson<ChatPayload>(message.Content);
            if (string.IsNullOrEmpty(payload.content))
            {
                Destroy(gameObject);
            }
            textName.text = message.Username;
            textTimeOtherPlayer.text = Utility.ConvertISOToHHMM(message.CreateTime);
            textMessageOtherPlayer.text = payload.content;
            float textWidth = textMessageOtherPlayer.preferredWidth;
            float textHeight = textMessageOtherPlayer.preferredHeight;
            rectTransformOtherPlayer.sizeDelta = new Vector2(textWidth + PADDING_WIDTH, rectTransformOtherPlayer.sizeDelta.y);
            if (textWidth + PADDING_WIDTH > MAX_WIDTH)
            {
                rectTransformOtherPlayer.sizeDelta = new Vector2(MAX_WIDTH, rectTransformOtherPlayer.sizeDelta.y);
                // imageNarrow.position = new Vector2(imageNarrow.position.x, imageNarrow.position.y - 30f);
            }
            Debug.Log("TEXT HEIGHT:" + textHeight);
            // else if (textWidth + 30 < 300)
            // {
            //     rectTransform.sizeDelta = new Vector2(textWidth + 30, rectTransform.sizeDelta.y);
            // }

        }
    }
}
