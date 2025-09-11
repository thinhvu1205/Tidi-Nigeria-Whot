using System;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using UnityEngine;

public class SettingPresenter
{
    public static SettingPresenter Instance { get; private set; }
    private SettingsView settingView;

    public void Init(SettingsView view)
    {
        Instance = this;
        settingView = view;
    }

    public async UniTask UpdateConfig(bool isOpenSound, bool isOpenMusic, bool isVibration)
    {
        try
        {
            string config = $"\"sound\": \"{isOpenSound}\",\"music\": \"{isOpenMusic}\",\"vibration\": \"{isVibration}\"";
            await DataSender.UpdateConfig(config);
            // await settingView.ShowToast("Update avatar successfully!", true);
        }
        catch (Exception)
        {
            await settingView.ShowToast("Error when update config!");
            throw;
        }
    }
}

