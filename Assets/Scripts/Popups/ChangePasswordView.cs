using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChangePasswordView : BaseView
{
    [SerializeField] private TMP_InputField currentPasswordInputField, newPasswordInputField, reEnterPasswordInputField;

    public void OnSubmit()
    {
        string currentPassword = currentPasswordInputField.text;
        string newPassword = newPasswordInputField.text;
        string reEnterPassword = reEnterPasswordInputField.text;

        if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(reEnterPassword))
        {
            return;
        }

        if (newPassword != reEnterPassword)
        {
            return;
        }

        try
        {
            DataSender.ChangePassword(oldPassword: currentPassword, password: newPassword);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error changing password: {ex.Message}");
        }
    }
}
