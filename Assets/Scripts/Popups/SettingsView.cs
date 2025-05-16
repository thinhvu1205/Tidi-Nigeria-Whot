using System.Collections;
using System.Collections.Generic;
using Globals;
using UnityEngine;
using UnityEngine.UI;

public class SettingsView : BaseView
{
    [SerializeField] private Image toggleSoundImage, toggleMusicImage;
    
    protected override void Start()
    {
        base.Start();
        InitVisual();
    }

    private void InitVisual()
    {
        toggleSoundImage.gameObject.SetActive(Config.isOpenSound);
        toggleMusicImage.gameObject.SetActive(Config.isOpenMusic);
    }

    #region Button
    public void OnClickLogout()
    {

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
