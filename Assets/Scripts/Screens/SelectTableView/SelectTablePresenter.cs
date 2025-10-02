using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using UnityEngine;

public class SelectTablePresenter
{
    public static SelectTablePresenter Instance { get; private set; }
    private SelectTableView selectTableView;

    public void Init(SelectTableView view)
    {
        selectTableView = view;
    }

    public async UniTask<Bets> GetListBet(string currentGameId)
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            Bets bets = await DataSender.GetListBet(currentGameId);
            return bets;
        }
        catch (Exception)
        {
            selectTableView.OnError("Error while getting reward!");
            throw;
        }
    }

    public async UniTask<RpcFindMatchResponse> GetListTableByMarkUnit(string currentGameId, int markUnit)
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            RpcFindMatchResponse response = await DataSender.FindMatch(currentGameId, markUnit, false);
            return response;
        }
        catch (Exception)
        {
            selectTableView.OnError("Error while getting match!");
            throw;
        }
    }
}

