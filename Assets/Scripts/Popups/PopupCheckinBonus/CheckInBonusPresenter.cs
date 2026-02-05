using System;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using UnityEngine;

public class CheckInBonusPresenter
{
    private CheckInBonusView checkInBonusView;

    public void Init(CheckInBonusView view)
    {
        checkInBonusView = view;
    }

    public async UniTask<DailyRewardTemplate> GetDailyReward()
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            DailyRewardTemplate dailyReward = await DataSender.GetDailyRewardTemplate();
            return dailyReward;
        }
        catch (Exception e)
        {
            checkInBonusView.OnError("Error while getting reward!");
            return null;
        }
    }


    public async UniTask<Reward> GetClaimableDailyReward()
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            Reward reward = await DataSender.GetClaimableDailyReward();
            if (reward.CanClaim && reward.DeviceAllowed)
            {
                return reward;
            }
            return reward;
        }
        catch (Exception e)
        {
            checkInBonusView.OnError("Error while getting reward!");
            return null;
        }

    }

    public async UniTask<Reward> ClaimDailyReward()
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            Reward reward = await DataSender.ClaimDailyReward();
            checkInBonusView.ReceiveDailyReward();
            await checkInBonusView.OnSuccess();
            return reward;
        }
        catch (Exception e)
        {
            checkInBonusView.OnError("Error while claim gift!");
            return null;
        }
    }

    public async UniTask<WeeklyBonusTemplate> GetWeeklyReward()
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            WeeklyBonusTemplate weeklyReward = await DataSender.GetWeeklyRewardTemplate();
            return weeklyReward;
        }
        catch (Exception e)
        {
            checkInBonusView.OnError("Error while getting reward!");
            return null;
        }
    }
    
    public async UniTask<Reward> GetClaimableWeeklyReward()
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            Reward reward = await DataSender.GetClaimableWeeklyReward();
            // if (reward.CanClaim && reward.DeviceAllowed)
            // {
            //     return reward;
            // }
            return reward;
        }
        catch (Exception e)
        {
            checkInBonusView.OnError("Error while getting reward!");
            return null;
        }

    }

    public async UniTask<Reward> ClaimWeeklyReward()
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            Reward reward = await DataSender.ClaimWeeklyReward();
            await checkInBonusView.OnSuccess();
            checkInBonusView.ReceiveWeeklyReward();
            return reward;
        }
        catch (Exception e)
        {
            checkInBonusView.OnError("Error while claim gift!");
            return null;
        }
    }
}

