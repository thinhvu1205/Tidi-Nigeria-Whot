using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLayout : MonoBehaviour
{
    [SerializeField] private RectTransform avatar;
    [SerializeField] private RectTransform cards;
    [SerializeField] private RectTransform info;
    [SerializeField] private Transform username;
    [SerializeField] private Transform chip;
    [SerializeField] private Image backgroundImage;

    public enum EPlayerLayout
    {
        Left,
        Top,
        Right
    }

    public void SetLayout(EPlayerLayout layout)
    {
        switch (layout)
        {
            case EPlayerLayout.Left:
                SetOrder(avatar, 0);
                SetOrder(cards, 1);
                SetOrder(info, 2);
                break;

            case EPlayerLayout.Top:
                SetOrder(cards, 0);
                SetOrder(avatar, 1);
                SetOrder(info, 2);
                break;

            case EPlayerLayout.Right:
                SetOrder(info, 0);
                SetOrder(cards, 1);
                SetOrder(avatar, 2);
                username.Translate(54f, 0f, 0f);
                chip.Translate(54f, 0f, 0f);
                backgroundImage.transform.Rotate(0, 0, 180f);
                break;
        }
    }

    private void SetOrder(RectTransform target, int index)
    {
        target.SetSiblingIndex(index);
    }
}
