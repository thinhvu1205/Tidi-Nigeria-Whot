using System.Collections;
using System.Collections.Generic;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Avatar = Common.Objects.Avatar;

public class LeaderBoardItem : MonoBehaviour
{
    [SerializeField] private Avatar avatarImage;
    [SerializeField] private Image topImage;
    [SerializeField] private TextMeshProUGUI nameText, topText, chipValueText;
    [SerializeField] private List<Sprite> topSprites = new();

    public void SetData(string top, string name, string score, string avatarId = "", long vip = 0)
    {
        topText.text = top;
        nameText.text = name;
        chipValueText.text = Utility.FormatNumber(int.Parse(score));
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
