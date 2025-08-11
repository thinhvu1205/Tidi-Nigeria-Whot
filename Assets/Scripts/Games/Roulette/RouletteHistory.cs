using System.Collections;
using System.Collections.Generic;
using Globals;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RouletteHistory : MonoBehaviour
{
    public Sprite[] listBackgroundImage;
    public Image ImageResult;
    public TextMeshProUGUI TextResult;
    public SkeletonGraphic Animation;
    public int Value { get; private set; }
    public bool IsRed { get; private set; } = false;

    public void Init(int result, int num, bool isActiveAnimation)
    {
        Value = result;
        ImageResult.sprite = listBackgroundImage[num];
        ImageResult.SetNativeSize();
        TextResult.text = result.ToString();
        if (isActiveAnimation)
        {
            Utility.PlayAnimation(Animation, "khung1", true);
        }
        if (num == 1)
        {
            IsRed = true;
        }
    }
}
