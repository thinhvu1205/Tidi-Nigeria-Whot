using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Avatar = Common.Objects.Avatar;

public class SettingsView : BaseView
{
    [SerializeField] private Image toggleSoundImage, toggleMusicImage;
    [SerializeField] private TextMeshProUGUI displayNameText, userIdText;
    [SerializeField] private Avatar avatar;
    private SettingPresenter settingPresenter;

    protected override void Awake()
    {
        base.Awake();
        settingPresenter = new SettingPresenter();
        settingPresenter.Init(this);
    }
    protected override void Start()
    {
        base.Start();
        UpdateVisuals();
        // User.OnProfileUpdated += UpdateVisuals;

    }

    protected override void OnEnable()
    {
        base.OnEnable();
        User.OnProfileUpdated += UpdateVisuals;

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        User.OnProfileUpdated -= UpdateVisuals;
    }

    private void UpdateVisuals()
    {
        if (User.userProfile != null)
        {
            displayNameText.text = User.userProfile.DisplayName;
            userIdText.text = "ID: " + User.userProfile.UserSid;
            avatar.LoadAvatar(User.userProfile.AvatarId);

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
        await UIManager.Instance.LoadScene(Config.LOGIN_SCENE);
    }

    public void OnClickQuitGame()
    {
        Application.Quit();
    }

    public void OnClickSound()
    {
        Config.isOpenSound = !Config.isOpenSound;
        toggleSoundImage.gameObject.SetActive(Config.isOpenSound);
        Config.SaveConfigSettings();
        _ = settingPresenter.UpdateConfig(Config.isOpenSound, Config.isOpenMusic, Config.isVibration);
    }

    public void OnClickMusic()
    {
        Config.isOpenMusic = !Config.isOpenMusic;
        toggleMusicImage.gameObject.SetActive(Config.isOpenMusic);
        Config.SaveConfigSettings();
        SoundManager.Instance.PlayMusicLobby();
        _ = settingPresenter.UpdateConfig(Config.isOpenSound, Config.isOpenMusic, Config.isVibration);

    }

    public void OnClickPrivacyPolicy()
    {

    }

    public void OnClickFeedback()
    {
        Hide();
        UIManager.Instance.OpenFeedback();
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
