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
            await DataSender.LinkUsername(username: userName, password: password);
            UIManager.Instance.ShowAlertDialog("Change name successful!", () => Hide());
            Profile profile = await DataSender.GetProfile();
            UpdateProfile(profile);
        }
        catch (Exception ex)
        {
            UIManager.Instance.ShowAlertDialog(ex.Message);
            Debug.LogError($"Error changing name: {ex.Message}");
        }
    }

    private void UpdateProfile(Profile profile)
    {
        Debug.Log("Profile: " + profile.ToString());
        User.userMain = new()
        {
            userId = profile.UserId,
            userName = profile.UserName,
            displayName = profile.DisplayName,
            avatarId = profile.AvatarId,
            accountChip = profile.AccountChip,
            bankChip = profile.BankChip.ToString(),
            userSid = profile.UserSid.ToString(),
            vipLevel = profile.VipLevel
        };
        User.userMain.UpdateProfile();
    }
}
