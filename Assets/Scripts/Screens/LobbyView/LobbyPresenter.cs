using System;
using Cysharp.Threading.Tasks;
using Globals;
using Nakama;
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

    public async UniTask<IApiFriendList> GetListFriends()
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            IApiFriendList listFreeChip = await DataSender.GetListFriends();
            await lobbyView.OnSuccess();
            return listFreeChip;
        }
        catch (Exception)
        {
            lobbyView.OnError("Fail to get list friend!");
            return null;
        }
    }
    
    public async UniTask<bool> GetFreeChipList()
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            ListFreeChip listFreeChip = await DataSender.GetListClaimableFreeChips();
            await lobbyView.OnSuccess();
            bool hasFreeChip = listFreeChip.Freechips.Count > 0;
            return hasFreeChip;
        }
        catch (Exception)
        {
            lobbyView.OnError("Fail to ger free chip list!");
            return false;
        }
    }

    public async UniTask<double> GetVipFarmProgress()
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            var response = await DataSender.GetVipFarmProgress();
            await lobbyView.OnSuccess();
            double progress = response.Progress;
            return progress;
        }
        catch (Exception ex)
        {
            lobbyView.OnError("Fail to get Vip Farm progress");
            return 0;
        }
    }

    public async UniTask<ListInAppMessage> GetAnnouncementTicker()
    {
        var inAppMessageData = await DataSender.GetListInAppMessage(TypeInAppMessage.AnnouncementTicker);
        return inAppMessageData;
    }
}

