using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlackjackChipBet : MonoBehaviour
{
    public TextMeshProUGUI textValue;
    public Image imageHighlight;
    public Image imageChip;
    public Button button;


    public void OnSelect()
    {
        imageHighlight.gameObject.SetActive(true);
        imageChip.transform.localScale = Vector3.one * 1.2f;
    }

    public void OnUnselect()
    {
        imageHighlight.gameObject.SetActive(false);
        imageChip.transform.localScale = Vector3.one;
    }

    public void SetInfo(long value)
    {
        textValue.text = value.ToString();
        imageHighlight.gameObject.SetActive(false);
    }
}
