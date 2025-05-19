using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardTab : MonoBehaviour
{
    public event EventHandler OnTabClicked;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Image selectedBackgroundImage, unselectedBackgroundImage;

    private void Start()
    {
        selectedBackgroundImage.gameObject.SetActive(false);
        unselectedBackgroundImage.gameObject.SetActive(true);
    }

    public void SetData(string name)
    {
        nameText.text = name;
    }

    public void OnClickTab()
    {
        OnTabClicked?.Invoke(this, EventArgs.Empty);
    }

    public void SelectTab(bool isSelected)
    {
        Debug.Log("SelectTab: " + isSelected);
        selectedBackgroundImage.gameObject.SetActive(isSelected);
        unselectedBackgroundImage.gameObject.SetActive(!isSelected);
    }
}
