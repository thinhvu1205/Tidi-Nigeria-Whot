using System;
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
        if (User.Profile != null)
        {
            displayNameText.text = User.Profile.DisplayName;
            userIdText.text = "ID: " + User.Profile.Cid;
            avatar.LoadAvatar(User.Profile.AvatarId.ToString(), User.Profile.Vip);

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
        try
        {
            await NetworkManager.INSTANCE.LogoutAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning("Logout fail but local cleared: " + e);
        }

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
        UIManager.Instance.ShowConfirmDialog("Are you sure you want to delete your account?", () => _ = DeleteAccount(), null);
        
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
        if (Config.isConfigMode && !string.IsNullOrEmpty(Config.feedBackLink))
        {
            Application.OpenURL(Config.feedBackLink);
        }
        else
        {
            UIManager.Instance.OpenFeedback();
        }
    }

    public void OnClickPrivacyPolicy()
    {
        Application.OpenURL(Config.privacyPolicyLink);
    }


    public void OnClickFacebook()
    {
        Application.OpenURL(Config.facebookLink);
    }

    public void OnClickGroup()
    {
        Application.OpenURL(Config.groupLink);
    }
    #endregion

  


}
