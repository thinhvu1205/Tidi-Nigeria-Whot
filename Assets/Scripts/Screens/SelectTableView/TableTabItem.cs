using System.Collections;
using System.Collections.Generic;
using Globals;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TableTabItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image backgroundImage, backGroundImageActive;

    public void SetData(float mark, bool isDisable)
    {
        gameObject.name = "" + mark.ToString();
        amountText.text = Utility.FormatMoney((int)mark, true);
    }

    public void SetSelected()
    {
        backgroundImage.gameObject.SetActive(false);
        backGroundImageActive.gameObject.SetActive(true);
    }

    public void SetUnselected()
    {
        backgroundImage.gameObject.SetActive(true);
        backGroundImageActive.gameObject.SetActive(false);
    }
}
