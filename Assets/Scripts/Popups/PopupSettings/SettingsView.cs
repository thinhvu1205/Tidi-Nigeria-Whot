using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Avatar = Common.Objects.Avatar;

public class SettingsView : BaseView
{
    [SerializeField] private Image toggleSoundImage, toggleMusicImage;
    [SerializeField] private TextMeshProUGUI displayNameText, userIdText;
    [SerializeField] private Avatar avatar;
    [SerializeField] private Button btnSettingAccountDeletion;
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
            avatar.LoadAvatar(User.userProfile.AvatarId, User.userProfile.VipLevel);

        }
        toggleSoundImage.gameObject.SetActive(Config.isOpenSound);
        toggleMusicImage.gameObject.SetActive(Config.isOpenMusic);
        if (btnSettingAccountDeletion != null)
        {
            btnSettingAccountDeletion.gameObject.SetActive(FeatureManager.IsFeatureAllowed(FeatureName.FeatureSettingAccountDeletion));
        }
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
// #if UNITY_EDITOR
//         await UIManager.Instance.LoadScene(Config.LOGIN_SCENE);
// #endif
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


    public void OnClickDeleteAccount()
    {
        _ = DeleteAccount();
    }

    private async UniTask DeleteAccount()
    {
        UIManager.Instance.ShowProgressing();
        await DataSender.DeleteAccount();
        Config.loginType = LoginType.NONE;
        PlayerPrefs.SetInt(Config.AUTO_LOGIN, 0);
        PlayerPrefs.DeleteKey("UserName");
        PlayerPrefs.DeleteKey("PassWord");
    }

    public void OnClickFeedback()
    {
        Application.OpenURL(Config.feedBackLink);
    }

    public void OnClickPrivacyPolicy()
    {
        Application.OpenURL(Config.privacyPolicyLink);
    }


    public void OnClickFacebook()
    {
        Debug.Log("facebookLink: " + Config.facebookLink);
        Application.OpenURL(Config.facebookLink);
    }

    public void OnClickGroup()
    {
        Application.OpenURL(Config.groupLink);
    }
    #endregion

  


}
