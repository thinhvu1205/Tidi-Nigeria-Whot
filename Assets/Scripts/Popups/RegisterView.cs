using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using Screens.LoginView;
using TMPro;
using UnityEngine;

public class RegisterView : BaseView
{
    [SerializeField] private TMP_InputField userNameInputField, passwordInputField, reEnterPasswordInputField;
    [SerializeField] private LoginView loginView;

    protected override void OnEnable()
    {
        base.OnEnable();
        userNameInputField.text = "";
        passwordInputField.text = "";
        reEnterPasswordInputField.text = "";
    }
    public void OnSubmit()
    {
        HandleSubmit();
    }

    public override void OnClickCloseButton()
    {
        Hide(false);
    }

    public async void HandleSubmit()
    {
        string userName = userNameInputField.text.Trim();
        string password = passwordInputField.text.Trim();
        string reEnterPassword = reEnterPasswordInputField.text.Trim();

        if (string.IsNullOrEmpty(userName))
        {
            UIManager.Instance.ShowAlertDialog("Username is empty!");
            return;
        }
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(reEnterPassword))
        {
            UIManager.Instance.ShowAlertDialog("Password is empty!");
            return;
        }
        if (password != reEnterPassword)
        {
            UIManager.Instance.ShowAlertDialog("Passwords do not match!");
            return;
        }

        try
        {
            await NetworkManager.INSTANCE.CreateAccount(userName, password);
            UIManager.Instance.ShowAlertDialog("Register successful!", () =>
            {
                Hide(false);
                loginView.OnLoginSuccess();
            });
        }
        catch (Exception ex)
        {
            Error error = DataSender.DecodeFromJson<Error>(ex.Message);
            UIManager.Instance.ShowAlertDialog(error.Error_);
        }
    }
}
