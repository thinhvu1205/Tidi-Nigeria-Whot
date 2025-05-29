using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileView : BaseView
{
    public void OnClickChangePassword() => UIManager.Instance.OpenChangePassword();
    public void OnClickChangeName() => UIManager.Instance.OpenChangeName();

    public void OnClickLogout()
    {
        DataSender.Logout();
    }
    
}
