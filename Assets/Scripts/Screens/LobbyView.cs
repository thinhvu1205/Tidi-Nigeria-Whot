using System.Collections;
using System.Collections.Generic;
using Globals;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyView : BaseView
{
    #region Buttons
    public void Logout()
    {
        try
        {
            DataSender.Logout();
        }
        catch (System.Exception)
        {

            throw;
        }
    }

    public void OnClickLeaderboard() => UIManager.Instance.OpenLeaderboard();
    public void OnClickFreeChips() => UIManager.Instance.OpenFreeChips();
    public void OnClickShop() => UIManager.Instance.OpenShop();
    public void OnClickMail() => UIManager.Instance.OpenMail();
    public void OnClickChipOnline() => UIManager.Instance.OpenChipOnline();
    public void OnClickFriend() => UIManager.Instance.OpenFriend();
    public void OnClickSetting() => UIManager.Instance.OpenSetting();
    #endregion
}
