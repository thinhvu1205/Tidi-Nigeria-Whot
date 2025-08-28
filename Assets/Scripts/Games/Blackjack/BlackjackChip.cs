
using Games;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlackjackChip : ChipBet
{
    [SerializeField] private Image imageInsurance;
    [SerializeField] private TextMeshProUGUI textValue;

    public void SetInfo(int index, Vector2 startPosition, long value = 0)
    {
        imgChip.sprite = sprChips[index];
        transform.localPosition = startPosition;

        // Insurance Chip
        if (index == 5)
        {
            imageInsurance.gameObject.SetActive(true);
            textValue.text = value.ToString();
        }
        else
        {
            imageInsurance.gameObject.SetActive(false);
        }
    }
}
