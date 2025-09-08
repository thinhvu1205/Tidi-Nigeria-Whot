using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardItem : MonoBehaviour
{
    [SerializeField] private Image avatarImage, topImage;
    [SerializeField] private TextMeshProUGUI nameText, topText, chipValueText;
    [SerializeField] private List<Sprite> topSprites = new();

    public void SetData(string top, string name, string score)
    {
        topText.text = top;
        nameText.text = name;
        chipValueText.text = score;
        DisplayTopImage(int.Parse(top));
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
