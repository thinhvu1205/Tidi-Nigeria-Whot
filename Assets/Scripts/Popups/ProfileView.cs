using System.Collections;
using System.Collections.Generic;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileView : BaseView
{
    [SerializeField] private TextMeshProUGUI nameText, idText, chipText;
    [SerializeField] private Image avatarImage;
    [SerializeField] private Button changePasswordButton, changeNameButton;

    protected override void Awake()
    {
        base.Awake();
        UpdateProfileVisuals();
    }

    private void UpdateProfileVisuals()
    {
        if (User.userMain != null)
        {
            nameText.text = User.userMain.displayName;
            idText.text = "ID: " + User.userMain.userId;
            chipText.text = User.userMain.accountChip;
            if (IsDefaultName(User.userMain.displayName))
            {
                changeNameButton.gameObject.SetActive(true);
                changePasswordButton.gameObject.SetActive(false);
            }
            else
            {
                changeNameButton.gameObject.SetActive(false);
                changePasswordButton.gameObject.SetActive(true);
            }
        }
    }

    private bool IsDefaultName(string name)
    {
        return name.Contains("CGPD.");
    }

    #region Buttons
    public void OnClickChangePassword() => UIManager.Instance.OpenChangePassword();
    public void OnClickChangeName() => UIManager.Instance.OpenChangeName();

    public async void OnClickLogout()
    {
        await DataSender.Logout();
    }

    #endregion
    
}
