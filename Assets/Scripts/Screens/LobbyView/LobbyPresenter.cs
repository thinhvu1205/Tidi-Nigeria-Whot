using System;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using UnityEngine;

public class LobbyPresenter
{
    private LobbyView lobbyView;

    public void Init(LobbyView view)
    {
        lobbyView = view;
    }

    public async UniTask<GameListResponse> GetListGame()
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            GameListResponse gameListResponse = await DataSender.GetListGame();
            return gameListResponse;
        }
        catch (Exception e)
        {
            lobbyView.OnError("Error while getting game list!");
            return null;
        }
    }

    public async UniTask<(Reward, bool, bool, bool)> GetClaimableReward()
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            Reward dailyReward = await DataSender.GetClaimableDailyReward();
            Reward weeklyReward = await DataSender.GetClaimableWeeklyReward();
            Debug.Log("daily reward: " + dailyReward);
            Debug.Log("weekly reward: " + weeklyReward);
            bool canClaim = (dailyReward.CanClaim && dailyReward.DeviceAllowed) || (weeklyReward.CanClaim && weeklyReward.DeviceAllowed);
            bool isDeviceAllowed = dailyReward.DeviceAllowed && weeklyReward.DeviceAllowed;
            bool hasReachedMaxStreak = dailyReward.ReachMaxStreak;
            return (dailyReward, canClaim, isDeviceAllowed, hasReachedMaxStreak);
        }
        catch (Exception e)
        {
            lobbyView.OnError("Error while getting reward!");
            return (null, false, false, false);
        }
    }
}

