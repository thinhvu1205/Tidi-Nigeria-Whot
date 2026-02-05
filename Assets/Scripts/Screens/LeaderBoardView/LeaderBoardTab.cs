using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardTab : MonoBehaviour
{
    public event EventHandler<OnTabClickedEventArgs> OnTabClicked;
    public class OnTabClickedEventArgs : EventArgs
    {
        public string gameCode;
    }
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Image selectedBackgroundImage, unselectedBackgroundImage;
    public string GameCode { get; private set; }
    private void Start()
    {
        // selectedBackgroundImage.gameObject.SetActive(false);
        // unselectedBackgroundImage.gameObject.SetActive(true);
    }

    public void SetData(string name, string gameCode)
    {
        nameText.text = name;
        GameCode = gameCode;
    }

    public void OnClickTab()
    {
        OnTabClicked?.Invoke(this, new OnTabClickedEventArgs
        {
            gameCode = GameCode
        });
    }

    public void SelectTab(bool isSelected)
    {
        selectedBackgroundImage.gameObject.SetActive(isSelected);
        unselectedBackgroundImage.gameObject.SetActive(!isSelected);
        nameText.color = isSelected ? Color.white : Color.gray;
    }
}
