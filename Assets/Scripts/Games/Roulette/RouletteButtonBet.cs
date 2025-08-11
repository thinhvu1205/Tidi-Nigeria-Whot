using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RouletteButtonBet : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image imageBorder;
    [SerializeField] private TextMeshProUGUI textbet;
    private int id;

    public void SetInfo(int value)
    {
        textbet.text = value.ToString();
    }
    public void SetSelected(bool isSelected)
    {
        imageBorder.gameObject.SetActive(isSelected);
    }
}
