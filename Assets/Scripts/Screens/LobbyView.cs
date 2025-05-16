using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyView : BaseView
{
    #region Buttons

    public void OnClickLeaderboard() => UIManager.Instance.OpenLeaderboard();
    public void OnClickFreeChips() => UIManager.Instance.OpenFreeChips();
    public void OnClickShop() => UIManager.Instance.OpenShop();
    public void OnClickMail() => UIManager.Instance.OpenMail();
    public void OnClickChipOnline() => UIManager.Instance.OpenChipOnline();
    public void OnClickFriend() => UIManager.Instance.OpenFriend();
    public void OnClickSetting() => UIManager.Instance.OpenSetting();

    #endregion
}
