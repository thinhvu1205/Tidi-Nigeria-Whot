using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api;
using Cysharp.Threading.Tasks;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginView : BaseView
{
    [SerializeField] private GameObject loginView, loginForm;
    [SerializeField] private TMP_InputField idInputField, passwordInputField;
    private const string FIRST_LOGIN_KEY = "firstLogin";
    protected override void Awake()
    {
        base.Awake();
        loginView.SetActive(true);
        loginForm.SetActive(false);
    }

    protected override void Start()
    {

        HandleFirstLogin();
    }

    #region Buttons
    public void OnClickButtonLoginWithID()
    {
        loginForm.SetActive(true);
        loginView.SetActive(false);
    }

    public void OnClickButtonPlayGuest()
    {
        HandleClickButtonPlayGuest().Forget();
    }
    public async UniTask HandleClickButtonPlayGuest()
    {
        Config.loginType = LoginType.PLAYNOW;
        PlayerPrefs.SetInt(Config.TYPE_LOGIN_KEY, (int)Config.loginType);
        PlayerPrefs.Save();

        try
        {
            await DataSender.LoginAsGuest();
            await OpenLobbyView();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during guest login: {e.Message}");
            throw;
        }
    }

    public void OnClickButtonClose()
    {
        loginForm.SetActive(false);
        loginView.SetActive(true);
    }

    public async void OnClickButtonSubmit()
    {
        Config.loginType = LoginType.NORMAL;
        PlayerPrefs.SetInt(Config.TYPE_LOGIN_KEY, (int)Config.loginType);
        PlayerPrefs.Save();
        string id = idInputField.text;
        string password = passwordInputField.text;

        try
        {
            await DataSender.LoginWithAccount(id, password);
            await OpenLobbyView();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during login with ID: {e.Message}");
        }

    }
    #endregion

    private void HandleFirstLogin()
    {
        bool isFirstLogin = PlayerPrefs.GetInt(FIRST_LOGIN_KEY, 0) == 0;
        if (isFirstLogin)
        {
            Debug.Log("First login");
            PlayerPrefs.SetInt(FIRST_LOGIN_KEY, 1);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.Log("Not first login, reconnecting...");
            Reconnect();
        }
    }

    private async void Reconnect()
    {
        LoginType loginType = (LoginType)PlayerPrefs.GetInt(Config.TYPE_LOGIN_KEY, (int)LoginType.NONE);
        Config.loginType = loginType;
        Debug.Log($"Reconnecting with login type: {Config.loginType}");
        try
        {
            switch (Config.loginType)
            {
                case LoginType.PLAYNOW:
                    await DataSender.LoginAsGuest();
                    break;
                case LoginType.NORMAL:
                    string id = Config.userName;
                    string password = Config.userPass;
                    await DataSender.LoginWithAccount(id, password);
                    break;
                default:
                    Debug.LogWarning("Unknown login type, defaulting to guest login.");
                    break;
            }
            await OpenLobbyView();
        }
        catch (Exception)
        {
            Debug.LogError($"Error during reconnect with login type: {Config.loginType}");
        }
    }

    private async Task OpenLobbyView()
    {
        try
        {
            if (Config.isLoginSuccessful)
            {
                Profile profile = await DataSender.GetProfile();
                UpdateProfile(profile);
                SceneManager.LoadScene(Config.MAIN_SCENE);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"err : {e.Message}");
        }
    }

    private void UpdateProfile(Profile profile)
    {
        User.userMain = new()
        {
            userId = profile.UserId,
            displayName = profile.DisplayName,
            avatarId = profile.AvatarId,
            accountChip = profile.AccountChip.ToString(),
            bankChip = profile.BankChip.ToString(),
        };

    }
}
