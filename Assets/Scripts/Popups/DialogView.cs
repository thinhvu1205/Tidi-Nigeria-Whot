using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogView : BaseView
{
    [SerializeField] private Button cancelButton, confirmButton;
    [SerializeField] private TextMeshProUGUI textContent;
    private Action cancelCallback, confirmCallback;

    public void OnClickConfirmButton()
    {
        Hide(true, confirmCallback);
    }

    public void OnClickCancelButton()
    {
        Hide(true, cancelCallback);
    }

    public void ConfigConfirmButton(bool isShow, string buttonText, Action callback)
    {
        confirmButton.gameObject.SetActive(isShow);
        confirmButton.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;
        confirmCallback = callback;
    }

    public void ConfigCancelButton(bool isShow, string buttonText, Action callback)
    {
        cancelButton.gameObject.SetActive(isShow);
        cancelButton.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;
        cancelCallback = callback;
    }

    public void SetContent(string content)
    {
        textContent.text = content;
    }
}
