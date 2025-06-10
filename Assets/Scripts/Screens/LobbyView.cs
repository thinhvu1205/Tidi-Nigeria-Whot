using System.Collections;
using System.Collections.Generic;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyView : BaseView
{
    [SerializeField] private TextMeshProUGUI displayNameText, userIdText, accountChip;

    protected override void Awake()
    {
        base.Awake();
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (User.userMain != null)
        {
            displayNameText.text = User.userMain.displayName;
            userIdText.text = "ID: " + User.userMain.userSid;
            accountChip.text = User.userMain.accountChip;
        }
    }
    #region Buttons
    public void OnClickProfile() => UIManager.Instance.OpenProfile();
    public void OnClickLeaderboard() => UIManager.Instance.OpenLeaderboard();
    public void OnClickFreeChips() => UIManager.Instance.OpenFreeChips();
    public void OnClickShop() => UIManager.Instance.OpenShop();
    public void OnClickMail() => UIManager.Instance.OpenMail();
    public void OnClickChipOnline() => UIManager.Instance.OpenChipOnline();
    public void OnClickFriend() => UIManager.Instance.OpenFriend();
    public void OnClickSetting() => UIManager.Instance.OpenSetting();
    #endregion
}
