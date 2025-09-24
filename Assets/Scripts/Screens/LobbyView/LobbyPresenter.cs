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

    public async UniTask<Reward> GetClaimableReward()
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            Reward reward = await DataSender.GetClaimableDailyReward();
            Debug.Log("reward " + reward);
            if (reward.CanClaim && reward.DeviceAllowed)
            {
                return reward;
            }
            return reward;
        }
        catch (Exception e)
        {
            lobbyView.OnError("Error while getting reward!");
            return null;
        }
    }
}

