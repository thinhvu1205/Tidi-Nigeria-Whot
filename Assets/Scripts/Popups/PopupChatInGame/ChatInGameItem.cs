using System.Collections;
using System.Collections.Generic;
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

    public void SetInfo(ChatData data)
    {
        // Current player
        if (true)
        {
            currentPlayer.gameObject.SetActive(true);
            textTimeCurrentPlayer.text = "12:34";
            string text = "hellohelohghfodjisojfiosdfjsdiofs fjsdifojsdfjis";
            // for (int i = 0; i < index; i++)
            // {
            //     text += "hello hello hello hello hello hello hello hello hello hello hello";
            // }
            textMessageCurrentPlayer.text = text;

            float textWidth = textMessageCurrentPlayer.preferredWidth;
            float textHeight = textMessageCurrentPlayer.preferredHeight;
            rectTransformCurrentPlayer.sizeDelta = new Vector2(textWidth + PADDING_WIDTH, rectTransformCurrentPlayer.sizeDelta.y);
            if (textWidth + PADDING_WIDTH > MAX_WIDTH)
            {
                rectTransformCurrentPlayer.sizeDelta = new Vector2(MAX_WIDTH, textHeight);
                // imageNarrow.position = new Vector2(imageNarrow.position.x, imageNarrow.position.y - 30f);
            }
            else if (textWidth + 30 < 300)
            {
                rectTransformCurrentPlayer.sizeDelta = new Vector2(textWidth + PADDING_WIDTH, rectTransformCurrentPlayer.sizeDelta.y);
            }

        }
    }
}
