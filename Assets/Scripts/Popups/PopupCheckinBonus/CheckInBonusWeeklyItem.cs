using System.Collections;
using System.Collections.Generic;
using Proto;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckInBonusWeeklyItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textDay, textButton, textChip;
    [SerializeField] private Image imageChip, imageBoxChip, imageButton, imageBackground, imageIconChip;
    [SerializeField] private GameObject tick;
    [SerializeField] private SkeletonGraphic animationLight, animationGift;
    [SerializeField]
    private Sprite[]
        listSpriteBackground, // 0: purple, 1: yellow, 2: gray
        listSpriteBoxChip, // 0: enable, 1: disable
        listSpriteChipEnabled,
        listSpriteChipDisabled,
        listSpriteButton, // 0: yellow, 1: green, 2: gray
        listSpriteIconChip; // 0: enable, 1: disable

    public void SetInfo(RewardTemplate reward)
    {
        // textChip.text = reward.BasicChips
        // imageChip.sprite = 
    }
}
