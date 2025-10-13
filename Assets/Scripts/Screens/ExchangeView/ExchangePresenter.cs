using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Globals;
using Nakama;
using Proto;
using UnityEngine;

public class ExchangePresenter
{
    public static ExchangePresenter Instance { get; private set; }
    private ExchangeView exchangeView;

    public void Init(ExchangeView view)
    {
        Instance = this;
        exchangeView = view;
    }

    public async Task<ExchangeDealInShop> GetListExchangeDeal()
    {
        try
        {
            ExchangeDealInShop exchangeDealInShop = await DataSender.GetListExchangeDeal();
            return exchangeDealInShop;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<ListExchangeInfo> GetListExchange()
    {
        try
        {
            ListExchangeInfo exchangeInfo = await DataSender.GetListExchange();
            return exchangeInfo;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<ExchangeInfo> AddExchange(string cashId, string dealId)
    {
        try
        {
            ExchangeInfo exchangeInfo = await DataSender.AddExchange(cashId, dealId);
            return exchangeInfo;
        }
        catch (Exception)
        {
            return null;
        }
    }
}

