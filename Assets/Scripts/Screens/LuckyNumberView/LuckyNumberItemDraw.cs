using System;
using System.Globalization;
using Globals;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LuckyNumberItemDraw : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textDay;
    [SerializeField] private TextMeshProUGUI textDate;
    [SerializeField] private TextMeshProUGUI textTime;
    [SerializeField] private Image backGroundImage;
    [SerializeField] private Image drawNumImage;
    [SerializeField] private Sprite[] listSpriteBackground; // 0: Unselected, 1: Selected
    [SerializeField] private Sprite[] listSpriteDrawNum; 
    public long id;
    public bool isSelected = false;
    public bool isInteractable = true;

    public void SetInfo(LotteryDraw draw, int index)
    {
        id = draw.Id;
        long time = draw.DrawTimeUnix;
        textDay.text = Utility.UnixToDayOfWeek(time);
        textDate.text = Utility.UnixToDate(time);
        textTime.text = Utility.UnixToTime(time);
        if (index < 0 || index >= listSpriteDrawNum.Length) return;
        drawNumImage.sprite = listSpriteDrawNum[index];
    }

    public void ToggleSelected()
    {
        isSelected = !isSelected;
        if (isSelected)
        {
            backGroundImage.sprite = listSpriteBackground[1];
        }
        else
        {
            backGroundImage.sprite = listSpriteBackground[0];
        }
    }

}
