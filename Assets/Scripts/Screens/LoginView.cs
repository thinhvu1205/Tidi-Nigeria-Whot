using System;
using System.Collections;
using System.Collections.Generic;
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
        Config.loginType = LoginType.PLAYNOW;
        PlayerPrefs.SetInt(Config.TYPE_LOGIN_KEY, (int)Config.loginType);
        PlayerPrefs.Save();

        try
        {
            DataSender.LoginAsGuest();
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public void OnClickButtonClose()
    {
        loginForm.SetActive(false);
        loginView.SetActive(true);
    }

    public void OnClickButtonSubmit()
    {
        Config.loginType = LoginType.NORMAL;
        PlayerPrefs.SetInt(Config.TYPE_LOGIN_KEY, (int)Config.loginType);
        PlayerPrefs.Save();
        string id = idInputField.text;
        string password = passwordInputField.text;
        DataSender.LoginWithAccount(id, password);

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

    private void Reconnect()
    {
        LoginType loginType = (LoginType)PlayerPrefs.GetInt(Config.TYPE_LOGIN_KEY, (int)LoginType.NONE);
        Config.loginType = loginType;
        Debug.Log($"Reconnecting with login type: {Config.loginType}");
        try
        {
            switch (Config.loginType)
            {
                case LoginType.PLAYNOW:
                    OnClickButtonPlayGuest();
                    break;
                case LoginType.NORMAL:
                    string id = Config.userName;
                    string password = Config.userPass;
                    DataSender.LoginWithAccount(id, password);
                    break;
                default:
                    Debug.LogWarning("Unknown login type, defaulting to guest login.");
                    break;
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
}
