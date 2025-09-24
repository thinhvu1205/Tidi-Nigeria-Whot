using System;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using UnityEngine;

public class ProfilePresenter
{
    public static ProfilePresenter Instance { get; private set; }
    private ProfileView profileView;

    public void Init(ProfileView view)
    {
        Instance = this;
        profileView = view;
    }

    public async UniTask OnUpdateAvatar(string avatarName)
    {
        try
        {
            await DataSender.UpdateAvatar(avatarName);
            await profileView.ShowToast("Update avatar successfully!", true);
        }
        catch (Exception)
        {
            // await profileView.ShowToast("Error when update avatar!");
            throw;
        }
    }
}

