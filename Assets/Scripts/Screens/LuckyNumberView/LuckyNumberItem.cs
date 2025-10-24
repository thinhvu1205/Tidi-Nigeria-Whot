using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LuckyNumberItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textNumber;
    [SerializeField] private Sprite[] listBackgroundSprite; // 0: Unselected, 1: Selected
    [SerializeField] private Image imageBackground;
    public int number;
    public bool isSelected = false;

    public void SetInfo(int number)
    {
        this.number = number;
        textNumber.text = number.ToString();
    }

    public void ToggleSelected()
    {
        isSelected = !isSelected;
        if (isSelected)
        {
            imageBackground.sprite = listBackgroundSprite[1];
        }
        else
        {
            imageBackground.sprite = listBackgroundSprite[0];
        }
    }
}
