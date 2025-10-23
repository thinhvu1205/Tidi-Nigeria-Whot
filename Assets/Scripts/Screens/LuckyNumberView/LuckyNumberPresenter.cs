using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using UnityEngine;

public class LuckyNumberPresenter
{
    public static LuckyNumberPresenter Instance { get; private set; }
    private LuckyNumberView luckyNumberView;

    public void Init(LuckyNumberView view)
    {
        Instance = this;
        luckyNumberView = view;
    }

    public async UniTask<BuyLotteryTicketResponse> BuyLotteryTicket(List<int> numbers, long drawId)
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            BuyLotteryTicketResponse buyLotteryTicketResponse = await DataSender.BuyLotteryTicket(numbers, drawId);
            await luckyNumberView.OnSuccess("Lottery ticket purchased successfully!", true);
            return buyLotteryTicketResponse;
        }
        catch (Exception)
        {
            luckyNumberView.OnError("Failed to buy lottery ticket");
            return null;
        }
    }

    public async UniTask<BuyMultipleLotteryTicketsResponse> BuyMultipleLotteryTickets(List<LotteryTicketInput> tickets)
    {
        try
        {
            BuyMultipleLotteryTicketsResponse buyMultipleLotteryTicketsResponse = await DataSender.BuyMultipleLotteryTickets(tickets);
            return buyMultipleLotteryTicketsResponse;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async UniTask<GetAvailableDrawsResponse> GetAvailableDraws()
    {
        try
        {
            GetAvailableDrawsResponse response = await DataSender.GetAvailableDraws();
            return response;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async UniTask<GetLatestDrawResultResponse> GetLatestDrawResult()
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            GetLatestDrawResultResponse response = await DataSender.GetLatestDrawResult();
            await luckyNumberView.OnSuccess();
            return response;
        }
        catch (Exception)
        {
            luckyNumberView.OnError("Failed to get latest draw result");
            return null;
        }
    }

    public async UniTask<GetLotteryHistoryResponse> GetLotteryHistory()
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            GetLotteryHistoryResponse response = await DataSender.GetLotteryHistory();
            await luckyNumberView.OnSuccess();
            return response;
        }
        catch (Exception)
        {
            luckyNumberView.OnError("Failed to get lottery history");
            return null;
        }
    }

    public async UniTask<QuickPickResponse> QuickPick()
    {
        UIManager.Instance.ShowProgressing();
        try
        {
            QuickPickResponse response = await DataSender.QuickPick();
            await luckyNumberView.OnSuccess();
            return response;
        }
        catch (Exception)
        {
            luckyNumberView.OnError("Failed to generate quick pick numbers");
            return null;
        }
    }
}