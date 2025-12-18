using System;
using Cysharp.Threading.Tasks;
using Globals;
using Nakama;
using Proto;
using UnityEngine;

public class LeaderboardPresenter
{
    public static LeaderboardPresenter Instance { get; private set; }
    private LeaderBoardView leaderBoardView;

    public void Init(LeaderBoardView view)
    {
        Instance = this;
        leaderBoardView = view;
    }

    public async UniTask<GameListResponse> LoadGameList()
    {
        GameListResponse gameListResponse = await DataSender.GetListGame();
        return gameListResponse;
    }

    public async UniTask<LeaderBoardRecord> LoadInfo(string gameCode)
    {
        LeaderBoardRecord leaderBoardRecord = await DataSender.GetLeaderBoardRecord(gameCode);
        return leaderBoardRecord;
    }

    public async UniTask<IApiLeaderboardRecordList> LoadList(string gameCode, string userId = null)
    {
        IApiLeaderboardRecordList leaderboardRecordList = await NetworkManager.INSTANCE.GetListLeaderboard(gameCode, userId: userId);
        return leaderboardRecordList;
    }
}

