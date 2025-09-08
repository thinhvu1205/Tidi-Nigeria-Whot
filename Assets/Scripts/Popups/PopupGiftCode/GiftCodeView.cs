using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GiftCodeView : BaseView
{
    [SerializeField] private TMP_InputField giftCodeInputField;
    private GiftCodePresenter giftCodePresenter;

    protected override void Awake()
    {
        base.Awake();
        giftCodePresenter = new GiftCodePresenter();
        giftCodePresenter.Init(this);
    }

    public void OnClickButtonSubmit()
    {
        if (string.IsNullOrEmpty(giftCodeInputField.text)) return;
        UIManager.Instance.ShowProgressing();
        string giftCode = giftCodeInputField.text;
        _ = giftCodePresenter.OnSubmit(giftCode);
    }

    public void OnSubmitFinished(string message)
    {
        giftCodeInputField.text = "";
        UIManager.Instance.HideProgressing();
        UIManager.Instance.ShowAlertDialog(message);
    }
}
