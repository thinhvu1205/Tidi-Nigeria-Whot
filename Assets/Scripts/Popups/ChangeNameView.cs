using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChangeNameView : BaseView
{
    [SerializeField] private TMP_InputField userNameInputField, passwordInputField, reEnterPasswordInputField;

    public void OnSubmit()
    {
        string userName = userNameInputField.text;
        string password = passwordInputField.text;
        string reEnterPassword = reEnterPasswordInputField.text;

        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(reEnterPassword))
        {
            return;
        }

        if (password != reEnterPassword)
        {
            return;
        }

        try
        {
            DataSender.LinkUsername(username: userName, password: password);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error changing name: {ex.Message}");
        }
    }
}
