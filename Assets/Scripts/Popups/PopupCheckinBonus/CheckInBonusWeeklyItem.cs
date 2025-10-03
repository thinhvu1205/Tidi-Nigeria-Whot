using System;
using System.Collections;
using System.Collections.Generic;
using Globals;
using Proto;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CheckInBonusView;

public class CheckInBonusWeeklyItem : MonoBehaviour
{
    public static event Action OnButtonClicked;
    [SerializeField] private TextMeshProUGUI textDay, textButton, textChip;
    [SerializeField] private Image imageChip, imageBoxChip, imageButton, imageBackground, imageIconChip;
    [SerializeField] private GameObject tick;
    [SerializeField] private Button button;
    [SerializeField]
    private Sprite[]
        listSpriteBackground, // 0: purple, 1: yellow, 2: gray
        listSpriteBoxChip, // 0: enable, 1: disable
        listSpriteChipEnabled,
        listSpriteChipDisabled,
        listSpriteButton, // 0: yellow, 1: green, 2: gray
        listSpriteIconChip; // 0: enable, 1: disable

    public void SetInfo(int day, long amount, RewardState state)
    {
        textDay.text = "Day " + day;
        textChip.text = Utility.FormatMoney(amount, true);
        switch (state)
        {
            case RewardState.NOT_RECEIVE:
                textButton.text = "Receive";
                imageBoxChip.sprite = listSpriteBoxChip[1];
                imageButton.sprite = listSpriteButton[2];
                imageBackground.sprite = listSpriteBackground[2];
                imageChip.sprite = listSpriteChipDisabled[day - 1];
                imageIconChip.sprite = listSpriteIconChip[1];
                button.interactable = false;
                tick.SetActive(false);
                break;
            case RewardState.RECEIVABLE:
                textButton.text = "Receive";
                imageBoxChip.sprite = listSpriteBoxChip[0];
                imageButton.sprite = listSpriteButton[1];
                imageBackground.sprite = listSpriteBackground[1];
                imageChip.sprite = listSpriteChipEnabled[day - 1];
                imageIconChip.sprite = listSpriteIconChip[0];
                button.interactable = true;
                tick.SetActive(false);
                break;
            case RewardState.RECEIVED:
                imageBoxChip.sprite = listSpriteBoxChip[0];
                imageButton.sprite = listSpriteButton[0];
                imageBackground.sprite = listSpriteBackground[0];
                imageChip.sprite = listSpriteChipEnabled[day - 1];
                imageIconChip.sprite = listSpriteIconChip[0];
                textButton.text = "Received";
                button.interactable = false;
                tick.SetActive(true);
                break;
        }
    }

    public void OnClickRêcive()
    {
        OnButtonClicked?.Invoke();
    }
}
