using System;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using UnityEngine;

public class VipFarmPresenter
{
    public static VipFarmPresenter Instance { get; private set; }
    private VipFarmView vipFarmView;

    public void Init(VipFarmView view)
    {
        Instance = this;
        vipFarmView = view;
    }

    public async UniTask<UserVipFarmProgress> GetVipFarmProgress()
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            var response = await DataSender.GetVipFarmProgress();
            await vipFarmView.OnSuccess();
            return response;
        }
        catch (Exception ex)
        {
            vipFarmView.OnError("Fail to get Vip Farm progress");
            return null;
        }
    }

    public async UniTask<UserVipFarm> ClaimVipFarm()
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            var response = await DataSender.ClaimVipFarm();
            await vipFarmView.OnSuccess("", true);
            return response;
        }
        catch (Exception ex)
        {
            vipFarmView.OnError("Fail to claim Vip Farm");
            return null;
        }
    }
}

