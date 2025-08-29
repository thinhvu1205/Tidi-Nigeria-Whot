using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsView : BaseView
{
    [SerializeField] private Image toggleSoundImage, toggleMusicImage;
    [SerializeField] private TextMeshProUGUI displayNameText, userIdText;
    
    protected override void Start()
    {
        base.Start();
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (User.userMain != null)
        {
            displayNameText.text = User.userMain.displayName;
            userIdText.text = "ID: " + User.userMain.userSid;
        }
        toggleSoundImage.gameObject.SetActive(Config.isOpenSound);
        toggleMusicImage.gameObject.SetActive(Config.isOpenMusic);
    }

    #region Button
    public void OnClickLogout()
    {
        _ = HandleLogOut();
    }

    private async UniTask HandleLogOut()
    {
        // Global.ChatView.clearPrivateChat();
        // Global.BannerData.IsShow[Constants.BANNER_SHOW_TYPE.HOME_PAGE] = false;
        // this.onClose();
        await NetworkManager.INSTANCE.LogoutAsync();
        Config.loginType = LoginType.NONE;
        PlayerPrefs.SetInt(Config.AUTO_LOGIN, 0);
        PlayerPrefs.DeleteKey("UserName");
        PlayerPrefs.DeleteKey("PassWord");
        UIManager.Instance.OpenLoginScene();
    }

    public void OnClickQuitGame()
    {

    }

    public void OnClickSound()
    {
        Config.isOpenSound = !Config.isOpenSound;
        toggleSoundImage.gameObject.SetActive(Config.isOpenSound);
        Config.SaveConfigSettings();
    }

    public void OnClickMusic()
    {
        Config.isOpenMusic = !Config.isOpenMusic;
        toggleMusicImage.gameObject.SetActive(Config.isOpenMusic);
        Config.SaveConfigSettings();
        SoundManager.Instance.PlayMusicLobby();
    }

    public void OnClickPrivacyPolicy()
    {

    }

    public void OnClickFeedback()
    {

    }

    public void OnClickDeleteAccount()
    {

    }

    public void OnClickFacebook()
    {

    }

    public void OnClickGroup()
    {

    }
    #endregion

  


}
