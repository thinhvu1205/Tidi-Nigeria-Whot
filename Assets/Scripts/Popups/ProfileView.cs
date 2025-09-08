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
        if (User.userProfile != null)
        {
            nameText.text = User.userProfile.DisplayName;
            idText.text = "ID: " + User.userProfile.UserSid;
            chipText.text = User.userProfile.AccountChip.ToString();
            if (IsDefaultName(User.userProfile.DisplayName))
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
    public void OnClickChangePassword() {
        Hide();
        UIManager.Instance.OpenChangePassword();
    } 
    public void OnClickChangeName() {
        Hide();
        UIManager.Instance.OpenChangeName();
    } 

    #endregion
    
}
