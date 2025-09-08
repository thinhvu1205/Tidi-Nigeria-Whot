using System;
using System.Threading.Tasks;
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

    public async Task<GameListResponse> LoadGameList()
    {
        GameListResponse gameListResponse = await DataSender.GetListGame();
        return gameListResponse;
    }

    public async Task<LeaderBoardRecord> LoadInfo(string gameCode)
    {
        LeaderBoardRecord leaderBoardRecord = await DataSender.GetLeaderBoardRecord(gameCode);
        return leaderBoardRecord;
    }

    public async Task<IApiLeaderboardRecordList> LoadList(string gameCode)
    {
        IApiLeaderboardRecordList leaderboardRecordList = await NetworkManager.INSTANCE.GetListLeaderboard(gameCode);
        return leaderboardRecordList;
    }
}

