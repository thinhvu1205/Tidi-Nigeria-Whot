using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLayout : MonoBehaviour
{
    [SerializeField] private RectTransform avatar;
    [SerializeField] private RectTransform cards;
    [SerializeField] private RectTransform info;
    [SerializeField] private Transform username;
    [SerializeField] private Transform chip;
    [SerializeField] private Transform effect;
    [SerializeField] private Transform remainingCardsParent;
    [SerializeField] private Transform lastCardNoti;
    [SerializeField] private Image backgroundImage;
    private EPlayerLayout currentLayout;
    public enum EPlayerLayout
    {
        Left,
        Top,
        Right
    }

    public EPlayerLayout GetCurrentLayout()
    {
        return currentLayout;
    }

    public void SetLayout(EPlayerLayout layout)
    {
        Debug.Log($"Setting layout {layout} for player");
        currentLayout = layout;
        switch (layout)
        {
            case EPlayerLayout.Left:
                SetOrder(avatar, 0);
                SetOrder(cards, 1);
                SetOrder(info, 2);
                remainingCardsParent.Translate(-100f, 0f, 0f);
                lastCardNoti.Translate(50f, 0f, 0f);
                break;

            case EPlayerLayout.Top:
                SetOrder(cards, 0);
                SetOrder(avatar, 1);
                SetOrder(info, 2);
                remainingCardsParent.Translate(-72f, 0f, 0f);
                break;

            case EPlayerLayout.Right:
                SetOrder(info, 0);
                SetOrder(cards, 1);
                SetOrder(avatar, 2);
                username.Translate(-64f, 0f, 0f);
                username.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.BottomRight;
                chip.Translate(54f, 0f, 0f);
                effect.Translate(116f, 0f, 0f);
                backgroundImage.transform.Rotate(0, 0, 180f);
                remainingCardsParent.Translate(100f, 0f, 0f);
                lastCardNoti.Translate(180f, 0f, 0f);
                break;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(cards.parent as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(info.parent as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(avatar.parent as RectTransform);

    }

    private void SetOrder(RectTransform target, int index)
    {
        target.SetSiblingIndex(index);
    }
}
