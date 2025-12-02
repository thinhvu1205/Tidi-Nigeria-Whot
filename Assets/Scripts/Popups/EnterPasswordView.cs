using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnterPasswordView : BaseView
{
    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] Button buttonJoin;
    private string correctPassword;

    protected override void Awake()
    {
        base.Awake();
    }

    public void SetPassword(string password)
    {
        correctPassword = password;
        Debug.Log("correctPassword: " + correctPassword);
    }

    public void SetOnClickListener(Func<UniTask> callback)
    {
        buttonJoin.onClick.AddListener(() =>
        {
            Debug.Log("passwordInputField: " + passwordInputField.text);
            if (passwordInputField.text != correctPassword)
            {
                UIManager.Instance.ShowAlertDialog("Wrong password!");
            }
            _ = callback.Invoke();
            Hide();
        });
    }
}
