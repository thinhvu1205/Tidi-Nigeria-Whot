using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoginView : BaseView
{
    [SerializeField] private GameObject loginView, loginForm; 
    [SerializeField] private TMP_InputField idInputField, passwordInputField;

    protected override void Awake()
    {
        base.Awake();
        loginView.SetActive(true);
        loginForm.SetActive(false);
    }

    #region Buttons
    public void OnClickButtonLoginWithID()
    {
        loginForm.SetActive(true);
        loginView.SetActive(false);
    }

    public void OnClickButtonPlayGuest()
    {

    }

    public void OnClickButtonClose()
    {
        loginForm.SetActive(false);
        loginView.SetActive(true);
    }

    public void OnClickButtonSubmit()
    {
        Debug.Log("id: " + idInputField.text);
        Debug.Log("password: " + passwordInputField.text);
    }
    #endregion
}
