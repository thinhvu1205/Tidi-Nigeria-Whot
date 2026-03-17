using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using UnityEngine;
using Yuujins.Cfg.Game.V1;
using Yuujins.Match.V1;

public class SelectTablePresenter
{
    public static SelectTablePresenter Instance { get; private set; }
    private SelectTableView selectTableView;

    public void Init(SelectTableView view)
    {
        selectTableView = view;
    }

    /// <summary>Gọi match_list_bet_levels (lobby) lấy danh sách mức cược PVP/bet theo game_id. Không dùng prefix cfg.</summary>
    public async UniTask<ListBetLevelsResponse> GetListBet(uint gameId)
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            var bets = await DataSender.MatchListBetLevelsAsync(gameId);
            return bets ?? new ListBetLevelsResponse();
        }
        catch (Exception)
        {
            selectTableView.OnError("Error while getting reward!");
            throw;
        }
    }

    public async UniTask<FindMatchResponse> GetListTableByMarkUnit(uint currentGameId, int markUnit)
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            return await DataSender.MatchFindMatchAsync(currentGameId, markUnit, false);
        }
        catch (Exception)
        {
            selectTableView.OnError("Error while getting match!");
            throw;
        }
    }

    public async UniTask<FindMatchResponse> FindTable(uint currentGameId, string tableId)
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            return await DataSender.MatchFindMatchAsync(currentGameId, 0, false, excludeMatchId: tableId);
        }
        catch (Exception)
        {
            selectTableView.OnError("Error while getting match!");
            throw;
        }
    }
}

