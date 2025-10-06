using System;
using Cysharp.Threading.Tasks;
using Globals;
using UnityEngine;

namespace Screens.LoginView
{
    public class LoginPresenter
    {
        public static LoginPresenter Instance { get; private set; }
        private LoginView loginView;
    
        public void Init(LoginView view)
        {
            Instance = this;
            loginView = view;
            // InitFacebook();
        }
    
        public async UniTask OnLoginGuest()
        {
            string deviceId = PlayerPrefs.GetString("deviceId", "");
            Config.loginType = LoginType.PLAYNOW;
        
            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = SystemInfo.deviceUniqueIdentifier;
                PlayerPrefs.SetString("deviceId", deviceId);
            }
            await StartLoginGuest(deviceId);
        }
    
        private async UniTask StartLoginGuest(string deviceId)
        {
            try
            {
                await NetworkManager.INSTANCE.LoginGuest(deviceId);
                PlayerPrefs.SetInt(Config.TYPE_LOGIN_KEY, (int)LoginType.PLAYNOW);
                PlayerPrefs.SetInt(Config.AUTO_LOGIN, 1);
                loginView.OnLoginSuccess();
            }
            catch (Exception err)
            {
                loginView.OnLoginError("Fail to connect to server");
                Debug.LogError($"Login guest error: {err}");
            }
        }
    
        public async UniTask OnLoginEmail(string email, string pass)
        {
            Config.loginType = LoginType.NORMAL;
        
            try
            {
                await NetworkManager.INSTANCE.LoginEmail("", pass, email);
                PlayerPrefs.SetInt(Config.TYPE_LOGIN_KEY, (int)LoginType.NORMAL);
                PlayerPrefs.SetInt(Config.AUTO_LOGIN, 1);
                PlayerPrefs.SetString("UserName", email);
                PlayerPrefs.SetString("PassWord", pass);
                loginView.OnLoginSuccess();
            }
            catch (Exception err)
            {
                loginView.OnLoginError(err.Message);
                Debug.LogError($"Login email error: {err}");
            }
        }
    
        public async UniTask OnLoginFB()
        {
            // Global.LoginType = Constants.LOGIN_TYPE.FB;
            try
            {
                string token = GetFacebookToken();
                await NetworkManager.INSTANCE.LoginFacebook(token);
                PlayerPrefs.SetInt("loginType", (int)Config.loginType);
                PlayerPrefs.SetString("isAutoLogin", "true");
                loginView.OnLoginSuccess();
            }
            catch (Exception err)
            {
                loginView.OnLoginError(err.Message);
                Debug.LogError($"Login FB error: {err}");
            }
        }

        private string GetFacebookToken()
        {
            return "";
        }
    }
}
