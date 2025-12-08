using System;
using Proto;
using Cysharp.Threading.Tasks;
using Globals;
using Google.Protobuf;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

namespace Screens.LoginView
{
    public class LoginView : BaseView
    {
        [SerializeField] private GameObject loginView, loginForm, registerPopup;
        [SerializeField] private TMP_InputField idInputField, passwordInputField;
        [SerializeField] private TMP_Dropdown serverDropdown;
        private const string FIRST_LOGIN_KEY = "firstLogin";
        private LoginPresenter loginPresenter;

        protected override void Awake()
        {
            base.Awake();
            loginView.SetActive(true);
            loginForm.SetActive(false);
            SoundManager.Instance.PlayMusicLobby();
            UIManager.Instance.HideProgressing();
            if (!UIManager.Instance.isShowingGlobalDialog)
            {
                UIManager.Instance.HideGlobalDialog();
            }
        }

        protected override void Start()
        {
            loginPresenter = new LoginPresenter();
            loginPresenter.Init(this);
            serverDropdown.onValueChanged.AddListener(OnSwitchServer);
            switch (PlayerPrefs.GetString("IpServer", NetworkManager.SERVER_TEST_PORT))
            {
                case NetworkManager.SERVER_TEST_PORT:
                    serverDropdown.value = 0;
                    break;
                case NetworkManager.SERVER_HUY_PORT:
                    serverDropdown.value = 1;
                    break;
                case NetworkManager.SERVER_TOAN_PORT:
                    serverDropdown.value = 2;
                    break;
                default:
                    serverDropdown.value = 0;
                    break;
            }
            HandleFirstLogin();
        }
    
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
                Debug.Log("Not first login, Try AutoLogin ...");
                // NetworkManager.INSTANCE.res
                OnAutoLogin();
            }
        }

        private void OnAutoLogin()
        {
            if(PlayerPrefs.GetInt(Config.AUTO_LOGIN, 0) == 0) 
                return; // Not Auto Login If User Log Out
        
            LoginType loginType = (LoginType)PlayerPrefs.GetInt(Config.TYPE_LOGIN_KEY, (int)LoginType.NONE);
            Config.loginType = loginType;
            Debug.Log($"AutoLogin with login type: {Config.loginType}");
            try
            {
                switch (Config.loginType)
                {
                    case LoginType.PLAYNOW:
                        OnClickButtonPlayGuest();
                        break;
                    case LoginType.NORMAL:
                        string userName = PlayerPrefs.GetString("UserName", string.Empty);
                        string password = PlayerPrefs.GetString("PassWord", string.Empty);
                        Debug.Log($"AutoLogin with ID: {userName}, Password: {password}");
                        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                        {
                            return;
                        }
                        UIManager.Instance.ShowProgressing();
                        _ = loginPresenter.OnLoginWithId(userName, password);
                        break;
                    default:
                        Debug.LogWarning("Unknown login type, defaulting to guest login.");
                        break;
                }
            }
            catch (Exception)
            {
                Debug.LogError($"Error during reconnect with login type: {Config.loginType}");
            }
        }
    
        public void OnLoginSuccess()
        {
            UIManager.Instance.HideProgressing();
            _ = OpenLobbyView();
        }
    
        public void OnLoginError(string message)
        {
            UIManager.Instance.HideProgressing();
            // Global.AlertView.SetData(UIManager.Instance.GetText("login_error"));
            // Global.AlertView.isLoginFailed = true;
            UIManager.Instance.ShowAlertDialog(message);
        }
    
        private async UniTask OpenLobbyView()
        {
            try
            {
                await UIManager.Instance.LoadScene(Config.MAIN_SCENE);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                // UIManager.Instance.ShowAlertDialog("Error: " + e.Message);
            }
        }
        
        #region Buttons
        public void OnClickButtonLoginWithID()
        {
            loginForm.SetActive(true);
            loginView.SetActive(false);
        }

        public void OnClickButtonPlayGuest()
        {
            UIManager.Instance.ShowProgressing();
            _ = loginPresenter.OnLoginGuest();
        }

        public void OnClickButtonRegister()
        {
            registerPopup.SetActive(true);
            // loginView.SetActive(false);
        }
    
        private async UniTask HandleClickButtonPlayGuest()
        {
            Config.loginType = LoginType.PLAYNOW;
            PlayerPrefs.SetInt(Config.TYPE_LOGIN_KEY, (int)Config.loginType);
            PlayerPrefs.Save();

            try
            {
                await loginPresenter.OnLoginGuest();
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
            loginForm.GetComponent<BaseView>().Hide(false);
            loginView.SetActive(true);
        }

        public void OnClickButtonSubmit()
        {
            UIManager.Instance.ShowProgressing();
            string userName = idInputField.text.Trim();
            string password = passwordInputField.text.Trim();
            _ = loginPresenter.OnLoginWithId(userName, password);
        }

        public void OnRegisterSuccess(string userName, string password)
        {
            UIManager.Instance.ShowProgressing();
            _ = loginPresenter.OnLoginWithId(userName, password);
        }

        public void OnLoginFbClick()
        {
            UIManager.Instance.ShowProgressing();
            _ = loginPresenter.OnLoginFB();
        }
        
        public void OnSwitchServer(int serverId)
        {
            NetworkManager.INSTANCE.SwitchServer(serverId, transform);
        }
    
        #endregion
    }
}
