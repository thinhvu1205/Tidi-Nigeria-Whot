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

    protected override void Awake()
    {
        base.Awake();
    }
    
    public void SetOnClickListener(Func<string, UniTask> callback)
    {
        buttonJoin.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowProgressing();
            callback.Invoke(passwordInputField.text);
            Hide();
        });
    }
}
