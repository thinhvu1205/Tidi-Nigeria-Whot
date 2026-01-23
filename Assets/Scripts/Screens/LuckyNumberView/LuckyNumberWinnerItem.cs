using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Avatar = Common.Objects.Avatar;
using Globals;
public class LuckyNumberWinnerItem : MonoBehaviour
{
    [SerializeField] private Avatar avatarImage;
    [SerializeField] private Image topImage;
    [SerializeField] private TextMeshProUGUI nameText, topText, chipValueText;
    [SerializeField] private List<Sprite> topSprites = new();
    
    public void SetData(string top, string name, long totalReward, string avatarId = "", long vip = 0)
    {
        topText.text = top;
        string nameTxt = name;
        if (name.Length > 10)
        {
            nameTxt = name.Substring(0,7) + "...";
        }
        nameText.text = nameTxt;
        chipValueText.text = Utility.FormatMoney(totalReward, true);
        DisplayTopImage(int.Parse(top));
        avatarImage.LoadAvatar(avatarId, vip);
    }
    
    private void DisplayTopImage(int top)
    {
        if (top > 3)
        {
            topImage.gameObject.SetActive(false);
            topText.gameObject.SetActive(true);
            topText.text = top.ToString();
        }
        else
        {
            topImage.gameObject.SetActive(true);
            topText.gameObject.SetActive(false);
            topImage.sprite = topSprites[top - 1];
        }
    }
}
