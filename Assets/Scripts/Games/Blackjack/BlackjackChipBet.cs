using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlackjackChipBet : MonoBehaviour
{
    public TextMeshProUGUI textValue;
    public Image imageHighlight;
    public Button button;


    public void OnSelect()
    {
        imageHighlight.gameObject.SetActive(true);
    }

    public void OnUnselect()
    {
        imageHighlight.gameObject.SetActive(false);
    }

    public void SetInfo(long value)
    {
        textValue.text = value.ToString();
        imageHighlight.gameObject.SetActive(false);
    }
}
