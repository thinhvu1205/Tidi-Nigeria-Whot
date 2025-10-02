using System;
using System.Collections;
using System.Collections.Generic;
using Globals;
using Proto;
using TMPro;
using UnityEngine;

public class ChangeNameView : BaseView
{
    [SerializeField] private TMP_InputField userNameInputField, passwordInputField, reEnterPasswordInputField;

    public void OnSubmit()
    {
        HandleSubmit();
    }

    public async void HandleSubmit()
    {
        string userName = userNameInputField.text;
        string password = passwordInputField.text;
        string reEnterPassword = reEnterPasswordInputField.text;

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
            UIManager.Instance.ShowProgressing();
            await DataSender.LinkUsername(username: userName, password: password);
            UIManager.Instance.ShowAlertDialog("Change name successful!", () => Hide());
            await UIManager.Instance.LoadProfileUser();
            UIManager.Instance.HideProgressing();
        }
        catch (Exception ex)
        {
            UIManager.Instance.HideProgressing();
            UIManager.Instance.ShowAlertDialog("Error:" + ex.Message);
        }
    }


}
