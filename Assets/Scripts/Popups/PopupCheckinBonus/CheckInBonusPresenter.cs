using System;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using UnityEngine;

public class CheckInBonusPresenter
{
    public static CheckInBonusPresenter Instance { get; private set; }
    private CheckInBonusView checkInBonusView;

    public void Init(CheckInBonusView view)
    {
        Instance = this;
        checkInBonusView = view;
    }

    public async UniTask<DailyRewardTemplate> GetDailyReward()
    {
        DailyRewardTemplate dailyReward = await DataSender.GetDailyRewardTemplate();
        return dailyReward;
    }
}

