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
        HandleSubmit();
    }
    public async void HandleSubmit()
    {
        string currentPassword = currentPasswordInputField.text;
        string newPassword = newPasswordInputField.text;
        string reEnterPassword = reEnterPasswordInputField.text;

        if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(reEnterPassword))
        {
            UIManager.Instance.ShowAlertDialog("Empty field!");
            return;
        }

        if (newPassword != reEnterPassword)
        {
            UIManager.Instance.ShowAlertDialog("Passwords do not match!");
            return;
        }

        try
        {
            await DataSender.ChangePassword(oldPassword: currentPassword, password: newPassword);
            UIManager.Instance.ShowAlertDialog("Change password successful!", () => Hide());
        }
        catch (Exception ex)
        {
            UIManager.Instance.ShowAlertDialog(ex.Message);
            Debug.LogError($"Error changing password: {ex.Message}");
        }
    }
}
